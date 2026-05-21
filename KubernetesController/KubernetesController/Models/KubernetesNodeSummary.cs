using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubernetesController.Models
{
    public class KubernetesNodeSummary
    {
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public string InternalIp { get; set; } = "";
        public string Status { get; set; } = "";
        public int Cpu { get; set; }
        public double MemoryGb { get; set; }
        public int PodCapacity { get; set; }
        public string OsImage { get; set; } = "";
        public string KubeletVersion { get; set; } = "";
        public string ContainerRuntime { get; set; } = "";
    }
}
