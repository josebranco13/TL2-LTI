using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesDashboardSummary
    {
        public int TotalNodes { get; set; }
        public int ReadyNodes { get; set; }
        public int NotReadyNodes { get; set; }
        public int TotalCpu { get; set; }
        public double TotalMemoryGiB { get; set; }
        public int TotalPodCapacity { get; set; }
        public string MasterIp { get; set; }
        public string TopImage { get; set; }

        public int TotalNamespaces { get; set; }
        public int TotalPods { get; set; }
        public int RunningPods { get; set; }
        public int TotalDeployments { get; set; }
        public int ActiveDeployments { get; set; }
        public int TotalServices { get; set; }
        public int TotalIngresses { get; set; }

        public Dictionary<string, int> PodsByNamespace { get; set; }
        public Dictionary<string, int> NodesStatus { get; set; }
        public Dictionary<string, int> CpuByNode { get; set; }
        public Dictionary<string, double> MemoryByNode { get; set; }
        public Dictionary<string, int> PodStatus { get; set; }
        public Dictionary<string, int> DeploymentsByNamespace { get; set; }
        public Dictionary<string, int> ServicesByNamespace { get; set; }
        public Dictionary<string, int> ImagesCount { get; set; }

        public KubernetesDashboardSummary()
        {
            MasterIp = "N/A";
            TopImage = "N/A";

            PodsByNamespace = new Dictionary<string, int>();
            NodesStatus = new Dictionary<string, int>();
            CpuByNode = new Dictionary<string, int>();
            MemoryByNode = new Dictionary<string, double>();
            PodStatus = new Dictionary<string, int>();
            DeploymentsByNamespace = new Dictionary<string, int>();
            ServicesByNamespace = new Dictionary<string, int>();
            ImagesCount = new Dictionary<string, int>();
        }
    }
}
