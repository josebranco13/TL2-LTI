using KubernetesController.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KubernetesController
{
    public partial class Form1 : Form
    {
        private KubernetesApiClient api;

        public Form1()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }



        private string BuildK3sBaseUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Insere o IP ou URL do master K3s.");

            input = input.Trim();

            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = "http://" + input;
            }

            UriBuilder uriBuilder = new UriBuilder(input);

            if (uriBuilder.Port == -1 || uriBuilder.Port == 80)
            {
                uriBuilder.Port = 8001;
            }

            return uriBuilder.Uri.ToString().TrimEnd('/');
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;
                Cursor = Cursors.WaitCursor;

                string baseUrl = BuildK3sBaseUrl(txtBaseUrl.Text);

                KubernetesApiClient api = new KubernetesApiClient(baseUrl);

                // Testa ligação ao master K3s
                await api.GetAsync("/version");

                MessageBox.Show("Ligação ao master K3s realizada com sucesso.");

                AppNavigator.NavigateTo(new DashboardForm(baseUrl));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Não foi possível ligar ao master K3s.\n\n" + ex.Message,
                    "Erro de ligação",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnConnect.Enabled = true;
                Cursor = Cursors.Default;
            }
        }
    }
}
