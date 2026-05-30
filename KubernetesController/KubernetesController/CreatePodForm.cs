using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreatePodForm : Form
    {
        private TextBox txtPodName;
        private ComboBox cmbNamespace;
        private NumericUpDown nudLabels;
        private ComboBox cmbContainers;
        private Button btnCreate;
        private Button btnCancel;

        private readonly List<string> namespaceOptions;
        private readonly List<string> containerOptions;

        public string PodName { get; private set; }
        public string NamespaceName { get; private set; }
        public string LabelsText { get; private set; }
        public string ContainersText { get; private set; }

        public CreatePodForm()
            : this(null, null)
        {
        }

        public CreatePodForm(IEnumerable<string> namespaces, IEnumerable<string> containers)
        {
            namespaceOptions = BuildOptions(namespaces, "default");
            containerOptions = BuildOptions(containers, "web:nginx");
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Pod";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(520, 250);

            Label lblPodName = CreateLabel("Nome do Pod", 20, 22);
            txtPodName = CreateTextBox(190, 18, 300, "test-pod");

            Label lblNamespace = CreateLabel("Namespace", 20, 62);
            cmbNamespace = CreateComboBox(190, 58, 300, namespaceOptions, false);
            SelectComboValue(cmbNamespace, "default");

            Label lblLabels = CreateLabel("N.º de Labels", 20, 102);
            nudLabels = new NumericUpDown();
            nudLabels.Location = new Point(190, 98);
            nudLabels.Size = new Size(90, 26);
            nudLabels.Minimum = 0;
            nudLabels.Maximum = 20;
            nudLabels.Value = 0;

            Label lblContainers = CreateLabel("Containers (nome:imagem)", 20, 142);
            cmbContainers = CreateComboBox(190, 138, 300, containerOptions, true);
            SelectComboValue(cmbContainers, "web:nginx");

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(190, 190);
            btnCreate.Size = new Size(130, 32);
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(340, 190);
            btnCancel.Size = new Size(150, 32);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblPodName);
            this.Controls.Add(txtPodName);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(cmbNamespace);
            this.Controls.Add(lblLabels);
            this.Controls.Add(nudLabels);
            this.Controls.Add(lblContainers);
            this.Controls.Add(cmbContainers);
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

            foreach (string value in values)
                combo.Items.Add(value);

            combo.AutoCompleteSource = AutoCompleteSource.ListItems;
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

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
            // Por pedido do utilizador, o formulário já não mostra campos Chave/Valor.
            // Assim, a criação do Pod não envia labels personalizadas.
            return string.Empty;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string podName = txtPodName.Text.Trim();
            string namespaceName = cmbNamespace.Text.Trim();
            string containers = cmbContainers.Text.Trim();

            if (string.IsNullOrWhiteSpace(podName))
            {
                MessageBox.Show("Indica o nome do pod.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPodName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            if (string.IsNullOrWhiteSpace(containers))
            {
                MessageBox.Show("Indica pelo menos um container no formato nome:imagem, por exemplo: web:nginx.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cmbContainers.Focus();
                return;
            }

            try
            {
                PodName = podName;
                NamespaceName = namespaceName;
                LabelsText = BuildLabelsText();
                ContainersText = containers;

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
