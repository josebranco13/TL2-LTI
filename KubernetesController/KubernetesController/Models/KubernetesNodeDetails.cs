using System.Collections.Generic;

namespace KubernetesController.Models
{
    public class KubernetesNodeDetails
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string InternalIp { get; set; }
        public string Hostname { get; set; }
        public string CreationTimestamp { get; set; }
        public string ResourceVersion { get; set; }
        public string ProviderId { get; set; }

        public string CpuCapacity { get; set; }
        public string CpuAllocatable { get; set; }
        public string MemoryCapacityGiB { get; set; }
        public string MemoryAllocatableGiB { get; set; }
        public string PodCapacity { get; set; }
        public string PodAllocatable { get; set; }
        public string StorageCapacityGiB { get; set; }
        public string StorageAllocatableGiB { get; set; }

        public string PodCIDR { get; set; }
        public string PodCIDRs { get; set; }
        public string KubeletPort { get; set; }
        public string FlannelPublicIp { get; set; }
        public string FlannelBackendType { get; set; }
        public string K3sInternalIp { get; set; }
        public string NodeArgs { get; set; }

        public string OsImage { get; set; }
        public string KernelVersion { get; set; }
        public string ContainerRuntime { get; set; }
        public string KubeletVersion { get; set; }
        public string OperatingSystem { get; set; }
        public string Architecture { get; set; }
        public string MachineID { get; set; }
        public string SystemUUID { get; set; }
        public string BootID { get; set; }
        public string SwapGiB { get; set; }

        public List<KubernetesNodeCondition> Conditions { get; set; }
        public List<KubernetesKeyValue> Labels { get; set; }
        public List<KubernetesKeyValue> Annotations { get; set; }
        public List<KubernetesNodeTaint> Taints { get; set; }
        public List<KubernetesNodeImage> Images { get; set; }

        public KubernetesNodeDetails()
        {
            Conditions = new List<KubernetesNodeCondition>();
            Labels = new List<KubernetesKeyValue>();
            Annotations = new List<KubernetesKeyValue>();
            Taints = new List<KubernetesNodeTaint>();
            Images = new List<KubernetesNodeImage>();
        }
    }

    public class KubernetesNodeCondition
    {
        public string Type { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Message { get; set; }
        public string LastHeartbeat { get; set; }
        public string LastTransition { get; set; }
    }

    public class KubernetesKeyValue
    {
        public string Chave { get; set; }
        public string Valor { get; set; }
    }

    public class KubernetesNodeTaint
    {
        public string Key { get; set; }
        public string Effect { get; set; }
        public string TimeAdded { get; set; }
    }

    public class KubernetesNodeImage
    {
        public string Imagem { get; set; }
        public string TamanhoMiB { get; set; }
    }
}
