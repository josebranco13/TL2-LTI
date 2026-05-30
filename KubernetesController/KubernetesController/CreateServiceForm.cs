using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreateServiceForm : Form
    {
        private TextBox txtServiceName;
        private ComboBox cmbNamespace;
        private ComboBox cmbType;
        private TextBox txtSelector;
        private NumericUpDown nudServicePort;
        private NumericUpDown nudTargetPort;
        private ComboBox cmbProtocol;
        private Button btnCreate;
        private Button btnCancel;

        private readonly List<string> namespaceOptions;

        public string ServiceName { get; private set; }
        public string NamespaceName { get; private set; }
        public string ServiceType { get; private set; }
        public string SelectorText { get; private set; }
        public string PortsText { get; private set; }

        public CreateServiceForm()
            : this(null)
        {
        }

        public CreateServiceForm(IEnumerable<string> namespaces)
        {
            namespaceOptions = BuildOptions(namespaces, "default");
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Service";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(555, 335);

            Label lblServiceName = CreateLabel("Nome do Service", 20, 22);
            txtServiceName = CreateTextBox(190, 18, 325, "test-service");

            Label lblNamespace = CreateLabel("Namespace", 20, 62);
            cmbNamespace = CreateComboBox(190, 58, 325, namespaceOptions, false);
            SelectComboValue(cmbNamespace, "default");

            Label lblType = CreateLabel("Tipo", 20, 102);
            cmbType = new ComboBox();
            cmbType.Location = new Point(190, 98);
            cmbType.Size = new Size(325, 28);
            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbType.Items.Add("ClusterIP");
            cmbType.Items.Add("NodePort");
            cmbType.Items.Add("LoadBalancer");
            cmbType.SelectedIndex = 0;

            Label lblSelector = CreateLabel("Selector (ex: app=nginx)", 20, 142);
            txtSelector = CreateTextBox(190, 138, 325, "app=nginx");

            Label lblPorts = CreateLabel("Ports", 20, 182);

            Label lblServicePort = new Label();
            lblServicePort.Text = "Service";
            lblServicePort.Location = new Point(190, 182);
            lblServicePort.Size = new Size(70, 22);

            nudServicePort = new NumericUpDown();
            nudServicePort.Location = new Point(190, 206);
            nudServicePort.Size = new Size(90, 26);
            nudServicePort.Minimum = 1;
            nudServicePort.Maximum = 65535;
            nudServicePort.Value = 80;

            Label lblTargetPort = new Label();
            lblTargetPort.Text = "Target";
            lblTargetPort.Location = new Point(300, 182);
            lblTargetPort.Size = new Size(70, 22);

            nudTargetPort = new NumericUpDown();
            nudTargetPort.Location = new Point(300, 206);
            nudTargetPort.Size = new Size(90, 26);
            nudTargetPort.Minimum = 1;
            nudTargetPort.Maximum = 65535;
            nudTargetPort.Value = 80;

            Label lblProtocol = new Label();
            lblProtocol.Text = "Protocolo";
            lblProtocol.Location = new Point(410, 182);
            lblProtocol.Size = new Size(90, 22);

            cmbProtocol = new ComboBox();
            cmbProtocol.Location = new Point(410, 205);
            cmbProtocol.Size = new Size(105, 28);
            cmbProtocol.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProtocol.Items.Add("TCP");
            cmbProtocol.Items.Add("UDP");
            cmbProtocol.SelectedIndex = 0;

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(190, 275);
            btnCreate.Size = new Size(150, 34);
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(365, 275);
            btnCancel.Size = new Size(150, 34);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblServiceName);
            this.Controls.Add(txtServiceName);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(cmbNamespace);
            this.Controls.Add(lblType);
            this.Controls.Add(cmbType);
            this.Controls.Add(lblSelector);
            this.Controls.Add(txtSelector);
            this.Controls.Add(lblPorts);
            this.Controls.Add(lblServicePort);
            this.Controls.Add(nudServicePort);
            this.Controls.Add(lblTargetPort);
            this.Controls.Add(nudTargetPort);
            this.Controls.Add(lblProtocol);
            this.Controls.Add(cmbProtocol);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
        }

        private List<string> BuildOptions(IEnumerable<string> values, string fallback)
        {
            List<string> result = new List<string>();

            if (values != null)
            {
                foreach (string value in values)
                {
                    if (!string.IsNullOrWhiteSpace(value) && !result.Any(v => string.Equals(v, value.Trim(), StringComparison.OrdinalIgnoreCase)))
                        result.Add(value.Trim());
                }
            }

            if (!string.IsNullOrWhiteSpace(fallback) && !result.Any(v => string.Equals(v, fallback, StringComparison.OrdinalIgnoreCase)))
                result.Insert(0, fallback);

            return result;
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

        private ComboBox CreateComboBox(int x, int y, int width, List<string> values, bool allowTyping)
        {
            ComboBox combo = new ComboBox();
            combo.Location = new Point(x, y);
            combo.Size = new Size(width, 28);
            combo.DropDownStyle = allowTyping ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
            combo.AutoCompleteSource = AutoCompleteSource.ListItems;
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

            foreach (string value in values)
                combo.Items.Add(value);

            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;

            return combo;
        }

        private void SelectComboValue(ComboBox combo, string value)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                if (string.Equals(combo.Items[i].ToString(), value, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string serviceName = txtServiceName.Text.Trim();
            string namespaceName = cmbNamespace.Text.Trim();
            string serviceType = cmbType.SelectedItem == null ? "ClusterIP" : cmbType.SelectedItem.ToString();
            string selector = txtSelector.Text.Trim();
            string protocol = cmbProtocol.SelectedItem == null ? "TCP" : cmbProtocol.SelectedItem.ToString();
            string ports = Convert.ToInt32(nudServicePort.Value).ToString() + ":" + Convert.ToInt32(nudTargetPort.Value).ToString() + ":" + protocol;

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
