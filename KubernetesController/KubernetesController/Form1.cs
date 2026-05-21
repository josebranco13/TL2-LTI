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

            txtToken.ShortcutsEnabled = true;
            txtBaseUrl.ShortcutsEnabled = true;
        }


        private string BuildK3sBaseUrl(string input, string token)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new Exception("Insere o IP ou URL do master K3s.");

            input = input.Trim();

            bool hasToken = !string.IsNullOrWhiteSpace(token);

            if (!input.StartsWith("http://") && !input.StartsWith("https://"))
            {
                input = hasToken ? "https://" + input : "http://" + input;
            }

            UriBuilder uriBuilder = new UriBuilder(input);

            if (uriBuilder.Port == -1 || uriBuilder.Port == 80 || uriBuilder.Port == 443)
            {
                uriBuilder.Port = hasToken ? 6443 : 8001;
            }

            return uriBuilder.Uri.ToString().TrimEnd('/');
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;
                Cursor = Cursors.WaitCursor;

                string token = txtToken.Text
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();

                string baseUrl = BuildK3sBaseUrl(txtBaseUrl.Text, token);

                KubernetesApiClient api = new KubernetesApiClient(baseUrl, token);

                await api.GetAsync("/version");

                SaveCredentialsIfNeeded(baseUrl, token);

                MessageBox.Show("Ligação ao master K3s realizada com sucesso.");

                AppNavigator.NavigateTo(new DashboardForm(baseUrl, token));
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

        private void SaveCredentialsIfNeeded(string baseUrl, string token)
        {
            if (chkCredenciais.Checked)
            {
                Properties.Settings.Default.K3sBaseUrl = baseUrl;
                Properties.Settings.Default.K3sToken = token;
                Properties.Settings.Default.RememberCredentials = true;
            }
            else
            {
                Properties.Settings.Default.K3sBaseUrl = "";
                Properties.Settings.Default.K3sToken = "";
                Properties.Settings.Default.RememberCredentials = false;
            }

            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            if (Properties.Settings.Default.RememberCredentials)
            {
                txtBaseUrl.Text = Properties.Settings.Default.K3sBaseUrl;
                txtToken.Text = Properties.Settings.Default.K3sToken;
                chkCredenciais.Checked = true;
            }
        }
    }
}
