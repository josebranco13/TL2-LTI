using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesNodesService
    {
        private readonly KubernetesApiClient api;

        public KubernetesNodesService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<string> GetNodesRawJsonAsync()
        {
            return await api.GetAsync("/api/v1/nodes");
        }

        public async Task<List<KubernetesNodeSummary>> GetNodesAsync()
        {
            string json = await api.GetAsync("/api/v1/nodes");

            List<KubernetesNodeSummary> nodes = new List<KubernetesNodeSummary>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items = doc.RootElement.GetProperty("items");

                foreach (JsonElement node in items.EnumerateArray())
                {
                    JsonElement metadata = node.GetProperty("metadata");
                    JsonElement status = node.GetProperty("status");
                    JsonElement capacity = status.GetProperty("capacity");

                    string name = GetStringValue(metadata, "name");
                    string role = GetNodeRole(metadata);
                    string internalIp = GetInternalIp(status);
                    string nodeStatus = GetNodeReadyStatus(status);

                    int cpu = GetIntValue(capacity, "cpu");
                    int podCapacity = GetIntValue(capacity, "pods");
                    double memoryGiB = GetMemoryGiB(capacity);

                    string osImage = "";
                    string kubeletVersion = "";
                    string containerRuntime = "";

                    JsonElement nodeInfo;
                    if (status.TryGetProperty("nodeInfo", out nodeInfo))
                    {
                        osImage = GetStringValue(nodeInfo, "osImage");
                        kubeletVersion = GetStringValue(nodeInfo, "kubeletVersion");
                        containerRuntime = GetStringValue(nodeInfo, "containerRuntimeVersion");
                    }

                    nodes.Add(new KubernetesNodeSummary
                    {
                        Name = name,
                        Role = role,
                        InternalIp = internalIp,
                        Status = nodeStatus,
                        Cpu = cpu,
                        MemoryGb = memoryGiB,
                        PodCapacity = podCapacity,
                        OsImage = osImage,
                        KubeletVersion = kubeletVersion,
                        ContainerRuntime = containerRuntime
                    });
                }
            }

            return nodes;
        }

        public async Task<List<KubernetesNodeDetails>> GetNodeDetailsAsync()
        {
            string json = await api.GetAsync("/api/v1/nodes");
            List<KubernetesNodeDetails> result = new List<KubernetesNodeDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items = doc.RootElement.GetProperty("items");

                foreach (JsonElement node in items.EnumerateArray())
                {
                    JsonElement metadata = node.GetProperty("metadata");
                    JsonElement status = node.GetProperty("status");
                    JsonElement spec = node.GetProperty("spec");
                    JsonElement capacity = status.GetProperty("capacity");
                    JsonElement allocatable = status.GetProperty("allocatable");
                    JsonElement nodeInfo = status.GetProperty("nodeInfo");

                    KubernetesNodeDetails details = new KubernetesNodeDetails();

                    details.Name = GetStringValue(metadata, "name");
                    details.Role = GetNodeRole(metadata);
                    details.Status = GetNodeReadyStatus(status);
                    details.InternalIp = GetInternalIp(status);
                    details.Hostname = GetAddress(status, "Hostname");
                    details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                    details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                    details.ProviderId = GetStringValue(spec, "providerID");

                    details.CpuCapacity = GetStringValue(capacity, "cpu");
                    details.CpuAllocatable = GetStringValue(allocatable, "cpu");
                    details.MemoryCapacityGiB = ConvertKiToGiB(GetStringValue(capacity, "memory"));
                    details.MemoryAllocatableGiB = ConvertKiToGiB(GetStringValue(allocatable, "memory"));
                    details.PodCapacity = GetStringValue(capacity, "pods");
                    details.PodAllocatable = GetStringValue(allocatable, "pods");
                    details.StorageCapacityGiB = ConvertKiToGiB(GetStringValue(capacity, "ephemeral-storage"));
                    details.StorageAllocatableGiB = ConvertBytesToGiB(GetStringValue(allocatable, "ephemeral-storage"));

                    details.PodCIDR = GetStringValue(spec, "podCIDR");
                    details.PodCIDRs = GetArrayAsText(spec, "podCIDRs");
                    details.KubeletPort = GetKubeletPort(status);
                    details.FlannelPublicIp = GetAnnotation(metadata, "flannel.alpha.coreos.com/public-ip");
                    details.FlannelBackendType = GetAnnotation(metadata, "flannel.alpha.coreos.com/backend-type");
                    details.K3sInternalIp = GetAnnotation(metadata, "k3s.io/internal-ip");
                    details.NodeArgs = GetAnnotation(metadata, "k3s.io/node-args");

                    details.OsImage = GetStringValue(nodeInfo, "osImage");
                    details.KernelVersion = GetStringValue(nodeInfo, "kernelVersion");
                    details.ContainerRuntime = GetStringValue(nodeInfo, "containerRuntimeVersion");
                    details.KubeletVersion = GetStringValue(nodeInfo, "kubeletVersion");
                    details.OperatingSystem = GetStringValue(nodeInfo, "operatingSystem");
                    details.Architecture = GetStringValue(nodeInfo, "architecture");
                    details.MachineID = GetStringValue(nodeInfo, "machineID");
                    details.SystemUUID = GetStringValue(nodeInfo, "systemUUID");
                    details.BootID = GetStringValue(nodeInfo, "bootID");
                    details.SwapGiB = GetSwapGiB(nodeInfo);

                    details.Conditions = GetConditions(status);
                    details.Labels = GetKeyValueList(metadata, "labels");
                    details.Annotations = GetKeyValueList(metadata, "annotations");
                    details.Taints = GetTaints(spec);
                    details.Images = GetImages(status);

                    result.Add(details);
                }
            }

            return result;
        }

        private string GetStringValue(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return "";

            if (property.ValueKind == JsonValueKind.String)
                return property.GetString() ?? "";

            return property.ToString();
        }

        private double GetMemoryGiB(JsonElement capacity)
        {
            JsonElement memoryElement;
            if (!capacity.TryGetProperty("memory", out memoryElement))
                return 0;

            string value = memoryElement.GetString() ?? "0Ki";
            value = value.Replace("Ki", "").Trim();

            double memoryKi;
            if (double.TryParse(value, out memoryKi))
                return Math.Round(memoryKi / 1024.0 / 1024.0, 0);

            return 0;
        }

        private int GetIntValue(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return 0;

            string value = property.GetString() ?? "0";

            int result;
            if (int.TryParse(value, out result))
                return result;

            return 0;
        }

        private string GetNodeReadyStatus(JsonElement status)
        {
            JsonElement conditions;
            if (!status.TryGetProperty("conditions", out conditions))
                return "Unknown";

            foreach (JsonElement condition in conditions.EnumerateArray())
            {
                string type = GetStringValue(condition, "type");
                string value = GetStringValue(condition, "status");

                if (type == "Ready")
                {
                    if (value == "True")
                        return "Ready";

                    if (value == "False")
                        return "Not Ready";

                    return "Unknown";
                }
            }

            return "Unknown";
        }

        private string GetInternalIp(JsonElement status)
        {
            return GetAddress(status, "InternalIP");
        }

        private string GetAddress(JsonElement status, string addressType)
        {
            JsonElement addresses;
            if (!status.TryGetProperty("addresses", out addresses))
                return "N/A";

            foreach (JsonElement address in addresses.EnumerateArray())
            {
                string type = GetStringValue(address, "type");

                if (type == addressType)
                    return GetStringValue(address, "address");
            }

            return "N/A";
        }

        private string GetNodeRole(JsonElement metadata)
        {
            JsonElement labels;
            if (!metadata.TryGetProperty("labels", out labels))
                return "Worker";

            JsonElement label;
            if (labels.TryGetProperty("node-role.kubernetes.io/control-plane", out label) ||
                labels.TryGetProperty("node-role.kubernetes.io/master", out label))
            {
                return "Master";
            }

            return "Worker";
        }

        private string GetAnnotation(JsonElement metadata, string key)
        {
            JsonElement annotations;
            if (!metadata.TryGetProperty("annotations", out annotations))
                return "N/A";

            JsonElement value;
            if (!annotations.TryGetProperty(key, out value))
                return "N/A";

            return value.ToString();
        }

        private string GetArrayAsText(JsonElement element, string propertyName)
        {
            JsonElement array;
            if (!element.TryGetProperty(propertyName, out array))
                return "N/A";

            List<string> values = new List<string>();

            foreach (JsonElement item in array.EnumerateArray())
                values.Add(item.ToString());

            return string.Join(", ", values);
        }

        private string GetKubeletPort(JsonElement status)
        {
            JsonElement endpoints;
            if (!status.TryGetProperty("daemonEndpoints", out endpoints))
                return "N/A";

            JsonElement kubelet;
            if (!endpoints.TryGetProperty("kubeletEndpoint", out kubelet))
                return "N/A";

            JsonElement port;
            if (!kubelet.TryGetProperty("Port", out port))
                return "N/A";

            return port.ToString();
        }

        private string GetSwapGiB(JsonElement nodeInfo)
        {
            JsonElement swap;
            if (!nodeInfo.TryGetProperty("swap", out swap))
                return "0";

            JsonElement capacity;
            if (!swap.TryGetProperty("capacity", out capacity))
                return "0";

            double bytes;
            if (double.TryParse(capacity.ToString(), out bytes))
                return Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 2).ToString("F2");

            return "0";
        }

        private string ConvertKiToGiB(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "0";

            value = value.Replace("Ki", "").Trim();

            double ki;
            if (double.TryParse(value, out ki))
                return Math.Round(ki / 1024.0 / 1024.0, 2).ToString("F2");

            return "0";
        }

        private string ConvertBytesToGiB(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "0";

            double bytes;
            if (double.TryParse(value, out bytes))
                return Math.Round(bytes / 1024.0 / 1024.0 / 1024.0, 2).ToString("F2");

            return "0";
        }

        private List<KubernetesNodeCondition> GetConditions(JsonElement status)
        {
            List<KubernetesNodeCondition> result = new List<KubernetesNodeCondition>();

            JsonElement conditions;
            if (!status.TryGetProperty("conditions", out conditions))
                return result;

            foreach (JsonElement condition in conditions.EnumerateArray())
            {
                result.Add(new KubernetesNodeCondition
                {
                    Type = GetStringValue(condition, "type"),
                    Status = GetStringValue(condition, "status"),
                    Reason = GetStringValue(condition, "reason"),
                    Message = GetStringValue(condition, "message"),
                    LastHeartbeat = GetStringValue(condition, "lastHeartbeatTime"),
                    LastTransition = GetStringValue(condition, "lastTransitionTime")
                });
            }

            return result;
        }

        private List<KubernetesKeyValue> GetKeyValueList(JsonElement element, string propertyName)
        {
            List<KubernetesKeyValue> result = new List<KubernetesKeyValue>();

            JsonElement obj;
            if (!element.TryGetProperty(propertyName, out obj))
                return result;

            foreach (JsonProperty property in obj.EnumerateObject())
            {
                result.Add(new KubernetesKeyValue
                {
                    Chave = property.Name,
                    Valor = property.Value.ToString()
                });
            }

            return result;
        }

        private List<KubernetesNodeTaint> GetTaints(JsonElement spec)
        {
            List<KubernetesNodeTaint> result = new List<KubernetesNodeTaint>();

            JsonElement taints;
            if (!spec.TryGetProperty("taints", out taints))
                return result;

            foreach (JsonElement taint in taints.EnumerateArray())
            {
                result.Add(new KubernetesNodeTaint
                {
                    Key = GetStringValue(taint, "key"),
                    Effect = GetStringValue(taint, "effect"),
                    TimeAdded = GetStringValue(taint, "timeAdded")
                });
            }

            return result;
        }

        private List<KubernetesNodeImage> GetImages(JsonElement status)
        {
            List<KubernetesNodeImage> result = new List<KubernetesNodeImage>();

            JsonElement images;
            if (!status.TryGetProperty("images", out images))
                return result;

            foreach (JsonElement image in images.EnumerateArray())
            {
                string imageName = "N/A";

                JsonElement names;
                if (image.TryGetProperty("names", out names) && names.GetArrayLength() > 0)
                    imageName = names[names.GetArrayLength() - 1].ToString();

                double sizeBytes = 0;
                JsonElement size;
                if (image.TryGetProperty("sizeBytes", out size))
                    double.TryParse(size.ToString(), out sizeBytes);

                result.Add(new KubernetesNodeImage
                {
                    Imagem = imageName,
                    TamanhoMiB = Math.Round(sizeBytes / 1024.0 / 1024.0, 2).ToString("F2")
                });
            }

            return result;
        }
    }
}
