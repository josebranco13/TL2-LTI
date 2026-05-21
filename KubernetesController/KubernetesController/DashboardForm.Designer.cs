namespace KubernetesController
{
    partial class DashboardForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tabKubernetesController = new System.Windows.Forms.TabControl();
            this.tabDashboard = new System.Windows.Forms.TabPage();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.btnRefreshDashboard = new System.Windows.Forms.Button();
            this.lblConnectionInfo = new System.Windows.Forms.Label();
            this.lblDashboardTitle = new System.Windows.Forms.Label();
            this.cardTotalNodes = new System.Windows.Forms.Panel();
            this.lblTotalNodesTitle = new System.Windows.Forms.Label();
            this.lblTotalNodesValue = new System.Windows.Forms.Label();
            this.cardReadyNodes = new System.Windows.Forms.Panel();
            this.lblReadyNodesTitle = new System.Windows.Forms.Label();
            this.lblReadyNodesValue = new System.Windows.Forms.Label();
            this.cardNotReadyNodes = new System.Windows.Forms.Panel();
            this.lblNotReadyNodesTitle = new System.Windows.Forms.Label();
            this.lblNotReadyNodesValue = new System.Windows.Forms.Label();
            this.cardCpuTotal = new System.Windows.Forms.Panel();
            this.lblCpuTotalTitle = new System.Windows.Forms.Label();
            this.lblCpuTotalValue = new System.Windows.Forms.Label();
            this.cardMemoryTotal = new System.Windows.Forms.Panel();
            this.lblMemoryTotalTitle = new System.Windows.Forms.Label();
            this.lblMemoryTotalValue = new System.Windows.Forms.Label();
            this.cardPodCapacity = new System.Windows.Forms.Panel();
            this.lblPodCapacityTitle = new System.Windows.Forms.Label();
            this.lblPodCapacityValue = new System.Windows.Forms.Label();
            this.cardNamespaces = new System.Windows.Forms.Panel();
            this.lblTotalNamespacesTitle = new System.Windows.Forms.Label();
            this.lblTotalNamespacesValue = new System.Windows.Forms.Label();
            this.cardPods = new System.Windows.Forms.Panel();
            this.lblTotalPodsTitle = new System.Windows.Forms.Label();
            this.lblTotalPodsValue = new System.Windows.Forms.Label();
            this.cardRunningPods = new System.Windows.Forms.Panel();
            this.lblRunningPodsTitle = new System.Windows.Forms.Label();
            this.lblRunningPodsValue = new System.Windows.Forms.Label();
            this.cardDeployments = new System.Windows.Forms.Panel();
            this.lblTotalDeploymentsTitle = new System.Windows.Forms.Label();
            this.lblTotalDeploymentsValue = new System.Windows.Forms.Label();
            this.cardActiveDeployments = new System.Windows.Forms.Panel();
            this.lblActiveDeploymentsTitle = new System.Windows.Forms.Label();
            this.lblActiveDeploymentsValue = new System.Windows.Forms.Label();
            this.cardServices = new System.Windows.Forms.Panel();
            this.lblTotalServicesTitle = new System.Windows.Forms.Label();
            this.lblTotalServicesValue = new System.Windows.Forms.Label();
            this.cardIngresses = new System.Windows.Forms.Panel();
            this.lblTotalIngressesTitle = new System.Windows.Forms.Label();
            this.lblTotalIngressesValue = new System.Windows.Forms.Label();
            this.cardMasterIp = new System.Windows.Forms.Panel();
            this.lblMasterIpTitle = new System.Windows.Forms.Label();
            this.lblMasterIpValue = new System.Windows.Forms.Label();
            this.cardTopImage = new System.Windows.Forms.Panel();
            this.lblTopImageTitle = new System.Windows.Forms.Label();
            this.lblTopImageValue = new System.Windows.Forms.Label();
            this.chartPodsByNamespace = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartNodesStatus = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartCpuByNode = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartPodStatus = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartDeploymentsByNamespace = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartDeploymentStatus = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartServicesByNamespace = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartKubeletVersions = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartMemoryByNode = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.chartImages = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.tabNodes = new System.Windows.Forms.TabPage();
            this.pnlNodes = new System.Windows.Forms.Panel();
            this.txtNodesRawJson = new System.Windows.Forms.TextBox();
            this.dgvNodes = new System.Windows.Forms.DataGridView();
            this.btnRefreshNodes = new System.Windows.Forms.Button();
            this.tabNamespaces = new System.Windows.Forms.TabPage();
            this.tabPods = new System.Windows.Forms.TabPage();
            this.tabDeployments = new System.Windows.Forms.TabPage();
            this.tabServices = new System.Windows.Forms.TabPage();
            this.tabKubernetesController.SuspendLayout();
            this.tabDashboard.SuspendLayout();
            this.pnlDashboard.SuspendLayout();
            this.cardTotalNodes.SuspendLayout();
            this.cardReadyNodes.SuspendLayout();
            this.cardNotReadyNodes.SuspendLayout();
            this.cardCpuTotal.SuspendLayout();
            this.cardMemoryTotal.SuspendLayout();
            this.cardPodCapacity.SuspendLayout();
            this.cardNamespaces.SuspendLayout();
            this.cardPods.SuspendLayout();
            this.cardRunningPods.SuspendLayout();
            this.cardDeployments.SuspendLayout();
            this.cardActiveDeployments.SuspendLayout();
            this.cardServices.SuspendLayout();
            this.cardIngresses.SuspendLayout();
            this.cardMasterIp.SuspendLayout();
            this.cardTopImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPodsByNamespace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartNodesStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCpuByNode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPodStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartDeploymentsByNamespace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartDeploymentStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartServicesByNamespace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartKubeletVersions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartMemoryByNode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartImages)).BeginInit();
            this.tabNodes.SuspendLayout();
            this.pnlNodes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNodes)).BeginInit();
            this.SuspendLayout();
            // 
            // tabKubernetesController
            // 
            this.tabKubernetesController.Controls.Add(this.tabDashboard);
            this.tabKubernetesController.Controls.Add(this.tabNodes);
            this.tabKubernetesController.Controls.Add(this.tabNamespaces);
            this.tabKubernetesController.Controls.Add(this.tabPods);
            this.tabKubernetesController.Controls.Add(this.tabDeployments);
            this.tabKubernetesController.Controls.Add(this.tabServices);
            this.tabKubernetesController.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabKubernetesController.Location = new System.Drawing.Point(0, 0);
            this.tabKubernetesController.Name = "tabKubernetesController";
            this.tabKubernetesController.SelectedIndex = 0;
            this.tabKubernetesController.Size = new System.Drawing.Size(1400, 850);
            this.tabKubernetesController.TabIndex = 0;
            // 
            // tabDashboard
            // 
            this.tabDashboard.Controls.Add(this.pnlDashboard);
            this.tabDashboard.Location = new System.Drawing.Point(4, 29);
            this.tabDashboard.Name = "tabDashboard";
            this.tabDashboard.Padding = new System.Windows.Forms.Padding(3);
            this.tabDashboard.Size = new System.Drawing.Size(1392, 817);
            this.tabDashboard.TabIndex = 0;
            this.tabDashboard.Text = "Dashboard";
            this.tabDashboard.UseVisualStyleBackColor = true;
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.AutoScroll = true;
            this.pnlDashboard.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(235)))), ((int)(((byte)(235)))));
            this.pnlDashboard.Controls.Add(this.btnRefreshDashboard);
            this.pnlDashboard.Controls.Add(this.lblConnectionInfo);
            this.pnlDashboard.Controls.Add(this.lblDashboardTitle);
            this.pnlDashboard.Controls.Add(this.cardTotalNodes);
            this.pnlDashboard.Controls.Add(this.cardReadyNodes);
            this.pnlDashboard.Controls.Add(this.cardNotReadyNodes);
            this.pnlDashboard.Controls.Add(this.cardCpuTotal);
            this.pnlDashboard.Controls.Add(this.cardMemoryTotal);
            this.pnlDashboard.Controls.Add(this.cardPodCapacity);
            this.pnlDashboard.Controls.Add(this.cardNamespaces);
            this.pnlDashboard.Controls.Add(this.cardPods);
            this.pnlDashboard.Controls.Add(this.cardRunningPods);
            this.pnlDashboard.Controls.Add(this.cardDeployments);
            this.pnlDashboard.Controls.Add(this.cardActiveDeployments);
            this.pnlDashboard.Controls.Add(this.cardServices);
            this.pnlDashboard.Controls.Add(this.cardIngresses);
            this.pnlDashboard.Controls.Add(this.cardMasterIp);
            this.pnlDashboard.Controls.Add(this.cardTopImage);
            this.pnlDashboard.Controls.Add(this.chartPodsByNamespace);
            this.pnlDashboard.Controls.Add(this.chartNodesStatus);
            this.pnlDashboard.Controls.Add(this.chartCpuByNode);
            this.pnlDashboard.Controls.Add(this.chartPodStatus);
            this.pnlDashboard.Controls.Add(this.chartDeploymentsByNamespace);
            this.pnlDashboard.Controls.Add(this.chartDeploymentStatus);
            this.pnlDashboard.Controls.Add(this.chartServicesByNamespace);
            this.pnlDashboard.Controls.Add(this.chartKubeletVersions);
            this.pnlDashboard.Controls.Add(this.chartMemoryByNode);
            this.pnlDashboard.Controls.Add(this.chartImages);
            this.pnlDashboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDashboard.Location = new System.Drawing.Point(3, 3);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Size = new System.Drawing.Size(1386, 811);
            this.pnlDashboard.TabIndex = 0;
            // 
            // btnRefreshDashboard
            // 
            this.btnRefreshDashboard.Location = new System.Drawing.Point(1190, 35);
            this.btnRefreshDashboard.Name = "btnRefreshDashboard";
            this.btnRefreshDashboard.Size = new System.Drawing.Size(140, 35);
            this.btnRefreshDashboard.TabIndex = 2;
            this.btnRefreshDashboard.Text = "Atualizar";
            this.btnRefreshDashboard.UseVisualStyleBackColor = true;
            this.btnRefreshDashboard.Click += new System.EventHandler(this.btnRefreshDashboard_Click);
            // 
            // lblConnectionInfo
            // 
            this.lblConnectionInfo.AutoSize = true;
            this.lblConnectionInfo.Location = new System.Drawing.Point(30, 61);
            this.lblConnectionInfo.Name = "lblConnectionInfo";
            this.lblConnectionInfo.Size = new System.Drawing.Size(75, 20);
            this.lblConnectionInfo.TabIndex = 1;
            this.lblConnectionInfo.Text = "Ligado a:";
            // 
            // lblDashboardTitle
            // 
            this.lblDashboardTitle.AutoSize = true;
            this.lblDashboardTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDashboardTitle.Location = new System.Drawing.Point(30, 20);
            this.lblDashboardTitle.Name = "lblDashboardTitle";
            this.lblDashboardTitle.Size = new System.Drawing.Size(436, 32);
            this.lblDashboardTitle.TabIndex = 0;
            this.lblDashboardTitle.Text = "Dashboard Cluster Kubernetes";
            // 
            // Cards
            // 
            this.ConfigureCard(this.cardTotalNodes, this.lblTotalNodesTitle, this.lblTotalNodesValue, "Total de Nodes", "0", 30, 100, 180, 82);
            this.ConfigureCard(this.cardReadyNodes, this.lblReadyNodesTitle, this.lblReadyNodesValue, "Nodes Prontos", "0", 225, 100, 180, 82);
            this.ConfigureCard(this.cardNotReadyNodes, this.lblNotReadyNodesTitle, this.lblNotReadyNodesValue, "Nodes Not Ready", "0", 420, 100, 180, 82);
            this.ConfigureCard(this.cardCpuTotal, this.lblCpuTotalTitle, this.lblCpuTotalValue, "CPU Total (cores)", "0", 615, 100, 180, 82);
            this.ConfigureCard(this.cardMemoryTotal, this.lblMemoryTotalTitle, this.lblMemoryTotalValue, "Memória Total", "0 GiB", 810, 100, 220, 82);
            this.ConfigureCard(this.cardPodCapacity, this.lblPodCapacityTitle, this.lblPodCapacityValue, "Capacidade de Pods", "0", 1045, 100, 220, 82);
            this.ConfigureCard(this.cardNamespaces, this.lblTotalNamespacesTitle, this.lblTotalNamespacesValue, "Total Namespaces", "0", 30, 200, 180, 82);
            this.ConfigureCard(this.cardPods, this.lblTotalPodsTitle, this.lblTotalPodsValue, "Total de Pods", "0", 225, 200, 180, 82);
            this.ConfigureCard(this.cardRunningPods, this.lblRunningPodsTitle, this.lblRunningPodsValue, "Pods em Execução", "0", 420, 200, 180, 82);
            this.ConfigureCard(this.cardDeployments, this.lblTotalDeploymentsTitle, this.lblTotalDeploymentsValue, "Deployments", "0", 615, 200, 180, 82);
            this.ConfigureCard(this.cardActiveDeployments, this.lblActiveDeploymentsTitle, this.lblActiveDeploymentsValue, "Deployments Ativos", "0", 810, 200, 220, 82);
            this.ConfigureCard(this.cardServices, this.lblTotalServicesTitle, this.lblTotalServicesValue, "Total de Services", "0", 1045, 200, 220, 82);
            this.ConfigureCard(this.cardIngresses, this.lblTotalIngressesTitle, this.lblTotalIngressesValue, "Ingress Disponíveis", "0", 30, 300, 180, 82);
            this.ConfigureCard(this.cardMasterIp, this.lblMasterIpTitle, this.lblMasterIpValue, "Master IP", "N/A", 225, 300, 375, 82);
            this.ConfigureCard(this.cardTopImage, this.lblTopImageTitle, this.lblTopImageValue, "Top Imagem", "N/A", 615, 300, 650, 82);
            // 
            // Charts
            // 
            this.ConfigureChartBox(this.chartPodsByNamespace, 30, 420, 600, 320);
            this.ConfigureChartBox(this.chartNodesStatus, 660, 420, 600, 320);
            this.ConfigureChartBox(this.chartCpuByNode, 30, 770, 600, 320);
            this.ConfigureChartBox(this.chartPodStatus, 660, 770, 600, 320);
            this.ConfigureChartBox(this.chartDeploymentsByNamespace, 30, 1120, 600, 320);
            this.ConfigureChartBox(this.chartDeploymentStatus, 660, 1120, 600, 320);
            this.ConfigureChartBox(this.chartServicesByNamespace, 30, 1470, 600, 320);
            this.ConfigureChartBox(this.chartKubeletVersions, 660, 1470, 600, 320);
            this.ConfigureChartBox(this.chartMemoryByNode, 30, 1820, 600, 320);
            this.ConfigureChartBox(this.chartImages, 660, 1820, 600, 320);
            // 
            // tabNodes
            // 
            this.tabNodes.Controls.Add(this.pnlNodes);
            this.tabNodes.Location = new System.Drawing.Point(4, 29);
            this.tabNodes.Name = "tabNodes";
            this.tabNodes.Padding = new System.Windows.Forms.Padding(3);
            this.tabNodes.Size = new System.Drawing.Size(1392, 817);
            this.tabNodes.TabIndex = 1;
            this.tabNodes.Text = "Nodes";
            this.tabNodes.UseVisualStyleBackColor = true;
            // 
            // pnlNodes
            // 
            this.pnlNodes.AutoScroll = true;
            this.pnlNodes.Controls.Add(this.txtNodesRawJson);
            this.pnlNodes.Controls.Add(this.dgvNodes);
            this.pnlNodes.Controls.Add(this.btnRefreshNodes);
            this.pnlNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlNodes.Location = new System.Drawing.Point(3, 3);
            this.pnlNodes.Name = "pnlNodes";
            this.pnlNodes.Size = new System.Drawing.Size(1386, 811);
            this.pnlNodes.TabIndex = 0;
            // 
            // txtNodesRawJson
            // 
            this.txtNodesRawJson.Location = new System.Drawing.Point(30, 380);
            this.txtNodesRawJson.Multiline = true;
            this.txtNodesRawJson.Name = "txtNodesRawJson";
            this.txtNodesRawJson.ReadOnly = true;
            this.txtNodesRawJson.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtNodesRawJson.Size = new System.Drawing.Size(1300, 850);
            this.txtNodesRawJson.TabIndex = 2;
            this.txtNodesRawJson.WordWrap = false;
            // 
            // dgvNodes
            // 
            this.dgvNodes.AllowUserToAddRows = false;
            this.dgvNodes.AllowUserToDeleteRows = false;
            this.dgvNodes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvNodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvNodes.Location = new System.Drawing.Point(30, 70);
            this.dgvNodes.Name = "dgvNodes";
            this.dgvNodes.ReadOnly = true;
            this.dgvNodes.RowHeadersWidth = 62;
            this.dgvNodes.RowTemplate.Height = 28;
            this.dgvNodes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvNodes.Size = new System.Drawing.Size(1300, 280);
            this.dgvNodes.TabIndex = 1;
            // 
            // btnRefreshNodes
            // 
            this.btnRefreshNodes.Location = new System.Drawing.Point(30, 20);
            this.btnRefreshNodes.Name = "btnRefreshNodes";
            this.btnRefreshNodes.Size = new System.Drawing.Size(160, 35);
            this.btnRefreshNodes.TabIndex = 0;
            this.btnRefreshNodes.Text = "Atualizar Nodes";
            this.btnRefreshNodes.UseVisualStyleBackColor = true;
            this.btnRefreshNodes.Click += new System.EventHandler(this.btnRefreshNodes_Click);
            // 
            // Other tabs
            // 
            this.ConfigureEmptyTab(this.tabNamespaces, "tabNamespaces", "Namespaces", 2);
            this.ConfigureEmptyTab(this.tabPods, "tabPods", "Pods", 3);
            this.ConfigureEmptyTab(this.tabDeployments, "tabDeployments", "Deployments", 4);
            this.ConfigureEmptyTab(this.tabServices, "tabServices", "Services", 5);
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1400, 850);
            this.Controls.Add(this.tabKubernetesController);
            this.Name = "DashboardForm";
            this.Text = "Gestor de Kubernetes";
            this.Load += new System.EventHandler(this.DashboardForm_Load);
            this.tabKubernetesController.ResumeLayout(false);
            this.tabDashboard.ResumeLayout(false);
            this.pnlDashboard.ResumeLayout(false);
            this.pnlDashboard.PerformLayout();
            this.cardTotalNodes.ResumeLayout(false);
            this.cardTotalNodes.PerformLayout();
            this.cardReadyNodes.ResumeLayout(false);
            this.cardReadyNodes.PerformLayout();
            this.cardNotReadyNodes.ResumeLayout(false);
            this.cardNotReadyNodes.PerformLayout();
            this.cardCpuTotal.ResumeLayout(false);
            this.cardCpuTotal.PerformLayout();
            this.cardMemoryTotal.ResumeLayout(false);
            this.cardMemoryTotal.PerformLayout();
            this.cardPodCapacity.ResumeLayout(false);
            this.cardPodCapacity.PerformLayout();
            this.cardNamespaces.ResumeLayout(false);
            this.cardNamespaces.PerformLayout();
            this.cardPods.ResumeLayout(false);
            this.cardPods.PerformLayout();
            this.cardRunningPods.ResumeLayout(false);
            this.cardRunningPods.PerformLayout();
            this.cardDeployments.ResumeLayout(false);
            this.cardDeployments.PerformLayout();
            this.cardActiveDeployments.ResumeLayout(false);
            this.cardActiveDeployments.PerformLayout();
            this.cardServices.ResumeLayout(false);
            this.cardServices.PerformLayout();
            this.cardIngresses.ResumeLayout(false);
            this.cardIngresses.PerformLayout();
            this.cardMasterIp.ResumeLayout(false);
            this.cardMasterIp.PerformLayout();
            this.cardTopImage.ResumeLayout(false);
            this.cardTopImage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chartPodsByNamespace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartNodesStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartCpuByNode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartPodStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartDeploymentsByNamespace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartDeploymentStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartServicesByNamespace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartKubeletVersions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartMemoryByNode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartImages)).EndInit();
            this.tabNodes.ResumeLayout(false);
            this.pnlNodes.ResumeLayout(false);
            this.pnlNodes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvNodes)).EndInit();
            this.ResumeLayout(false);
        }

        private void ConfigureCard(System.Windows.Forms.Panel card, System.Windows.Forms.Label title, System.Windows.Forms.Label value, string titleText, string valueText, int x, int y, int width, int height)
        {
            card.BackColor = System.Drawing.Color.White;
            card.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            card.Location = new System.Drawing.Point(x, y);
            card.Name = titleText.Replace(" ", "");
            card.Size = new System.Drawing.Size(width, height);
            card.TabIndex = 10;
            card.Controls.Add(title);
            card.Controls.Add(value);

            title.AutoSize = true;
            title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title.Location = new System.Drawing.Point(10, 10);
            title.Name = card.Name + "Title";
            title.Size = new System.Drawing.Size(80, 20);
            title.TabIndex = 0;
            title.Text = titleText;

            value.AutoEllipsis = true;
            value.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            value.Location = new System.Drawing.Point(10, 38);
            value.Name = card.Name + "Value";
            value.Size = new System.Drawing.Size(width - 20, 30);
            value.TabIndex = 1;
            value.Text = valueText;
            value.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        }

        private void ConfigureChartBox(System.Windows.Forms.DataVisualization.Charting.Chart chart, int x, int y, int width, int height)
        {
            chart.BackColor = System.Drawing.Color.White;
            chart.BorderlineColor = System.Drawing.Color.LightGray;
            chart.BorderlineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chart.Location = new System.Drawing.Point(x, y);
            chart.Name = "chart";
            chart.Size = new System.Drawing.Size(width, height);
            chart.TabIndex = 30;
            chart.Text = "chart";
        }

        private void ConfigureEmptyTab(System.Windows.Forms.TabPage tab, string name, string text, int index)
        {
            tab.Location = new System.Drawing.Point(4, 29);
            tab.Name = name;
            tab.Padding = new System.Windows.Forms.Padding(3);
            tab.Size = new System.Drawing.Size(1392, 817);
            tab.TabIndex = index;
            tab.Text = text;
            tab.UseVisualStyleBackColor = true;
        }

        #endregion

        private System.Windows.Forms.TabControl tabKubernetesController;
        private System.Windows.Forms.TabPage tabDashboard;
        private System.Windows.Forms.TabPage tabNodes;
        private System.Windows.Forms.TabPage tabNamespaces;
        private System.Windows.Forms.TabPage tabPods;
        private System.Windows.Forms.TabPage tabDeployments;
        private System.Windows.Forms.TabPage tabServices;

        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Label lblDashboardTitle;
        private System.Windows.Forms.Label lblConnectionInfo;
        private System.Windows.Forms.Button btnRefreshDashboard;

        private System.Windows.Forms.Panel cardTotalNodes;
        private System.Windows.Forms.Label lblTotalNodesTitle;
        private System.Windows.Forms.Label lblTotalNodesValue;
        private System.Windows.Forms.Panel cardReadyNodes;
        private System.Windows.Forms.Label lblReadyNodesTitle;
        private System.Windows.Forms.Label lblReadyNodesValue;
        private System.Windows.Forms.Panel cardNotReadyNodes;
        private System.Windows.Forms.Label lblNotReadyNodesTitle;
        private System.Windows.Forms.Label lblNotReadyNodesValue;
        private System.Windows.Forms.Panel cardCpuTotal;
        private System.Windows.Forms.Label lblCpuTotalTitle;
        private System.Windows.Forms.Label lblCpuTotalValue;
        private System.Windows.Forms.Panel cardMemoryTotal;
        private System.Windows.Forms.Label lblMemoryTotalTitle;
        private System.Windows.Forms.Label lblMemoryTotalValue;
        private System.Windows.Forms.Panel cardPodCapacity;
        private System.Windows.Forms.Label lblPodCapacityTitle;
        private System.Windows.Forms.Label lblPodCapacityValue;
        private System.Windows.Forms.Panel cardNamespaces;
        private System.Windows.Forms.Label lblTotalNamespacesTitle;
        private System.Windows.Forms.Label lblTotalNamespacesValue;
        private System.Windows.Forms.Panel cardPods;
        private System.Windows.Forms.Label lblTotalPodsTitle;
        private System.Windows.Forms.Label lblTotalPodsValue;
        private System.Windows.Forms.Panel cardRunningPods;
        private System.Windows.Forms.Label lblRunningPodsTitle;
        private System.Windows.Forms.Label lblRunningPodsValue;
        private System.Windows.Forms.Panel cardDeployments;
        private System.Windows.Forms.Label lblTotalDeploymentsTitle;
        private System.Windows.Forms.Label lblTotalDeploymentsValue;
        private System.Windows.Forms.Panel cardActiveDeployments;
        private System.Windows.Forms.Label lblActiveDeploymentsTitle;
        private System.Windows.Forms.Label lblActiveDeploymentsValue;
        private System.Windows.Forms.Panel cardServices;
        private System.Windows.Forms.Label lblTotalServicesTitle;
        private System.Windows.Forms.Label lblTotalServicesValue;
        private System.Windows.Forms.Panel cardIngresses;
        private System.Windows.Forms.Label lblTotalIngressesTitle;
        private System.Windows.Forms.Label lblTotalIngressesValue;
        private System.Windows.Forms.Panel cardMasterIp;
        private System.Windows.Forms.Label lblMasterIpTitle;
        private System.Windows.Forms.Label lblMasterIpValue;
        private System.Windows.Forms.Panel cardTopImage;
        private System.Windows.Forms.Label lblTopImageTitle;
        private System.Windows.Forms.Label lblTopImageValue;

        private System.Windows.Forms.DataVisualization.Charting.Chart chartPodsByNamespace;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartNodesStatus;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartCpuByNode;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPodStatus;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartDeploymentsByNamespace;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartDeploymentStatus;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartServicesByNamespace;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartKubeletVersions;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartMemoryByNode;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartImages;

        private System.Windows.Forms.Panel pnlNodes;
        private System.Windows.Forms.Button btnRefreshNodes;
        private System.Windows.Forms.DataGridView dgvNodes;
        private System.Windows.Forms.TextBox txtNodesRawJson;
    }
}
