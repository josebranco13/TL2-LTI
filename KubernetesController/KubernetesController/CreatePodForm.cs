using System;
using System.Drawing;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreatePodForm : Form
    {
        private TextBox txtPodName;
        private TextBox txtNamespace;
        private TextBox txtLabels;
        private TextBox txtContainers;
        private Button btnCreate;
        private Button btnCancel;

        public string PodName { get; private set; }
        public string NamespaceName { get; private set; }
        public string LabelsText { get; private set; }
        public string ContainersText { get; private set; }

        public CreatePodForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Pod";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(420, 245);

            Label lblPodName = CreateLabel("Nome do Pod", 20, 22);
            txtPodName = CreateTextBox(170, 18, 220, "test-pod");

            Label lblNamespace = CreateLabel("Namespace", 20, 62);
            txtNamespace = CreateTextBox(170, 58, 220, "default");

            Label lblLabels = CreateLabel("Labels (opcional)", 20, 102);
            txtLabels = CreateTextBox(170, 98, 220, "");

            Label lblContainers = CreateLabel("Containers (nome:imagem)", 20, 142);
            txtContainers = CreateTextBox(170, 138, 220, "web:nginx");

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(170, 190);
            btnCreate.Size = new Size(110, 32);
            btnCreate.UseVisualStyleBackColor = true;
            btnCreate.Click += btnCreate_Click;

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(290, 190);
            btnCancel.Size = new Size(100, 32);
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.DialogResult = DialogResult.Cancel;

            this.Controls.Add(lblPodName);
            this.Controls.Add(txtPodName);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(txtNamespace);
            this.Controls.Add(lblLabels);
            this.Controls.Add(txtLabels);
            this.Controls.Add(lblContainers);
            this.Controls.Add(txtContainers);
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
            label.Size = new Size(145, 24);
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
            string podName = txtPodName.Text.Trim();
            string namespaceName = txtNamespace.Text.Trim();
            string labels = txtLabels.Text.Trim();
            string containers = txtContainers.Text.Trim();

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
                txtContainers.Focus();
                return;
            }

            PodName = podName;
            NamespaceName = namespaceName;
            LabelsText = labels;
            ContainersText = containers;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
