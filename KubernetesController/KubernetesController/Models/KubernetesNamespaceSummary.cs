namespace KubernetesController.Models
{
    public class KubernetesNamespaceSummary
    {
        public string Name { get; set; }
        public string Phase { get; set; }
        public string CreationTimestamp { get; set; }
        public string ResourceVersion { get; set; }
        public string Uid { get; set; }
        public int LabelsCount { get; set; }
        public string Finalizers { get; set; }
        public string ManagedBy { get; set; }

        public KubernetesNamespaceSummary()
        {
            Name = "";
            Phase = "";
            CreationTimestamp = "";
            ResourceVersion = "";
            Uid = "";
            LabelsCount = 0;
            Finalizers = "";
            ManagedBy = "";
        }
    }
}
