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
    /// Subventana táctica desacoplada para la configuración de red y multijugador UDP.
    /// </summary>
    public sealed class VentanaConfiguracionRed : Form
    {
        private readonly GestorRedClienteServidor _gestorRed;
        private readonly Action<ModoRedJuego, string, int> _onConfiguracionAplicada;

        private RadioButton rbHost = null!;
        private RadioButton rbCliente = null!;
        private RadioButton rbLocal = null!;
        private TextBox txtIp = null!;
        private TextBox txtPuerto = null!;
        private Label lblEstadoRed = null!;
        private Button btnAplicar = null!;
        private Button btnCerrar = null!;

        public VentanaConfiguracionRed(
            GestorRedClienteServidor gestorRed,
            Action<ModoRedJuego, string, int> onConfiguracionAplicada,
            ModoRedJuego modoActual,
            string ipActual,
            int puertoActual)
        {
            _gestorRed = gestorRed ?? throw new ArgumentNullException(nameof(gestorRed));
            _onConfiguracionAplicada = onConfiguracionAplicada ?? throw new ArgumentNullException(nameof(onConfiguracionAplicada));

            InicializarComponentes(modoActual, ipActual, puertoActual);
        }

        private void InicializarComponentes(ModoRedJuego modoActual, string ipActual, int puertoActual)
        {
            this.Text = "CÓNDOR TÁCTICO | ENLACE TÁCTICO Y CONFIGURACIÓN DE RED";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(460, 340);
            this.BackColor = Color.FromArgb(16, 26, 42);
            this.ForeColor = Color.FromArgb(220, 235, 250);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            Font fTitulo = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            Font fMono = new Font("Consolas", 9.5f, FontStyle.Regular);

            // GroupBox: Modo de Operación
            GroupBox gbModo = new GroupBox
            {
                Text = " MODALIDAD DE COMUNICACIÓN UDP ",
                ForeColor = Color.FromArgb(100, 210, 255),
                Font = fTitulo,
                Location = new Point(16, 14),
                Size = new Size(428, 115)
            };

            rbHost = new RadioButton
            {
                Text = "🛰️ SERVIDOR HOST (Crea sala de vuelo y escucha paquetes UDP)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(16, 26),
                Size = new Size(396, 24),
                Checked = modoActual == ModoRedJuego.Servidor_Host_Online
            };

            rbCliente = new RadioButton
            {
                Text = "📡 CLIENTE REMOTO (Conectar a sala aérea por IP y puerto)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(16, 54),
                Size = new Size(396, 24),
                Checked = modoActual == ModoRedJuego.Cliente_Remoto_Online
            };

            rbLocal = new RadioButton
            {
                Text = "⚡ MODO LOCAL AISLADO (Combate simulado contra IA)",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(16, 82),
                Size = new Size(396, 24),
                Checked = modoActual == ModoRedJuego.Local_Combate_IA
            };

            gbModo.Controls.Add(rbHost);
            gbModo.Controls.Add(rbCliente);
            gbModo.Controls.Add(rbLocal);
            this.Controls.Add(gbModo);

            // GroupBox: Parámetros del Socket
            GroupBox gbParam = new GroupBox
            {
                Text = " PARÁMETROS DEL SOCKET DE RED ",
                ForeColor = Color.FromArgb(100, 210, 255),
                Font = fTitulo,
                Location = new Point(16, 138),
                Size = new Size(428, 105)
            };

            Label lblIp = new Label
            {
                Text = "Dirección IP:",
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(16, 28),
                AutoSize = true
            };
            gbParam.Controls.Add(lblIp);

            txtIp = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(ipActual) ? "127.0.0.1" : ipActual,
                Font = fMono,
                Location = new Point(110, 25),
                Size = new Size(130, 24),
                BackColor = Color.FromArgb(28, 42, 64),
                ForeColor = Color.Cyan,
                BorderStyle = BorderStyle.FixedSingle
            };
            gbParam.Controls.Add(txtIp);

            Label lblPuerto = new Label
            {
                Text = "Puerto UDP:",
                ForeColor = Color.FromArgb(200, 220, 240),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Location = new Point(255, 28),
                AutoSize = true
            };
            gbParam.Controls.Add(lblPuerto);

            txtPuerto = new TextBox
            {
                Text = puertoActual > 0 ? puertoActual.ToString() : "15050",
                Font = fMono,
                Location = new Point(340, 25),
                Size = new Size(72, 24),
                BackColor = Color.FromArgb(28, 42, 64),
                ForeColor = Color.LightGreen,
                BorderStyle = BorderStyle.FixedSingle
            };
            gbParam.Controls.Add(txtPuerto);

            lblEstadoRed = new Label
            {
                Text = $"Estado actual: {_gestorRed.ModoActual} | Socket: {(_gestorRed.ModoActual != ModoRedJuego.Local_Combate_IA ? "Activo" : "Inactivo")}",
                ForeColor = Color.FromArgb(170, 230, 180),
                Font = new Font("Consolas", 8.2f, FontStyle.Regular),
                Location = new Point(16, 64),
                Size = new Size(396, 28)
            };
            gbParam.Controls.Add(lblEstadoRed);
            this.Controls.Add(gbParam);

            // Botones de acción
            btnAplicar = new Button
            {
                Text = "⚡ CONECTAR / APLICAR",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 150, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(16, 260),
                Size = new Size(240, 36),
                Cursor = Cursors.Hand
            };
            btnAplicar.Click += BtnAplicar_Click;
            this.Controls.Add(btnAplicar);

            btnCerrar = new Button
            {
                Text = "CERRAR",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 60, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(270, 260),
                Size = new Size(174, 36),
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }

        private void BtnAplicar_Click(object? sender, EventArgs e)
        {
            ModoRedJuego modo = ModoRedJuego.Local_Combate_IA;
            if (rbHost.Checked) modo = ModoRedJuego.Servidor_Host_Online;
            else if (rbCliente.Checked) modo = ModoRedJuego.Cliente_Remoto_Online;

            string ip = txtIp.Text.Trim();
            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

            if (!int.TryParse(txtPuerto.Text.Trim(), out int puerto) || puerto < 1024 || puerto > 65535)
            {
                MessageBox.Show(
                    "El puerto debe ser un número entero válido entre 1024 y 65535.",
                    "Puerto Inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                _onConfiguracionAplicada(modo, ip, puerto);
                lblEstadoRed.Text = $"Estado actual: {modo} ({ip}:{puerto}) - Configurado correctamente.";
                MessageBox.Show(
                    $"Configuración de red actualizada exitosamente:\nModo: {modo}\nEndpoint: {ip}:{puerto}",
                    "Enlace de Red Establecido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar socket de red: {ex.Message}",
                    "Falla de Comunicación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
