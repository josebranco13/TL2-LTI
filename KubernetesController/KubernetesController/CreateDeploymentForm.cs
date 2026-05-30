using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreateDeploymentForm : Form
    {
        private Label lblDeploymentName;
        private Label lblContainers;
        private Label lblReplicas;
        private Label lblNamespace;
        private Label lblLabels;
        private TextBox txtDeploymentName;
        private ComboBox cmbContainers;
        private NumericUpDown nudReplicas;
        private ComboBox cmbNamespace;
        private NumericUpDown nudLabels;
        private Button btnCreate;
        private Button btnCancel;

        private readonly List<string> namespaceOptions;
        private readonly List<string> containerOptions;

        public string DeploymentName
        {
            get { return txtDeploymentName.Text.Trim(); }
        }

        public string ContainersText
        {
            get { return cmbContainers.Text.Trim(); }
        }

        public string ReplicasText
        {
            get { return Convert.ToInt32(nudReplicas.Value).ToString(); }
        }

        public string NamespaceName
        {
            get { return cmbNamespace.Text.Trim(); }
        }

        public string LabelsText { get; private set; }

        public CreateDeploymentForm()
            : this(null, null)
        {
        }

        public CreateDeploymentForm(IEnumerable<string> namespaces, IEnumerable<string> containers)
        {
            namespaceOptions = BuildOptions(namespaces, "default");
            containerOptions = BuildOptions(containers, "web:nginx");
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Criar Deployment";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(535, 300);

            lblDeploymentName = CreateLabel("Nome do Deployment:", 18, 22, 180);
            txtDeploymentName = CreateTextBox(205, 20, 300, "");

            lblContainers = CreateLabel("Containers (nome:imagem):", 18, 62, 180);
            cmbContainers = CreateComboBox(205, 60, 300, containerOptions, true);
            SelectComboValue(cmbContainers, "web:nginx");

            lblReplicas = CreateLabel("N.º Réplicas:", 18, 102, 180);
            nudReplicas = new NumericUpDown();
            nudReplicas.Location = new Point(205, 100);
            nudReplicas.Size = new Size(110, 27);
            nudReplicas.Minimum = 0;
            nudReplicas.Maximum = 1000;
            nudReplicas.Value = 1;

            lblNamespace = CreateLabel("Namespace:", 18, 142, 180);
            cmbNamespace = CreateComboBox(205, 140, 300, namespaceOptions, false);
            SelectComboValue(cmbNamespace, "default");

            lblLabels = CreateLabel("N.º de Labels:", 18, 182, 180);
            nudLabels = new NumericUpDown();
            nudLabels.Location = new Point(205, 180);
            nudLabels.Size = new Size(110, 27);
            nudLabels.Minimum = 0;
            nudLabels.Maximum = 20;
            nudLabels.Value = 0;
            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(165, 245);
            btnCreate.Size = new Size(150, 34);
            btnCreate.Click += new EventHandler(btnCreate_Click);

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(335, 245);
            btnCancel.Size = new Size(150, 34);
            btnCancel.DialogResult = DialogResult.Cancel;

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;

            this.Controls.Add(lblDeploymentName);
            this.Controls.Add(txtDeploymentName);
            this.Controls.Add(lblContainers);
            this.Controls.Add(cmbContainers);
            this.Controls.Add(lblReplicas);
            this.Controls.Add(nudReplicas);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(cmbNamespace);
            this.Controls.Add(lblLabels);
            this.Controls.Add(nudLabels);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

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

        private Label CreateLabel(string text, int x, int y, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Location = new Point(x, y);
            label.Size = new Size(width, 23);
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private TextBox CreateTextBox(int x, int y, int width, string value)
        {
            TextBox textBox = new TextBox();
            textBox.Location = new Point(x, y);
            textBox.Size = new Size(width, 27);
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

            if (combo.DropDownStyle == ComboBoxStyle.DropDown)
                combo.Text = value;
        }

        private string BuildLabelsText()
        {
            return string.Empty;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DeploymentName))
            {
                MessageBox.Show("Indica o nome do deployment.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDeploymentName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ContainersText))
            {
                MessageBox.Show("Indica pelo menos um container no formato nome:imagem.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbContainers.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(NamespaceName))
            {
                MessageBox.Show("Seleciona um namespace.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbNamespace.Focus();
                return;
            }

            try
            {
                LabelsText = BuildLabelsText();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
