using KubernetesController.Models;
using KubernetesController.Services;
using System;
using System.Collections.Generic;
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
        private bool layoutEventsConfigured;

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

            txtNodesRawJson.Multiline = true;
            txtNodesRawJson.ReadOnly = true;
            txtNodesRawJson.ScrollBars = ScrollBars.Both;
            txtNodesRawJson.WordWrap = false;

            lblTopImageValue.AutoSize = false;
            lblTopImageValue.AutoEllipsis = false;
            lblTopImageValue.TextAlign = ContentAlignment.MiddleLeft;

            if (!layoutEventsConfigured)
            {
                layoutEventsConfigured = true;
                this.Resize += DashboardForm_Resize;
                tabKubernetesController.SelectedIndexChanged += TabKubernetesController_SelectedIndexChanged;
            }

            ArrangeDashboardLayout();
            ArrangeNodesLayout();
        }

        private void DashboardForm_Resize(object sender, EventArgs e)
        {
            ArrangeDashboardLayout();
            ArrangeNodesLayout();
        }

        private void TabKubernetesController_SelectedIndexChanged(object sender, EventArgs e)
        {
            ArrangeDashboardLayout();
            ArrangeNodesLayout();
        }

        private void ResetDashboardScroll()
        {
            if (pnlDashboard == null)
                return;

            // Evita que, depois de atualizar, o AutoScroll mantenha uma posição antiga
            // e empurre o conteúdo para baixo, criando espaço vazio no topo.
            pnlDashboard.AutoScrollPosition = new Point(0, 0);
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
            int contentWidth = Math.Max(780, pnlNodes.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshNodes.Location = new Point(margin, 20);
            btnRefreshNodes.Size = new Size(160, 35);

            dgvNodes.Location = new Point(margin, 70);
            dgvNodes.Size = new Size(contentWidth, 280);

            txtNodesRawJson.Location = new Point(margin, 380);
            txtNodesRawJson.Size = new Size(contentWidth, Math.Max(420, pnlNodes.ClientSize.Height - 430));

            pnlNodes.AutoScrollMinSize = new Size(contentWidth + (margin * 2), txtNodesRawJson.Bottom + margin);
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

        private async void DashboardForm_Load(object sender, EventArgs e)
        {
            if (api == null || nodesService == null || dashboardService == null)
                return;

            try
            {
                lblConnectionInfo.Text = "Ligado a: " + baseUrl;

                await api.GetAsync("/version");
                ResetDashboardScroll();
                await LoadDashboardAsync();
                ResetDashboardScroll();
                ArrangeDashboardLayout();

                await LoadNodesTabAsync();
                ArrangeNodesLayout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao carregar dados do cluster.\n\n" + ex.Message,
                    "Erro",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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

        private async Task LoadNodesTabAsync()
        {
            if (nodesService == null)
                return;

            List<KubernetesNodeSummary> nodes = await nodesService.GetNodesAsync();

            dgvNodes.DataSource = null;
            dgvNodes.DataSource = nodes;
            ConfigureNodesGridHeaders();

            string rawJson = await nodesService.GetNodesRawJsonAsync();
            txtNodesRawJson.Text = FormatJson(rawJson);

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
