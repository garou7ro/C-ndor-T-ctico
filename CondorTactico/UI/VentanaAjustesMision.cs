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
    /// Subventana desacoplada para configurar los parámetros tácticos de la misión aérea y composición de escuadrones.
    /// </summary>
    public sealed class VentanaAjustesMision : Form
    {
        private readonly Action<string, TipoAeronave, TipoAeronave, bool> _onDesplegarMision;

        private TextBox txtPiloto = null!;
        private ComboBox cmbModeloJugador = null!;
        private ComboBox cmbModeloRival = null!;
        private CheckBox chkEscolta = null!;
        private Button btnLanzar = null!;
        private Button btnCancelar = null!;

        public VentanaAjustesMision(
            string pilotoActual,
            TipoAeronave modeloJugadorActual,
            TipoAeronave modeloRivalActual,
            bool escoltaActual,
            Action<string, TipoAeronave, TipoAeronave, bool> onDesplegarMision)
        {
            _onDesplegarMision = onDesplegarMision ?? throw new ArgumentNullException(nameof(onDesplegarMision));

            InicializarComponentes(pilotoActual, modeloJugadorActual, modeloRivalActual, escoltaActual);
        }

        private void InicializarComponentes(
            string pilotoActual,
            TipoAeronave modeloJugadorActual,
            TipoAeronave modeloRivalActual,
            bool escoltaActual)
        {
            this.Text = "CÓNDOR TÁCTICO | PLANIFICACIÓN DE MISIÓN Y ESCUADRONES";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(480, 360);
            this.BackColor = Color.FromArgb(16, 26, 42);
            this.ForeColor = Color.FromArgb(220, 235, 250);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            Font fTitulo = new Font("Segoe UI", 10f, FontStyle.Bold);
            Font fMono = new Font("Consolas", 9.5f, FontStyle.Regular);

            // GroupBox 1: Configuración del Piloto Aliado
            GroupBox gbPiloto = new GroupBox
            {
                Text = " FUERZA AÉREA ALIADA (JUGADOR) ",
                ForeColor = Color.FromArgb(100, 210, 255),
                Font = fTitulo,
                Location = new Point(16, 14),
                Size = new Size(448, 115)
            };

            Label lblPiloto = new Label
            {
                Text = "Indicativo Piloto:",
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(16, 32),
                AutoSize = true
            };
            gbPiloto.Controls.Add(lblPiloto);

            txtPiloto = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(pilotoActual) ? "CÓNDOR-01" : pilotoActual,
                Font = fMono,
                Location = new Point(140, 28),
                Size = new Size(160, 24),
                BackColor = Color.FromArgb(28, 42, 64),
                ForeColor = Color.Cyan,
                BorderStyle = BorderStyle.FixedSingle
            };
            gbPiloto.Controls.Add(txtPiloto);

            Label lblNave = new Label
            {
                Text = "Aeronave Activa:",
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(16, 72),
                AutoSize = true
            };
            gbPiloto.Controls.Add(lblNave);

            cmbModeloJugador = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = fMono,
                Location = new Point(140, 68),
                Size = new Size(280, 24)
            };
            cmbModeloJugador.Items.AddRange(new object[]
            {
                "Northrop Grumman B-2 Spirit (AF 82-1071)",
                "Lockheed Martin F-35A Lightning II"
            });
            cmbModeloJugador.SelectedIndex = modeloJugadorActual == TipoAeronave.B2_Spirit_Stealth ? 0 : 1;
            gbPiloto.Controls.Add(cmbModeloJugador);

            this.Controls.Add(gbPiloto);

            // GroupBox 2: Fuerza Opositora
            GroupBox gbRival = new GroupBox
            {
                Text = " FUERZA OPOSITORA (AGRESORES) ",
                ForeColor = Color.FromArgb(255, 120, 120),
                Font = fTitulo,
                Location = new Point(16, 138),
                Size = new Size(448, 125)
            };

            Label lblRival = new Label
            {
                Text = "Líder Rival:",
                ForeColor = Color.FromArgb(255, 200, 200),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(16, 32),
                AutoSize = true
            };
            gbRival.Controls.Add(lblRival);

            cmbModeloRival = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = fMono,
                Location = new Point(140, 28),
                Size = new Size(280, 24)
            };
            cmbModeloRival.Items.AddRange(new object[]
            {
                "Escuadrón Mixto (B-2 Spirit + F-35A)",
                "Líder B-2 Spirit Rival Furtivo",
                "Líder F-35A Lightning II Rival"
            });
            cmbModeloRival.SelectedIndex = 0;
            gbRival.Controls.Add(cmbModeloRival);

            chkEscolta = new CheckBox
            {
                Text = "Desplegar escolta de ala táctica adicional en formación",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(16, 75),
                Size = new Size(400, 24),
                Checked = escoltaActual
            };
            gbRival.Controls.Add(chkEscolta);

            this.Controls.Add(gbRival);

            // Botones de Acción
            btnLanzar = new Button
            {
                Text = "🚀 DESPLEGAR MISIÓN TÁCTICA",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 160, 210),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(16, 280),
                Size = new Size(260, 38),
                Cursor = Cursors.Hand
            };
            btnLanzar.Click += (s, e) =>
            {
                string piloto = string.IsNullOrWhiteSpace(txtPiloto.Text) ? "CÓNDOR-01" : txtPiloto.Text.Trim();
                TipoAeronave tJug = cmbModeloJugador.SelectedIndex == 0 ? TipoAeronave.B2_Spirit_Stealth : TipoAeronave.F35_Lightning_II;
                TipoAeronave tRiv = cmbModeloRival.SelectedIndex == 1 ? TipoAeronave.B2_Spirit_Stealth : TipoAeronave.F35_Lightning_II;
                bool mixto = cmbModeloRival.SelectedIndex == 0 || chkEscolta.Checked;

                _onDesplegarMision(piloto, tJug, tRiv, mixto);
                this.Close();
            };
            this.Controls.Add(btnLanzar);

            btnCancelar = new Button
            {
                Text = "CANCELAR",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(290, 280),
                Size = new Size(174, 38),
                Cursor = Cursors.Hand
            };
            btnCancelar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancelar);
        }
    }
}
