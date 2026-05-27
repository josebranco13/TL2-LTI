using System;
using System.Drawing;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreateServiceForm : Form
    {
        private TextBox txtServiceName;
        private TextBox txtNamespace;
        private ComboBox cmbType;
        private TextBox txtSelector;
        private TextBox txtPorts;
        private Button btnCreate;
        private Button btnCancel;

        public string ServiceName { get; private set; }
        public string NamespaceName { get; private set; }
        public string ServiceType { get; private set; }
        public string SelectorText { get; private set; }
        public string PortsText { get; private set; }

        public CreateServiceForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Service";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(455, 285);

            Label lblServiceName = CreateLabel("Nome do Service", 20, 22);
            txtServiceName = CreateTextBox(190, 18, 230, "test-service");

            Label lblNamespace = CreateLabel("Namespace", 20, 62);
            txtNamespace = CreateTextBox(190, 58, 230, "default");

            Label lblType = CreateLabel("Tipo", 20, 102);
            cmbType = new ComboBox();
            cmbType.Location = new Point(190, 98);
            cmbType.Size = new Size(230, 28);
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.Add("ClusterIP");
            cmbType.Items.Add("NodePort");
            cmbType.Items.Add("LoadBalancer");
            cmbType.SelectedIndex = 0;

            Label lblSelector = CreateLabel("Selector (ex: app=nginx)", 20, 142);
            txtSelector = CreateTextBox(190, 138, 230, "app=nginx");

            Label lblPorts = CreateLabel("Ports (ex: 80:80:TCP)", 20, 182);
            txtPorts = CreateTextBox(190, 178, 230, "80:80:TCP");

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(190, 230);
            btnCreate.Size = new Size(110, 32);
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(320, 230);
            btnCancel.Size = new Size(100, 32);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblServiceName);
            this.Controls.Add(txtServiceName);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(txtNamespace);
            this.Controls.Add(lblType);
            this.Controls.Add(cmbType);
            this.Controls.Add(lblSelector);
            this.Controls.Add(txtSelector);
            this.Controls.Add(lblPorts);
            this.Controls.Add(txtPorts);
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
            string serviceName = txtServiceName.Text.Trim();
            string namespaceName = txtNamespace.Text.Trim();
            string serviceType = cmbType.SelectedItem == null ? "ClusterIP" : cmbType.SelectedItem.ToString();
            string selector = txtSelector.Text.Trim();
            string ports = txtPorts.Text.Trim();

            if (string.IsNullOrWhiteSpace(serviceName))
            {
                MessageBox.Show("Indica o nome do service.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtServiceName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            if (string.IsNullOrWhiteSpace(selector))
            {
                MessageBox.Show("Indica o selector do service, por exemplo: app=nginx.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSelector.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ports))
            {
                MessageBox.Show("Indica pelo menos uma porta, por exemplo: 80:80:TCP.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPorts.Focus();
                return;
            }

            ServiceName = serviceName;
            NamespaceName = namespaceName;
            ServiceType = serviceType;
            SelectorText = selector;
            PortsText = ports;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
