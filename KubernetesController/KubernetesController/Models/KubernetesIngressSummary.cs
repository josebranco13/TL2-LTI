namespace KubernetesController.Models
{
    public class KubernetesIngressSummary
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public string Hosts { get; set; }
        public string Paths { get; set; }
        public string Services { get; set; }
        public string Address { get; set; }
        public string CreatedAt { get; set; }
        public string ManagedBy { get; set; }

        public KubernetesIngressSummary()
        {
            Name = "";
            Namespace = "";
            ClassName = "";
            Hosts = "";
            Paths = "";
            Services = "";
            Address = "";
            CreatedAt = "";
            ManagedBy = "";
        }
    }
}
