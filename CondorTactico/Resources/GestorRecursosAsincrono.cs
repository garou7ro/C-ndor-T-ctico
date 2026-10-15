// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 3.1.0
// ==============================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace CondorTactico
{
    /// <summary>
    /// Motor de gestión de recursos y streaming en segundo plano de alto rendimiento.
    /// Garantiza que la carga y decodificación de texturas, mallas y audio ocurra en hilos de fondo
    /// sin producir caídas de cuadros (framerate drops) ni micro-tirones (stuttering) en el bucle principal.
    /// </summary>
    public sealed class GestorRecursosAsincrono : IDisposable
    {
        private static readonly Lazy<GestorRecursosAsincrono> _instanciaLazy =
            new Lazy<GestorRecursosAsincrono>(() => new GestorRecursosAsincrono());

        public static GestorRecursosAsincrono Instancia => _instanciaLazy.Value;

        // Caché en memoria concurrente O(1) de recursos listos
        private readonly ConcurrentDictionary<string, IRecursoJuego> _cache = new ConcurrentDictionary<string, IRecursoJuego>();

        // Canal de trabajo en segundo plano para recepción no bloqueante
        private readonly Channel<SolicitudCargaRecurso> _canalCarga;
        private readonly CancellationTokenSource _ctsWorker = new CancellationTokenSource();
        private readonly Task _workerTask;

        // Eventos y callbacks para HUD y telemetría
        public event Action<string, double>? AlActualizarProgreso;
        public event Action<string, IRecursoJuego>? AlCompletarRecurso;

        // Límites de memoria y telemetría en tiempo real
        public long LimitePresupuestoMemoriaBytes { get; set; } = 256 * 1024 * 1024; // 256 MB por defecto
        private long _memoriaEnCacheBytes = 0;
        private int _colaPendienteContador = 0;

        public int TotalRecursosEnCache => _cache.Count;
        public int TareasPendientesEnCola => Math.Max(0, _colaPendienteContador);
        public double MemoriaTotalMB => Interlocked.Read(ref _memoriaEnCacheBytes) / (1024.0 * 1024.0);

        public GestorRecursosAsincrono()
        {
            var opcionesCanal = new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false
            };
            _canalCarga = Channel.CreateUnbounded<SolicitudCargaRecurso>(opcionesCanal);
            _workerTask = Task.Run(ProcesarColaSegundoPlanoAsync);
        }

        /// <summary>
        /// Bucle de procesamiento en segundo plano continuo que consume las solicitudes de carga.
        /// </summary>
        private async Task ProcesarColaSegundoPlanoAsync()
        {
            var lector = _canalCarga.Reader;
            while (!_ctsWorker.IsCancellationRequested && await lector.WaitToReadAsync(_ctsWorker.Token).ConfigureAwait(false))
            {
                while (lector.TryRead(out SolicitudCargaRecurso? solicitud))
                {
                    Interlocked.Decrement(ref _colaPendienteContador);

                    if (solicitud.CancellationToken.IsCancellationRequested)
                    {
                        solicitud.Tcs.TrySetCanceled();
                        continue;
                    }

                    try
                    {
                        // Si ya está cargado en caché, devolverlo inmediatamente
                        if (_cache.TryGetValue(solicitud.Id, out IRecursoJuego? existente) && existente.Estado == EstadoRecurso.Listo)
                        {
                            existente.MarcarAcceso();
                            solicitud.Tcs.TrySetResult(existente);
                            continue;
                        }

                        // Ejecutar la fábrica de generación/carga en este hilo de fondo
                        IRecursoJuego recurso = solicitud.FactoriaCarga();
                        recurso.MarcarAcceso();

                        // Registrar en la caché atómicamente
                        _cache.AddOrUpdate(
                            solicitud.Id,
                            recurso,
                            (key, viejo) =>
                            {
                                Interlocked.Add(ref _memoriaEnCacheBytes, -viejo.TamanoBytesAproximado);
                                viejo.Dispose();
                                return recurso;
                            });

                        Interlocked.Add(ref _memoriaEnCacheBytes, recurso.TamanoBytesAproximado);

                        // Evaluar desalojo LRU si se supera el presupuesto
                        VerificarPresupuestoMemoria();

                        solicitud.Tcs.TrySetResult(recurso);
                        AlCompletarRecurso?.Invoke(solicitud.Id, recurso);
                    }
                    catch (Exception ex)
                    {
                        solicitud.Tcs.TrySetException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Registra un recurso ya generado o cargado en la caché gestionada y actualiza la telemetría de memoria.
        /// </summary>
        public void RegistrarRecursoListo(IRecursoJuego recurso)
        {
            if (recurso == null) return;
            recurso.MarcarAcceso();

            _cache.AddOrUpdate(
                recurso.Id,
                recurso,
                (key, viejo) =>
                {
                    Interlocked.Add(ref _memoriaEnCacheBytes, -viejo.TamanoBytesAproximado);
                    viejo.Dispose();
                    return recurso;
                });

            Interlocked.Add(ref _memoriaEnCacheBytes, recurso.TamanoBytesAproximado);
            AlCompletarRecurso?.Invoke(recurso.Id, recurso);
        }

        /// <summary>
        /// Obtiene un recurso de forma INMEDIATA en O(1) desde el hilo de renderizado sin congelar la pantalla.
        /// Si el recurso aún no ha terminado de cargar, retorna el fallback por defecto y encola su carga.
        /// </summary>
        public T ObtenerRecurso<T>(string id, T fallback, Func<T>? factoriaSiFalta = null, PrioridadCarga prioridad = PrioridadCarga.Normal) where T : class, IRecursoJuego
        {
            if (_cache.TryGetValue(id, out IRecursoJuego? recurso))
            {
                if (recurso.Estado == EstadoRecurso.Listo && recurso is T recursoTipado)
                {
                    recursoTipado.MarcarAcceso();
                    return recursoTipado;
                }
            }

            // Si no está y se proporcionó una factoría, encolarla para carga en segundo plano
            if (factoriaSiFalta != null && !_cache.ContainsKey(id))
            {
                SolicitarCargaSegundoPlano(id, factoriaSiFalta, prioridad);
            }

            return fallback;
        }

        /// <summary>
        /// Solicita de manera no bloqueante la carga de un recurso en segundo plano.
        /// </summary>
        public Task<IRecursoJuego> SolicitarCargaSegundoPlano(
            string id,
            Func<IRecursoJuego> factoria,
            PrioridadCarga prioridad = PrioridadCarga.Normal,
            CancellationToken ct = default)
        {
            if (_cache.TryGetValue(id, out IRecursoJuego? existente) && existente.Estado == EstadoRecurso.Listo)
            {
                existente.MarcarAcceso();
                return Task.FromResult(existente);
            }

            Interlocked.Increment(ref _colaPendienteContador);
            var solicitud = new SolicitudCargaRecurso(id, prioridad, factoria, ct);
            _canalCarga.Writer.TryWrite(solicitud);
            return solicitud.Tcs.Task;
        }

        /// <summary>
        /// Precarga un conjunto de recursos en segundo plano informando progreso (0.0 a 1.0) sin bloquear la UI.
        /// </summary>
        public async Task PrecargarLoteAsincronoAsync(
            IReadOnlyList<(string id, Func<IRecursoJuego> factoria, PrioridadCarga prioridad)> lote,
            IProgress<double>? progreso = null)
        {
            if (lote == null || lote.Count == 0) return;

            int completados = 0;
            var tareas = new List<Task>(lote.Count);

            foreach (var item in lote)
            {
                var tarea = SolicitarCargaSegundoPlano(item.id, item.factoria, item.prioridad)
                    .ContinueWith(t =>
                    {
                        int actual = Interlocked.Increment(ref completados);
                        double pct = (double)actual / lote.Count;
                        progreso?.Report(pct);
                        AlActualizarProgreso?.Invoke(item.id, pct);
                    }, TaskScheduler.Default);

                tareas.Add(tarea);
            }

            await Task.WhenAll(tareas).ConfigureAwait(false);
        }

        /// <summary>
        /// Política de desalojo LRU (Least Recently Used) que libera recursos antiguos si se excede el límite de RAM.
        /// </summary>
        private void VerificarPresupuestoMemoria()
        {
            long memoriaActual = Interlocked.Read(ref _memoriaEnCacheBytes);
            if (memoriaActual <= LimitePresupuestoMemoriaBytes) return;

            // Ordenar por el tick de acceso más antiguo
            var candidatos = _cache.Values
                .Where(r => r.Estado == EstadoRecurso.Listo)
                .OrderBy(r => r.UltimoAccesoTicks)
                .ToList();

            foreach (var recurso in candidatos)
            {
                if (Interlocked.Read(ref _memoriaEnCacheBytes) <= LimitePresupuestoMemoriaBytes * 0.85)
                    break; // Llegamos a una zona segura

                if (_cache.TryRemove(recurso.Id, out IRecursoJuego? removido))
                {
                    Interlocked.Add(ref _memoriaEnCacheBytes, -removido.TamanoBytesAproximado);
                    removido.Dispose();
                }
            }
        }

        public void LimpiarCache()
        {
            foreach (var kvp in _cache)
            {
                if (_cache.TryRemove(kvp.Key, out IRecursoJuego? r))
                {
                    r.Dispose();
                }
            }
            Interlocked.Exchange(ref _memoriaEnCacheBytes, 0);
        }

        public void Dispose()
        {
            _ctsWorker.Cancel();
            _canalCarga.Writer.TryComplete();
            LimpiarCache();
            _ctsWorker.Dispose();
        }
    }
}


