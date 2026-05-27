namespace KubernetesController.Models
{
    public class KubernetesServiceSummary
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public string ClusterIP { get; set; }
        public string ExternalIP { get; set; }
        public string Ports { get; set; }
        public string Selector { get; set; }
        public string CreatedAt { get; set; }
        public string ManagedBy { get; set; }

        public KubernetesServiceSummary()
        {
            Name = "";
            Namespace = "";
            Type = "";
            ClusterIP = "";
            ExternalIP = "";
            Ports = "";
            Selector = "";
            CreatedAt = "";
            ManagedBy = "";
        }
    }
}
