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
        private TextBox txtLabels;
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
            this.ClientSize = new Size(430, 180);

            lblNamespaceName = new Label();
            lblNamespaceName.Text = "Nome do Namespace:";
            lblNamespaceName.AutoSize = true;
            lblNamespaceName.Location = new Point(18, 28);

            txtNamespaceName = new TextBox();
            txtNamespaceName.Name = "txtNamespaceName";
            txtNamespaceName.Location = new Point(185, 25);
            txtNamespaceName.Size = new Size(220, 24);
            txtNamespaceName.ShortcutsEnabled = true;

            lblLabels = new Label();
            lblLabels.Text = "Labels (opcional):";
            lblLabels.AutoSize = true;
            lblLabels.Location = new Point(18, 72);

            txtLabels = new TextBox();
            txtLabels.Name = "txtLabels";
            txtLabels.Location = new Point(185, 69);
            txtLabels.Size = new Size(220, 24);
            txtLabels.ShortcutsEnabled = true;

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(120, 120);
            btnCreate.Size = new Size(190, 35);
            btnCreate.Click += new EventHandler(btnCreate_Click);

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(315, 120);
            btnCancel.Size = new Size(90, 35);
            btnCancel.Click += new EventHandler(btnCancel_Click);

            this.Controls.Add(lblNamespaceName);
            this.Controls.Add(txtNamespaceName);
            this.Controls.Add(lblLabels);
            this.Controls.Add(txtLabels);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;
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
            LabelsText = txtLabels.Text.Trim();
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
