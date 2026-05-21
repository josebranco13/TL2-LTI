using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

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

                    string name = metadata.GetProperty("name").GetString() ?? "";
                    string role = GetNodeRole(metadata);
                    string internalIp = GetInternalIp(status);
                    string nodeStatus = GetNodeReadyStatus(status);

                    int cpu = GetIntValue(capacity, "cpu");
                    int podCapacity = GetIntValue(capacity, "pods");
                    double memoryGiB = GetMemoryGiB(capacity);

                    string osImage = "";
                    string kubeletVersion = "";
                    string containerRuntime = "";

                    if (status.TryGetProperty("nodeInfo", out JsonElement nodeInfo))
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

        private string GetStringValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property))
                return "";

            return property.GetString() ?? "";
        }

        private double GetMemoryGiB(JsonElement capacity)
        {
            if (!capacity.TryGetProperty("memory", out JsonElement memoryElement))
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
            if (!element.TryGetProperty(propertyName, out JsonElement property))
                return 0;

            string value = property.GetString() ?? "0";

            if (int.TryParse(value, out int result))
                return result;

            return 0;
        }

        private string GetNodeReadyStatus(JsonElement status)
        {
            if (!status.TryGetProperty("conditions", out JsonElement conditions))
                return "Unknown";

            foreach (JsonElement condition in conditions.EnumerateArray())
            {
                string type = condition.GetProperty("type").GetString() ?? "";
                string value = condition.GetProperty("status").GetString() ?? "";

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
            if (!status.TryGetProperty("addresses", out JsonElement addresses))
                return "N/A";

            foreach (JsonElement address in addresses.EnumerateArray())
            {
                string type = address.GetProperty("type").GetString() ?? "";

                if (type == "InternalIP")
                    return address.GetProperty("address").GetString() ?? "N/A";
            }

            return "N/A";
        }

        private string GetNodeRole(JsonElement metadata)
        {
            if (!metadata.TryGetProperty("labels", out JsonElement labels))
                return "Worker";

            if (labels.TryGetProperty("node-role.kubernetes.io/control-plane", out _) ||
                labels.TryGetProperty("node-role.kubernetes.io/master", out _))
            {
                return "Master";
            }

            return "Worker";
        }
    }
}
