namespace KubernetesController.Models
{
    public class KubernetesNamespaceSummary
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string CreatedAt { get; set; }
        public string ResourceVersion { get; set; }
        public string Uid { get; set; }
        public int Labels { get; set; }
        public string Finalizers { get; set; }
        public string ManagedBy { get; set; }

        public KubernetesNamespaceSummary()
        {
            Name = "";
            Status = "";
            CreatedAt = "";
            ResourceVersion = "";
            Uid = "";
            Labels = 0;
            Finalizers = "";
            ManagedBy = "";
        }
    }
}
