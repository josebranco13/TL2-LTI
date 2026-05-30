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
        private KubernetesController.Services.KubernetesPodsService podsService;
        private List<KubernetesPodDetails> podDetails = new List<KubernetesPodDetails>();
        private Panel pnlPodsContent;
        private Button btnRefreshPods;
        private Button btnCreatePod;
        private Button btnDeletePod;
        private DataGridView dgvPods;
        private TabControl tabPodDetails;
        private DataGridView dgvPodSummary;
        private DataGridView dgvPodContainers;
        private DataGridView dgvPodConditions;
        private DataGridView dgvPodLabels;
        private DataGridView dgvPodAnnotations;
        private DataGridView dgvPodOwners;
        private DataGridView dgvPodVolumes;
        private DataGridView dgvPodTolerations;

        private void ConfigurePodsTabControls()
        {
            if (tabPods == null)
                return;

            // Se os controlos já existirem, garante que continuam associados à aba.
            // Isto corrige casos em que a TabPage fica vazia após alterações no Designer
            // ou após aplicar patches sucessivos ao DashboardForm.
            if (pnlPodsContent != null && dgvPods != null)
            {
                if (!tabPods.Controls.Contains(pnlPodsContent))
                    tabPods.Controls.Add(pnlPodsContent);

                pnlPodsContent.Dock = DockStyle.Fill;
                pnlPodsContent.Visible = true;
                pnlPodsContent.BringToFront();
                return;
            }

            tabPods.SuspendLayout();
            tabPods.Controls.Clear();

            pnlPodsContent = new Panel();
            pnlPodsContent.Name = "pnlPodsContent";
            pnlPodsContent.Dock = DockStyle.Fill;
            pnlPodsContent.AutoScroll = true;
            pnlPodsContent.Visible = true;
            tabPods.Controls.Add(pnlPodsContent);

            btnRefreshPods = new Button();
            btnRefreshPods.Name = "btnRefreshPods";
            btnRefreshPods.Text = "Atualizar Pods";
            btnRefreshPods.UseVisualStyleBackColor = true;
            btnRefreshPods.Click += new EventHandler(btnRefreshPods_Click);
            pnlPodsContent.Controls.Add(btnRefreshPods);

            btnCreatePod = new Button();
            btnCreatePod.Name = "btnCreatePod";
            btnCreatePod.Text = "Criar";
            btnCreatePod.UseVisualStyleBackColor = true;
            btnCreatePod.Click += new EventHandler(btnCreatePod_Click);
            pnlPodsContent.Controls.Add(btnCreatePod);

            btnDeletePod = new Button();
            btnDeletePod.Name = "btnDeletePod";
            btnDeletePod.Text = "Eliminar";
            btnDeletePod.UseVisualStyleBackColor = true;
            btnDeletePod.Click += new EventHandler(btnDeletePod_Click);
            pnlPodsContent.Controls.Add(btnDeletePod);

            dgvPods = new DataGridView();
            dgvPods.Name = "dgvPods";
            dgvPods.ReadOnly = true;
            dgvPods.AllowUserToAddRows = false;
            dgvPods.AllowUserToDeleteRows = false;
            dgvPods.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPods.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvPods.ScrollBars = ScrollBars.Both;
            dgvPods.RowHeadersVisible = false;
            dgvPods.SelectionChanged += new EventHandler(dgvPods_SelectionChanged);
            pnlPodsContent.Controls.Add(dgvPods);

            tabPodDetails = new TabControl();
            tabPodDetails.Name = "tabPodDetails";

            dgvPodSummary = CreateDetailsGrid("dgvPodSummary");
            dgvPodContainers = CreateDetailsGrid("dgvPodContainers");
            dgvPodConditions = CreateDetailsGrid("dgvPodConditions");
            dgvPodLabels = CreateDetailsGrid("dgvPodLabels");
            dgvPodAnnotations = CreateDetailsGrid("dgvPodAnnotations");
            dgvPodOwners = CreateDetailsGrid("dgvPodOwners");
            dgvPodVolumes = CreateDetailsGrid("dgvPodVolumes");
            dgvPodTolerations = CreateDetailsGrid("dgvPodTolerations");

            AddDetailsTab(tabPodDetails, "Resumo", dgvPodSummary);
            AddDetailsTab(tabPodDetails, "Containers", dgvPodContainers);
            AddDetailsTab(tabPodDetails, "Condições", dgvPodConditions);
            AddDetailsTab(tabPodDetails, "Labels", dgvPodLabels);
            AddDetailsTab(tabPodDetails, "Annotations", dgvPodAnnotations);
            AddDetailsTab(tabPodDetails, "Owners", dgvPodOwners);
            AddDetailsTab(tabPodDetails, "Volumes", dgvPodVolumes);
            AddDetailsTab(tabPodDetails, "Tolerations", dgvPodTolerations);

            pnlPodsContent.Controls.Add(tabPodDetails);

            pnlPodsContent.BringToFront();
            tabPods.ResumeLayout(false);
        }

        private void ArrangePodsLayout()
        {
            if (pnlPodsContent == null || dgvPods == null || tabPodDetails == null)
                ConfigurePodsTabControls();

            if (pnlPodsContent == null)
                return;

            pnlPodsContent.Dock = DockStyle.Fill;
            pnlPodsContent.Visible = true;
            pnlPodsContent.BringToFront();

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlPodsContent.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshPods.Location = new Point(margin, 20);
            btnRefreshPods.Size = new Size(170, 35);

            btnCreatePod.Location = new Point(margin + contentWidth - 360, 20);
            btnCreatePod.Size = new Size(170, 35);

            btnDeletePod.Location = new Point(margin + contentWidth - 170, 20);
            btnDeletePod.Size = new Size(170, 35);

            dgvPods.Location = new Point(margin, 70);
            dgvPods.Size = new Size(contentWidth, 285);

            tabPodDetails.Location = new Point(margin, dgvPods.Bottom + gap);
            tabPodDetails.Size = new Size(contentWidth, Math.Max(360, pnlPodsContent.ClientSize.Height - tabPodDetails.Top - margin));

            pnlPodsContent.AutoScrollMinSize = new Size(contentWidth + (margin * 2), tabPodDetails.Bottom + margin);
        }

        private async Task LoadPodsTabAsync()
        {
            if (pnlPodsContent == null || dgvPods == null)
                ConfigurePodsTabControls();

            if (podsService == null || dgvPods == null)
                return;

            podDetails = await podsService.GetPodDetailsAsync();

            List<KubernetesPodSummary> pods = podDetails.Select(p => new KubernetesPodSummary
            {
                Name = p.Name,
                Namespace = p.Namespace,
                Phase = p.Phase,
                Ready = p.Ready,
                Restarts = p.Restarts,
                Node = p.NodeName,
                PodIP = p.PodIP,
                HostIP = p.HostIP,
                Containers = p.Containers.Count,
                Images = p.ImagesText,
                QosClass = p.QosClass,
                CreatedAt = p.CreationTimestamp,
                ControlledBy = p.ControlledBy
            }).ToList();

            dgvPods.DataSource = null;
            dgvPods.DataSource = pods;
            ConfigurePodsGridHeaders();

            if (dgvPods.Rows.Count > 0)
                dgvPods.Rows[0].Selected = true;

            if (podDetails.Count > 0)
                ShowPodDetails(podDetails[0]);
            else
                ClearPodDetails();

            ArrangePodsLayout();
        }

        private void ConfigurePodsGridHeaders()
        {
            SetColumnHeader(dgvPods, "Name", "Nome");
            SetColumnHeader(dgvPods, "Namespace", "Namespace");
            SetColumnHeader(dgvPods, "Phase", "Estado");
            SetColumnHeader(dgvPods, "Ready", "Ready");
            SetColumnHeader(dgvPods, "Restarts", "Restarts");
            SetColumnHeader(dgvPods, "Node", "Node");
            SetColumnHeader(dgvPods, "PodIP", "Pod IP");
            SetColumnHeader(dgvPods, "HostIP", "Host IP");
            SetColumnHeader(dgvPods, "Containers", "Containers");
            SetColumnHeader(dgvPods, "Images", "Imagens");
            SetColumnHeader(dgvPods, "QosClass", "QoS");
            SetColumnHeader(dgvPods, "CreatedAt", "Criado em");
            SetColumnHeader(dgvPods, "ControlledBy", "Controlado por");

            if (dgvPods.Columns["Images"] != null)
                dgvPods.Columns["Images"].FillWeight = 180;

            if (dgvPods.Columns["Name"] != null)
                dgvPods.Columns["Name"].FillWeight = 150;
        }

        private void SetColumnHeader(DataGridView grid, string columnName, string header)
        {
            if (grid != null && grid.Columns[columnName] != null)
                grid.Columns[columnName].HeaderText = header;
        }

        private void dgvPods_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPods == null || dgvPods.CurrentRow == null || dgvPods.CurrentRow.DataBoundItem == null)
                return;

            KubernetesPodSummary selectedPod = dgvPods.CurrentRow.DataBoundItem as KubernetesPodSummary;
            if (selectedPod == null || podDetails == null)
                return;

            KubernetesPodDetails selectedDetails = podDetails.FirstOrDefault(p => p.Name == selectedPod.Name && p.Namespace == selectedPod.Namespace);
            if (selectedDetails != null)
                ShowPodDetails(selectedDetails);
        }

        private void ShowPodDetails(KubernetesPodDetails pod)
        {
            if (pod == null)
            {
                ClearPodDetails();
                return;
            }

            dgvPodSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", pod.Name },
                { "Namespace", pod.Namespace },
                { "Estado", pod.Phase },
                { "Ready", pod.Ready },
                { "Restarts", pod.Restarts.ToString() },
                { "UID", pod.Uid },
                { "Criado em", pod.CreationTimestamp },
                { "Resource Version", pod.ResourceVersion },
                { "Node", pod.NodeName },
                { "Pod IP", pod.PodIP },
                { "Host IP", pod.HostIP },
                { "Start Time", pod.StartTime },
                { "QoS", pod.QosClass },
                { "Restart Policy", pod.RestartPolicy },
                { "Service Account", pod.ServiceAccount },
                { "DNS Policy", pod.DnsPolicy },
                { "Priority Class", pod.PriorityClassName },
                { "Controlado por", pod.ControlledBy },
                { "Imagens", pod.ImagesText }
            });

            dgvPodContainers.DataSource = null;
            dgvPodContainers.DataSource = pod.Containers;

            dgvPodConditions.DataSource = null;
            dgvPodConditions.DataSource = pod.Conditions;

            dgvPodLabels.DataSource = null;
            dgvPodLabels.DataSource = pod.Labels;

            dgvPodAnnotations.DataSource = null;
            dgvPodAnnotations.DataSource = pod.Annotations;

            dgvPodOwners.DataSource = null;
            dgvPodOwners.DataSource = pod.Owners;

            dgvPodVolumes.DataSource = null;
            dgvPodVolumes.DataSource = pod.Volumes;

            dgvPodTolerations.DataSource = null;
            dgvPodTolerations.DataSource = pod.Tolerations;
        }

        private void ClearPodDetails()
        {
            if (dgvPodSummary != null)
                dgvPodSummary.DataSource = null;

            if (dgvPodContainers != null)
                dgvPodContainers.DataSource = null;

            if (dgvPodConditions != null)
                dgvPodConditions.DataSource = null;

            if (dgvPodLabels != null)
                dgvPodLabels.DataSource = null;

            if (dgvPodAnnotations != null)
                dgvPodAnnotations.DataSource = null;

            if (dgvPodOwners != null)
                dgvPodOwners.DataSource = null;

            if (dgvPodVolumes != null)
                dgvPodVolumes.DataSource = null;

            if (dgvPodTolerations != null)
                dgvPodTolerations.DataSource = null;
        }

        private async void btnRefreshPods_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshPods.Enabled = false;
                await LoadPodsTabAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao atualizar pods.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRefreshPods.Enabled = true;
            }
        }

        private async void btnCreatePod_Click(object sender, EventArgs e)
        {
            if (podsService == null)
                return;

            using (CreatePodForm form = new CreatePodForm(await GetNamespaceOptionsForFormsAsync(), await GetContainerOptionsForFormsAsync()))
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    btnCreatePod.Enabled = false;
                    await podsService.CreatePodAsync(form.PodName, form.NamespaceName, form.LabelsText, form.ContainersText);
                    MessageBox.Show("Pod criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await Task.Delay(1000);
                    await LoadPodsTabAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erro ao criar pod.\n\n" + ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                finally
                {
                    btnCreatePod.Enabled = true;
                }
            }
        }

        private async void btnDeletePod_Click(object sender, EventArgs e)
        {
            if (podsService == null || dgvPods == null || dgvPods.CurrentRow == null)
                return;

            KubernetesPodSummary selectedPod = dgvPods.CurrentRow.DataBoundItem as KubernetesPodSummary;
            if (selectedPod == null)
                return;

            string warning = "Tem a certeza que pretende eliminar o pod '" + selectedPod.Name + "' no namespace '" + selectedPod.Namespace + "'?";

            if (!string.IsNullOrWhiteSpace(selectedPod.ControlledBy))
            {
                warning += "\n\nAtenção: este pod é controlado por " + selectedPod.ControlledBy + ". " +
                           "Se pertencer a um Deployment, ReplicaSet, DaemonSet ou Job, o Kubernetes pode recriá-lo automaticamente.";
            }

            DialogResult confirm = MessageBox.Show(
                warning,
                "Confirmar eliminação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnDeletePod.Enabled = false;
                await podsService.DeletePodAsync(selectedPod.Namespace, selectedPod.Name);
                MessageBox.Show("Pedido de eliminação enviado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Delay(1500);
                await LoadPodsTabAsync();
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao eliminar pod.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnDeletePod.Enabled = true;
            }
        }
    }
}
