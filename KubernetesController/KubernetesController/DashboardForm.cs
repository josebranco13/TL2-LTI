using KubernetesController.Models;
using KubernetesController.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace KubernetesController
{
    public partial class DashboardForm : Form
    {
        private string baseUrl;
        private string token;

        private KubernetesApiClient api;
        private KubernetesNodesService nodesService;
        private KubernetesDashboardService dashboardService;
        private KubernetesNamespacesService namespacesService;
        private bool layoutEventsConfigured;

        private List<KubernetesNodeDetails> nodeDetails = new List<KubernetesNodeDetails>();
        private bool nodeDetailsVisible = false;
        private Button btnToggleNodeDetails;
        private TabControl tabNodeDetails;
        private DataGridView dgvNodeSummary;
        private DataGridView dgvNodeResources;
        private DataGridView dgvNodeConditions;
        private DataGridView dgvNodeNetwork;
        private DataGridView dgvNodeSystem;
        private DataGridView dgvNodeLabels;
        private DataGridView dgvNodeAnnotations;
        private DataGridView dgvNodeTaints;
        private DataGridView dgvNodeImages;

        private List<KubernetesNamespaceDetails> namespaceDetails = new List<KubernetesNamespaceDetails>();
        private Panel pnlNamespacesContent;
        private Button btnRefreshNamespaces;
        private Button btnCreateNamespace;
        private Button btnDeleteNamespace;
        private DataGridView dgvNamespaces;
        private TabControl tabNamespaceDetails;
        private DataGridView dgvNamespaceSummary;
        private DataGridView dgvNamespaceLabels;
        private DataGridView dgvNamespaceFinalizers;
        private DataGridView dgvNamespaceManagedFields;

        // Construtor vazio necessário para o Designer do Visual Studio
        public DashboardForm()
        {
            InitializeComponent();
            ConfigureStaticLayout();
        }

        // Construtor usado pela aplicação quando a ligação ao K3s é feita com sucesso
        public DashboardForm(string baseUrl, string token)
        {
            InitializeComponent();

            this.baseUrl = baseUrl;
            this.token = token;

            this.api = new KubernetesApiClient(baseUrl, token);
            this.nodesService = new KubernetesNodesService(this.api);
            this.dashboardService = new KubernetesDashboardService(this.api);
            this.namespacesService = new KubernetesNamespacesService(this.api);
            this.podsService = new KubernetesPodsService(this.api);
            this.deploymentsService = new KubernetesDeploymentsService(this.api);

            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            ConfigureStaticLayout();
        }

        private void ConfigureStaticLayout()
        {
            tabKubernetesController.Dock = DockStyle.Fill;

            pnlDashboard.Dock = DockStyle.Fill;
            pnlDashboard.AutoScroll = true;

            pnlNodes.Dock = DockStyle.Fill;
            pnlNodes.AutoScroll = true;

            dgvNodes.ScrollBars = ScrollBars.Both;
            dgvNodes.ReadOnly = true;
            dgvNodes.AllowUserToAddRows = false;
            dgvNodes.AllowUserToDeleteRows = false;
            dgvNodes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNodes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            txtNodesRawJson.Visible = false;

            ConfigureNodeDetailsControls();
            ConfigureNamespaceTabControls();
            ConfigurePodsTabControls();
            ConfigureDeploymentTabControls();

            lblTopImageValue.AutoSize = false;
            lblTopImageValue.AutoEllipsis = false;
            lblTopImageValue.TextAlign = ContentAlignment.MiddleLeft;

            if (!layoutEventsConfigured)
            {
                layoutEventsConfigured = true;
                this.Resize += DashboardForm_Resize;
                tabKubernetesController.SelectedIndexChanged += TabKubernetesController_SelectedIndexChanged;
                dgvNodes.SelectionChanged += dgvNodes_SelectionChanged;
                if (dgvNamespaces != null)
                    dgvNamespaces.SelectionChanged += dgvNamespaces_SelectionChanged;
            }

            ArrangeDashboardLayout();
            ArrangeNodesLayout();
            ArrangeNamespacesLayout();
            ArrangePodsLayout();
            ArrangeDeploymentsLayout();
        }

        private void DashboardForm_Resize(object sender, EventArgs e)
        {
            ArrangeDashboardLayout();
            ArrangeNodesLayout();
            ArrangeNamespacesLayout();
            ArrangePodsLayout();
            ArrangeDeploymentsLayout();
        }

        private async void TabKubernetesController_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrangeDashboardLayout();
            ArrangeNodesLayout();
            ArrangeNamespacesLayout();

            // A aba Pods é criada e carregada também quando o utilizador entra nela.
            // Isto evita a situação em que a aba fica vazia se o carregamento inicial
            // do cluster parar antes de chegar aos pods.
            if (tabKubernetesController.SelectedTab == tabPods)
            {
                ConfigurePodsTabControls();
                ArrangePodsLayout();

                if (podsService != null && dgvPods != null && dgvPods.Rows.Count == 0)
                {
                    try
                    {
                        await LoadPodsTabAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Erro ao carregar pods.\n\n" + ex.Message,
                            "Erro",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
            else
            {
                ArrangePodsLayout();
            }

            if (tabKubernetesController.SelectedTab == tabDeployments)
            {
                ConfigureDeploymentTabControls();
                ArrangeDeploymentsLayout();

                if (deploymentsService != null && dgvDeployments != null && dgvDeployments.Rows.Count == 0)
                {
                    try
                    {
                        await LoadDeploymentsTabAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(
                            "Erro ao carregar deployments.\n\n" + ex.Message,
                            "Erro",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }
                }
            }
            else
            {
                ArrangeDeploymentsLayout();
            }
        }

        private void ResetDashboardScroll()
        {
            if (pnlDashboard == null)
                return;

            // Evita que, depois de atualizar, o AutoScroll mantenha uma posição antiga
            // e empurre o conteúdo para baixo, criando espaço vazio no topo.
            pnlDashboard.AutoScrollPosition = new Point(0, 0);
        }

        private void ResetNodesScroll()
        {
            if (pnlNodes == null)
                return;

            // Quando os detalhes são escondidos, o painel pode manter a posição antiga
            // do scroll. Isto cria um espaço vazio no topo. Ao voltar ao topo,
            // a tabela dos nodes fica novamente encostada à posição correta.
            pnlNodes.AutoScrollPosition = new Point(0, 0);
        }

        private void ArrangeDashboardLayout()
        {
            if (pnlDashboard == null)
                return;

            ResetDashboardScroll();

            int margin = 24;
            int gap = 12;
            int availableWidth = pnlDashboard.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2);
            int contentWidth = Math.Max(780, availableWidth);

            pnlDashboard.SuspendLayout();

            lblDashboardTitle.Location = new Point(margin, 18);
            lblConnectionInfo.Location = new Point(margin, 58);

            btnRefreshDashboard.Size = new Size(140, 35);
            btnRefreshDashboard.Location = new Point(margin + contentWidth - btnRefreshDashboard.Width, 34);

            List<Panel> cards = GetDashboardCards();

            int cardMinWidth = 175;
            int cardHeight = 78;
            int cardColumns = Math.Max(1, (contentWidth + gap) / (cardMinWidth + gap));
            cardColumns = Math.Min(cardColumns, 6);
            int cardWidth = (contentWidth - (gap * (cardColumns - 1))) / cardColumns;

            int y = 100;
            int currentRow = 0;
            int currentColumn = 0;

            foreach (Panel card in cards)
            {
                int cardSpan = GetDashboardCardSpan(card, cardColumns);

                if (currentColumn + cardSpan > cardColumns)
                {
                    currentRow++;
                    currentColumn = 0;
                }

                int cardActualWidth = (cardWidth * cardSpan) + (gap * (cardSpan - 1));

                card.Location = new Point(
                    margin + (currentColumn * (cardWidth + gap)),
                    y + (currentRow * (cardHeight + gap))
                );
                card.Size = new Size(cardActualWidth, cardHeight);

                foreach (Control child in card.Controls)
                {
                    Label label = child as Label;
                    if (label == null)
                        continue;

                    if (label.Location.Y >= 30)
                    {
                        int labelHeight = card == cardTopImage ? 40 : label.Height;
                        label.Size = new Size(Math.Max(40, cardActualWidth - 20), labelHeight);
                    }
                }

                currentColumn += cardSpan;

                if (currentColumn >= cardColumns)
                {
                    currentRow++;
                    currentColumn = 0;
                }
            }

            int cardRows = currentRow + (currentColumn > 0 ? 1 : 0);
            y = y + (cardRows * (cardHeight + gap)) + 25;

            List<Chart> charts = GetDashboardCharts();
            int chartColumns = contentWidth >= 950 ? 2 : 1;
            int chartWidth = (contentWidth - (gap * (chartColumns - 1))) / chartColumns;
            int chartHeight = Math.Max(260, Math.Min(360, (int)(chartWidth * 0.55)));

            for (int i = 0; i < charts.Count; i++)
            {
                int row = i / chartColumns;
                int col = i % chartColumns;

                Chart chart = charts[i];
                chart.Location = new Point(margin + (col * (chartWidth + gap)), y + (row * (chartHeight + gap)));
                chart.Size = new Size(chartWidth, chartHeight);
                chart.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            int chartRows = (int)Math.Ceiling(charts.Count / (double)chartColumns);
            int totalHeight = y + (chartRows * (chartHeight + gap)) + margin;

            pnlDashboard.AutoScrollMinSize = new Size(contentWidth + (margin * 2), totalHeight);
            pnlDashboard.ResumeLayout();
        }

        private void ArrangeNodesLayout()
        {
            if (pnlNodes == null)
                return;

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlNodes.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshNodes.Location = new Point(margin, 20);
            btnRefreshNodes.Size = new Size(170, 35);

            dgvNodes.Location = new Point(margin, 70);
            dgvNodes.Size = new Size(contentWidth, 250);

            int bottomContent = dgvNodes.Bottom;

            if (btnToggleNodeDetails != null)
            {
                btnToggleNodeDetails.Size = new Size(170, 35);
                btnToggleNodeDetails.Text = nodeDetailsVisible ? "Menos detalhes" : "Mais detalhes";
                btnToggleNodeDetails.Location = new Point(margin + contentWidth - btnToggleNodeDetails.Width, dgvNodes.Bottom + gap);
                btnToggleNodeDetails.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                bottomContent = btnToggleNodeDetails.Bottom;
            }

            if (tabNodeDetails != null)
            {
                tabNodeDetails.Visible = nodeDetailsVisible;

                if (nodeDetailsVisible)
                {
                    int detailsTop = bottomContent + gap;
                    tabNodeDetails.Location = new Point(margin, detailsTop);
                    tabNodeDetails.Size = new Size(contentWidth, Math.Max(420, pnlNodes.ClientSize.Height - detailsTop - margin));
                    tabNodeDetails.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                    bottomContent = tabNodeDetails.Bottom;
                }
            }

            pnlNodes.AutoScrollMinSize = new Size(contentWidth + (margin * 2), bottomContent + margin);
        }


        private void ConfigureNamespaceTabControls()
        {
            if (pnlNamespacesContent != null)
                return;

            tabNamespaces.Controls.Clear();

            pnlNamespacesContent = new Panel();
            pnlNamespacesContent.Name = "pnlNamespacesContent";
            pnlNamespacesContent.Dock = DockStyle.Fill;
            pnlNamespacesContent.AutoScroll = true;
            tabNamespaces.Controls.Add(pnlNamespacesContent);

            btnRefreshNamespaces = new Button();
            btnRefreshNamespaces.Name = "btnRefreshNamespaces";
            btnRefreshNamespaces.Text = "Atualizar Namespaces";
            btnRefreshNamespaces.UseVisualStyleBackColor = true;
            btnRefreshNamespaces.Click += new EventHandler(btnRefreshNamespaces_Click);
            pnlNamespacesContent.Controls.Add(btnRefreshNamespaces);

            btnCreateNamespace = new Button();
            btnCreateNamespace.Name = "btnCreateNamespace";
            btnCreateNamespace.Text = "Criar";
            btnCreateNamespace.UseVisualStyleBackColor = true;
            btnCreateNamespace.Click += new EventHandler(btnCreateNamespace_Click);
            pnlNamespacesContent.Controls.Add(btnCreateNamespace);

            btnDeleteNamespace = new Button();
            btnDeleteNamespace.Name = "btnDeleteNamespace";
            btnDeleteNamespace.Text = "Eliminar";
            btnDeleteNamespace.UseVisualStyleBackColor = true;
            btnDeleteNamespace.Click += new EventHandler(btnDeleteNamespace_Click);
            pnlNamespacesContent.Controls.Add(btnDeleteNamespace);

            dgvNamespaces = new DataGridView();
            dgvNamespaces.Name = "dgvNamespaces";
            dgvNamespaces.ReadOnly = true;
            dgvNamespaces.AllowUserToAddRows = false;
            dgvNamespaces.AllowUserToDeleteRows = false;
            dgvNamespaces.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNamespaces.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNamespaces.ScrollBars = ScrollBars.Both;
            dgvNamespaces.RowHeadersVisible = false;
            pnlNamespacesContent.Controls.Add(dgvNamespaces);

            tabNamespaceDetails = new TabControl();
            tabNamespaceDetails.Name = "tabNamespaceDetails";

            dgvNamespaceSummary = CreateDetailsGrid("dgvNamespaceSummary");
            dgvNamespaceLabels = CreateDetailsGrid("dgvNamespaceLabels");
            dgvNamespaceFinalizers = CreateDetailsGrid("dgvNamespaceFinalizers");
            dgvNamespaceManagedFields = CreateDetailsGrid("dgvNamespaceManagedFields");

            AddDetailsTab(tabNamespaceDetails, "Resumo", dgvNamespaceSummary);
            AddDetailsTab(tabNamespaceDetails, "Labels", dgvNamespaceLabels);
            AddDetailsTab(tabNamespaceDetails, "Finalizers", dgvNamespaceFinalizers);
            AddDetailsTab(tabNamespaceDetails, "Managed Fields", dgvNamespaceManagedFields);

            pnlNamespacesContent.Controls.Add(tabNamespaceDetails);
        }

        private DataGridView CreateDetailsGrid(string name)
        {
            DataGridView grid = new DataGridView();
            grid.Name = name;
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars = ScrollBars.Both;
            grid.RowHeadersVisible = false;
            return grid;
        }

        private void AddDetailsTab(TabControl tabControl, string title, DataGridView grid)
        {
            TabPage page = new TabPage();
            page.Text = title;
            page.Controls.Add(grid);
            tabControl.TabPages.Add(page);
        }

        private void ArrangeNamespacesLayout()
        {
            if (pnlNamespacesContent == null)
                return;

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlNamespacesContent.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshNamespaces.Location = new Point(margin, 20);
            btnRefreshNamespaces.Size = new Size(190, 35);

            btnCreateNamespace.Location = new Point(margin + contentWidth - 360, 20);
            btnCreateNamespace.Size = new Size(170, 35);

            btnDeleteNamespace.Location = new Point(margin + contentWidth - 170, 20);
            btnDeleteNamespace.Size = new Size(170, 35);

            dgvNamespaces.Location = new Point(margin, 70);
            dgvNamespaces.Size = new Size(contentWidth, 250);

            tabNamespaceDetails.Location = new Point(margin, dgvNamespaces.Bottom + gap);
            tabNamespaceDetails.Size = new Size(contentWidth, Math.Max(360, pnlNamespacesContent.ClientSize.Height - tabNamespaceDetails.Top - margin));

            pnlNamespacesContent.AutoScrollMinSize = new Size(contentWidth + (margin * 2), tabNamespaceDetails.Bottom + margin);
        }

        private void ConfigureNodeDetailsControls()
        {
            if (tabNodeDetails != null)
                return;

            if (pnlNodes.Controls.Contains(txtNodesRawJson))
                pnlNodes.Controls.Remove(txtNodesRawJson);

            btnToggleNodeDetails = new Button();
            btnToggleNodeDetails.Name = "btnToggleNodeDetails";
            btnToggleNodeDetails.Text = "Mais detalhes";
            btnToggleNodeDetails.UseVisualStyleBackColor = true;
            btnToggleNodeDetails.Click += new EventHandler(btnToggleNodeDetails_Click);
            pnlNodes.Controls.Add(btnToggleNodeDetails);

            tabNodeDetails = new TabControl();
            tabNodeDetails.Name = "tabNodeDetails";
            tabNodeDetails.Visible = nodeDetailsVisible;

            dgvNodeSummary = CreateNodeDetailsGrid("dgvNodeSummary");
            dgvNodeResources = CreateNodeDetailsGrid("dgvNodeResources");
            dgvNodeConditions = CreateNodeDetailsGrid("dgvNodeConditions");
            dgvNodeNetwork = CreateNodeDetailsGrid("dgvNodeNetwork");
            dgvNodeSystem = CreateNodeDetailsGrid("dgvNodeSystem");
            dgvNodeLabels = CreateNodeDetailsGrid("dgvNodeLabels");
            dgvNodeAnnotations = CreateNodeDetailsGrid("dgvNodeAnnotations");
            dgvNodeTaints = CreateNodeDetailsGrid("dgvNodeTaints");
            dgvNodeImages = CreateNodeDetailsGrid("dgvNodeImages");

            AddNodeDetailsTab("Resumo", dgvNodeSummary);
            AddNodeDetailsTab("Recursos", dgvNodeResources);
            AddNodeDetailsTab("Condições", dgvNodeConditions);
            AddNodeDetailsTab("Rede / K3s", dgvNodeNetwork);
            AddNodeDetailsTab("Sistema", dgvNodeSystem);
            AddNodeDetailsTab("Labels", dgvNodeLabels);
            AddNodeDetailsTab("Annotations", dgvNodeAnnotations);
            AddNodeDetailsTab("Taints", dgvNodeTaints);
            AddNodeDetailsTab("Imagens", dgvNodeImages);

            pnlNodes.Controls.Add(tabNodeDetails);
        }

        private DataGridView CreateNodeDetailsGrid(string name)
        {
            DataGridView grid = new DataGridView();
            grid.Name = name;
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars = ScrollBars.Both;
            grid.RowHeadersVisible = false;
            return grid;
        }

        private void AddNodeDetailsTab(string title, DataGridView grid)
        {
            TabPage page = new TabPage();
            page.Text = title;
            page.Controls.Add(grid);
            tabNodeDetails.TabPages.Add(page);
        }

        private List<Panel> GetDashboardCards()
        {
            return new List<Panel>
            {
                cardTotalNodes,
                cardReadyNodes,
                cardNotReadyNodes,
                cardCpuTotal,
                cardMemoryTotal,
                cardPodCapacity,
                cardNamespaces,
                cardPods,
                cardRunningPods,
                cardDeployments,
                cardActiveDeployments,
                cardServices,
                cardIngresses,
                cardMasterIp,
                cardTopImage
            };
        }

        private int GetDashboardCardSpan(Panel card, int cardColumns)
        {
            if (card == cardTopImage)
            {
                if (cardColumns >= 4)
                    return 3;

                if (cardColumns >= 2)
                    return 2;

                return 1;
            }

            return 1;
        }

        private List<Chart> GetDashboardCharts()
        {
            return new List<Chart>
            {
                chartPodsByNamespace,
                chartNodesStatus,
                chartCpuByNode,
                chartPodStatus,
                chartDeploymentsByNamespace,
                chartDeploymentStatus,
                chartServicesByNamespace,
                chartKubeletVersions,
                chartMemoryByNode,
                chartImages
            };
        }

        private void btnToggleNodeDetails_Click(object sender, EventArgs e)
        {
            nodeDetailsVisible = !nodeDetailsVisible;

            if (tabNodeDetails != null)
                tabNodeDetails.Visible = nodeDetailsVisible;

            if (btnToggleNodeDetails != null)
                btnToggleNodeDetails.Text = nodeDetailsVisible ? "Menos detalhes" : "Mais detalhes";

            ArrangeNodesLayout();

            // Se os detalhes forem fechados enquanto o utilizador está mais abaixo no scroll,
            // o AutoScroll mantém essa posição antiga e deixa uma área vazia por cima.
            // Ao esconder os detalhes, voltamos automaticamente ao topo da aba Nodes.
            if (!nodeDetailsVisible)
            {
                ResetNodesScroll();
                ArrangeNodesLayout();
            }
        }

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            if (api == null || nodesService == null || dashboardService == null)
                return;

            lblConnectionInfo.Text = "Ligado a: " + baseUrl;

            try
            {
                await api.GetAsync("/version");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao testar ligação ao cluster.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            // Cada área é carregada de forma independente. Assim, se um pedido falhar
            // temporariamente, as outras abas continuam a aparecer e podem ser atualizadas
            // pelo respetivo botão.
            await TryLoadSectionAsync("dashboard", async () =>
            {
                ResetDashboardScroll();
                await LoadDashboardAsync();
                ResetDashboardScroll();
                ArrangeDashboardLayout();
            });

            await TryLoadSectionAsync("nodes", async () =>
            {
                await LoadNodesTabAsync();
                ArrangeNodesLayout();
            });

            await TryLoadSectionAsync("namespaces", async () =>
            {
                await LoadNamespacesTabAsync();
                ArrangeNamespacesLayout();
            });

            await TryLoadSectionAsync("pods", async () =>
            {
                ConfigurePodsTabControls();
                await LoadPodsTabAsync();
                ArrangePodsLayout();
            });

            await TryLoadSectionAsync("deployments", async () =>
            {
                ConfigureDeploymentTabControls();
                await LoadDeploymentsTabAsync();
                ArrangeDeploymentsLayout();
            });
        }

        private async Task TryLoadSectionAsync(string sectionName, Func<Task> loadAction)
        {
            try
            {
                await loadAction();
            }
            catch (Exception ex)
            {
                // Não bloqueia o arranque da aplicação. O utilizador pode voltar a tentar
                // através do botão Atualizar da respetiva aba.
                Console.WriteLine("Erro ao carregar " + sectionName + ": " + ex.Message);
            }
        }

        private async Task LoadDashboardAsync()
        {
            KubernetesDashboardSummary summary = await dashboardService.GetDashboardSummaryAsync();

            lblTotalNodesValue.Text = summary.TotalNodes.ToString();
            lblReadyNodesValue.Text = summary.ReadyNodes.ToString();
            lblNotReadyNodesValue.Text = summary.NotReadyNodes.ToString();
            lblCpuTotalValue.Text = summary.TotalCpu.ToString();
            lblMemoryTotalValue.Text = summary.TotalMemoryGiB.ToString("F2") + " GiB";
            lblPodCapacityValue.Text = summary.TotalPodCapacity.ToString();
            lblMasterIpValue.Text = summary.MasterIp;

            lblTotalNamespacesValue.Text = summary.TotalNamespaces.ToString();
            lblTotalPodsValue.Text = summary.TotalPods.ToString();
            lblRunningPodsValue.Text = summary.RunningPods.ToString();
            lblTotalDeploymentsValue.Text = summary.TotalDeployments.ToString();
            lblActiveDeploymentsValue.Text = summary.ActiveDeployments.ToString();
            lblTotalServicesValue.Text = summary.TotalServices.ToString();
            lblTotalIngressesValue.Text = summary.TotalIngresses.ToString();
            lblTopImageValue.Text = summary.TopImage;
            lblTopImageValue.AutoSize = false;
            lblTopImageValue.TextAlign = ContentAlignment.MiddleLeft;
            lblTopImageValue.Text = summary.TopImage;

            LoadPieChart(chartNodesStatus, "Estado dos Nodes", summary.NodesStatus);
            LoadColumnChart(chartPodsByNamespace, "Pods por Namespace", summary.PodsByNamespace);
            LoadPieChart(chartCpuByNode, "Distribuição de CPU por Node", summary.CpuByNode);
            LoadColumnChart(chartMemoryByNode, "Memória por Node (GiB)", summary.MemoryByNode);
            LoadPieChart(chartPodStatus, "Estado dos Pods", summary.PodStatus);
            LoadColumnChart(chartDeploymentsByNamespace, "Deployments por Namespace", summary.DeploymentsByNamespace);
            ForceIntegerYAxis(chartDeploymentsByNamespace);

            LoadPieChart(chartDeploymentStatus, "Estado dos Deployments", summary.DeploymentStatus);

            LoadColumnChart(chartServicesByNamespace, "Services por Namespace", summary.ServicesByNamespace);
            ForceIntegerYAxis(chartServicesByNamespace);

            LoadColumnChart(chartKubeletVersions, "Distribuição das Versões do Kubelet", summary.KubeletVersions);
            ForceIntegerYAxis(chartKubeletVersions);
            LoadBarChart(chartImages, "Imagens encontradas", summary.ImagesCount, 8);
            ForceIntegerYAxis(chartImages);
        }


        private async Task LoadNamespacesTabAsync()
        {
            if (namespacesService == null)
                return;

            namespaceDetails = await namespacesService.GetNamespaceDetailsAsync();

            List<KubernetesNamespaceSummary> namespaces = namespaceDetails.Select(ns => new KubernetesNamespaceSummary
            {
                Name = ns.Name,
                Status = ns.Phase,
                CreatedAt = ns.CreationTimestamp,
                ResourceVersion = ns.ResourceVersion,
                Uid = ns.Uid,
                Labels = ns.Labels.Count,
                Finalizers = ns.FinalizersText,
                ManagedBy = ns.ManagedBy
            }).ToList();

            dgvNamespaces.DataSource = null;
            dgvNamespaces.DataSource = namespaces;
            ConfigureNamespacesGridHeaders();

            if (dgvNamespaces.Rows.Count > 0)
                dgvNamespaces.Rows[0].Selected = true;

            if (namespaceDetails.Count > 0)
                ShowNamespaceDetails(namespaceDetails[0]);
            else
                ClearNamespaceDetails();

            ArrangeNamespacesLayout();
        }

        private void ConfigureNamespacesGridHeaders()
        {
            if (dgvNamespaces.Columns["Name"] != null)
                dgvNamespaces.Columns["Name"].HeaderText = "Nome";

            if (dgvNamespaces.Columns["Status"] != null)
                dgvNamespaces.Columns["Status"].HeaderText = "Estado";

            if (dgvNamespaces.Columns["CreatedAt"] != null)
                dgvNamespaces.Columns["CreatedAt"].HeaderText = "Criado em";

            if (dgvNamespaces.Columns["ResourceVersion"] != null)
                dgvNamespaces.Columns["ResourceVersion"].HeaderText = "Resource Version";

            if (dgvNamespaces.Columns["Uid"] != null)
                dgvNamespaces.Columns["Uid"].HeaderText = "UID";

            if (dgvNamespaces.Columns["Labels"] != null)
                dgvNamespaces.Columns["Labels"].HeaderText = "Labels";

            if (dgvNamespaces.Columns["Finalizers"] != null)
                dgvNamespaces.Columns["Finalizers"].HeaderText = "Finalizers";

            if (dgvNamespaces.Columns["ManagedBy"] != null)
                dgvNamespaces.Columns["ManagedBy"].HeaderText = "Gerido por";
        }

        private void dgvNamespaces_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNamespaces == null || dgvNamespaces.CurrentRow == null || dgvNamespaces.CurrentRow.DataBoundItem == null)
                return;

            KubernetesNamespaceSummary selectedNamespace = dgvNamespaces.CurrentRow.DataBoundItem as KubernetesNamespaceSummary;
            if (selectedNamespace == null || namespaceDetails == null)
                return;

            KubernetesNamespaceDetails selectedDetails = namespaceDetails.FirstOrDefault(n => n.Name == selectedNamespace.Name);
            if (selectedDetails != null)
                ShowNamespaceDetails(selectedDetails);
        }

        private void ShowNamespaceDetails(KubernetesNamespaceDetails ns)
        {
            if (ns == null)
            {
                ClearNamespaceDetails();
                return;
            }

            dgvNamespaceSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", ns.Name },
                { "Estado", ns.Phase },
                { "UID", ns.Uid },
                { "Criado em", ns.CreationTimestamp },
                { "Resource Version", ns.ResourceVersion },
                { "Finalizers", ns.FinalizersText },
                { "Gerido por", ns.ManagedBy }
            });

            dgvNamespaceLabels.DataSource = null;
            dgvNamespaceLabels.DataSource = ns.Labels;

            dgvNamespaceFinalizers.DataSource = null;
            dgvNamespaceFinalizers.DataSource = ns.Finalizers;

            dgvNamespaceManagedFields.DataSource = null;
            dgvNamespaceManagedFields.DataSource = ns.ManagedFields;
        }

        private void ClearNamespaceDetails()
        {
            if (dgvNamespaceSummary != null)
                dgvNamespaceSummary.DataSource = null;

            if (dgvNamespaceLabels != null)
                dgvNamespaceLabels.DataSource = null;

            if (dgvNamespaceFinalizers != null)
                dgvNamespaceFinalizers.DataSource = null;

            if (dgvNamespaceManagedFields != null)
                dgvNamespaceManagedFields.DataSource = null;
        }

        private async Task LoadNodesTabAsync()
        {
            if (nodesService == null)
                return;

            List<KubernetesNodeSummary> nodes = await nodesService.GetNodesAsync();
            nodeDetails = await nodesService.GetNodeDetailsAsync();

            dgvNodes.DataSource = null;
            dgvNodes.DataSource = nodes;
            ConfigureNodesGridHeaders();

            if (dgvNodes.Rows.Count > 0)
                dgvNodes.Rows[0].Selected = true;

            if (nodeDetails.Count > 0)
                ShowNodeDetails(nodeDetails[0]);
            else
                ClearNodeDetails();

            ArrangeNodesLayout();
        }

        private void ConfigureNodesGridHeaders()
        {
            if (dgvNodes.Columns["MemoryGb"] != null)
                dgvNodes.Columns["MemoryGb"].HeaderText = "Memória (GiB)";

            if (dgvNodes.Columns["InternalIp"] != null)
                dgvNodes.Columns["InternalIp"].HeaderText = "IP Interno";

            if (dgvNodes.Columns["PodCapacity"] != null)
                dgvNodes.Columns["PodCapacity"].HeaderText = "Capacidade de Pods";

            if (dgvNodes.Columns["OsImage"] != null)
                dgvNodes.Columns["OsImage"].HeaderText = "Imagem do SO";

            if (dgvNodes.Columns["KubeletVersion"] != null)
                dgvNodes.Columns["KubeletVersion"].HeaderText = "Versão do Kubelet";

            if (dgvNodes.Columns["ContainerRuntime"] != null)
                dgvNodes.Columns["ContainerRuntime"].HeaderText = "Runtime";
        }

        private void dgvNodes_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvNodes.CurrentRow == null || dgvNodes.CurrentRow.DataBoundItem == null)
                return;

            KubernetesNodeSummary selectedNode = dgvNodes.CurrentRow.DataBoundItem as KubernetesNodeSummary;
            if (selectedNode == null || nodeDetails == null)
                return;

            KubernetesNodeDetails selectedDetails = nodeDetails.FirstOrDefault(n => n.Name == selectedNode.Name);
            if (selectedDetails != null)
                ShowNodeDetails(selectedDetails);
        }

        private void ShowNodeDetails(KubernetesNodeDetails node)
        {
            if (node == null)
            {
                ClearNodeDetails();
                return;
            }

            dgvNodeSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", node.Name },
                { "Role", node.Role },
                { "Estado", node.Status },
                { "IP Interno", node.InternalIp },
                { "Hostname", node.Hostname },
                { "Criado em", node.CreationTimestamp },
                { "Resource Version", node.ResourceVersion },
                { "Provider ID", node.ProviderId }
            });

            dgvNodeResources.DataSource = ToTable(new Dictionary<string, string>
            {
                { "CPU Capacity", node.CpuCapacity },
                { "CPU Allocatable", node.CpuAllocatable },
                { "Memória Capacity", node.MemoryCapacityGiB + " GiB" },
                { "Memória Allocatable", node.MemoryAllocatableGiB + " GiB" },
                { "Pods Capacity", node.PodCapacity },
                { "Pods Allocatable", node.PodAllocatable },
                { "Storage Capacity", node.StorageCapacityGiB + " GiB" },
                { "Storage Allocatable", node.StorageAllocatableGiB + " GiB" }
            });

            dgvNodeNetwork.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Pod CIDR", node.PodCIDR },
                { "Pod CIDRs", node.PodCIDRs },
                { "Kubelet Port", node.KubeletPort },
                { "Flannel Public IP", node.FlannelPublicIp },
                { "Flannel Backend", node.FlannelBackendType },
                { "K3s Internal IP", node.K3sInternalIp },
                { "Node Args", node.NodeArgs }
            });

            dgvNodeSystem.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Sistema Operativo", node.OsImage },
                { "Kernel", node.KernelVersion },
                { "Container Runtime", node.ContainerRuntime },
                { "Kubelet", node.KubeletVersion },
                { "Sistema", node.OperatingSystem },
                { "Arquitetura", node.Architecture },
                { "Machine ID", node.MachineID },
                { "System UUID", node.SystemUUID },
                { "Boot ID", node.BootID },
                { "Swap", node.SwapGiB + " GiB" }
            });

            dgvNodeConditions.DataSource = null;
            dgvNodeConditions.DataSource = node.Conditions;

            dgvNodeLabels.DataSource = null;
            dgvNodeLabels.DataSource = node.Labels;

            dgvNodeAnnotations.DataSource = null;
            dgvNodeAnnotations.DataSource = node.Annotations;

            dgvNodeTaints.DataSource = null;
            dgvNodeTaints.DataSource = node.Taints;

            dgvNodeImages.DataSource = null;
            dgvNodeImages.DataSource = node.Images;
        }

        private void ClearNodeDetails()
        {
            dgvNodeSummary.DataSource = null;
            dgvNodeResources.DataSource = null;
            dgvNodeConditions.DataSource = null;
            dgvNodeNetwork.DataSource = null;
            dgvNodeSystem.DataSource = null;
            dgvNodeLabels.DataSource = null;
            dgvNodeAnnotations.DataSource = null;
            dgvNodeTaints.DataSource = null;
            dgvNodeImages.DataSource = null;
        }

        private DataTable ToTable(Dictionary<string, string> values)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Campo");
            table.Columns.Add("Valor");

            foreach (KeyValuePair<string, string> item in values)
                table.Rows.Add(item.Key, item.Value);

            return table;
        }

        private string FormatJson(string json)
        {
            using (JsonDocument document = JsonDocument.Parse(json))
            {
                return JsonSerializer.Serialize(
                    document.RootElement,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                );
            }
        }

        private void LoadPieChart(Chart chart, string title, Dictionary<string, int> data)
        {
            PrepareChart(chart, title);

            Series series = new Series(title)
            {
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true
            };

            if (data == null || data.Count == 0)
            {
                series.Points.AddXY("Sem dados", 0);
            }
            else
            {
                foreach (KeyValuePair<string, int> item in data.OrderBy(i => i.Key))
                    series.Points.AddXY(item.Key, item.Value);
            }

            chart.Series.Add(series);
            chart.Legends.Add(new Legend("Legenda"));
        }

        private void LoadColumnChart(Chart chart, string title, Dictionary<string, int> data)
        {
            PrepareChart(chart, title);

            Series series = new Series(title)
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            if (data == null || data.Count == 0)
            {
                series.Points.AddXY("Sem dados", 0);
            }
            else
            {
                foreach (KeyValuePair<string, int> item in data.OrderBy(i => i.Key))
                    series.Points.AddXY(item.Key, item.Value);
            }

            chart.Series.Add(series);
        }

        private void LoadColumnChart(Chart chart, string title, Dictionary<string, double> data)
        {
            PrepareChart(chart, title);

            Series series = new Series(title)
            {
                ChartType = SeriesChartType.Column,
                IsValueShownAsLabel = true
            };

            if (data == null || data.Count == 0)
            {
                series.Points.AddXY("Sem dados", 0);
            }
            else
            {
                foreach (KeyValuePair<string, double> item in data.OrderBy(i => i.Key))
                    series.Points.AddXY(item.Key, item.Value);
            }

            chart.Series.Add(series);
        }

        private void LoadBarChart(Chart chart, string title, Dictionary<string, int> data, int maxItems)
        {
            PrepareChart(chart, title);

            Series series = new Series(title)
            {
                ChartType = SeriesChartType.Bar,
                IsValueShownAsLabel = true
            };

            if (data == null || data.Count == 0)
            {
                series.Points.AddXY("Sem dados", 0);
            }
            else
            {
                foreach (KeyValuePair<string, int> item in data.OrderByDescending(i => i.Value).Take(maxItems))
                    series.Points.AddXY(ShortenLabel(item.Key, 45), item.Value);
            }

            chart.Series.Add(series);
        }

        private void PrepareChart(Chart chart, string title)
        {
            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.Titles.Clear();
            chart.Legends.Clear();

            ChartArea area = new ChartArea("MainArea");
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisY.MajorGrid.Enabled = true;

            chart.ChartAreas.Add(area);
            chart.Titles.Add(title);
        }

        private void ForceIntegerYAxis(Chart chart)
        {
            if (chart.ChartAreas.Count == 0)
                return;

            ChartArea area = chart.ChartAreas[0];

            area.AxisY.Minimum = 0;
            area.AxisY.Interval = 1;
            area.AxisY.LabelStyle.Format = "0";
            area.AxisY.MajorGrid.Interval = 1;
        }

        private string ShortenLabel(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "N/A";

            if (value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }


        private async void btnRefreshNamespaces_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshNamespaces.Enabled = false;
                await LoadNamespacesTabAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao atualizar namespaces.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRefreshNamespaces.Enabled = true;
            }
        }

        private async void btnCreateNamespace_Click(object sender, EventArgs e)
        {
            if (namespacesService == null)
                return;

            using (CreateNamespaceForm form = new CreateNamespaceForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    btnCreateNamespace.Enabled = false;
                    await namespacesService.CreateNamespaceAsync(form.NamespaceName, form.LabelsText);
                    MessageBox.Show("Namespace criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await LoadNamespacesTabAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Erro ao criar namespace.\n\n" + ex.Message,
                        "Erro",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
                finally
                {
                    btnCreateNamespace.Enabled = true;
                }
            }
        }

        private async void btnDeleteNamespace_Click(object sender, EventArgs e)
        {
            if (namespacesService == null || dgvNamespaces == null || dgvNamespaces.CurrentRow == null)
                return;

            KubernetesNamespaceSummary selectedNamespace = dgvNamespaces.CurrentRow.DataBoundItem as KubernetesNamespaceSummary;
            if (selectedNamespace == null)
                return;

            DialogResult confirm = MessageBox.Show(
                "Tem a certeza que pretende eliminar o namespace '" + selectedNamespace.Name + "'?",
                "Confirmar eliminação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnDeleteNamespace.Enabled = false;
                await namespacesService.DeleteNamespaceAsync(selectedNamespace.Name);
                MessageBox.Show("Pedido de eliminação enviado com sucesso. O namespace pode ficar alguns segundos em Terminating.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Delay(2000);
                await LoadNamespacesTabAsync();
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao eliminar namespace.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnDeleteNamespace.Enabled = true;
            }
        }

        private async void btnRefreshDashboard_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshDashboard.Enabled = false;
                ResetDashboardScroll();
                await LoadDashboardAsync();
                ResetDashboardScroll();
                ArrangeDashboardLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao atualizar dashboard.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRefreshDashboard.Enabled = true;
            }
        }

        private async void btnRefreshNodes_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshNodes.Enabled = false;
                await LoadNodesTabAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao atualizar nodes.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                btnRefreshNodes.Enabled = true;
            }
        }
    }
}
