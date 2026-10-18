// ==============================================================================
// PROYECTO: CÓNDOR TÁCTICO (SIMULADOR DE COMBATE AÉREO 2D)
// AUTORES:
//   - RONAL ABELIN LARICO CCASO
//   - CLEVER NANDOR MAMANI HUALLPA
//   - JHON YOSSEP HINOJOSA APAZA
//   - JHON ALEX HUARACALLO SUCARI
//   - HECTOR CARLOS TORRES FLORES
// INSTITUCION: UNIVERSIDAD NACIONAL DE JULIACA (UNAJ)
// VERSION: 4.0.0
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CondorTactico.Audio;

namespace CondorTactico.UI
{
    public enum PestanaCentroMando
    {
        Tienda = 0,
        Inventario = 1,
        Progresion = 2,
        EstadisticasLogros = 3,
        Configuracion = 4
    }

    /// <summary>
    /// Centro de Mando Unificado que gestiona Tienda, Inventario/Hangar, Progresión,
    /// Estadísticas/Logros y Configuración de Accesibilidad con persistencia inmediata.
    /// </summary>
    public sealed class VentanaCentroMando : Form
    {
        private PestanaCentroMando _pestanaActual;
        private CategoriaArticulo? _filtroCategoriaTienda = null;

        private readonly Label _lblSaldoMonedas = new Label();
        private readonly Label _lblRangoNivel = new Label();
        private readonly Panel _pnlContenido = new Panel();
        private readonly Button[] _botonesPestana = new Button[5];

        public VentanaCentroMando(PestanaCentroMando pestanaInicial = PestanaCentroMando.Tienda)
        {
            _pestanaActual = pestanaInicial;
            ConfigurarFormulario();
            ConstruirMarcoNavegacion();
            MostrarPestana(_pestanaActual);
        }

        private void ConfigurarFormulario()
        {
            Text = "🦅 CENTRO DE MANDO TÁCTICO — TIENDA, HANGAR, PROGRESIÓN Y AJUSTES | UNAJ";
            Size = new Size(980, 680);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(10, 16, 28);
            ForeColor = Color.White;
            DoubleBuffered = true;
        }

        private void ConstruirMarcoNavegacion()
        {
            // Cabecera superior con saldo e identidad
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 62,
                BackColor = Color.FromArgb(14, 24, 42)
            };

            var lblTitulo = new Label
            {
                Text = "🦅 CENTRO DE MANDO Y LOGÍSTICA AÉREA",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 215, 255),
                Location = new Point(18, 8),
                AutoSize = true
            };
            pnlTop.Controls.Add(lblTitulo);

            _lblRangoNivel.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            _lblRangoNivel.ForeColor = Color.FromArgb(180, 210, 240);
            _lblRangoNivel.Location = new Point(20, 34);
            _lblRangoNivel.AutoSize = true;
            pnlTop.Controls.Add(_lblRangoNivel);

            _lblSaldoMonedas.Font = new Font("Consolas", 13f, FontStyle.Bold);
            _lblSaldoMonedas.ForeColor = Color.FromArgb(255, 215, 60);
            _lblSaldoMonedas.Bounds = new Rectangle(640, 16, 310, 30);
            _lblSaldoMonedas.TextAlign = ContentAlignment.MiddleRight;
            pnlTop.Controls.Add(_lblSaldoMonedas);

            Controls.Add(pnlTop);

            // Barra de pestañas
            var pnlTabs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 46,
                BackColor = Color.FromArgb(18, 30, 50)
            };

            string[] nombresTabs =
            {
                "◈ TIENDA TÁCTICA",
                "✈ INVENTARIO Y HANGAR",
                "★ PROGRESIÓN Y NIVEL",
                "✦ ESTADÍSTICAS Y LOGROS",
                "⚙ CONFIGURACIÓN"
            };

            int tabW = 186;
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                var btn = new Button
                {
                    Text = nombresTabs[i],
                    Bounds = new Rectangle(14 + (i * (tabW + 6)), 6, tabW, 34),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 1;
                btn.Click += (s, e) => MostrarPestana((PestanaCentroMando)idx);
                _botonesPestana[i] = btn;
                pnlTabs.Controls.Add(btn);
            }

            Controls.Add(pnlTabs);

            // Pie de ventana
            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(14, 22, 36)
            };

            var btnRegresar = new Button
            {
                Text = "✔ GUARDAR Y VOLVER",
                Bounds = new Rectangle(730, 8, 220, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 145, 195),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.0f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRegresar.Click += (s, e) =>
            {
                GestorEstadoGlobal.GuardarTodo();
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlBottom.Controls.Add(btnRegresar);

            var lblNota = new Label
            {
                Text = "► Todos los desbloqueos, mejoras y preferencias se guardan automáticamente en disco.",
                Font = new Font("Segoe UI", 8.8f, FontStyle.Italic),
                ForeColor = Color.FromArgb(150, 175, 200),
                Location = new Point(18, 16),
                AutoSize = true
            };
            pnlBottom.Controls.Add(lblNota);

            Controls.Add(pnlBottom);

            // Contenedor dinámico central
            _pnlContenido.Dock = DockStyle.Fill;
            _pnlContenido.BackColor = Color.FromArgb(10, 16, 28);
            Controls.Add(_pnlContenido);
            _pnlContenido.BringToFront();

            ActualizarCabecera();
        }

        private void ActualizarCabecera()
        {
            PerfilJugador p = GestorEstadoGlobal.Perfil;
            _lblSaldoMonedas.Text = $"★ CRÉDITOS: {p.Monedas:N0}";
            _lblRangoNivel.Text = $"Piloto: {p.NombrePiloto}  |  Rango: {p.ObtenerNombreRangoMilitar()} (Nivel {p.CalcularNivelJugador()})  |  XP: {p.ExperienciaTotal:N0}";

            for (int i = 0; i < _botonesPestana.Length; i++)
            {
                bool activo = i == (int)_pestanaActual;
                _botonesPestana[i].BackColor = activo ? Color.FromArgb(0, 150, 210) : Color.FromArgb(24, 38, 62);
                _botonesPestana[i].ForeColor = activo ? Color.White : Color.FromArgb(180, 200, 225);
            }
        }

        public void MostrarPestana(PestanaCentroMando pestana)
        {
            _pestanaActual = pestana;
            ActualizarCabecera();

            _pnlContenido.SuspendLayout();
            _pnlContenido.Controls.Clear();

            switch (pestana)
            {
                case PestanaCentroMando.Tienda:
                    ConstruirVistaTienda();
                    break;
                case PestanaCentroMando.Inventario:
                    ConstruirVistaInventario();
                    break;
                case PestanaCentroMando.Progresion:
                    ConstruirVistaProgresion();
                    break;
                case PestanaCentroMando.EstadisticasLogros:
                    ConstruirVistaEstadisticasYLogros();
                    break;
                case PestanaCentroMando.Configuracion:
                    ConstruirVistaConfiguracion();
                    break;
            }

            _pnlContenido.ResumeLayout();
        }

        // =========================================================================
        // 1. VISTA DE TIENDA TÁCTICA
        // =========================================================================
        private void ConstruirVistaTienda()
        {
            var pnlFiltros = new Panel
            {
                Bounds = new Rectangle(16, 12, 930, 38),
                BackColor = Color.FromArgb(16, 26, 44)
            };

            (string Etiqueta, CategoriaArticulo? Cat)[] filtros =
            {
                ("TODOS LOS ARTÍCULOS", null),
                ("✈ AERONAVES", CategoriaArticulo.Aeronave),
                ("◈ APARIENCIAS (SKINS)", CategoriaArticulo.Apariencia),
                ("⚙ MEJORAS DE NAVE", CategoriaArticulo.MejoraPermanente),
                ("⚡ PODERES ESPECIALES", CategoriaArticulo.PoderEspecial)
            };

            for (int i = 0; i < filtros.Length; i++)
            {
                var f = filtros[i];
                bool sel = _filtroCategoriaTienda == f.Cat;
                var b = new Button
                {
                    Text = f.Etiqueta,
                    Bounds = new Rectangle(8 + i * 182, 4, 176, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = sel ? Color.FromArgb(220, 150, 20) : Color.FromArgb(28, 44, 70),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                b.Click += (s, e) =>
                {
                    _filtroCategoriaTienda = f.Cat;
                    MostrarPestana(PestanaCentroMando.Tienda);
                };
                pnlFiltros.Controls.Add(b);
            }
            _pnlContenido.Controls.Add(pnlFiltros);

            var panelTarjetas = new FlowLayoutPanel
            {
                Bounds = new Rectangle(16, 56, 932, 465),
                AutoScroll = true,
                BackColor = Color.FromArgb(12, 20, 34),
                Padding = new Padding(8)
            };

            IEnumerable<ArticuloCatalogo> lista = GestorEstadoGlobal.CatalogoTienda;
            if (_filtroCategoriaTienda.HasValue)
            {
                lista = lista.Where(a => a.Categoria == _filtroCategoriaTienda.Value);
            }

            PerfilJugador perfil = GestorEstadoGlobal.Perfil;

            foreach (ArticuloCatalogo art in lista)
            {
                var card = new Panel
                {
                    Size = new Size(440, 126),
                    BackColor = Color.FromArgb(20, 32, 52),
                    Margin = new Padding(6)
                };

                var barraColor = new Panel
                {
                    Bounds = new Rectangle(0, 0, 6, 126),
                    BackColor = art.ColorRepresentativo
                };
                card.Controls.Add(barraColor);

                var lblNom = new Label
                {
                    Text = art.Nombre,
                    Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                    ForeColor = art.ColorRepresentativo,
                    Location = new Point(16, 10),
                    AutoSize = true
                };
                card.Controls.Add(lblNom);

                var lblDesc = new Label
                {
                    Text = art.Descripcion,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(200, 215, 235),
                    Bounds = new Rectangle(16, 34, 410, 44)
                };
                card.Controls.Add(lblDesc);

                bool esMejora = art.Categoria == CategoriaArticulo.MejoraPermanente;
                int nivelActual = esMejora ? perfil.ObtenerNivelMejora(art.Id) : 0;
                bool yaComprado = !esMejora && perfil.ArticulosComprados.Contains(art.Id);
                bool alMaximo = esMejora && nivelActual >= art.NivelMaximo;
                int precio = GestorEstadoGlobal.CalcularPrecioActualArticulo(art);

                var lblEstado = new Label
                {
                    Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                    Location = new Point(16, 86),
                    AutoSize = true
                };

                var btnAccion = new Button
                {
                    Bounds = new Rectangle(275, 80, 152, 34),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };

                if (esMejora)
                {
                    string barras = new string('█', nivelActual) + new string('░', art.NivelMaximo - nivelActual);
                    lblEstado.Text = alMaximo
                        ? $"NIVEL MÁXIMO [{barras}]"
                        : $"Nivel {nivelActual}/{art.NivelMaximo} [{barras}] · 🪙 {precio}";
                    lblEstado.ForeColor = alMaximo ? Color.LimeGreen : Color.Gold;

                    btnAccion.Text = alMaximo ? "✔ MÁXIMO" : $"🔧 MEJORAR ({precio})";
                    btnAccion.Enabled = !alMaximo;
                    btnAccion.BackColor = alMaximo ? Color.FromArgb(40, 70, 50) : Color.FromArgb(0, 145, 110);
                    btnAccion.ForeColor = Color.White;
                }
                else
                {
                    lblEstado.Text = yaComprado ? "ADQUIRIDO EN HANGAR" : $"PRECIO: 🪙 {precio} CRÉDITOS";
                    lblEstado.ForeColor = yaComprado ? Color.LightGreen : Color.Gold;

                    btnAccion.Text = yaComprado ? "✔ ADQUIRIDO" : $"🛒 COMPRAR ({precio})";
                    btnAccion.Enabled = !yaComprado;
                    btnAccion.BackColor = yaComprado ? Color.FromArgb(45, 65, 85) : Color.FromArgb(200, 125, 15);
                    btnAccion.ForeColor = Color.White;
                }

                btnAccion.Click += (s, e) =>
                {
                    DialogResult conf = MessageBox.Show(
                        this,
                        $"¿Confirmas la adquisición de '{art.Nombre}' por {precio} Créditos Tácticos?",
                        "Confirmar Transacción Táctica",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (conf == DialogResult.Yes)
                    {
                        if (GestorEstadoGlobal.IntentarComprarArticulo(art.Id, out string msg))
                        {
                            SintetizadorAudioTactico.ReproducirAlertaRadar();
                            MostrarPestana(PestanaCentroMando.Tienda);
                            MessageBox.Show(this, msg, "Transacción Completada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(this, msg, "Operación Rechazada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                };

                card.Controls.Add(lblEstado);
                card.Controls.Add(btnAccion);
                panelTarjetas.Controls.Add(card);
            }

            _pnlContenido.Controls.Add(panelTarjetas);
        }

        // =========================================================================
        // 2. VISTA DE INVENTARIO Y HANGAR
        // =========================================================================
        private void ConstruirVistaInventario()
        {
            PerfilJugador p = GestorEstadoGlobal.Perfil;

            // Panel izquierdo: Vista previa gráfica de la aeronave equipada y sus sistemas
            var pnlPreview = new Panel
            {
                Bounds = new Rectangle(16, 14, 340, 505),
                BackColor = Color.FromArgb(16, 26, 44)
            };

            var picHangar = new PictureBox
            {
                Bounds = new Rectangle(16, 16, 308, 220),
                BackColor = Color.FromArgb(10, 18, 32)
            };
            picHangar.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color cSkin = GestorEstadoGlobal.ObtenerColorSkinEquipada();
                using var penGrid = new Pen(Color.FromArgb(30, cSkin), 1f);
                for (int x = 0; x < picHangar.Width; x += 32) g.DrawLine(penGrid, x, 0, x, picHangar.Height);
                for (int y = 0; y < picHangar.Height; y += 32) g.DrawLine(penGrid, 0, y, picHangar.Width, y);

                using var brGlow = new SolidBrush(Color.FromArgb(40, cSkin));
                g.FillEllipse(brGlow, 74, 30, 160, 160);

                CacheSpritesAeronaves.InicializarCache();
                Bitmap sprite = CacheSpritesAeronaves.ObtenerSpriteNave(
                    GestorEstadoGlobal.ObtenerModeloAeronaveEquipada(),
                    esRival: false,
                    acelerando: true,
                    0f);

                g.DrawImage(sprite, (picHangar.Width - sprite.Width) / 2, (picHangar.Height - sprite.Height) / 2);

                using var penSkin = new Pen(cSkin, 2f);
                g.DrawRectangle(penSkin, 2, 2, picHangar.Width - 5, picHangar.Height - 5);
            };
            pnlPreview.Controls.Add(picHangar);

            var naveEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.AeronaveEquipadaId);
            var skinEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.SkinEquipadaId);
            var poderEq = GestorEstadoGlobal.CatalogoTienda.FirstOrDefault(a => a.Id == p.PoderEquipadoId);

            int hpTotal = 30 + (p.ObtenerNivelMejora("MEJORA_BLINDAJE") * 6) + (p.AeronaveEquipadaId == "NAVE_CONDOR_X" ? 8 : 0);
            int escudoTotal = 12 + (p.ObtenerNivelMejora("MEJORA_ESCUDO") * 4);
            int bonoVel = p.ObtenerNivelMejora("MEJORA_MOTOR") * 8;
            int bonoCanon = p.ObtenerNivelMejora("MEJORA_CANON") * 15;

            var lblResumen = new Label
            {
                Bounds = new Rectangle(16, 248, 308, 245),
                Font = new Font("Consolas", 9.2f, FontStyle.Regular),
                ForeColor = Color.FromArgb(200, 230, 255),
                Text =
                    $"=== CONFIGURACIÓN ACTIVA ===\n\n" +
                    $"✈️ Aeronave : {naveEq?.Nombre}\n" +
                    $"🎨 Skin     : {skinEq?.Nombre}\n" +
                    $"⚡ Poder [Q]: {poderEq?.Nombre}\n\n" +
                    $"=== PARÁMETROS EN COMBATE ===\n" +
                    $"❤️ Integridad Casco : {hpTotal} HP\n" +
                    $"🛡️ Escudo Deflector : {escudoTotal} PTS\n" +
                    $"🚀 Impulso Motor    : +{bonoVel}%\n" +
                    $"🔥 Cadencia Balística: +{bonoCanon}%"
            };
            pnlPreview.Controls.Add(lblResumen);
            _pnlContenido.Controls.Add(pnlPreview);

            // Panel derecho: Lista de elementos desbloqueados para equipar
            var panelItems = new FlowLayoutPanel
            {
                Bounds = new Rectangle(368, 14, 580, 505),
                AutoScroll = true,
                BackColor = Color.FromArgb(14, 22, 38),
                Padding = new Padding(8)
            };

            var adquiridos = GestorEstadoGlobal.CatalogoTienda
                .Where(a => a.Categoria != CategoriaArticulo.MejoraPermanente && p.ArticulosComprados.Contains(a.Id));

            foreach (var art in adquiridos)
            {
                bool estaEquipado =
                    p.AeronaveEquipadaId == art.Id ||
                    p.SkinEquipadaId == art.Id ||
                    p.PoderEquipadoId == art.Id;

                var itemPanel = new Panel
                {
                    Size = new Size(540, 86),
                    BackColor = estaEquipado ? Color.FromArgb(22, 48, 72) : Color.FromArgb(20, 30, 48),
                    Margin = new Padding(5)
                };

                itemPanel.Controls.Add(new Label
                {
                    Text = $"[{art.Categoria.ToString().ToUpper()}] {art.Nombre}",
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = art.ColorRepresentativo,
                    Location = new Point(14, 10),
                    AutoSize = true
                });

                itemPanel.Controls.Add(new Label
                {
                    Text = art.Descripcion,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.Silver,
                    Bounds = new Rectangle(14, 34, 370, 42)
                });

                var btnEq = new Button
                {
                    Text = estaEquipado ? "★ EQUIPADO" : "EQUIPAR AHORA",
                    Bounds = new Rectangle(395, 24, 130, 36),
                    FlatStyle = FlatStyle.Flat,
                    Enabled = !estaEquipado,
                    BackColor = estaEquipado ? Color.FromArgb(30, 120, 80) : Color.FromArgb(0, 140, 200),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnEq.Click += (s, e) =>
                {
                    if (GestorEstadoGlobal.EquiparArticulo(art.Id, out _))
                    {
                        SintetizadorAudioTactico.ReproducirAlertaRadar();
                        MostrarPestana(PestanaCentroMando.Inventario);
                    }
                };
                itemPanel.Controls.Add(btnEq);
                panelItems.Controls.Add(itemPanel);
            }

            _pnlContenido.Controls.Add(panelItems);
        }

        // =========================================================================
        // 3. VISTA DE PROGRESIÓN, RANGO Y SUMINISTRO DIARIO
        // =========================================================================
        private void ConstruirVistaProgresion()
        {
            PerfilJugador p = GestorEstadoGlobal.Perfil;
            int nivel = p.CalcularNivelJugador();
            int xpBase = p.ObtenerXPInicioNivelActual();
            int xpMeta = p.ObtenerXPParaSiguienteNivel();
            int xpProgreso = Math.Max(0, p.ExperienciaTotal - xpBase);
            int xpRequerida = Math.Max(1, xpMeta - xpBase);

            var pnlRango = new Panel
            {
                Bounds = new Rectangle(18, 16, 926, 175),
                BackColor = Color.FromArgb(16, 28, 48)
            };

            pnlRango.Controls.Add(new Label
            {
                Text = $"🎖️ RANGO ACTUAL: {p.ObtenerNombreRangoMilitar().ToUpper()} (NIVEL {nivel})",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                Location = new Point(20, 16),
                AutoSize = true
            });

            pnlRango.Controls.Add(new Label
            {
                Text = $"Indicativo del Piloto:",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.Silver,
                Location = new Point(20, 56),
                AutoSize = true
            });

            var txtPiloto = new TextBox
            {
                Text = p.NombrePiloto,
                Bounds = new Rectangle(155, 53, 180, 26),
                Font = new Font("Consolas", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(10, 16, 28),
                ForeColor = Color.Cyan
            };
            pnlRango.Controls.Add(txtPiloto);

            var btnGuardarNombre = new Button
            {
                Text = "ACTUALIZAR INDICATIVO",
                Bounds = new Rectangle(345, 52, 165, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 130, 180),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardarNombre.Click += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(txtPiloto.Text))
                {
                    p.NombrePiloto = txtPiloto.Text.Trim().ToUpperInvariant();
                    GestorEstadoGlobal.GuardarTodo();
                    ActualizarCabecera();
                    MessageBox.Show(this, $"Indicativo actualizado a '{p.NombrePiloto}'.", "Perfil Actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            pnlRango.Controls.Add(btnGuardarNombre);

            // Barra de experiencia
            pnlRango.Controls.Add(new Label
            {
                Text = $"Progreso al Nivel {nivel + 1}: {xpProgreso:N0} / {xpRequerida:N0} XP (Total: {p.ExperienciaTotal:N0} XP)",
                Font = new Font("Consolas", 9.5f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 96),
                AutoSize = true
            });

            var barraXP = new ProgressBar
            {
                Bounds = new Rectangle(20, 122, 560, 26),
                Minimum = 0,
                Maximum = xpRequerida,
                Value = Math.Clamp(xpProgreso, 0, xpRequerida)
            };
            pnlRango.Controls.Add(barraXP);

            // Botón de recompensa diaria
            var btnDiaria = new Button
            {
                Text = $"🎁 RECLAMAR SUMINISTRO DIARIO\n(Racha actual: {p.RachaDiasRecompensa} días)",
                Bounds = new Rectangle(610, 88, 290, 64),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 130, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDiaria.Click += (s, e) =>
            {
                if (GestorEstadoGlobal.ReclamarRecompensaDiaria(out _, out _, out string msg))
                {
                    SintetizadorAudioTactico.ReproducirAlertaRadar();
                    MostrarPestana(PestanaCentroMando.Progresion);
                    MessageBox.Show(this, msg, "Suministro Diario Entregado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(this, msg, "Suministro Diario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            pnlRango.Controls.Add(btnDiaria);

            _pnlContenido.Controls.Add(pnlRango);

            // Escalafón Militar y Avance de Campaña
            var grpEscalafon = new GroupBox
            {
                Text = " Escalafón de Rangos y Avance de Operaciones (Modo Casual) ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(120, 220, 255),
                Bounds = new Rectangle(18, 204, 926, 315),
                BackColor = Color.FromArgb(14, 22, 38)
            };

            var lstProgreso = new ListBox
            {
                Bounds = new Rectangle(18, 28, 890, 268),
                Font = new Font("Consolas", 10f),
                BackColor = Color.FromArgb(8, 14, 24),
                ForeColor = Color.FromArgb(200, 235, 255)
            };

            lstProgreso.Items.Add("=== PROGRESO EN CAMPAÑA CASUAL (5 OPERACIONES) ===");
            foreach (var niv in GestorEstadoGlobal.NivelesCampana)
            {
                bool desbloq = niv.NumeroNivel <= p.NivelMaximoCampanaDesbloqueado;
                int est = p.EstrellasPorNivel.TryGetValue(niv.NumeroNivel, out int eVal) ? eVal : 0;
                int rec = p.MejorPuntajePorNivel.TryGetValue(niv.NumeroNivel, out int pVal) ? pVal : 0;
                string txtEst = new string('★', est) + new string('☆', 3 - est);
                string estado = desbloq ? $"[DESBLOQUEADO] Estrellas: {txtEst} | Récord: {rec:N0} pts" : "[BLOQUEADO 🔒]";
                lstProgreso.Items.Add($"  Nivel {niv.NumeroNivel} - {niv.CodigoOperacion,-32} -> {estado}");
            }

            lstProgreso.Items.Add("");
            lstProgreso.Items.Add("=== ESCALAFÓN DE RANGOS DE LA FUERZA AÉREA ===");
            string[] rangos =
            {
                "Nivel 1 (0 XP)      : Cadete de Vuelo",
                "Nivel 2 (120 XP)    : Alférez Táctico",
                "Nivel 3 (480 XP)    : Teniente de Escuadrilla",
                "Nivel 4 (1,080 XP)  : Capitán Interceptor",
                "Nivel 5 (1,920 XP)  : Mayor de Combate",
                "Nivel 6 (3,000 XP)  : Comandante Aéreo",
                "Nivel 7 (4,320 XP)  : Coronel de los Andes",
                "Nivel 8+ (5,880 XP) : Brigadier Estratosférico / Gran Mariscal Cóndor"
            };
            foreach (string r in rangos) lstProgreso.Items.Add("  " + r);

            grpEscalafon.Controls.Add(lstProgreso);
            _pnlContenido.Controls.Add(grpEscalafon);
        }

        // =========================================================================
        // 4. VISTA DE ESTADÍSTICAS Y LOGROS
        // =========================================================================
        private void ConstruirVistaEstadisticasYLogros()
        {
            PerfilJugador p = GestorEstadoGlobal.Perfil;
            double precision = p.DisparosTotales > 0 ? (p.ImpactosTotales * 100.0 / p.DisparosTotales) : 0.0;
            double winRate = p.PartidasJugadas > 0 ? (p.VictoriasTotales * 100.0 / p.PartidasJugadas) : 0.0;

            var grpStats = new GroupBox
            {
                Text = " 📊 Hoja de Servicio y Estadísticas ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                Bounds = new Rectangle(16, 14, 360, 505),
                BackColor = Color.FromArgb(14, 24, 42)
            };

            var lblStats = new Label
            {
                Bounds = new Rectangle(18, 32, 325, 450),
                Font = new Font("Consolas", 10f),
                ForeColor = Color.White,
                Text =
                    $"Piloto Activo     : {p.NombrePiloto}\n" +
                    $"Nivel / Rango     : Nv.{p.CalcularNivelJugador()} ({p.ObtenerNombreRangoMilitar()})\n\n" +
                    $"--- RENDIMIENTO EN COMBATE ---\n" +
                    $"Partidas Jugadas  : {p.PartidasJugadas}\n" +
                    $"Victorias Totales : {p.VictoriasTotales}\n" +
                    $"Derrotas Totales  : {p.DerrotasTotales}\n" +
                    $"Tasa de Victoria  : {winRate:F1}%\n\n" +
                    $"--- REGISTRO BALÍSTICO ---\n" +
                    $"Bajas Confirmadas : {p.BajasTotales}\n" +
                    $"Jefes Abatidos    : {p.JefesDerrotados}\n" +
                    $"Disparos Totales  : {p.DisparosTotales}\n" +
                    $"Impactos Directos : {p.ImpactosTotales}\n" +
                    $"Precisión Global  : {precision:F1}%\n" +
                    $"Combo Récord      : x{p.ComboMaximoHistorico}\n\n" +
                    $"--- ECONOMÍA Y RED ---\n" +
                    $"Créditos Actuales : {p.Monedas:N0}\n" +
                    $"Créditos Históricos: {p.MonedasAcumuladasHistorico:N0}\n" +
                    $"Partidas Online   : {p.PartidasMultijugadorJugadas}\n" +
                    $"Logros Obtenidos  : {p.LogrosDesbloqueados.Count} / {GestorEstadoGlobal.CatalogoLogros.Count}"
            };
            grpStats.Controls.Add(lblStats);
            _pnlContenido.Controls.Add(grpStats);

            // Panel de Logros
            var grpLogros = new GroupBox
            {
                Text = $" 🏆 Medallero de Logros y Desafíos ({p.LogrosDesbloqueados.Count}/{GestorEstadoGlobal.CatalogoLogros.Count}) ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.Gold,
                Bounds = new Rectangle(390, 14, 556, 505),
                BackColor = Color.FromArgb(14, 24, 42)
            };

            var flowLogros = new FlowLayoutPanel
            {
                Bounds = new Rectangle(12, 26, 532, 466),
                AutoScroll = true,
                BackColor = Color.FromArgb(10, 18, 32)
            };

            foreach (var logro in GestorEstadoGlobal.CatalogoLogros)
            {
                bool desbloqueado = p.LogrosDesbloqueados.Contains(logro.Id);
                var card = new Panel
                {
                    Size = new Size(500, 74),
                    BackColor = desbloqueado ? Color.FromArgb(20, 52, 44) : Color.FromArgb(20, 26, 38),
                    Margin = new Padding(4)
                };

                card.Controls.Add(new Label
                {
                    Text = (desbloqueado ? "✅ " : "🔒 ") + logro.Titulo,
                    Font = new Font("Segoe UI", 9.8f, FontStyle.Bold),
                    ForeColor = desbloqueado ? Color.FromArgb(100, 255, 160) : Color.Gray,
                    Location = new Point(12, 8),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = logro.Descripcion,
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = Color.FromArgb(200, 215, 230),
                    Location = new Point(12, 30),
                    AutoSize = true
                });

                card.Controls.Add(new Label
                {
                    Text = $"Recompensa: 🪙 +{logro.RecompensaMonedas} | +{logro.RecompensaXP} XP",
                    Font = new Font("Consolas", 8.5f, FontStyle.Bold),
                    ForeColor = Color.Gold,
                    Location = new Point(12, 50),
                    AutoSize = true
                });

                flowLogros.Controls.Add(card);
            }

            grpLogros.Controls.Add(flowLogros);
            _pnlContenido.Controls.Add(grpLogros);
        }

        // =========================================================================
        // 5. VISTA DE CONFIGURACIÓN Y ACCESIBILIDAD
        // =========================================================================
        private void ConstruirVistaConfiguracion()
        {
            ConfiguracionJuego cfg = GestorEstadoGlobal.Configuracion;
            Font fSub = new Font("Segoe UI", 10f, FontStyle.Bold);
            Font fNorm = new Font("Segoe UI", 9f);

            // Audio
            var grpAudio = new GroupBox
            {
                Text = " 🔊 Audio y Efectos Acústicos ",
                Font = fSub,
                ForeColor = Color.FromArgb(0, 215, 255),
                Bounds = new Rectangle(18, 14, 450, 225),
                BackColor = Color.FromArgb(16, 24, 40)
            };

            var lblVolGen = new Label { Text = $"Volumen General: {cfg.VolumenGeneral}%", Location = new Point(16, 28), AutoSize = true, Font = fNorm };
            var tbVolGen = new TrackBar { Bounds = new Rectangle(180, 22, 240, 32), Minimum = 0, Maximum = 100, Value = cfg.VolumenGeneral, TickFrequency = 10 };
            tbVolGen.Scroll += (s, e) => { cfg.VolumenGeneral = tbVolGen.Value; lblVolGen.Text = $"Volumen General: {cfg.VolumenGeneral}%"; };

            var lblVolMus = new Label { Text = $"Volumen Música: {cfg.VolumenMusica}%", Location = new Point(16, 72), AutoSize = true, Font = fNorm };
            var tbVolMus = new TrackBar { Bounds = new Rectangle(180, 66, 240, 32), Minimum = 0, Maximum = 100, Value = cfg.VolumenMusica, TickFrequency = 10 };
            tbVolMus.Scroll += (s, e) => { cfg.VolumenMusica = tbVolMus.Value; lblVolMus.Text = $"Volumen Música: {cfg.VolumenMusica}%"; };

            var lblVolSfx = new Label { Text = $"Efectos Tácticos: {cfg.VolumenEfectos}%", Location = new Point(16, 116), AutoSize = true, Font = fNorm };
            var tbVolSfx = new TrackBar { Bounds = new Rectangle(180, 110, 240, 32), Minimum = 0, Maximum = 100, Value = cfg.VolumenEfectos, TickFrequency = 10 };
            tbVolSfx.Scroll += (s, e) => { cfg.VolumenEfectos = tbVolSfx.Value; lblVolSfx.Text = $"Efectos Tácticos: {cfg.VolumenEfectos}%"; };

            var chkSilencio = new CheckBox
            {
                Text = "Silenciar todo el sintetizador de audio (Tecla M)",
                Location = new Point(18, 168),
                AutoSize = true,
                Font = fNorm,
                Checked = cfg.AudioSilenciado
            };
            chkSilencio.CheckedChanged += (s, e) =>
            {
                cfg.AudioSilenciado = chkSilencio.Checked;
                if (SintetizadorAudioTactico.AudioHabilitado == cfg.AudioSilenciado)
                {
                    SintetizadorAudioTactico.AlternarAudio();
                }
            };

            grpAudio.Controls.AddRange(new Control[] { lblVolGen, tbVolGen, lblVolMus, tbVolMus, lblVolSfx, tbVolSfx, chkSilencio });
            _pnlContenido.Controls.Add(grpAudio);

            // Gráficos y Pantalla
            var grpVideo = new GroupBox
            {
                Text = " 🖥️ Gráficos, Resolución y HUD ",
                Font = fSub,
                ForeColor = Color.FromArgb(100, 240, 160),
                Bounds = new Rectangle(486, 14, 456, 225),
                BackColor = Color.FromArgb(16, 24, 40)
            };

            grpVideo.Controls.Add(new Label { Text = "Resolución de Ventana:", Location = new Point(16, 32), AutoSize = true, Font = fNorm });
            var cmbRes = new ComboBox { Bounds = new Rectangle(190, 28, 240, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = fNorm };
            cmbRes.Items.AddRange(new object[] { "1020 x 680 (Estándar)", "1280 x 740 (HD Panorámico)", "1440 x 820 (Widescreen)", "1600 x 900 (Alta Definición)" });
            cmbRes.SelectedIndex = Math.Clamp(cfg.IndiceResolucion, 0, 3);
            cmbRes.SelectedIndexChanged += (s, e) => cfg.IndiceResolucion = cmbRes.SelectedIndex;
            grpVideo.Controls.Add(cmbRes);

            grpVideo.Controls.Add(new Label { Text = "Calidad de Renderizado:", Location = new Point(16, 72), AutoSize = true, Font = fNorm });
            var cmbCal = new ComboBox { Bounds = new Rectangle(190, 68, 240, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = fNorm };
            cmbCal.Items.AddRange(new object[] { "Rápida (Bajo consumo)", "Equilibrada (Bilinear)", "Ultra (4x SSAA Bicúbico)" });
            cmbCal.SelectedIndex = Math.Clamp(cfg.CalidadGrafica, 0, 2);
            cmbCal.SelectedIndexChanged += (s, e) => cfg.CalidadGrafica = cmbCal.SelectedIndex;
            grpVideo.Controls.Add(cmbCal);

            var chkFull = new CheckBox { Text = "Modo Pantalla Completa al combatir", Location = new Point(18, 112), AutoSize = true, Font = fNorm, Checked = cfg.PantallaCompleta };
            chkFull.CheckedChanged += (s, e) => cfg.PantallaCompleta = chkFull.Checked;

            var chkHud = new CheckBox { Text = "Mostrar HUD Militar Holográfico (Tecla H)", Location = new Point(18, 144), AutoSize = true, Font = fNorm, Checked = cfg.MostrarHudMilitar };
            chkHud.CheckedChanged += (s, e) => cfg.MostrarHudMilitar = chkHud.Checked;

            var chkTel = new CheckBox { Text = "Mostrar Telemetría y Profiling al iniciar (Tecla F3)", Location = new Point(18, 176), AutoSize = true, Font = fNorm, Checked = cfg.MostrarTelemetria };
            chkTel.CheckedChanged += (s, e) => cfg.MostrarTelemetria = chkTel.Checked;

            grpVideo.Controls.AddRange(new Control[] { chkFull, chkHud, chkTel });
            _pnlContenido.Controls.Add(grpVideo);

            // Controles y Sensibilidad
            var grpControles = new GroupBox
            {
                Text = " 🎮 Controles, Teclado y Ratón ",
                Font = fSub,
                ForeColor = Color.FromArgb(255, 200, 80),
                Bounds = new Rectangle(18, 252, 450, 210),
                BackColor = Color.FromArgb(16, 24, 40)
            };

            grpControles.Controls.Add(new Label { Text = "Esquema de Entrada:", Location = new Point(16, 32), AutoSize = true, Font = fNorm });
            var cmbCtrl = new ComboBox { Bounds = new Rectangle(180, 28, 245, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = fNorm };
            cmbCtrl.Items.AddRange(new object[]
            {
                "Híbrido (WASD + Flechas + Teclado)",
                "Táctico Izquierdo (Solo WASD)",
                "Clásico Derecho (Solo Flechas)",
                "Guiado por Ratón + Clic Disparo"
            });
            cmbCtrl.SelectedIndex = Math.Clamp(cfg.EsquemaControl, 0, 3);
            cmbCtrl.SelectedIndexChanged += (s, e) => cfg.EsquemaControl = cmbCtrl.SelectedIndex;
            grpControles.Controls.Add(cmbCtrl);

            var lblSens = new Label { Text = $"Sensibilidad de Vuelo: {cfg.SensibilidadVuelo}%", Location = new Point(16, 78), AutoSize = true, Font = fNorm };
            var tbSens = new TrackBar { Bounds = new Rectangle(195, 70, 230, 32), Minimum = 50, Maximum = 150, Value = Math.Clamp(cfg.SensibilidadVuelo, 50, 150), TickFrequency = 10 };
            tbSens.Scroll += (s, e) => { cfg.SensibilidadVuelo = tbSens.Value; lblSens.Text = $"Sensibilidad de Vuelo: {cfg.SensibilidadVuelo}%"; };
            grpControles.Controls.AddRange(new Control[] { lblSens, tbSens });

            grpControles.Controls.Add(new Label
            {
                Text = "Atajos activos: Disparo [Espacio/Enter/Clic] · Poder Especial [Q/E]\nCambiar Nave [1/2] · Pausa [P/Esc] · HUD [H] · Audio [M]",
                Location = new Point(16, 136),
                Size = new Size(415, 52),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.Silver
            });
            _pnlContenido.Controls.Add(grpControles);

            // Accesibilidad e Idioma
            var grpAcc = new GroupBox
            {
                Text = " ♿ Accesibilidad, Cámara e Idioma ",
                Font = fSub,
                ForeColor = Color.FromArgb(220, 140, 255),
                Bounds = new Rectangle(486, 252, 456, 210),
                BackColor = Color.FromArgb(16, 24, 40)
            };

            grpAcc.Controls.Add(new Label { Text = "Idioma de Interfaz:", Location = new Point(16, 32), AutoSize = true, Font = fNorm });
            var cmbIdioma = new ComboBox { Bounds = new Rectangle(190, 28, 240, 26), DropDownStyle = ComboBoxStyle.DropDownList, Font = fNorm };
            cmbIdioma.Items.AddRange(new object[] { "Español (Latinoamérica / Perú)", "English (International)" });
            cmbIdioma.SelectedIndex = cfg.Idioma == "EN" ? 1 : 0;
            cmbIdioma.SelectedIndexChanged += (s, e) => cfg.Idioma = cmbIdioma.SelectedIndex == 1 ? "EN" : "ES";
            grpAcc.Controls.Add(cmbIdioma);

            var lblShake = new Label { Text = $"Sacudida Cámara: {cfg.IntensidadSacudidaCamara}%", Location = new Point(16, 78), AutoSize = true, Font = fNorm };
            var tbShake = new TrackBar { Bounds = new Rectangle(190, 70, 240, 32), Minimum = 0, Maximum = 100, Value = Math.Clamp(cfg.IntensidadSacudidaCamara, 0, 100), TickFrequency = 10 };
            tbShake.Scroll += (s, e) => { cfg.IntensidadSacudidaCamara = tbShake.Value; lblShake.Text = $"Sacudida Cámara: {cfg.IntensidadSacudidaCamara}%"; };
            grpAcc.Controls.AddRange(new Control[] { lblShake, tbShake });

            var chkSub = new CheckBox { Text = "Activar Subtítulos y Alertas Visuales de Amenaza", Location = new Point(18, 124), AutoSize = true, Font = fNorm, Checked = cfg.SubtitulosAlertasVisuales };
            chkSub.CheckedChanged += (s, e) => cfg.SubtitulosAlertasVisuales = chkSub.Checked;

            var chkContraste = new CheckBox { Text = "Modo de Alto Contraste para Retículas y Proyectiles", Location = new Point(18, 156), AutoSize = true, Font = fNorm, Checked = cfg.ModoAltoContraste };
            chkContraste.CheckedChanged += (s, e) => cfg.ModoAltoContraste = chkContraste.Checked;

            grpAcc.Controls.AddRange(new Control[] { chkSub, chkContraste });
            _pnlContenido.Controls.Add(grpAcc);

            // Botón Restaurar valores predeterminados
            var btnRestaurar = new Button
            {
                Text = "🔄 RESTAURAR VALORES PREDETERMINADOS DE FÁBRICA",
                Bounds = new Rectangle(18, 474, 450, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(130, 50, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnRestaurar.Click += (s, e) =>
            {
                cfg.RestaurarValoresPredeterminados();
                GestorEstadoGlobal.GuardarTodo();
                MostrarPestana(PestanaCentroMando.Configuracion);
                MessageBox.Show(this, "Se han restaurado todas las configuraciones a sus valores originales.", "Configuración Restaurada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            _pnlContenido.Controls.Add(btnRestaurar);
        }
    }
}
