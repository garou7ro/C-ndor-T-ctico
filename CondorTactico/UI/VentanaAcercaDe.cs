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

using System.Drawing;
using System.Windows.Forms;

namespace CondorTactico.UI
{
    /// <summary>
    /// Subventana informativa de créditos académicos, autores de la UNAJ y guía de mandos tácticos.
    /// </summary>
    public sealed class VentanaAcercaDe : Form
    {
        public VentanaAcercaDe()
        {
            InicializarComponentes();
        }

        private void InicializarComponentes()
        {
            this.Text = "CÓNDOR TÁCTICO | ACERCA DEL PROYECTO Y CRÉDITOS UNAJ";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(500, 430);
            this.BackColor = Color.FromArgb(14, 22, 35);
            this.ForeColor = Color.FromArgb(220, 235, 250);
            this.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            Font fTitulo = new Font("Segoe UI", 12f, FontStyle.Bold);
            Font fSub = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Font fMono = new Font("Consolas", 8.8f, FontStyle.Regular);

            Label lblTitulo = new Label
            {
                Text = "🦅 CÓNDOR TÁCTICO v3.1",
                ForeColor = Color.FromArgb(80, 230, 255),
                Font = fTitulo,
                Location = new Point(20, 16),
                AutoSize = true
            };
            this.Controls.Add(lblTitulo);

            Label lblSubtitulo = new Label
            {
                Text = "Simulador de Combate Aéreo 2D en Tiempo Real\nUniversidad Nacional de Juliaca (UNAJ)",
                ForeColor = Color.FromArgb(170, 210, 245),
                Font = fSub,
                Location = new Point(20, 46),
                AutoSize = true
            };
            this.Controls.Add(lblSubtitulo);

            GroupBox gbAutores = new GroupBox
            {
                Text = " EQUIPO DE INGENIERÍA Y AUTORES (UNAJ) ",
                ForeColor = Color.FromArgb(100, 220, 255),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(20, 95),
                Size = new Size(460, 140)
            };

            string autores =
                " • RONAL ABELIN LARICO CCASO\r\n" +
                " • CLEVER NANDOR MAMANI HUALLPA\r\n" +
                " • JHON YOSSEP HINOJOSA APAZA\r\n" +
                " • JHON ALEX HUARACALLO SUCARI\r\n" +
                " • HECTOR CARLOS TORRES FLORES\r\n\r\n" +
                " Escuela Profesional de Ingeniería de Sistemas - UNAJ (2026)";

            Label lblListaAutores = new Label
            {
                Text = autores,
                ForeColor = Color.FromArgb(235, 245, 255),
                Font = fMono,
                Location = new Point(14, 22),
                Size = new Size(430, 110)
            };
            gbAutores.Controls.Add(lblListaAutores);
            this.Controls.Add(gbAutores);

            GroupBox gbControles = new GroupBox
            {
                Text = " GUÍA DE MANDOS Y VUELO TÁCTICO ",
                ForeColor = Color.FromArgb(100, 255, 180),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(20, 245),
                Size = new Size(460, 125)
            };

            string controles =
                " [Flechas / WASD]  : Vuelo y maniobras multidireccionales\r\n" +
                " [Espacio / Enter] : Disparo táctico (Doble Salva B-2 / Misil F-35)\r\n" +
                " [1] / [2]         : Alternar B-2 Spirit AF 82-1071 / F-35A\r\n" +
                " [F3]              : Activar / Desactivar HUD de Telemetría\r\n" +
                " [F4]              : Exportar Reporte de Métricas a archivo\r\n" +
                " [R]               : Reiniciar combate tras victoria o derrota";

            Label lblListaControles = new Label
            {
                Text = controles,
                ForeColor = Color.FromArgb(200, 255, 220),
                Font = fMono,
                Location = new Point(14, 22),
                Size = new Size(430, 95)
            };
            gbControles.Controls.Add(lblListaControles);
            this.Controls.Add(gbControles);

            Button btnCerrar = new Button
            {
                Text = "ENTENDIDO",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 140, 190),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(170, 380),
                Size = new Size(160, 36),
                Cursor = Cursors.Hand
            };
            btnCerrar.Click += (s, e) => this.Close();
            this.Controls.Add(btnCerrar);
        }
    }
}
