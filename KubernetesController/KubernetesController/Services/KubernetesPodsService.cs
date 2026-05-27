using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesPodsService
    {
        private readonly KubernetesApiClient api;

        public KubernetesPodsService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<List<KubernetesPodSummary>> GetPodsAsync()
        {
            List<KubernetesPodDetails> details = await GetPodDetailsAsync();
            return details.Select(p => ToSummary(p)).ToList();
        }

        public async Task<List<KubernetesPodDetails>> GetPodDetailsAsync()
        {
            string json = await api.GetAsync("/api/v1/pods");
            List<KubernetesPodDetails> pods = new List<KubernetesPodDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items;
                if (!doc.RootElement.TryGetProperty("items", out items) || items.ValueKind != JsonValueKind.Array)
                    return pods;

                foreach (JsonElement pod in items.EnumerateArray())
                    pods.Add(ParsePodDetails(pod));
            }

            return pods;
        }

        public async Task CreatePodAsync(string podName, string namespaceName, string labelsText, string containersText)
        {
            if (string.IsNullOrWhiteSpace(podName))
                throw new ArgumentException("O nome do pod não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            List<Dictionary<string, object>> containers = ParseContainers(containersText);
            if (containers.Count == 0)
                throw new ArgumentException("Indica pelo menos um container no formato nome:imagem, por exemplo: web:nginx.");

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = podName.Trim();
            metadata["namespace"] = namespaceName.Trim();

            Dictionary<string, string> labels = ParseLabels(labelsText);
            if (labels.Count > 0)
                metadata["labels"] = labels;

            Dictionary<string, object> spec = new Dictionary<string, object>();
            spec["containers"] = containers;
            spec["restartPolicy"] = "Always";

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["apiVersion"] = "v1";
            body["kind"] = "Pod";
            body["metadata"] = metadata;
            body["spec"] = spec;

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string json = JsonSerializer.Serialize(body);

            await api.PostAsync("/api/v1/namespaces/" + encodedNamespace + "/pods", json);
        }

        public async Task DeletePodAsync(string namespaceName, string podName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Seleciona um namespace válido.");

            if (string.IsNullOrWhiteSpace(podName))
                throw new ArgumentException("Seleciona um pod válido.");

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string encodedPod = Uri.EscapeDataString(podName.Trim());

            await api.DeleteAsync("/api/v1/namespaces/" + encodedNamespace + "/pods/" + encodedPod);
            await Task.Delay(1000);
        }

        private KubernetesPodSummary ToSummary(KubernetesPodDetails pod)
        {
            return new KubernetesPodSummary
            {
                Name = pod.Name,
                Namespace = pod.Namespace,
                Phase = pod.Phase,
                Ready = pod.Ready,
                Restarts = pod.Restarts,
                Node = pod.NodeName,
                PodIP = pod.PodIP,
                HostIP = pod.HostIP,
                Containers = pod.Containers.Count,
                Images = pod.ImagesText,
                QosClass = pod.QosClass,
                CreatedAt = pod.CreationTimestamp,
                ControlledBy = pod.ControlledBy
            };
        }

        private KubernetesPodDetails ParsePodDetails(JsonElement pod)
        {
            KubernetesPodDetails details = new KubernetesPodDetails();

            JsonElement metadata;
            if (pod.TryGetProperty("metadata", out metadata))
            {
                details.Name = GetStringValue(metadata, "name");
                details.Namespace = GetStringValue(metadata, "namespace");
                details.Uid = GetStringValue(metadata, "uid");
                details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                details.Labels = GetKeyValueList(metadata, "labels");
                details.Annotations = GetKeyValueList(metadata, "annotations");
                details.Owners = GetOwners(metadata);
                details.ControlledBy = GetControlledBy(details.Owners);
            }

            JsonElement spec;
            if (pod.TryGetProperty("spec", out spec))
            {
                details.NodeName = GetStringValue(spec, "nodeName");
                details.RestartPolicy = GetStringValue(spec, "restartPolicy");
                details.ServiceAccount = GetStringValue(spec, "serviceAccountName");
                details.DnsPolicy = GetStringValue(spec, "dnsPolicy");
                details.PriorityClassName = GetStringValue(spec, "priorityClassName");
                details.Volumes = GetVolumes(spec);
                details.Tolerations = GetTolerations(spec);
                details.Containers = GetSpecContainers(spec);
            }

            JsonElement status;
            if (pod.TryGetProperty("status", out status))
            {
                details.Phase = GetStringValue(status, "phase");
                details.PodIP = GetStringValue(status, "podIP");
                details.HostIP = GetStringValue(status, "hostIP");
                details.StartTime = GetStringValue(status, "startTime");
                details.QosClass = GetStringValue(status, "qosClass");
                details.Conditions = GetConditions(status);
                ApplyContainerStatuses(details, status);
            }

            details.Restarts = details.Containers.Sum(c => c.Restarts);
            details.Ready = GetReadyText(details.Containers);
            details.ImagesText = string.Join(", ", details.Containers.Select(c => c.Image).Where(i => !string.IsNullOrWhiteSpace(i)).Distinct().ToArray());

            return details;
        }

        private List<KubernetesPodContainer> GetSpecContainers(JsonElement spec)
        {
            List<KubernetesPodContainer> containers = new List<KubernetesPodContainer>();

            JsonElement array;
            if (!spec.TryGetProperty("containers", out array) || array.ValueKind != JsonValueKind.Array)
                return containers;

            foreach (JsonElement container in array.EnumerateArray())
            {
                containers.Add(new KubernetesPodContainer
                {
                    Name = GetStringValue(container, "name"),
                    Image = GetStringValue(container, "image"),
                    ImagePullPolicy = GetStringValue(container, "imagePullPolicy"),
                    Ports = GetContainerPorts(container),
                    Requests = GetResourcesText(container, "requests"),
                    Limits = GetResourcesText(container, "limits"),
                    Ready = "False",
                    Started = "False",
                    State = "Waiting",
                    ContainerId = ""
                });
            }

            return containers;
        }

        private void ApplyContainerStatuses(KubernetesPodDetails details, JsonElement status)
        {
            JsonElement statuses;
            if (!status.TryGetProperty("containerStatuses", out statuses) || statuses.ValueKind != JsonValueKind.Array)
                return;

            foreach (JsonElement statusItem in statuses.EnumerateArray())
            {
                string name = GetStringValue(statusItem, "name");
                KubernetesPodContainer container = details.Containers.FirstOrDefault(c => c.Name == name);

                if (container == null)
                {
                    container = new KubernetesPodContainer { Name = name };
                    details.Containers.Add(container);
                }

                if (string.IsNullOrWhiteSpace(container.Image))
                    container.Image = GetStringValue(statusItem, "image");

                container.Ready = GetBoolText(statusItem, "ready");
                container.Started = GetBoolText(statusItem, "started");
                container.Restarts = GetIntValue(statusItem, "restartCount");
                container.ContainerId = GetStringValue(statusItem, "containerID");
                container.State = GetContainerState(statusItem);
            }
        }

        private string GetReadyText(List<KubernetesPodContainer> containers)
        {
            if (containers == null || containers.Count == 0)
                return "0/0";

            int ready = containers.Count(c => string.Equals(c.Ready, "True", StringComparison.OrdinalIgnoreCase));
            return ready + "/" + containers.Count;
        }

        private string GetContainerState(JsonElement statusItem)
        {
            JsonElement state;
            if (!statusItem.TryGetProperty("state", out state) || state.ValueKind != JsonValueKind.Object)
                return "Unknown";

            foreach (JsonProperty property in state.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    string reason = GetStringValue(property.Value, "reason");
                    if (!string.IsNullOrWhiteSpace(reason))
                        return property.Name + " (" + reason + ")";
                }

                return property.Name;
            }

            return "Unknown";
        }

        private string GetContainerPorts(JsonElement container)
        {
            JsonElement ports;
            if (!container.TryGetProperty("ports", out ports) || ports.ValueKind != JsonValueKind.Array)
                return "";

            List<string> values = new List<string>();

            foreach (JsonElement port in ports.EnumerateArray())
            {
                string name = GetStringValue(port, "name");
                string containerPort = GetStringValue(port, "containerPort");
                string protocol = GetStringValue(port, "protocol");
                string hostPort = GetStringValue(port, "hostPort");

                string text = "";
                if (!string.IsNullOrWhiteSpace(name))
                    text += name + ": ";

                text += containerPort;

                if (!string.IsNullOrWhiteSpace(hostPort))
                    text = hostPort + ":" + text;

                if (!string.IsNullOrWhiteSpace(protocol))
                    text += "/" + protocol;

                values.Add(text);
            }

            return string.Join(", ", values.ToArray());
        }

        private string GetResourcesText(JsonElement container, string resourceType)
        {
            JsonElement resources;
            if (!container.TryGetProperty("resources", out resources) || resources.ValueKind != JsonValueKind.Object)
                return "";

            JsonElement values;
            if (!resources.TryGetProperty(resourceType, out values) || values.ValueKind != JsonValueKind.Object)
                return "";

            List<string> parts = new List<string>();
            foreach (JsonProperty property in values.EnumerateObject())
                parts.Add(property.Name + "=" + GetElementText(property.Value));

            return string.Join(", ", parts.ToArray());
        }

        private List<KubernetesPodCondition> GetConditions(JsonElement status)
        {
            List<KubernetesPodCondition> conditions = new List<KubernetesPodCondition>();

            JsonElement array;
            if (!status.TryGetProperty("conditions", out array) || array.ValueKind != JsonValueKind.Array)
                return conditions;

            foreach (JsonElement condition in array.EnumerateArray())
            {
                conditions.Add(new KubernetesPodCondition
                {
                    Type = GetStringValue(condition, "type"),
                    Status = GetStringValue(condition, "status"),
                    Reason = GetStringValue(condition, "reason"),
                    Message = GetStringValue(condition, "message"),
                    LastTransition = GetStringValue(condition, "lastTransitionTime")
                });
            }

            return conditions;
        }

        private List<KubernetesPodOwner> GetOwners(JsonElement metadata)
        {
            List<KubernetesPodOwner> owners = new List<KubernetesPodOwner>();

            JsonElement array;
            if (!metadata.TryGetProperty("ownerReferences", out array) || array.ValueKind != JsonValueKind.Array)
                return owners;

            foreach (JsonElement owner in array.EnumerateArray())
            {
                owners.Add(new KubernetesPodOwner
                {
                    Kind = GetStringValue(owner, "kind"),
                    Name = GetStringValue(owner, "name"),
                    Controller = GetBoolText(owner, "controller")
                });
            }

            return owners;
        }

        private string GetControlledBy(List<KubernetesPodOwner> owners)
        {
            if (owners == null || owners.Count == 0)
                return "";

            KubernetesPodOwner controller = owners.FirstOrDefault(o => string.Equals(o.Controller, "True", StringComparison.OrdinalIgnoreCase));
            if (controller == null)
                controller = owners[0];

            return controller.Kind + "/" + controller.Name;
        }

        private List<KubernetesPodVolume> GetVolumes(JsonElement spec)
        {
            List<KubernetesPodVolume> volumes = new List<KubernetesPodVolume>();

            JsonElement array;
            if (!spec.TryGetProperty("volumes", out array) || array.ValueKind != JsonValueKind.Array)
                return volumes;

            foreach (JsonElement volume in array.EnumerateArray())
            {
                string name = GetStringValue(volume, "name");
                string type = "";
                string detail = "";

                foreach (JsonProperty property in volume.EnumerateObject())
                {
                    if (property.Name == "name")
                        continue;

                    type = property.Name;
                    detail = GetVolumeDetail(property.Value);
                    break;
                }

                volumes.Add(new KubernetesPodVolume
                {
                    Name = name,
                    Type = type,
                    Detail = detail
                });
            }

            return volumes;
        }

        private string GetVolumeDetail(JsonElement value)
        {
            if (value.ValueKind != JsonValueKind.Object)
                return GetElementText(value);

            string name = GetStringValue(value, "name");
            if (!string.IsNullOrWhiteSpace(name))
                return name;

            if (value.TryGetProperty("medium", out JsonElement medium))
                return "medium=" + GetElementText(medium);

            return value.ToString();
        }

        private List<KubernetesPodToleration> GetTolerations(JsonElement spec)
        {
            List<KubernetesPodToleration> tolerations = new List<KubernetesPodToleration>();

            JsonElement array;
            if (!spec.TryGetProperty("tolerations", out array) || array.ValueKind != JsonValueKind.Array)
                return tolerations;

            foreach (JsonElement toleration in array.EnumerateArray())
            {
                tolerations.Add(new KubernetesPodToleration
                {
                    Key = GetStringValue(toleration, "key"),
                    Operator = GetStringValue(toleration, "operator"),
                    Value = GetStringValue(toleration, "value"),
                    Effect = GetStringValue(toleration, "effect"),
                    TolerationSeconds = GetStringValue(toleration, "tolerationSeconds")
                });
            }

            return tolerations;
        }

        private Dictionary<string, string> ParseLabels(string labelsText)
        {
            Dictionary<string, string> labels = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(labelsText))
                return labels;

            string[] pairs = labelsText.Split(',');
            foreach (string pair in pairs)
            {
                if (string.IsNullOrWhiteSpace(pair))
                    continue;

                string[] parts = pair.Split(new char[] { '=' }, 2);
                if (parts.Length != 2)
                    throw new ArgumentException("As labels devem estar no formato chave=valor, exemplo: app=web,ambiente=teste");

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Existe uma label sem chave.");

                labels[key] = value;
            }

            return labels;
        }

        private List<Dictionary<string, object>> ParseContainers(string containersText)
        {
            List<Dictionary<string, object>> containers = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(containersText))
                return containers;

            string normalized = containersText.Replace("\r\n", ",").Replace("\n", ",");
            string[] parts = normalized.Split(',');
            int index = 1;

            foreach (string rawPart in parts)
            {
                string part = rawPart.Trim();
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                int separatorIndex = part.IndexOf(';');
                if (separatorIndex < 0)
                    separatorIndex = part.IndexOf(':');

                string name;
                string image;

                if (separatorIndex > 0)
                {
                    name = part.Substring(0, separatorIndex).Trim();
                    image = part.Substring(separatorIndex + 1).Trim();
                }
                else
                {
                    name = "container" + index;
                    image = part;
                }

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(image))
                    throw new ArgumentException("Containers inválidos. Usa o formato nome:imagem, por exemplo: web:nginx.");

                Dictionary<string, object> container = new Dictionary<string, object>();
                container["name"] = name;
                container["image"] = image;
                container["imagePullPolicy"] = "IfNotPresent";

                containers.Add(container);
                index++;
            }

            return containers;
        }

        private List<KubernetesKeyValue> GetKeyValueList(JsonElement element, string propertyName)
        {
            List<KubernetesKeyValue> values = new List<KubernetesKeyValue>();

            JsonElement obj;
            if (!element.TryGetProperty(propertyName, out obj) || obj.ValueKind != JsonValueKind.Object)
                return values;

            foreach (JsonProperty property in obj.EnumerateObject())
            {
                values.Add(new KubernetesKeyValue
                {
                    Chave = property.Name,
                    Valor = GetElementText(property.Value)
                });
            }

            return values;
        }

        private string GetStringValue(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return "";

            return GetElementText(property);
        }

        private string GetElementText(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return element.GetString() ?? "";

            if (element.ValueKind == JsonValueKind.True)
                return "True";

            if (element.ValueKind == JsonValueKind.False)
                return "False";

            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return "";

            return element.ToString();
        }

        private string GetBoolText(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return "False";

            if (property.ValueKind == JsonValueKind.True)
                return "True";

            if (property.ValueKind == JsonValueKind.False)
                return "False";

            return GetElementText(property);
        }

        private int GetIntValue(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return 0;

            if (property.ValueKind == JsonValueKind.Number)
            {
                int value;
                if (property.TryGetInt32(out value))
                    return value;
            }

            int parsed;
            if (int.TryParse(GetElementText(property), out parsed))
                return parsed;

            return 0;
        }
    }
}
