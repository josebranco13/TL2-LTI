using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesIngressDetails
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
        public string ResourceVersion { get; set; }
        public string CreationTimestamp { get; set; }
        public string ClassName { get; set; }
        public string HostsText { get; set; }
        public string PathsText { get; set; }
        public string ServicesText { get; set; }
        public string Address { get; set; }
        public string ManagedBy { get; set; }

        public List<KubernetesIngressRule> Rules { get; set; }
        public List<KubernetesIngressTls> Tls { get; set; }
        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesKeyValue> Annotations { get; set; }
        public List<KubernetesIngressLoadBalancerIngress> LoadBalancerIngresses { get; set; }
        public List<KubernetesIngressManagedField> ManagedFields { get; set; }

        public KubernetesIngressDetails()
        {
            Name = "";
            Namespace = "";
            Uid = "";
            ResourceVersion = "";
            CreationTimestamp = "";
            ClassName = "";
            HostsText = "";
            PathsText = "";
            ServicesText = "";
            Address = "";
            ManagedBy = "";
            Rules = new List<KubernetesIngressRule>();
            Tls = new List<KubernetesIngressTls>();
            Labels = new List<KubernetesKeyValue>();
            Annotations = new List<KubernetesKeyValue>();
            LoadBalancerIngresses = new List<KubernetesIngressLoadBalancerIngress>();
            ManagedFields = new List<KubernetesIngressManagedField>();
        }
    }

    public class KubernetesIngressRule
    {
        public string Host { get; set; }
        public string Path { get; set; }
        public string PathType { get; set; }
        public string Service { get; set; }
        public string ServicePort { get; set; }
    }

    public class KubernetesIngressTls
    {
        public string Hosts { get; set; }
        public string SecretName { get; set; }
    }

    public class KubernetesIngressLoadBalancerIngress
    {
        public string IP { get; set; }
        public string Hostname { get; set; }
    }

    public class KubernetesIngressManagedField
    {
        public string Manager { get; set; }
        public string Operation { get; set; }
        public string ApiVersion { get; set; }
        public string Time { get; set; }
        public string Subresource { get; set; }
    }
}
