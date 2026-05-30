using System;
using System.Drawing;
using System.Windows.Forms;

namespace KubernetesController
{
    public class CreateNamespaceForm : Form
    {
        private Label lblNamespaceName;
        private Label lblLabels;
        private TextBox txtNamespaceName;
        private NumericUpDown nudLabels;
        private Button btnCreate;
        private Button btnCancel;

        public string NamespaceName { get; private set; }
        public string LabelsText { get; private set; }

        public CreateNamespaceForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Criar Namespace";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(510, 170);

            lblNamespaceName = CreateLabel("Nome do Namespace:", 18, 26, 165);

            txtNamespaceName = new TextBox();
            txtNamespaceName.Name = "txtNamespaceName";
            txtNamespaceName.Location = new Point(190, 23);
            txtNamespaceName.Size = new Size(285, 24);
            txtNamespaceName.ShortcutsEnabled = true;

            lblLabels = CreateLabel("N.º de Labels:", 18, 68, 165);

            nudLabels = new NumericUpDown();
            nudLabels.Name = "nudLabels";
            nudLabels.Location = new Point(190, 65);
            nudLabels.Size = new Size(90, 24);
            nudLabels.Minimum = 0;
            nudLabels.Maximum = 20;
            nudLabels.Value = 0;

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(190, 115);
            btnCreate.Size = new Size(135, 32);
            btnCreate.Click += new EventHandler(btnCreate_Click);

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(340, 115);
            btnCancel.Size = new Size(135, 32);
            btnCancel.Click += new EventHandler(btnCancel_Click);

            this.Controls.Add(lblNamespaceName);
            this.Controls.Add(txtNamespaceName);
            this.Controls.Add(lblLabels);
            this.Controls.Add(nudLabels);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
        }

        private Label CreateLabel(string text, int x, int y, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = false;
            label.Location = new Point(x, y);
            label.Size = new Size(width, 24);
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamespaceName.Text))
            {
                MessageBox.Show("Indica o nome do namespace.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNamespaceName.Focus();
                return;
            }

            NamespaceName = txtNamespaceName.Text.Trim();
            LabelsText = string.Empty;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
