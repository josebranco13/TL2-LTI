using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KubernetesController.Services;

namespace KubernetesController
{
    public partial class DashboardForm : Form
    {
        private readonly string baseUrl;
        private readonly string token;
        private readonly KubernetesApiClient api;

        public DashboardForm(string baseUrl)
        {
            InitializeComponent();

            this.baseUrl = baseUrl;
            this.token = token;
            this.api = new KubernetesApiClient(baseUrl, token);

            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Opcional: mostrar o URL ligado numa label
                //lblApiUrl.Text = $"Ligado a: {baseUrl}";

                // Teste inicial ao cluster
                string version = await api.GetAsync("/version");

                //txtOutput.Text = version;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar dados do cluster.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}
