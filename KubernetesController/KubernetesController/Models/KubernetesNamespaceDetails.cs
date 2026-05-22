using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesNamespaceDetails
    {
        public string Name { get; set; }
        public string Phase { get; set; }
        public string Uid { get; set; }
        public string CreationTimestamp { get; set; }
        public string ResourceVersion { get; set; }
        public string FinalizersText { get; set; }
        public string ManagedBy { get; set; }

        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesNamespaceFinalizer> Finalizers { get; set; }
        public List<KubernetesNamespaceManagedField> ManagedFields { get; set; }

        public KubernetesNamespaceDetails()
        {
            Name = "";
            Phase = "";
            Uid = "";
            CreationTimestamp = "";
            ResourceVersion = "";
            FinalizersText = "";
            ManagedBy = "";

            Labels = new List<KubernetesKeyValue>();
            Finalizers = new List<KubernetesNamespaceFinalizer>();
            ManagedFields = new List<KubernetesNamespaceManagedField>();
        }
    }

    public class KubernetesNamespaceFinalizer
    {
        public string Nome { get; set; }

        public KubernetesNamespaceFinalizer()
        {
            Nome = "";
        }
    }

    public class KubernetesNamespaceManagedField
    {
        public string Manager { get; set; }
        public string Operation { get; set; }
        public string ApiVersion { get; set; }
        public string Time { get; set; }
        public string FieldsType { get; set; }

        public KubernetesNamespaceManagedField()
        {
            Manager = "";
            Operation = "";
            ApiVersion = "";
            Time = "";
            FieldsType = "";
        }
    }
}
