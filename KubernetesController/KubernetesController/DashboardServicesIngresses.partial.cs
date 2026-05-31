using KubernetesController.Models;
using KubernetesController.Services;
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
        private KubernetesServicesService servicesService;
        private KubernetesIngressesService ingressesService;

        private List<KubernetesServiceDetails> serviceDetails = new List<KubernetesServiceDetails>();
        private List<KubernetesIngressDetails> ingressDetails = new List<KubernetesIngressDetails>();

        private Panel pnlServicesContent;
        private TabControl tabServicesIngresses;
        private TabPage tabServicesInner;
        private TabPage tabIngressesInner;

        private Panel pnlServicesList;
        private Button btnRefreshServices;
        private Button btnCreateService;
        private Button btnExportService;
        private Button btnDeleteService;
        private DataGridView dgvServices;
        private TabControl tabServiceDetails;
        private DataGridView dgvServiceSummary;
        private DataGridView dgvServicePorts;
        private DataGridView dgvServiceSelector;
        private DataGridView dgvServiceLabels;
        private DataGridView dgvServiceAnnotations;
        private DataGridView dgvServiceLoadBalancer;
        private DataGridView dgvServiceFinalizers;
        private DataGridView dgvServiceManagedFields;

        private Panel pnlIngressesList;
        private Button btnRefreshIngresses;
        private Button btnCreateIngress;
        private Button btnExportIngress;
        private Button btnDeleteIngress;
        private DataGridView dgvIngresses;
        private TabControl tabIngressDetails;
        private DataGridView dgvIngressSummary;
        private DataGridView dgvIngressRules;
        private DataGridView dgvIngressTls;
        private DataGridView dgvIngressLabels;
        private DataGridView dgvIngressAnnotations;
        private DataGridView dgvIngressLoadBalancer;
        private DataGridView dgvIngressManagedFields;

        private void ConfigureServicesTabControls()
        {
            if (tabServices == null)
                return;

            if (pnlServicesContent != null && tabServicesIngresses != null)
            {
                if (!tabServices.Controls.Contains(pnlServicesContent))
                    tabServices.Controls.Add(pnlServicesContent);

                pnlServicesContent.Dock = DockStyle.Fill;
                pnlServicesContent.Visible = true;
                pnlServicesContent.BringToFront();
                return;
            }

            tabServices.SuspendLayout();
            tabServices.Controls.Clear();

            pnlServicesContent = new Panel();
            pnlServicesContent.Name = "pnlServicesContent";
            pnlServicesContent.Dock = DockStyle.Fill;
            pnlServicesContent.Visible = true;
            tabServices.Controls.Add(pnlServicesContent);

            tabServicesIngresses = new TabControl();
            tabServicesIngresses.Name = "tabServicesIngresses";
            tabServicesIngresses.Dock = DockStyle.Fill;
            pnlServicesContent.Controls.Add(tabServicesIngresses);

            tabServicesInner = new TabPage();
            tabServicesInner.Text = "Services";
            tabIngressesInner = new TabPage();
            tabIngressesInner.Text = "Ingresses";
            tabServicesIngresses.TabPages.Add(tabServicesInner);
            tabServicesIngresses.TabPages.Add(tabIngressesInner);

            ConfigureServicesInnerTab();
            ConfigureIngressesInnerTab();

            pnlServicesContent.BringToFront();
            tabServices.ResumeLayout(false);
        }

        private void ConfigureServicesInnerTab()
        {
            pnlServicesList = new Panel();
            pnlServicesList.Dock = DockStyle.Fill;
            pnlServicesList.AutoScroll = true;
            tabServicesInner.Controls.Add(pnlServicesList);

            btnRefreshServices = new Button();
            btnRefreshServices.Name = "btnRefreshServices";
            btnRefreshServices.Text = "Atualizar Services";
            btnRefreshServices.UseVisualStyleBackColor = true;
            btnRefreshServices.Click += new EventHandler(btnRefreshServices_Click);
            pnlServicesList.Controls.Add(btnRefreshServices);

            btnCreateService = new Button();
            btnCreateService.Name = "btnCreateService";
            btnCreateService.Text = "Criar";
            btnCreateService.UseVisualStyleBackColor = true;
            btnCreateService.Click += new EventHandler(btnCreateService_Click);
            pnlServicesList.Controls.Add(btnCreateService);

            btnExportService = new Button();
            btnExportService.Name = "btnExportService";
            btnExportService.Text = "Exportar";
            btnExportService.UseVisualStyleBackColor = true;
            btnExportService.Click += new EventHandler(btnExportService_Click);
            pnlServicesList.Controls.Add(btnExportService);

            btnDeleteService = new Button();
            btnDeleteService.Name = "btnDeleteService";
            btnDeleteService.Text = "Eliminar";
            btnDeleteService.UseVisualStyleBackColor = true;
            btnDeleteService.Click += new EventHandler(btnDeleteService_Click);
            pnlServicesList.Controls.Add(btnDeleteService);

            dgvServices = CreateListGrid("dgvServices");
            dgvServices.SelectionChanged += new EventHandler(dgvServices_SelectionChanged);
            pnlServicesList.Controls.Add(dgvServices);

            tabServiceDetails = new TabControl();
            tabServiceDetails.Name = "tabServiceDetails";

            dgvServiceSummary = CreateDetailsGrid("dgvServiceSummary");
            dgvServicePorts = CreateDetailsGrid("dgvServicePorts");
            dgvServiceSelector = CreateDetailsGrid("dgvServiceSelector");
            dgvServiceLabels = CreateDetailsGrid("dgvServiceLabels");
            dgvServiceAnnotations = CreateDetailsGrid("dgvServiceAnnotations");
            dgvServiceLoadBalancer = CreateDetailsGrid("dgvServiceLoadBalancer");
            dgvServiceFinalizers = CreateDetailsGrid("dgvServiceFinalizers");
            dgvServiceManagedFields = CreateDetailsGrid("dgvServiceManagedFields");

            AddDetailsTab(tabServiceDetails, "Resumo", dgvServiceSummary);
            AddDetailsTab(tabServiceDetails, "Ports", dgvServicePorts);
            AddDetailsTab(tabServiceDetails, "Selector", dgvServiceSelector);
            AddDetailsTab(tabServiceDetails, "Labels", dgvServiceLabels);
            AddDetailsTab(tabServiceDetails, "Annotations", dgvServiceAnnotations);
            AddDetailsTab(tabServiceDetails, "Load Balancer", dgvServiceLoadBalancer);
            AddDetailsTab(tabServiceDetails, "Finalizers", dgvServiceFinalizers);
            AddDetailsTab(tabServiceDetails, "Managed Fields", dgvServiceManagedFields);

            pnlServicesList.Controls.Add(tabServiceDetails);
        }

        private void ConfigureIngressesInnerTab()
        {
            pnlIngressesList = new Panel();
            pnlIngressesList.Dock = DockStyle.Fill;
            pnlIngressesList.AutoScroll = true;
            tabIngressesInner.Controls.Add(pnlIngressesList);

            btnRefreshIngresses = new Button();
            btnRefreshIngresses.Name = "btnRefreshIngresses";
            btnRefreshIngresses.Text = "Atualizar Ingresses";
            btnRefreshIngresses.UseVisualStyleBackColor = true;
            btnRefreshIngresses.Click += new EventHandler(btnRefreshIngresses_Click);
            pnlIngressesList.Controls.Add(btnRefreshIngresses);

            btnCreateIngress = new Button();
            btnCreateIngress.Name = "btnCreateIngress";
            btnCreateIngress.Text = "Criar";
            btnCreateIngress.UseVisualStyleBackColor = true;
            btnCreateIngress.Click += new EventHandler(btnCreateIngress_Click);
            pnlIngressesList.Controls.Add(btnCreateIngress);

            btnExportIngress = new Button();
            btnExportIngress.Name = "btnExportIngress";
            btnExportIngress.Text = "Exportar";
            btnExportIngress.UseVisualStyleBackColor = true;
            btnExportIngress.Click += new EventHandler(btnExportIngress_Click);
            pnlIngressesList.Controls.Add(btnExportIngress);

            btnDeleteIngress = new Button();
            btnDeleteIngress.Name = "btnDeleteIngress";
            btnDeleteIngress.Text = "Eliminar";
            btnDeleteIngress.UseVisualStyleBackColor = true;
            btnDeleteIngress.Click += new EventHandler(btnDeleteIngress_Click);
            pnlIngressesList.Controls.Add(btnDeleteIngress);

            dgvIngresses = CreateListGrid("dgvIngresses");
            dgvIngresses.SelectionChanged += new EventHandler(dgvIngresses_SelectionChanged);
            pnlIngressesList.Controls.Add(dgvIngresses);

            tabIngressDetails = new TabControl();
            tabIngressDetails.Name = "tabIngressDetails";

            dgvIngressSummary = CreateDetailsGrid("dgvIngressSummary");
            dgvIngressRules = CreateDetailsGrid("dgvIngressRules");
            dgvIngressTls = CreateDetailsGrid("dgvIngressTls");
            dgvIngressLabels = CreateDetailsGrid("dgvIngressLabels");
            dgvIngressAnnotations = CreateDetailsGrid("dgvIngressAnnotations");
            dgvIngressLoadBalancer = CreateDetailsGrid("dgvIngressLoadBalancer");
            dgvIngressManagedFields = CreateDetailsGrid("dgvIngressManagedFields");

            AddDetailsTab(tabIngressDetails, "Resumo", dgvIngressSummary);
            AddDetailsTab(tabIngressDetails, "Rules", dgvIngressRules);
            AddDetailsTab(tabIngressDetails, "TLS", dgvIngressTls);
            AddDetailsTab(tabIngressDetails, "Labels", dgvIngressLabels);
            AddDetailsTab(tabIngressDetails, "Annotations", dgvIngressAnnotations);
            AddDetailsTab(tabIngressDetails, "Load Balancer", dgvIngressLoadBalancer);
            AddDetailsTab(tabIngressDetails, "Managed Fields", dgvIngressManagedFields);

            pnlIngressesList.Controls.Add(tabIngressDetails);
        }

        private DataGridView CreateListGrid(string name)
        {
            DataGridView grid = new DataGridView();
            grid.Name = name;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars = ScrollBars.Both;
            grid.RowHeadersVisible = false;
            return grid;
        }

        private void ArrangeServicesLayout()
        {
            if (pnlServicesContent == null || tabServicesIngresses == null)
                ConfigureServicesTabControls();

            if (pnlServicesContent == null)
                return;

            pnlServicesContent.Dock = DockStyle.Fill;
            pnlServicesContent.Visible = true;
            pnlServicesContent.BringToFront();

            ArrangeServicesInnerLayout();
            ArrangeIngressesInnerLayout();
        }

        private void ArrangeServicesInnerLayout()
        {
            if (pnlServicesList == null || dgvServices == null || tabServiceDetails == null)
                return;

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlServicesList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshServices.Location = new Point(margin, 20);
            btnRefreshServices.Size = new Size(170, 35);

            btnCreateService.Location = new Point(margin + contentWidth - 550, 20);
            btnCreateService.Size = new Size(170, 35);

            btnExportService.Location = new Point(margin + contentWidth - 360, 20);
            btnExportService.Size = new Size(170, 35);

            btnDeleteService.Location = new Point(margin + contentWidth - 170, 20);
            btnDeleteService.Size = new Size(170, 35);

            dgvServices.Location = new Point(margin, 70);
            dgvServices.Size = new Size(contentWidth, 285);

            tabServiceDetails.Location = new Point(margin, dgvServices.Bottom + gap);
            tabServiceDetails.Size = new Size(contentWidth, Math.Max(360, pnlServicesList.ClientSize.Height - tabServiceDetails.Top - margin));

            pnlServicesList.AutoScrollMinSize = new Size(contentWidth + (margin * 2), tabServiceDetails.Bottom + margin);
        }

        private void ArrangeIngressesInnerLayout()
        {
            if (pnlIngressesList == null || dgvIngresses == null || tabIngressDetails == null)
                return;

            int margin = 24;
            int gap = 14;
            int contentWidth = Math.Max(780, pnlIngressesList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - (margin * 2));

            btnRefreshIngresses.Location = new Point(margin, 20);
            btnRefreshIngresses.Size = new Size(170, 35);

            btnCreateIngress.Location = new Point(margin + contentWidth - 550, 20);
            btnCreateIngress.Size = new Size(170, 35);

            btnExportIngress.Location = new Point(margin + contentWidth - 360, 20);
            btnExportIngress.Size = new Size(170, 35);

            btnDeleteIngress.Location = new Point(margin + contentWidth - 170, 20);
            btnDeleteIngress.Size = new Size(170, 35);

            dgvIngresses.Location = new Point(margin, 70);
            dgvIngresses.Size = new Size(contentWidth, 285);

            tabIngressDetails.Location = new Point(margin, dgvIngresses.Bottom + gap);
            tabIngressDetails.Size = new Size(contentWidth, Math.Max(360, pnlIngressesList.ClientSize.Height - tabIngressDetails.Top - margin));

            pnlIngressesList.AutoScrollMinSize = new Size(contentWidth + (margin * 2), tabIngressDetails.Bottom + margin);
        }

        private async Task LoadServicesAndIngressesTabAsync()
        {
            await LoadServicesOnlyAsync();
            await LoadIngressesOnlyAsync();
        }

        private async Task LoadServicesOnlyAsync()
        {
            if (pnlServicesContent == null || dgvServices == null)
                ConfigureServicesTabControls();

            if (servicesService == null || dgvServices == null)
                return;

            serviceDetails = await servicesService.GetServiceDetailsAsync();

            List<KubernetesServiceSummary> services = serviceDetails.Select(s => new KubernetesServiceSummary
            {
                Name = s.Name,
                Namespace = s.Namespace,
                Type = s.Type,
                ClusterIP = s.ClusterIP,
                ExternalIP = s.ExternalIP,
                Ports = s.PortsText,
                Selector = s.SelectorText,
                CreatedAt = s.CreationTimestamp,
                ManagedBy = s.ManagedBy
            }).ToList();

            dgvServices.DataSource = null;
            dgvServices.DataSource = services;
            ConfigureServicesGridHeaders();

            if (dgvServices.Rows.Count > 0)
                dgvServices.Rows[0].Selected = true;

            if (serviceDetails.Count > 0)
                ShowServiceDetails(serviceDetails[0]);
            else
                ClearServiceDetails();

            ArrangeServicesInnerLayout();
        }

        private async Task LoadIngressesOnlyAsync()
        {
            if (pnlServicesContent == null || dgvIngresses == null)
                ConfigureServicesTabControls();

            if (ingressesService == null || dgvIngresses == null)
                return;

            ingressDetails = await ingressesService.GetIngressDetailsAsync();

            List<KubernetesIngressSummary> ingresses = ingressDetails.Select(i => new KubernetesIngressSummary
            {
                Name = i.Name,
                Namespace = i.Namespace,
                ClassName = i.ClassName,
                Hosts = i.HostsText,
                Paths = i.PathsText,
                Services = i.ServicesText,
                Address = i.Address,
                CreatedAt = i.CreationTimestamp,
                ManagedBy = i.ManagedBy
            }).ToList();

            dgvIngresses.DataSource = null;
            dgvIngresses.DataSource = ingresses;
            ConfigureIngressesGridHeaders();

            if (dgvIngresses.Rows.Count > 0)
                dgvIngresses.Rows[0].Selected = true;

            if (ingressDetails.Count > 0)
                ShowIngressDetails(ingressDetails[0]);
            else
                ClearIngressDetails();

            ArrangeIngressesInnerLayout();
        }

        private void ConfigureServicesGridHeaders()
        {
            SetColumnHeader(dgvServices, "Name", "Nome");
            SetColumnHeader(dgvServices, "Namespace", "Namespace");
            SetColumnHeader(dgvServices, "Type", "Tipo");
            SetColumnHeader(dgvServices, "ClusterIP", "Cluster IP");
            SetColumnHeader(dgvServices, "ExternalIP", "External IP");
            SetColumnHeader(dgvServices, "Ports", "Ports");
            SetColumnHeader(dgvServices, "Selector", "Selector");
            SetColumnHeader(dgvServices, "CreatedAt", "Criado em");
            SetColumnHeader(dgvServices, "ManagedBy", "Gerido por");

            if (dgvServices.Columns["Ports"] != null)
                dgvServices.Columns["Ports"].FillWeight = 180;

            if (dgvServices.Columns["Selector"] != null)
                dgvServices.Columns["Selector"].FillWeight = 160;
        }

        private void ConfigureIngressesGridHeaders()
        {
            SetColumnHeader(dgvIngresses, "Name", "Nome");
            SetColumnHeader(dgvIngresses, "Namespace", "Namespace");
            SetColumnHeader(dgvIngresses, "ClassName", "Classe");
            SetColumnHeader(dgvIngresses, "Hosts", "Hosts");
            SetColumnHeader(dgvIngresses, "Paths", "Paths");
            SetColumnHeader(dgvIngresses, "Services", "Services");
            SetColumnHeader(dgvIngresses, "Address", "Endereço");
            SetColumnHeader(dgvIngresses, "CreatedAt", "Criado em");
            SetColumnHeader(dgvIngresses, "ManagedBy", "Gerido por");

            if (dgvIngresses.Columns["Hosts"] != null)
                dgvIngresses.Columns["Hosts"].FillWeight = 150;

            if (dgvIngresses.Columns["Services"] != null)
                dgvIngresses.Columns["Services"].FillWeight = 160;
        }

        private void dgvServices_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvServices == null || dgvServices.CurrentRow == null || dgvServices.CurrentRow.DataBoundItem == null)
                return;

            KubernetesServiceSummary selectedService = dgvServices.CurrentRow.DataBoundItem as KubernetesServiceSummary;
            if (selectedService == null || serviceDetails == null)
                return;

            KubernetesServiceDetails selectedDetails = serviceDetails.FirstOrDefault(s => s.Name == selectedService.Name && s.Namespace == selectedService.Namespace);
            if (selectedDetails != null)
                ShowServiceDetails(selectedDetails);
        }

        private void dgvIngresses_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvIngresses == null || dgvIngresses.CurrentRow == null || dgvIngresses.CurrentRow.DataBoundItem == null)
                return;

            KubernetesIngressSummary selectedIngress = dgvIngresses.CurrentRow.DataBoundItem as KubernetesIngressSummary;
            if (selectedIngress == null || ingressDetails == null)
                return;

            KubernetesIngressDetails selectedDetails = ingressDetails.FirstOrDefault(i => i.Name == selectedIngress.Name && i.Namespace == selectedIngress.Namespace);
            if (selectedDetails != null)
                ShowIngressDetails(selectedDetails);
        }

        private void ShowServiceDetails(KubernetesServiceDetails service)
        {
            if (service == null)
            {
                ClearServiceDetails();
                return;
            }

            dgvServiceSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", service.Name },
                { "Namespace", service.Namespace },
                { "Tipo", service.Type },
                { "UID", service.Uid },
                { "Criado em", service.CreationTimestamp },
                { "Resource Version", service.ResourceVersion },
                { "Cluster IP", service.ClusterIP },
                { "External IP", service.ExternalIP },
                { "Ports", service.PortsText },
                { "Selector", service.SelectorText },
                { "Session Affinity", service.SessionAffinity },
                { "IP Family Policy", service.IpFamilyPolicy },
                { "Internal Traffic Policy", service.InternalTrafficPolicy },
                { "External Traffic Policy", service.ExternalTrafficPolicy },
                { "Gerido por", service.ManagedBy }
            });

            dgvServicePorts.DataSource = null;
            dgvServicePorts.DataSource = service.Ports;
            dgvServiceSelector.DataSource = null;
            dgvServiceSelector.DataSource = service.Selector;
            dgvServiceLabels.DataSource = null;
            dgvServiceLabels.DataSource = service.Labels;
            dgvServiceAnnotations.DataSource = null;
            dgvServiceAnnotations.DataSource = service.Annotations;
            dgvServiceLoadBalancer.DataSource = null;
            dgvServiceLoadBalancer.DataSource = service.LoadBalancerIngresses;
            dgvServiceFinalizers.DataSource = null;
            dgvServiceFinalizers.DataSource = service.Finalizers;
            dgvServiceManagedFields.DataSource = null;
            dgvServiceManagedFields.DataSource = service.ManagedFields;
        }

        private void ShowIngressDetails(KubernetesIngressDetails ingress)
        {
            if (ingress == null)
            {
                ClearIngressDetails();
                return;
            }

            dgvIngressSummary.DataSource = ToTable(new Dictionary<string, string>
            {
                { "Nome", ingress.Name },
                { "Namespace", ingress.Namespace },
                { "Classe", ingress.ClassName },
                { "UID", ingress.Uid },
                { "Criado em", ingress.CreationTimestamp },
                { "Resource Version", ingress.ResourceVersion },
                { "Hosts", ingress.HostsText },
                { "Paths", ingress.PathsText },
                { "Services", ingress.ServicesText },
                { "Endereço", ingress.Address },
                { "Gerido por", ingress.ManagedBy }
            });

            dgvIngressRules.DataSource = null;
            dgvIngressRules.DataSource = ingress.Rules;
            dgvIngressTls.DataSource = null;
            dgvIngressTls.DataSource = ingress.Tls;
            dgvIngressLabels.DataSource = null;
            dgvIngressLabels.DataSource = ingress.Labels;
            dgvIngressAnnotations.DataSource = null;
            dgvIngressAnnotations.DataSource = ingress.Annotations;
            dgvIngressLoadBalancer.DataSource = null;
            dgvIngressLoadBalancer.DataSource = ingress.LoadBalancerIngresses;
            dgvIngressManagedFields.DataSource = null;
            dgvIngressManagedFields.DataSource = ingress.ManagedFields;
        }

        private void ClearServiceDetails()
        {
            if (dgvServiceSummary != null) dgvServiceSummary.DataSource = null;
            if (dgvServicePorts != null) dgvServicePorts.DataSource = null;
            if (dgvServiceSelector != null) dgvServiceSelector.DataSource = null;
            if (dgvServiceLabels != null) dgvServiceLabels.DataSource = null;
            if (dgvServiceAnnotations != null) dgvServiceAnnotations.DataSource = null;
            if (dgvServiceLoadBalancer != null) dgvServiceLoadBalancer.DataSource = null;
            if (dgvServiceFinalizers != null) dgvServiceFinalizers.DataSource = null;
            if (dgvServiceManagedFields != null) dgvServiceManagedFields.DataSource = null;
        }

        private void ClearIngressDetails()
        {
            if (dgvIngressSummary != null) dgvIngressSummary.DataSource = null;
            if (dgvIngressRules != null) dgvIngressRules.DataSource = null;
            if (dgvIngressTls != null) dgvIngressTls.DataSource = null;
            if (dgvIngressLabels != null) dgvIngressLabels.DataSource = null;
            if (dgvIngressAnnotations != null) dgvIngressAnnotations.DataSource = null;
            if (dgvIngressLoadBalancer != null) dgvIngressLoadBalancer.DataSource = null;
            if (dgvIngressManagedFields != null) dgvIngressManagedFields.DataSource = null;
        }

        private async void btnRefreshServices_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshServices.Enabled = false;
                await LoadServicesOnlyAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar services.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefreshServices.Enabled = true;
            }
        }

        private async void btnRefreshIngresses_Click(object sender, EventArgs e)
        {
            try
            {
                btnRefreshIngresses.Enabled = false;
                await LoadIngressesOnlyAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao atualizar ingresses.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRefreshIngresses.Enabled = true;
            }
        }

        private async void btnCreateService_Click(object sender, EventArgs e)
        {
            if (servicesService == null)
                return;

            using (CreateServiceForm form = new CreateServiceForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    btnCreateService.Enabled = false;
                    await servicesService.CreateServiceAsync(form.ServiceName, form.NamespaceName, form.ServiceType, form.SelectorText, form.PortsText);
                    MessageBox.Show("Service criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await Task.Delay(1000);
                    await LoadServicesOnlyAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar service.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnCreateService.Enabled = true;
                }
            }
        }

        private async void btnCreateIngress_Click(object sender, EventArgs e)
        {
            if (ingressesService == null)
                return;

            using (CreateIngressForm form = new CreateIngressForm())
            {
                if (form.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    btnCreateIngress.Enabled = false;
                    await ingressesService.CreateIngressAsync(form.IngressName, form.NamespaceName, form.Host, form.PathValue, form.ServiceName, form.ServicePort);
                    MessageBox.Show("Ingress criado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    await Task.Delay(1000);
                    await LoadIngressesOnlyAsync();
                    await LoadDashboardAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao criar ingress.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnCreateIngress.Enabled = true;
                }
            }
        }

        private async void btnExportService_Click(object sender, EventArgs e)
        {
            if (dgvServices == null || dgvServices.CurrentRow == null)
                return;

            KubernetesServiceSummary selectedService = dgvServices.CurrentRow.DataBoundItem as KubernetesServiceSummary;
            if (selectedService == null || string.IsNullOrWhiteSpace(selectedService.Name) || string.IsNullOrWhiteSpace(selectedService.Namespace))
                return;

            string encodedNamespace = Uri.EscapeDataString(selectedService.Namespace.Trim());
            string encodedName = Uri.EscapeDataString(selectedService.Name.Trim());

            try
            {
                btnExportService.Enabled = false;
                await ExportResourceAsync(
                    "/api/v1/namespaces/" + encodedNamespace + "/services/" + encodedName,
                    selectedService.Namespace + "-" + selectedService.Name,
                    "Exportar Service"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao exportar service.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportService.Enabled = true;
            }
        }

        private async void btnDeleteService_Click(object sender, EventArgs e)
        {
            if (servicesService == null || dgvServices == null || dgvServices.CurrentRow == null)
                return;

            KubernetesServiceSummary selectedService = dgvServices.CurrentRow.DataBoundItem as KubernetesServiceSummary;
            if (selectedService == null)
                return;

            DialogResult confirm = MessageBox.Show(
                "Tem a certeza que pretende eliminar o service '" + selectedService.Name + "' no namespace '" + selectedService.Namespace + "'?",
                "Confirmar eliminação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnDeleteService.Enabled = false;
                await servicesService.DeleteServiceAsync(selectedService.Namespace, selectedService.Name);
                MessageBox.Show("Pedido de eliminação enviado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Delay(1500);
                await LoadServicesOnlyAsync();
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao eliminar service.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDeleteService.Enabled = true;
            }
        }

        private async void btnExportIngress_Click(object sender, EventArgs e)
        {
            if (dgvIngresses == null || dgvIngresses.CurrentRow == null)
                return;

            KubernetesIngressSummary selectedIngress = dgvIngresses.CurrentRow.DataBoundItem as KubernetesIngressSummary;
            if (selectedIngress == null || string.IsNullOrWhiteSpace(selectedIngress.Name) || string.IsNullOrWhiteSpace(selectedIngress.Namespace))
                return;

            string encodedNamespace = Uri.EscapeDataString(selectedIngress.Namespace.Trim());
            string encodedName = Uri.EscapeDataString(selectedIngress.Name.Trim());

            try
            {
                btnExportIngress.Enabled = false;
                await ExportResourceAsync(
                    "/apis/networking.k8s.io/v1/namespaces/" + encodedNamespace + "/ingresses/" + encodedName,
                    selectedIngress.Namespace + "-" + selectedIngress.Name,
                    "Exportar Ingress"
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao exportar ingress.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExportIngress.Enabled = true;
            }
        }

        private async void btnDeleteIngress_Click(object sender, EventArgs e)
        {
            if (ingressesService == null || dgvIngresses == null || dgvIngresses.CurrentRow == null)
                return;

            KubernetesIngressSummary selectedIngress = dgvIngresses.CurrentRow.DataBoundItem as KubernetesIngressSummary;
            if (selectedIngress == null)
                return;

            DialogResult confirm = MessageBox.Show(
                "Tem a certeza que pretende eliminar o ingress '" + selectedIngress.Name + "' no namespace '" + selectedIngress.Namespace + "'?",
                "Confirmar eliminação",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                btnDeleteIngress.Enabled = false;
                await ingressesService.DeleteIngressAsync(selectedIngress.Namespace, selectedIngress.Name);
                MessageBox.Show("Pedido de eliminação enviado com sucesso.", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await Task.Delay(1500);
                await LoadIngressesOnlyAsync();
                await LoadDashboardAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao eliminar ingress.\n\n" + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDeleteIngress.Enabled = true;
            }
        }
    }
}
