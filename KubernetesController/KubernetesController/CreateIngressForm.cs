using System;
using System.Drawing;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreateIngressForm : Form
    {
        private TextBox txtIngressName;
        private TextBox txtNamespace;
        private TextBox txtHost;
        private TextBox txtPath;
        private TextBox txtServiceName;
        private TextBox txtServicePort;
        private Button btnCreate;
        private Button btnCancel;

        public string IngressName { get; private set; }
        public string NamespaceName { get; private set; }
        public string Host { get; private set; }
        public string PathValue { get; private set; }
        public string ServiceName { get; private set; }
        public string ServicePort { get; private set; }

        public CreateIngressForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Ingress";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(455, 325);

            Label lblIngressName = CreateLabel("Nome do Ingress", 20, 22);
            txtIngressName = CreateTextBox(190, 18, 230, "test-ingress");

            Label lblNamespace = CreateLabel("Namespace", 20, 62);
            txtNamespace = CreateTextBox(190, 58, 230, "default");

            Label lblHost = CreateLabel("Host (opcional)", 20, 102);
            txtHost = CreateTextBox(190, 98, 230, "");

            Label lblPath = CreateLabel("Path", 20, 142);
            txtPath = CreateTextBox(190, 138, 230, "/");

            Label lblServiceName = CreateLabel("Service destino", 20, 182);
            txtServiceName = CreateTextBox(190, 178, 230, "test-service");

            Label lblServicePort = CreateLabel("Porta do Service", 20, 222);
            txtServicePort = CreateTextBox(190, 218, 230, "80");

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(190, 270);
            btnCreate.Size = new Size(110, 32);
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(320, 270);
            btnCancel.Size = new Size(100, 32);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblIngressName);
            this.Controls.Add(txtIngressName);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(txtNamespace);
            this.Controls.Add(lblHost);
            this.Controls.Add(txtHost);
            this.Controls.Add(lblPath);
            this.Controls.Add(txtPath);
            this.Controls.Add(lblServiceName);
            this.Controls.Add(txtServiceName);
            this.Controls.Add(lblServicePort);
            this.Controls.Add(txtServicePort);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
        }

        private Label CreateLabel(string text, int x, int y)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(165, 24);
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private TextBox CreateTextBox(int x, int y, int width, string value)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 26);
            textBox.Text = value;
            return textBox;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string ingressName = txtIngressName.Text.Trim();
            string namespaceName = txtNamespace.Text.Trim();
            string host = txtHost.Text.Trim();
            string path = txtPath.Text.Trim();
            string serviceName = txtServiceName.Text.Trim();
            string servicePort = txtServicePort.Text.Trim();

            if (string.IsNullOrWhiteSpace(ingressName))
            {
                MessageBox.Show("Indica o nome do ingress.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtIngressName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            if (string.IsNullOrWhiteSpace(path))
                path = "/";

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                MessageBox.Show("Indica o service de destino.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            int numericPort;
            if (!int.TryParse(servicePort, out numericPort))
            {
                MessageBox.Show("A porta do service deve ser numérica.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServicePort.Focus();
                return;
            }

            IngressName = ingressName;
            NamespaceName = namespaceName;
            Host = host;
            PathValue = path;
            ServiceName = serviceName;
            ServicePort = servicePort;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
