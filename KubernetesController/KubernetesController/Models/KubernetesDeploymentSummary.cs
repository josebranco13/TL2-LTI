namespace KubernetesController.Models
{
    public class KubernetesDeploymentSummary
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Status { get; set; }
        public string Ready { get; set; }
        public int Replicas { get; set; }
        public int Available { get; set; }
        public int Updated { get; set; }
        public string Containers { get; set; }
        public string Images { get; set; }
        public string Strategy { get; set; }
        public string CreatedAt { get; set; }
        public string Revision { get; set; }

        public KubernetesDeploymentSummary()
        {
            Name = "";
            Namespace = "";
            Status = "";
            Ready = "";
            Containers = "";
            Images = "";
            Strategy = "";
            CreatedAt = "";
            Revision = "";
        }
    }
}
