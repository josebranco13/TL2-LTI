using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesDeploymentDetails
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
        public string CreationTimestamp { get; set; }
        public string ResourceVersion { get; set; }
        public string Revision { get; set; }
        public string Status { get; set; }
        public string ReadyText { get; set; }
        public int Replicas { get; set; }
        public int ReadyReplicas { get; set; }
        public int AvailableReplicas { get; set; }
        public int UpdatedReplicas { get; set; }
        public int UnavailableReplicas { get; set; }
        public int TerminatingReplicas { get; set; }
        public string StrategyType { get; set; }
        public string MaxUnavailable { get; set; }
        public string MaxSurge { get; set; }
        public string SelectorText { get; set; }
        public string LabelsText { get; set; }
        public string ContainersText { get; set; }
        public string ImagesText { get; set; }
        public string ManagedBy { get; set; }

        public List<KubernetesDeploymentContainer> Containers { get; set; }
        public List<KubernetesDeploymentCondition> Conditions { get; set; }
        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesKeyValue> Annotations { get; set; }
        public List<KubernetesKeyValue> Selector { get; set; }
        public List<KubernetesKeyValue> Strategy { get; set; }
        public List<KubernetesDeploymentManagedField> ManagedFields { get; set; }

        public KubernetesDeploymentDetails()
        {
            Name = "";
            Namespace = "";
            Uid = "";
            CreationTimestamp = "";
            ResourceVersion = "";
            Revision = "";
            Status = "";
            ReadyText = "";
            StrategyType = "";
            MaxUnavailable = "";
            MaxSurge = "";
            SelectorText = "";
            LabelsText = "";
            ContainersText = "";
            ImagesText = "";
            ManagedBy = "";

            Containers = new List<KubernetesDeploymentContainer>();
            Conditions = new List<KubernetesDeploymentCondition>();
            Labels = new List<KubernetesKeyValue>();
            Annotations = new List<KubernetesKeyValue>();
            Selector = new List<KubernetesKeyValue>();
            Strategy = new List<KubernetesKeyValue>();
            ManagedFields = new List<KubernetesDeploymentManagedField>();
        }
    }

    public class KubernetesDeploymentContainer
    {
        public string Nome { get; set; }
        public string Imagem { get; set; }
        public string ImagePullPolicy { get; set; }
        public string Ports { get; set; }
        public string Requests { get; set; }
        public string Limits { get; set; }
        public string Args { get; set; }
        public string Env { get; set; }

        public KubernetesDeploymentContainer()
        {
            Nome = "";
            Imagem = "";
            ImagePullPolicy = "";
            Ports = "";
            Requests = "";
            Limits = "";
            Args = "";
            Env = "";
        }
    }

    public class KubernetesDeploymentCondition
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
        public string LastUpdateTime { get; set; }
        public string LastTransitionTime { get; set; }

        public KubernetesDeploymentCondition()
        {
            Type = "";
            Status = "";
            Reason = "";
            Message = "";
            LastUpdateTime = "";
            LastTransitionTime = "";
        }
    }

    public class KubernetesDeploymentManagedField
    {
        public string Manager { get; set; }
        public string Operation { get; set; }
        public string ApiVersion { get; set; }
        public string Time { get; set; }
        public string FieldsType { get; set; }

        public KubernetesDeploymentManagedField()
        {
            Manager = "";
            Operation = "";
            ApiVersion = "";
            Time = "";
            FieldsType = "";
        }
    }
}
