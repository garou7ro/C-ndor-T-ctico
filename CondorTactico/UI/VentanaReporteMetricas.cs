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
using System.Drawing;
using System.Windows.Forms;

namespace CondorTactico.UI
{
    /// <summary>
    /// Subventana desacoplada para telemetría en tiempo real, métricas de ingeniería de software y profiling Big-O.
    /// </summary>
    public sealed class VentanaReporteMetricas : Form
    {
        private readonly ProfilerMotor _profiler;
        private readonly GestorRedClienteServidor _gestorRed;
        private readonly AeronaveBase _naveJugador;

        private Label lblFps = null!;
        private Label lblCpu = null!;
        private Label lblMemoria = null!;
        private Label lblBigO = null!;
        private Label lblRed = null!;
        private TextBox txtReportePreview = null!;
        private Button btnExportar = null!;
        private Button btnCerrar = null!;
        private System.Windows.Forms.Timer _timerActualizacion = null!;

        public VentanaReporteMetricas(
            ProfilerMotor profiler,
            GestorRedClienteServidor gestorRed,
            AeronaveBase naveJugador)
        {
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
            _gestorRed = gestorRed ?? throw new ArgumentNullException(nameof(gestorRed));
            _naveJugador = naveJugador ?? throw new ArgumentNullException(nameof(naveJugador));

            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "CÓNDOR TÁCTICO | TELEMETRÍA, PROFILING Y AUDITORÍA BIG-O";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(540, 480);
            this.BackColor = Color.FromArgb(14, 23, 36);
            this.ForeColor = Color.FromArgb(220, 235, 250);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            Font fTitulo = new Font("Segoe UI", 10f, FontStyle.Bold);
            Font fMono = new Font("Consolas", 8.8f, FontStyle.Regular);

            // GroupBox 1: Rendimiento y CPU
            GroupBox gbRendimiento = new GroupBox
            {
                Text = " RENDIMIENTO DEL MOTOR Y HARDWARE ",
                ForeColor = Color.FromArgb(100, 220, 255),
                Font = fTitulo,
                Location = new Point(16, 12),
                Size = new Size(508, 115)
            };

            lblFps = new Label
            {
                Text = "FPS: Calculando... | V-Sync: 60 Hz",
                ForeColor = Color.FromArgb(80, 255, 140),
                Font = fMono,
                Location = new Point(14, 25),
                Size = new Size(480, 20)
            };
            gbRendimiento.Controls.Add(lblFps);

            lblCpu = new Label
            {
                Text = "Tiempo de Cuadro (Física + Render): ... µs",
                ForeColor = Color.FromArgb(240, 240, 255),
                Font = fMono,
                Location = new Point(14, 50),
                Size = new Size(480, 20)
            };
            gbRendimiento.Controls.Add(lblCpu);

            lblMemoria = new Label
            {
                Text = "Memoria RAM Gestionada: ... MB | GC Gen 0/1/2",
                ForeColor = Color.FromArgb(255, 220, 100),
                Font = fMono,
                Location = new Point(14, 75),
                Size = new Size(480, 20)
            };
            gbRendimiento.Controls.Add(lblMemoria);
            this.Controls.Add(gbRendimiento);

            // GroupBox 2: Algoritmia Big-O y Red
            GroupBox gbBigO = new GroupBox
            {
                Text = " COMPLEJIDAD BIG-O Y TELEMETRÍA DE RED ",
                ForeColor = Color.FromArgb(100, 220, 255),
                Font = fTitulo,
                Location = new Point(16, 134),
                Size = new Size(508, 95)
            };

            lblBigO = new Label
            {
                Text = "Grilla Hash O(N+K): Eficiencia y comparaciones",
                ForeColor = Color.FromArgb(200, 230, 255),
                Font = fMono,
                Location = new Point(14, 25),
                Size = new Size(480, 20)
            };
            gbBigO.Controls.Add(lblBigO);

            lblRed = new Label
            {
                Text = "Red UDP: Modo | Ping RTT | Paquetes",
                ForeColor = Color.FromArgb(170, 240, 190),
                Font = fMono,
                Location = new Point(14, 55),
                Size = new Size(480, 20)
            };
            gbBigO.Controls.Add(lblRed);
            this.Controls.Add(gbBigO);

            // Vista previa del reporte
            txtReportePreview = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(20, 32, 48),
                ForeColor = Color.FromArgb(210, 235, 255),
                Font = new Font("Consolas", 8.2f, FontStyle.Regular),
                Location = new Point(16, 238),
                Size = new Size(508, 175),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtReportePreview);

            // Botones de Acción
            btnExportar = new Button
            {
                Text = "💾 EXPORTAR REPORTE (.TXT)",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(85, 70, 160),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(16, 424),
                Size = new Size(260, 38),
                Cursor = Cursors.Hand
            };
            btnExportar.Click += (s, e) =>
            {
                try
                {
                    string ruta = _profiler.ExportarReporteMetricas(_gestorRed, _naveJugador);
                    MessageBox.Show(
                        $"Reporte exportado exitosamente en:\n{ruta}",
                        "Reporte Guardado",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al exportar: {ex.Message}",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            };
            this.Controls.Add(btnExportar);

            btnCerrar = new Button
            {
                Text = "CERRAR",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(290, 424),
                Size = new Size(234, 38),
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);

            // Timer para refresco dinámico cada 200 ms
            _timerActualizacion = new System.Windows.Forms.Timer { Interval = 200 };
            _timerActualizacion.Tick += (s, e) => ActualizarDatosEnVivo();
            _timerActualizacion.Start();

            ActualizarDatosEnVivo();
        }

        private void ActualizarDatosEnVivo()
        {
            double ahorro = 0.0;
            if (_profiler.ComparacionesFuerzaBruta > 0)
            {
                ahorro = 100.0 * (1.0 - (double)_profiler.ComparacionesGrillaEspacial / _profiler.ComparacionesFuerzaBruta);
                if (ahorro < 0) ahorro = 0;
            }

            lblFps.Text = $"FPS: {_profiler.FpsActual:F1} | Cuadro Total: {_profiler.TiempoFrameTotalMs:F2} ms";
            lblCpu.Text = $"CPU Update: {_profiler.TiempoUpdateUs:F0} µs | CPU Render: {_profiler.TiempoRenderUs:F0} µs";
            lblMemoria.Text = $"Memoria RAM: {_profiler.MemoriaGestionadaMB:F2} MB (WS: {_profiler.MemoriaProcesoMB:F0} MB) | GC: {_profiler.RecoleccionesGen0}/{_profiler.RecoleccionesGen1}/{_profiler.RecoleccionesGen2}";
            lblBigO.Text = $"Grilla O(N+K): {_profiler.ComparacionesGrillaEspacial} comp. vs {_profiler.ComparacionesFuerzaBruta} en O(N*M) ({ahorro:F1}% CPU optimizado)";
            lblRed.Text = $"Red: {_gestorRed.ModoActual} | Latencia RTT: {_gestorRed.LatenciaRttMs:F2} ms | Paquetes TX: {_gestorRed.PaquetesEnviados} | RX: {_gestorRed.PaquetesRecibidos}";

            txtReportePreview.Text =
                $"=== CÓNDOR TÁCTICO v3.1 | TELEMETRÍA EN TIEMPO REAL ===\r\n" +
                $"Aeronave Jugador: {_naveJugador.NombreModelo} [{_naveJugador.Matricula}]\r\n" +
                $"Vida Actual: {_naveJugador.VidaActual} / {_naveJugador.VidaMaxima} HP | Escudo: {_naveJugador.EscudoActual}\r\n" +
                $"Puntaje: {_naveJugador.PuntajeTactico} pts | Bajas: {_naveJugador.BajasConfirmadas}\r\n" +
                $"Proyectiles Activos N: {_profiler.TotalProyectilesN} | Naves en Escena M: {_profiler.TotalAeronavesM}\r\n" +
                $"Optimización Big-O: {ahorro:F2}% ahorro de CPU en colisiones espaciales\r\n" +
                $"Streaming Asíncrono: {GestorRecursosAsincrono.Instancia.TotalRecursosEnCache} recursos en caché ({GestorRecursosAsincrono.Instancia.MemoriaTotalMB:F2} MB)\r\n" +
                $"Estado Red UDP: {_gestorRed.EstadoConexionTexto} (RTT: {_gestorRed.LatenciaRttMs:F2} ms)\r\n";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _timerActualizacion.Stop();
            _timerActualizacion.Dispose();
            base.OnFormClosing(e);
        }
    }
}
