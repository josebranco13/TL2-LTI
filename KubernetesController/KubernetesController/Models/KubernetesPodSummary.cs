namespace KubernetesController.Models
{
    public class KubernetesPodSummary
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Phase { get; set; }
        public string Ready { get; set; }
        public int Restarts { get; set; }
        public string Node { get; set; }
        public string PodIP { get; set; }
        public string HostIP { get; set; }
        public int Containers { get; set; }
        public string Images { get; set; }
        public string QosClass { get; set; }
        public string CreatedAt { get; set; }
        public string ControlledBy { get; set; }

        public KubernetesPodSummary()
        {
            Name = "";
            Namespace = "";
            Phase = "";
            Ready = "";
            Node = "";
            PodIP = "";
            HostIP = "";
            Images = "";
            QosClass = "";
            CreatedAt = "";
            ControlledBy = "";
        }
    }
}
