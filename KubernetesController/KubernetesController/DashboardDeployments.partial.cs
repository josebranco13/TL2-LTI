using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KubernetesController
{
    public partial class DashboardForm
    {
        private KubernetesController.Services.KubernetesDeploymentsService deploymentsService;
        private List<KubernetesDeploymentDetails> deploymentDetails = new List<KubernetesDeploymentDetails>();
        private Panel pnlDeploymentsContent;
        private Button btnRefreshDeployments;
        private Button btnCreateDeployment;
        private Button btnExportDeployment;
        private Button btnDeleteDeployment;
        private DataGridView dgvDeployments;
        private TabControl tabDeploymentDetails;
        private DataGridView dgvDeploymentSummary;
        private DataGridView dgvDeploymentReplicas;
        private DataGridView dgvDeploymentContainers;
        private DataGridView dgvDeploymentConditions;
        private DataGridView dgvDeploymentLabels;
        private DataGridView dgvDeploymentAnnotations;
        private DataGridView dgvDeploymentSelector;
        private DataGridView dgvDeploymentStrategy;
        private DataGridView dgvDeploymentManagedFields;

        private void ConfigureDeploymentTabControls()
        {
            if (tabDeployments == null)
                return;

            if (pnlDeploymentsContent != null && dgvDeployments != null)
            {
                if (!tabDeployments.Controls.Contains(pnlDeploymentsContent))
                    tabDeployments.Controls.Add(pnlDeploymentsContent);

                pnlDeploymentsContent.Dock = DockStyle.Fill;
                pnlDeploymentsContent.Visible = true;
                pnlDeploymentsContent.BringToFront();
                return;
            }

            tabDeployments.SuspendLayout();
            tabDeployments.Controls.Clear();

            pnlDeploymentsContent = new Panel();
            pnlDeploymentsContent.Name = "pnlDeploymentsContent";
            pnlDeploymentsContent.Dock = DockStyle.Fill;
            pnlDeploymentsContent.AutoScroll = true;
            pnlDeploymentsContent.Visible = true;
            tabDeployments.Controls.Add(pnlDeploymentsContent);

            btnRefreshDeployments = new Button();
            btnRefreshDeployments.Name = "btnRefreshDeployments";
            btnRefreshDeployments.Text = "Atualizar Deployments";
            btnRefreshDeployments.UseVisualStyleBackColor = true;
            btnRefreshDeployments.Click += new EventHandler(btnRefreshDeployments_Click);
            pnlDeploymentsContent.Controls.Add(btnRefreshDeployments);

            btnCreateDeployment = new Button();
            btnCreateDeployment.Name = "btnCreateDeployment";
            btnCreateDeployment.Text = "Criar";
            btnCreateDeployment.UseVisualStyleBackColor = true;
            btnCreateDeployment.Click += new EventHandler(btnCreateDeployment_Click);
            pnlDeploymentsContent.Controls.Add(btnCreateDeployment);

            btnExportDeployment = new Button();
            btnExportDeployment.Name = "btnExportDeployment";
            btnExportDeployment.Text = "Exportar";
            btnExportDeployment.UseVisualStyleBackColor = true;
            btnExportDeployment.Click += new EventHandler(btnExportDeployment_Click);
            pnlDeploymentsContent.Controls.Add(btnExportDeployment);

            btnDeleteDeployment = new Button();
            btnDeleteDeployment.Name = "btnDeleteDeployment";
            btnDeleteDeployment.Text = "Eliminar";
            btnDeleteDeployment.UseVisualStyleBackColor = true;
            btnDeleteDeployment.Click += new EventHandler(btnDeleteDeployment_Click);
            pnlDeploymentsContent.Controls.Add(btnDeleteDeployment);

            dgvDeployments = new DataGridView();
            dgvDeployments.Name = "dgvDeployments";
            dgvDeployments.ReadOnly = true;
            dgvDeployments.AllowUserToAddRows = false;
            dgvDeployments.AllowUserToDeleteRows = false;
            dgvDeployments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvDeployments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDeployments.ScrollBars = ScrollBars.Both;
            dgvDeployments.RowHeadersVisible = false;
            dgvDeployments.SelectionChanged += new EventHandler(dgvDeployments_SelectionChanged);
            pnlDeploymentsContent.Controls.Add(dgvDeployments);

            tabDeploymentDetails = new TabControl();
            tabDeploymentDetails.Name = "tabDeploymentDetails";

            dgvDeploymentSummary = CreateDetailsGrid("dgvDeploymentSummary");
            dgvDeploymentReplicas = CreateDetailsGrid("dgvDeploymentReplicas");
            dgvDeploymentContainers = CreateDetailsGrid("dgvDeploymentContainers");
            dgvDeploymentConditions = CreateDetailsGrid("dgvDeploymentConditions");
            dgvDeploymentLabels = CreateDetailsGrid("dgvDeploymentLabels");
            dgvDeploymentAnnotations = CreateDetailsGrid("dgvDeploymentAnnotations");
            dgvDeploymentSelector = CreateDetailsGrid("dgvDeploymentSelector");
            dgvDeploymentStrategy = CreateDetailsGrid("dgvDeploymentStrategy");
            dgvDeploymentManagedFields = CreateDetailsGrid("dgvDeploymentManagedFields");

            AddDetailsTab(tabDeploymentDetails, "Resumo", dgvDeploymentSummary);
            AddDetailsTab(tabDeploymentDetails, "Réplicas", dgvDeploymentReplicas);
            AddDetailsTab(tabDeploymentDetails, "Containers", dgvDeploymentContainers);
            AddDetailsTab(tabDeploymentDetails, "Condições", dgvDeploymentConditions);
            AddDetailsTab(tabDeploymentDetails, "Labels", dgvDeploymentLabels);
            AddDetailsTab(tabDeploymentDetails, "Annotations", dgvDeploymentAnnotations);
            AddDetailsTab(tabDeploymentDetails, "Selector", dgvDeploymentSelector);
            AddDetailsTab(tabDeploymentDetails, "Estratégia", dgvDeploymentStrategy);
            AddDetailsTab(tabDeploymentDetails, "Managed Fields", dgvDeploymentManagedFields);

            pnlDeploymentsContent.Controls.Add(tabDeploymentDetails);

            pnlDeploymentsContent.BringToFront();
            tabDeployments.ResumeLayout(false);
        }

        private void ArrangeDeploymentsLayout()
        {
            if (pnlDeploymentsContent == null || dgvDeployments == null || tabDeploymentDetails == null)
                ConfigureDeploymentTabControls();

            if (pnlDeploymentsContent == null)
                return;

            pnlDeploymentsContent.Dock = DockStyle.Fill;
            pnlDeploymentsContent.Visible = true;
            pnlDeploymentsContent.BringToFront();

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlDeploymentsContent.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshDeployments.Location = new Point(margin, 20);
            btnRefreshDeployments.Size = new Size(190, 35);

            btnCreateDeployment.Location = new Point(margin + contentWidth - 550, 20);
            btnCreateDeployment.Size = new Size(170, 35);

            btnExportDeployment.Location = new Point(margin + contentWidth - 360, 20);
            btnExportDeployment.Size = new Size(170, 35);

            btnDeleteDeployment.Location = new Point(margin + contentWidth - 170, 20);
            btnDeleteDeployment.Size = new Size(170, 35);

            dgvDeployments.Location = new Point(margin, 70);
            dgvDeployments.Size = new Size(contentWidth, 285);

            tabDeploymentDetails.Location = new Point(margin, dgvDeployments.Bottom + gap);
            tabDeploymentDetails.Size = new Size(contentWidth, Math.Max(360, pnlDeploymentsContent.ClientSize.Height - tabDeploymentDetails.Top - margin));

            pnlDeploymentsContent.AutoScrollMinSize = new Size(contentWidth + (margin * 2), tabDeploymentDetails.Bottom + margin);
        }

        private async Task LoadDeploymentsTabAsync()
        {
            if (pnlDeploymentsContent == null || dgvDeployments == null)
                ConfigureDeploymentTabControls();

            if (deploymentsService == null || dgvDeployments == null)
                return;

            deploymentDetails = await deploymentsService.GetDeploymentDetailsAsync();

            List<KubernetesDeploymentSummary> deployments = deploymentDetails.Select(d => new KubernetesDeploymentSummary
            {
                Name = d.Name,
                Namespace = d.Namespace,
                Status = d.Status,
                Ready = d.ReadyText,
                Replicas = d.Replicas,
                Available = d.AvailableReplicas,
                Updated = d.UpdatedReplicas,
                Containers = d.ContainersText,
                Images = d.ImagesText,
                Strategy = d.StrategyType,
                CreatedAt = d.CreationTimestamp,
                Revision = d.Revision
            }).ToList();

            dgvDeployments.DataSource = null;
            dgvDeployments.DataSource = deployments;
            ConfigureDeploymentsGridHeaders();

            if (dgvDeployments.Rows.Count > 0)
                dgvDeployments.Rows[0].Selected = true;

            if (deploymentDetails.Count > 0)
                ShowDeploymentDetails(deploymentDetails[0]);
            else
                ClearDeploymentDetails();

            ArrangeDeploymentsLayout();
        }

        private void ConfigureDeploymentsGridHeaders()
        {
            SetDeploymentColumnHeader("Name", "Nome");
            SetDeploymentColumnHeader("Namespace", "Namespace");
            SetDeploymentColumnHeader("Status", "Estado");
            SetDeploymentColumnHeader("Ready", "Ready");
            SetDeploymentColumnHeader("Replicas", "Réplicas");
            SetDeploymentColumnHeader("Available", "Disponíveis");
            SetDeploymentColumnHeader("Updated", "Atualizadas");
            SetDeploymentColumnHeader("Containers", "Containers");
            SetDeploymentColumnHeader("Images", "Imagens");
            SetDeploymentColumnHeader("Strategy", "Estratégia");
            SetDeploymentColumnHeader("CreatedAt", "Criado em");
            SetDeploymentColumnHeader("Revision", "Revisão");

            if (dgvDeployments.Columns["Name"] != null)
                dgvDeployments.Columns["Name"].FillWeight = 150;

            if (dgvDeployments.Columns["Images"] != null)
                dgvDeployments.Columns["Images"].FillWeight = 190;
        }

        private void SetDeploymentColumnHeader(string columnName, string header)
        {
            if (dgvDeployments != null && dgvDeployments.Columns[columnName] != null)
                dgvDeployments.Columns[columnName].HeaderText = header;
        }

        private void dgvDeployments_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDeployments == null || dgvDeployments.CurrentRow == null || dgvDeployments.CurrentRow.DataBoundItem == null)
                return;

            KubernetesDeploymentSummary selectedDeployment = dgvDeployments.CurrentRow.DataBoundItem as KubernetesDeploymentSummary;
            if (selectedDeployment == null || deploymentDetails == null)
                return;

            KubernetesDeploymentDetails selectedDetails = deploymentDetails.FirstOrDefault(d => d.Name == selectedDeployment.Name && d.Namespace == selectedDeployment.Namespace);
            if (selectedDetails != null)
                ShowDeploymentDetails(selectedDetails);
        }

        private void ShowDeploymentDetails(KubernetesDeploymentDetails deployment)
        {
            if (deployment == null)
            {
                ClearDeploymentDetails();
                return;
            }

            dgvDeploymentSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", deployment.Name },
                { "Namespace", deployment.Namespace },
                { "Estado", deployment.Status },
                { "Ready", deployment.ReadyText },
                { "UID", deployment.Uid },
                { "Criado em", deployment.CreationTimestamp },
                { "Resource Version", deployment.ResourceVersion },
                { "Revisão", deployment.Revision },
                { "Gerido por", deployment.ManagedBy },
                { "Containers", deployment.ContainersText },
                { "Imagens", deployment.ImagesText },
                { "Selector", deployment.SelectorText },
                { "Estratégia", deployment.StrategyType }
            });

            dgvDeploymentReplicas.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Réplicas desejadas", deployment.Replicas.ToString() },
                { "Ready Replicas", deployment.ReadyReplicas.ToString() },
                { "Available Replicas", deployment.AvailableReplicas.ToString() },
                { "Updated Replicas", deployment.UpdatedReplicas.ToString() },
                { "Unavailable Replicas", deployment.UnavailableReplicas.ToString() },
                { "Terminating Replicas", deployment.TerminatingReplicas.ToString() }
            });

            dgvDeploymentContainers.DataSource = null;
            dgvDeploymentContainers.DataSource = deployment.Containers;

            dgvDeploymentConditions.DataSource = null;
            dgvDeploymentConditions.DataSource = deployment.Conditions;

            dgvDeploymentLabels.DataSource = null;
            dgvDeploymentLabels.DataSource = deployment.Labels;

            dgvDeploymentAnnotations.DataSource = null;
            dgvDeploymentAnnotations.DataSource = deployment.Annotations;

            dgvDeploymentSelector.DataSource = null;
            dgvDeploymentSelector.DataSource = deployment.Selector;

            dgvDeploymentStrategy.DataSource = null;
            dgvDeploymentStrategy.DataSource = deployment.Strategy;

            dgvDeploymentManagedFields.DataSource = null;
            dgvDeploymentManagedFields.DataSource = deployment.ManagedFields;
        }

        private void ClearDeploymentDetails()
        {
            if (dgvDeploymentSummary != null)
                dgvDeploymentSummary.DataSource = null;

            if (dgvDeploymentReplicas != null)
                dgvDeploymentReplicas.DataSource = null;

            if (dgvDeploymentContainers != null)
                dgvDeploymentContainers.DataSource = null;

            if (dgvDeploymentConditions != null)
                dgvDeploymentConditions.DataSource = null;

            if (dgvDeploymentLabels != null)
                dgvDeploymentLabels.DataSource = null;

            if (dgvDeploymentAnnotations != null)
                dgvDeploymentAnnotations.DataSource = null;

            if (dgvDeploymentSelector != null)
                dgvDeploymentSelector.DataSource = null;

            if (dgvDeploymentStrategy != null)
                dgvDeploymentStrategy.DataSource = null;

            if (dgvDeploymentManagedFields != null)
                dgvDeploymentManagedFields.DataSource = null;
        }

        private async void btnRefreshDeployments_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshDeployments.Enabled = false;
                await LoadDeploymentsTabAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao atualizar deployments.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRefreshDeployments.Enabled = true;
            }
        }

        private async void btnCreateDeployment_Click(object sender, EventArgs e)
        {
            if (deploymentsService == null)
                return;

            using (CreateDeploymentForm form = new CreateDeploymentForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    btnCreateDeployment.Enabled = false;
                    await deploymentsService.CreateDeploymentAsync(form.DeploymentName, form.ContainersText, form.ReplicasText, form.NamespaceName, form.LabelsText);
                    MessageBox.Show("Deployment criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await Task.Delay(1000);
                    await LoadDeploymentsTabAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erro ao criar deployment.\n\n" + ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                finally
                {
                    btnCreateDeployment.Enabled = true;
                }
            }
        }

        private async void btnExportDeployment_Click(object sender, EventArgs e)
        {
            if (dgvDeployments == null || dgvDeployments.CurrentRow == null)
                return;

            KubernetesDeploymentSummary selectedDeployment = dgvDeployments.CurrentRow.DataBoundItem as KubernetesDeploymentSummary;
            if (selectedDeployment == null || string.IsNullOrWhiteSpace(selectedDeployment.Name) || string.IsNullOrWhiteSpace(selectedDeployment.Namespace))
                return;

            string encodedNamespace = Uri.EscapeDataString(selectedDeployment.Namespace.Trim());
            string encodedName = Uri.EscapeDataString(selectedDeployment.Name.Trim());

            try
            {
                btnExportDeployment.Enabled = false;
                await ExportResourceAsync(
                    "/apis/apps/v1/namespaces/" + encodedNamespace + "/deployments/" + encodedName,
                    selectedDeployment.Namespace + "-" + selectedDeployment.Name,
                    "Exportar Deployment"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao exportar deployment.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportDeployment.Enabled = true;
            }
        }

        private async void btnDeleteDeployment_Click(object sender, EventArgs e)
        {
            if (deploymentsService == null || dgvDeployments == null || dgvDeployments.CurrentRow == null)
                return;

            KubernetesDeploymentSummary selectedDeployment = dgvDeployments.CurrentRow.DataBoundItem as KubernetesDeploymentSummary;
            if (selectedDeployment == null)
                return;

            DialogResult confirm = MessageBox.Show(
                "Tem a certeza que pretende eliminar o deployment '" + selectedDeployment.Name + "' no namespace '" + selectedDeployment.Namespace + "'?\n\n" +
                "A eliminação do Deployment também vai terminar os pods geridos por ele.",
                "Confirmar eliminação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnDeleteDeployment.Enabled = false;
                await deploymentsService.DeleteDeploymentAsync(selectedDeployment.Namespace, selectedDeployment.Name);
                MessageBox.Show("Pedido de eliminação enviado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Delay(1500);
                await LoadDeploymentsTabAsync();
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao eliminar deployment.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnDeleteDeployment.Enabled = true;
            }
        }
    }
}
