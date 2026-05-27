using System;
using System.Drawing;
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
        private TextBox txtContainers;
        private TextBox txtReplicas;
        private TextBox txtNamespace;
        private TextBox txtLabels;
        private Button btnCreate;
        private Button btnCancel;

        public string DeploymentName
        {
            get { return txtDeploymentName.Text.Trim(); }
        }

        public string ContainersText
        {
            get { return txtContainers.Text.Trim(); }
        }

        public string ReplicasText
        {
            get { return txtReplicas.Text.Trim(); }
        }

        public string NamespaceName
        {
            get { return txtNamespace.Text.Trim(); }
        }

        public string LabelsText
        {
            get { return txtLabels.Text.Trim(); }
        }

        public CreateDeploymentForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Criar Deployment";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ClientSize = new Size(410, 265);

            lblDeploymentName = new Label();
            lblDeploymentName.Text = "Nome do Deployment:";
            lblDeploymentName.Location = new Point(18, 22);
            lblDeploymentName.Size = new Size(160, 23);

            txtDeploymentName = new TextBox();
            txtDeploymentName.Location = new Point(185, 20);
            txtDeploymentName.Size = new Size(200, 27);

            lblContainers = new Label();
            lblContainers.Text = "Containers (nome:imagem):";
            lblContainers.Location = new Point(18, 62);
            lblContainers.Size = new Size(165, 23);

            txtContainers = new TextBox();
            txtContainers.Location = new Point(185, 60);
            txtContainers.Size = new Size(200, 27);
            txtContainers.Text = "web:nginx";

            lblReplicas = new Label();
            lblReplicas.Text = "N.º Réplicas:";
            lblReplicas.Location = new Point(18, 102);
            lblReplicas.Size = new Size(160, 23);

            txtReplicas = new TextBox();
            txtReplicas.Location = new Point(185, 100);
            txtReplicas.Size = new Size(200, 27);
            txtReplicas.Text = "1";

            lblNamespace = new Label();
            lblNamespace.Text = "Namespace (opcional):";
            lblNamespace.Location = new Point(18, 142);
            lblNamespace.Size = new Size(160, 23);

            txtNamespace = new TextBox();
            txtNamespace.Location = new Point(185, 140);
            txtNamespace.Size = new Size(200, 27);
            txtNamespace.Text = "default";

            lblLabels = new Label();
            lblLabels.Text = "Labels (opcional):";
            lblLabels.Location = new Point(18, 182);
            lblLabels.Size = new Size(160, 23);

            txtLabels = new TextBox();
            txtLabels.Location = new Point(185, 180);
            txtLabels.Size = new Size(200, 27);

            btnCreate = new Button();
            btnCreate.Text = "Criar";
            btnCreate.Location = new Point(105, 222);
            btnCreate.Size = new Size(120, 32);
            btnCreate.Click += new EventHandler(btnCreate_Click);

            btnCancel = new Button();
            btnCancel.Text = "Cancelar";
            btnCancel.Location = new Point(240, 222);
            btnCancel.Size = new Size(120, 32);
            btnCancel.DialogResult = DialogResult.Cancel;

            this.AcceptButton = btnCreate;
            this.CancelButton = btnCancel;

            this.Controls.Add(lblDeploymentName);
            this.Controls.Add(txtDeploymentName);
            this.Controls.Add(lblContainers);
            this.Controls.Add(txtContainers);
            this.Controls.Add(lblReplicas);
            this.Controls.Add(txtReplicas);
            this.Controls.Add(lblNamespace);
            this.Controls.Add(txtNamespace);
            this.Controls.Add(lblLabels);
            this.Controls.Add(txtLabels);
            this.Controls.Add(btnCreate);
            this.Controls.Add(btnCancel);
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
                txtContainers.Focus();
                return;
            }

            int replicas;
            if (!int.TryParse(ReplicasText, out replicas) || replicas < 0)
            {
                MessageBox.Show("O número de réplicas tem de ser um número inteiro igual ou superior a zero.", "Validação", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtReplicas.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
