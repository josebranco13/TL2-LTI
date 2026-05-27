using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesPodDetails
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
        public string ResourceVersion { get; set; }
        public string CreationTimestamp { get; set; }
        public string Phase { get; set; }
        public string Ready { get; set; }
        public int Restarts { get; set; }
        public string NodeName { get; set; }
        public string PodIP { get; set; }
        public string HostIP { get; set; }
        public string StartTime { get; set; }
        public string QosClass { get; set; }
        public string RestartPolicy { get; set; }
        public string ServiceAccount { get; set; }
        public string DnsPolicy { get; set; }
        public string PriorityClassName { get; set; }
        public string ControlledBy { get; set; }
        public string ImagesText { get; set; }

        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesKeyValue> Annotations { get; set; }
        public List<KubernetesPodOwner> Owners { get; set; }
        public List<KubernetesPodContainer> Containers { get; set; }
        public List<KubernetesPodCondition> Conditions { get; set; }
        public List<KubernetesPodVolume> Volumes { get; set; }
        public List<KubernetesPodToleration> Tolerations { get; set; }

        public KubernetesPodDetails()
        {
            Name = "";
            Namespace = "";
            Uid = "";
            ResourceVersion = "";
            CreationTimestamp = "";
            Phase = "";
            Ready = "";
            NodeName = "";
            PodIP = "";
            HostIP = "";
            StartTime = "";
            QosClass = "";
            RestartPolicy = "";
            ServiceAccount = "";
            DnsPolicy = "";
            PriorityClassName = "";
            ControlledBy = "";
            ImagesText = "";

            Labels = new List<KubernetesKeyValue>();
            Annotations = new List<KubernetesKeyValue>();
            Owners = new List<KubernetesPodOwner>();
            Containers = new List<KubernetesPodContainer>();
            Conditions = new List<KubernetesPodCondition>();
            Volumes = new List<KubernetesPodVolume>();
            Tolerations = new List<KubernetesPodToleration>();
        }
    }

    public class KubernetesPodContainer
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public string State { get; set; }
        public string Ready { get; set; }
        public string Started { get; set; }
        public int Restarts { get; set; }
        public string Ports { get; set; }
        public string Requests { get; set; }
        public string Limits { get; set; }
        public string ImagePullPolicy { get; set; }
        public string ContainerId { get; set; }
    }

    public class KubernetesPodCondition
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
        public string LastTransition { get; set; }
    }

    public class KubernetesPodOwner
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public string Controller { get; set; }
    }

    public class KubernetesPodVolume
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
    }

    public class KubernetesPodToleration
    {
        public string Key { get; set; }
        public string Operator { get; set; }
        public string Value { get; set; }
        public string Effect { get; set; }
        public string TolerationSeconds { get; set; }
    }
}
