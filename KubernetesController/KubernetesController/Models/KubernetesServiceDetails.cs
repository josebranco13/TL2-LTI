using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesServiceDetails
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Uid { get; set; }
        public string ResourceVersion { get; set; }
        public string CreationTimestamp { get; set; }
        public string Type { get; set; }
        public string ClusterIP { get; set; }
        public string ExternalIP { get; set; }
        public string PortsText { get; set; }
        public string SelectorText { get; set; }
        public string SessionAffinity { get; set; }
        public string IpFamilyPolicy { get; set; }
        public string InternalTrafficPolicy { get; set; }
        public string ExternalTrafficPolicy { get; set; }
        public string ManagedBy { get; set; }

        public List<KubernetesServicePort> Ports { get; set; }
        public List<KubernetesKeyValue> Selector { get; set; }
        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesKeyValue> Annotations { get; set; }
        public List<KubernetesServiceLoadBalancerIngress> LoadBalancerIngresses { get; set; }
        public List<KubernetesServiceFinalizer> Finalizers { get; set; }
        public List<KubernetesServiceManagedField> ManagedFields { get; set; }

        public KubernetesServiceDetails()
        {
            Name = "";
            Namespace = "";
            Uid = "";
            ResourceVersion = "";
            CreationTimestamp = "";
            Type = "";
            ClusterIP = "";
            ExternalIP = "";
            PortsText = "";
            SelectorText = "";
            SessionAffinity = "";
            IpFamilyPolicy = "";
            InternalTrafficPolicy = "";
            ExternalTrafficPolicy = "";
            ManagedBy = "";
            Ports = new List<KubernetesServicePort>();
            Selector = new List<KubernetesKeyValue>();
            Labels = new List<KubernetesKeyValue>();
            Annotations = new List<KubernetesKeyValue>();
            LoadBalancerIngresses = new List<KubernetesServiceLoadBalancerIngress>();
            Finalizers = new List<KubernetesServiceFinalizer>();
            ManagedFields = new List<KubernetesServiceManagedField>();
        }
    }

    public class KubernetesServicePort
    {
        public string Name { get; set; }
        public string Protocol { get; set; }
        public string Port { get; set; }
        public string TargetPort { get; set; }
        public string NodePort { get; set; }
    }

    public class KubernetesServiceLoadBalancerIngress
    {
        public string IP { get; set; }
        public string Hostname { get; set; }
        public string IPMode { get; set; }
    }

    public class KubernetesServiceFinalizer
    {
        public string Finalizer { get; set; }
    }

    public class KubernetesServiceManagedField
    {
        public string Manager { get; set; }
        public string Operation { get; set; }
        public string ApiVersion { get; set; }
        public string Time { get; set; }
        public string Subresource { get; set; }
    }
}
