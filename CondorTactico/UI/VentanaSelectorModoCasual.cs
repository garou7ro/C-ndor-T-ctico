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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CondorTactico.Audio;

namespace CondorTactico.UI
{
    /// <summary>
    /// Selector de Operaciones del Modo Casual (Campaña de 5 Niveles con Jefes y Peligros Ambientales),
    /// Batalla Libre y Tutorial Interactivo.
    /// </summary>
    public sealed class VentanaSelectorModoCasual : Form
    {
        public ModalidadPartida ModalidadElegida { get; private set; } = ModalidadPartida.CampanaCasual;
        public int NivelElegido { get; private set; } = 1;

        private readonly Panel _pnlDetalleNivel = new Panel();
        private readonly Button[] _botonesNivel = new Button[5];

        public VentanaSelectorModoCasual()
        {
            NivelElegido = Math.Clamp(GestorEstadoGlobal.Perfil.NivelMaximoCampanaDesbloqueado, 1, 5);
            ConfigurarFormulario();
            ConstruirInterfaz();
            SeleccionarNivel(NivelElegido);
        }

        private void ConfigurarFormulario()
        {
            Text = "🏔️ MODO CASUAL — CAMPAÑA AÉREA PROGRESIVA Y MODALIDADES | CÓNDOR TÁCTICO";
            Size = new Size(950, 640);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(10, 16, 28);
            ForeColor = Color.White;
            DoubleBuffered = true;
        }

        private void ConstruirInterfaz()
        {
            var pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 58,
                BackColor = Color.FromArgb(14, 24, 42)
            };

            pnlHeader.Controls.Add(new Label
            {
                Text = "🏔️ MAPA TÁCTICO DE OPERACIONES — MODO CASUAL Y ENTRENAMIENTO",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 220, 255),
                Location = new Point(18, 15),
                AutoSize = true
            });

            var btnContinuar = new Button
            {
                Text = $"▶ CONTINUAR PROGRESO (NIVEL {GestorEstadoGlobal.Perfil.NivelMaximoCampanaDesbloqueado})",
                Bounds = new Rectangle(640, 11, 280, 36),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 155, 95),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnContinuar.Click += (s, e) =>
            {
                ModalidadElegida = ModalidadPartida.CampanaCasual;
                NivelElegido = GestorEstadoGlobal.Perfil.NivelMaximoCampanaDesbloqueado;
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlHeader.Controls.Add(btnContinuar);
            Controls.Add(pnlHeader);

            // Columna izquierda: Los 5 niveles de la campaña
            var grpNiveles = new GroupBox
            {
                Text = " Selecciona una Operación de la Campaña ",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 215, 255),
                Bounds = new Rectangle(18, 70, 370, 440),
                BackColor = Color.FromArgb(14, 22, 36)
            };

            PerfilJugador p = GestorEstadoGlobal.Perfil;
            for (int i = 0; i < GestorEstadoGlobal.NivelesCampana.Count; i++)
            {
                var def = GestorEstadoGlobal.NivelesCampana[i];
                bool desbloqueado = def.NumeroNivel <= p.NivelMaximoCampanaDesbloqueado;
                int estrellas = p.EstrellasPorNivel.TryGetValue(def.NumeroNivel, out int eVal) ? eVal : 0;
                string txtEst = new string('★', estrellas) + new string('☆', 3 - estrellas);

                var btn = new Button
                {
                    Text = desbloqueado
                        ? $"{def.CodigoOperacion}\nEstrellas: {txtEst}  |  Jefe: {def.ModeloJefe}"
                        : $"🔒 {def.CodigoOperacion}\n(Completa el Nivel {def.NumeroNivel - 1} para desbloquear)",
                    Bounds = new Rectangle(16, 30 + (i * 78), 338, 68),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 8.8f, FontStyle.Bold),
                    Cursor = desbloqueado ? Cursors.Hand : Cursors.No
                };

                int numNiv = def.NumeroNivel;
                btn.Click += (s, e) =>
                {
                    if (!desbloqueado)
                    {
                        MessageBox.Show(this, $"Debes superar primero la Operación {numNiv - 1} para desbloquear este sector aéreo.", "Sector Bloqueado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    SintetizadorAudioTactico.ReproducirAlertaRadar();
                    SeleccionarNivel(numNiv);
                };

                _botonesNivel[i] = btn;
                grpNiveles.Controls.Add(btn);
            }
            Controls.Add(grpNiveles);

            // Columna derecha: Briefing detallado del nivel seleccionado
            _pnlDetalleNivel.Bounds = new Rectangle(404, 76, 514, 434);
            _pnlDetalleNivel.BackColor = Color.FromArgb(16, 26, 44);
            Controls.Add(_pnlDetalleNivel);

            // Barra inferior: Otras modalidades (Tutorial, Batalla Libre, Volver)
            var btnTutorial = new Button
            {
                Text = "▶ INICIAR TUTORIAL GUIADO",
                Bounds = new Rectangle(18, 526, 275, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 125, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTutorial.Click += (s, e) =>
            {
                ModalidadElegida = ModalidadPartida.TutorialInteractivo;
                NivelElegido = 1;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnTutorial);

            var btnBatallaLibre = new Button
            {
                Text = "⚡ BATALLA LIBRE (ESCARAMUZA)",
                Bounds = new Rectangle(305, 526, 275, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(145, 95, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBatallaLibre.Click += (s, e) =>
            {
                ModalidadElegida = ModalidadPartida.BatallaLibre;
                NivelElegido = 1;
                DialogResult = DialogResult.OK;
                Close();
            };
            Controls.Add(btnBatallaLibre);

            var btnVolver = new Button
            {
                Text = "✖ VOLVER AL MENÚ PRINCIPAL",
                Bounds = new Rectangle(592, 526, 326, 48),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(75, 45, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.2f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnVolver.Click += (s, e) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
            Controls.Add(btnVolver);
        }

        private void SeleccionarNivel(int numeroNivel)
        {
            NivelElegido = Math.Clamp(numeroNivel, 1, GestorEstadoGlobal.NivelesCampana.Count);
            PerfilJugador p = GestorEstadoGlobal.Perfil;

            for (int i = 0; i < _botonesNivel.Length; i++)
            {
                bool desbloq = (i + 1) <= p.NivelMaximoCampanaDesbloqueado;
                bool activo = (i + 1) == NivelElegido;
                _botonesNivel[i].BackColor = activo
                    ? Color.FromArgb(0, 140, 205)
                    : (desbloq ? Color.FromArgb(24, 38, 62) : Color.FromArgb(18, 22, 30));
                _botonesNivel[i].ForeColor = desbloq ? Color.White : Color.Gray;
            }

            DefinicionNivelCampana def = GestorEstadoGlobal.NivelesCampana[NivelElegido - 1];
            _pnlDetalleNivel.SuspendLayout();
            _pnlDetalleNivel.Controls.Clear();

            // Banner pintado con el gradiente atmosférico propio del escenario
            var picEscenario = new PictureBox
            {
                Bounds = new Rectangle(16, 16, 482, 115)
            };
            picEscenario.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using var grad = new LinearGradientBrush(picEscenario.ClientRectangle, def.ColorCieloSuperior, def.ColorCieloInferior, LinearGradientMode.Vertical);
                g.FillRectangle(grad, picEscenario.ClientRectangle);

                using var fOp = new Font("Segoe UI", 13f, FontStyle.Bold);
                using var fEsc = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                g.DrawString(def.CodigoOperacion, fOp, Brushes.White, 14, 14);
                g.DrawString($"◈ Escenario: {def.NombreEscenario}", fEsc, Brushes.Cyan, 14, 46);
                g.DrawString($"▲ Peligro Ambiental: {def.PeligroAmbiental}", fEsc, Brushes.Gold, 14, 74);
            };
            _pnlDetalleNivel.Controls.Add(picEscenario);

            int recordPts = p.MejorPuntajePorNivel.TryGetValue(def.NumeroNivel, out int rVal) ? rVal : 0;
            int estrellas = p.EstrellasPorNivel.TryGetValue(def.NumeroNivel, out int eVal) ? eVal : 0;

            var lblInfo = new Label
            {
                Bounds = new Rectangle(18, 144, 478, 215),
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(220, 235, 250),
                Text =
                    $"■ DESCRIPCIÓN DE INTELIGENCIA:\n{def.DescripcionMision}\n\n" +
                    $"▶ OBJETIVO PRINCIPAL:\n   • {def.ObjetivoPrincipal}\n\n" +
                    $"★ OBJETIVO SECUNDARIO (BONIFICACIÓN):\n   • {def.ObjetivoSecundario}\n\n" +
                    $"❖ JEFE FINAL DE SECTOR:\n   • {def.NombreJefeFinal} ({def.VidaJefe} HP + Patrones Especiales)\n\n" +
                    $"◈ RECOMPENSA BASE: +{def.RecompensaMonedasBase} Créditos  |  +{def.RecompensaXPBase} XP\n" +
                    $"✦ TU RÉCORD ACTUAL: {recordPts:N0} pts ({new string('★', estrellas)}{new string('☆', 3 - estrellas)})"
            };
            _pnlDetalleNivel.Controls.Add(lblInfo);

            var btnDesplegarNivel = new Button
            {
                Text = $"▶ DESPLEGAR EN {def.CodigoOperacion}",
                Bounds = new Rectangle(16, 368, 482, 50),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(20, 165, 95),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDesplegarNivel.Click += (s, e) =>
            {
                ModalidadElegida = ModalidadPartida.CampanaCasual;
                DialogResult = DialogResult.OK;
                Close();
            };
            _pnlDetalleNivel.Controls.Add(btnDesplegarNivel);

            _pnlDetalleNivel.ResumeLayout();
        }
    }
}
