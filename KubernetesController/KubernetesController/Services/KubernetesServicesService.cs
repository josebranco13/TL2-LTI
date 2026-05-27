using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesServicesService
    {
        private readonly KubernetesApiClient api;

        public KubernetesServicesService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<List<KubernetesServiceSummary>> GetServicesAsync()
        {
            List<KubernetesServiceDetails> details = await GetServiceDetailsAsync();
            return details.Select(s => ToSummary(s)).ToList();
        }

        public async Task<List<KubernetesServiceDetails>> GetServiceDetailsAsync()
        {
            string json = await api.GetAsync("/api/v1/services");
            List<KubernetesServiceDetails> services = new List<KubernetesServiceDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items;
                if (!doc.RootElement.TryGetProperty("items", out items) || items.ValueKind != JsonValueKind.Array)
                    return services;

                foreach (JsonElement service in items.EnumerateArray())
                    services.Add(ParseServiceDetails(service));
            }

            return services;
        }

        public async Task CreateServiceAsync(string serviceName, string namespaceName, string serviceType, string selectorText, string portsText)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("O nome do service não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            if (string.IsNullOrWhiteSpace(serviceType))
                serviceType = "ClusterIP";

            Dictionary<string, string> selector = ParseKeyValueText(selectorText);
            if (selector.Count == 0)
                throw new ArgumentException("Indica pelo menos um selector, por exemplo: app=nginx.");

            List<Dictionary<string, object>> ports = ParsePorts(portsText);
            if (ports.Count == 0)
                throw new ArgumentException("Indica pelo menos uma porta, por exemplo: 80:80:TCP.");

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = serviceName.Trim();
            metadata["namespace"] = namespaceName.Trim();

            Dictionary<string, object> spec = new Dictionary<string, object>();
            spec["type"] = NormalizeServiceType(serviceType);
            spec["selector"] = selector;
            spec["ports"] = ports;

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["apiVersion"] = "v1";
            body["kind"] = "Service";
            body["metadata"] = metadata;
            body["spec"] = spec;

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string json = JsonSerializer.Serialize(body);

            await api.PostAsync("/api/v1/namespaces/" + encodedNamespace + "/services", json);
        }

        public async Task DeleteServiceAsync(string namespaceName, string serviceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Seleciona um namespace válido.");

            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Seleciona um service válido.");

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string encodedService = Uri.EscapeDataString(serviceName.Trim());

            await api.DeleteAsync("/api/v1/namespaces/" + encodedNamespace + "/services/" + encodedService);
            await Task.Delay(1000);
        }

        private KubernetesServiceSummary ToSummary(KubernetesServiceDetails service)
        {
            return new KubernetesServiceSummary
            {
                Name = service.Name,
                Namespace = service.Namespace,
                Type = service.Type,
                ClusterIP = service.ClusterIP,
                ExternalIP = service.ExternalIP,
                Ports = service.PortsText,
                Selector = service.SelectorText,
                CreatedAt = service.CreationTimestamp,
                ManagedBy = service.ManagedBy
            };
        }

        private KubernetesServiceDetails ParseServiceDetails(JsonElement service)
        {
            KubernetesServiceDetails details = new KubernetesServiceDetails();

            JsonElement metadata;
            if (service.TryGetProperty("metadata", out metadata))
            {
                details.Name = GetStringValue(metadata, "name");
                details.Namespace = GetStringValue(metadata, "namespace");
                details.Uid = GetStringValue(metadata, "uid");
                details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                details.Labels = GetKeyValueList(metadata, "labels");
                details.Annotations = GetKeyValueList(metadata, "annotations");
                details.Finalizers = GetFinalizers(metadata);
                details.ManagedFields = GetManagedFields(metadata);
                details.ManagedBy = GetManagedBy(details.ManagedFields);
            }

            JsonElement spec;
            if (service.TryGetProperty("spec", out spec))
            {
                details.Type = GetStringValue(spec, "type");
                details.ClusterIP = GetStringValue(spec, "clusterIP");
                details.SessionAffinity = GetStringValue(spec, "sessionAffinity");
                details.IpFamilyPolicy = GetStringValue(spec, "ipFamilyPolicy");
                details.InternalTrafficPolicy = GetStringValue(spec, "internalTrafficPolicy");
                details.ExternalTrafficPolicy = GetStringValue(spec, "externalTrafficPolicy");
                details.Ports = GetPorts(spec);
                details.Selector = GetKeyValueListFromObject(spec, "selector");
                details.SelectorText = JoinKeyValues(details.Selector);
                details.PortsText = string.Join("; ", details.Ports.Select(p => FormatPort(p)).ToArray());
            }

            JsonElement status;
            if (service.TryGetProperty("status", out status))
            {
                details.LoadBalancerIngresses = GetLoadBalancerIngresses(status);
                details.ExternalIP = GetExternalIp(details.LoadBalancerIngresses);
            }

            if (string.IsNullOrWhiteSpace(details.ExternalIP))
                details.ExternalIP = "-";

            return details;
        }

        private List<KubernetesServicePort> GetPorts(JsonElement spec)
        {
            List<KubernetesServicePort> portsList = new List<KubernetesServicePort>();

            JsonElement ports;
            if (!spec.TryGetProperty("ports", out ports) || ports.ValueKind != JsonValueKind.Array)
                return portsList;

            foreach (JsonElement port in ports.EnumerateArray())
            {
                portsList.Add(new KubernetesServicePort
                {
                    Name = GetStringValue(port, "name"),
                    Protocol = GetStringValue(port, "protocol"),
                    Port = GetScalarText(port, "port"),
                    TargetPort = GetScalarText(port, "targetPort"),
                    NodePort = GetScalarText(port, "nodePort")
                });
            }

            return portsList;
        }

        private string FormatPort(KubernetesServicePort port)
        {
            string text = port.Port;

            if (!string.IsNullOrWhiteSpace(port.TargetPort))
                text += "->" + port.TargetPort;

            if (!string.IsNullOrWhiteSpace(port.Protocol))
                text += "/" + port.Protocol;

            if (!string.IsNullOrWhiteSpace(port.NodePort))
                text += " node:" + port.NodePort;

            return text;
        }

        private List<KubernetesServiceLoadBalancerIngress> GetLoadBalancerIngresses(JsonElement status)
        {
            List<KubernetesServiceLoadBalancerIngress> result = new List<KubernetesServiceLoadBalancerIngress>();

            JsonElement lb;
            if (!status.TryGetProperty("loadBalancer", out lb) || lb.ValueKind != JsonValueKind.Object)
                return result;

            JsonElement ingress;
            if (!lb.TryGetProperty("ingress", out ingress) || ingress.ValueKind != JsonValueKind.Array)
                return result;

            foreach (JsonElement item in ingress.EnumerateArray())
            {
                result.Add(new KubernetesServiceLoadBalancerIngress
                {
                    IP = GetStringValue(item, "ip"),
                    Hostname = GetStringValue(item, "hostname"),
                    IPMode = GetStringValue(item, "ipMode")
                });
            }

            return result;
        }

        private string GetExternalIp(List<KubernetesServiceLoadBalancerIngress> ingresses)
        {
            if (ingresses == null || ingresses.Count == 0)
                return "-";

            return string.Join(", ", ingresses.Select(i => !string.IsNullOrWhiteSpace(i.IP) ? i.IP : i.Hostname).Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
        }

        private List<KubernetesServiceFinalizer> GetFinalizers(JsonElement metadata)
        {
            List<KubernetesServiceFinalizer> finalizers = new List<KubernetesServiceFinalizer>();

            JsonElement array;
            if (!metadata.TryGetProperty("finalizers", out array) || array.ValueKind != JsonValueKind.Array)
                return finalizers;

            foreach (JsonElement item in array.EnumerateArray())
                finalizers.Add(new KubernetesServiceFinalizer { Finalizer = item.GetString() ?? "" });

            return finalizers;
        }

        private List<KubernetesServiceManagedField> GetManagedFields(JsonElement metadata)
        {
            List<KubernetesServiceManagedField> fields = new List<KubernetesServiceManagedField>();

            JsonElement array;
            if (!metadata.TryGetProperty("managedFields", out array) || array.ValueKind != JsonValueKind.Array)
                return fields;

            foreach (JsonElement item in array.EnumerateArray())
            {
                fields.Add(new KubernetesServiceManagedField
                {
                    Manager = GetStringValue(item, "manager"),
                    Operation = GetStringValue(item, "operation"),
                    ApiVersion = GetStringValue(item, "apiVersion"),
                    Time = GetStringValue(item, "time"),
                    Subresource = GetStringValue(item, "subresource")
                });
            }

            return fields;
        }

        private string GetManagedBy(List<KubernetesServiceManagedField> fields)
        {
            if (fields == null || fields.Count == 0)
                return "unknown";

            KubernetesServiceManagedField first = fields.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Manager));
            return first == null ? "unknown" : first.Manager;
        }

        private Dictionary<string, string> ParseKeyValueText(string text)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(text))
                return values;

            string[] parts = text.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string[] keyValue = part.Split(new char[] { '=' }, 2);
                if (keyValue.Length == 2 && !string.IsNullOrWhiteSpace(keyValue[0]))
                    values[keyValue[0].Trim()] = keyValue[1].Trim();
            }

            return values;
        }

        private List<Dictionary<string, object>> ParsePorts(string portsText)
        {
            List<Dictionary<string, object>> ports = new List<Dictionary<string, object>>();

            if (string.IsNullOrWhiteSpace(portsText))
                return ports;

            string[] parts = portsText.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
            int index = 1;

            foreach (string rawPart in parts)
            {
                string part = rawPart.Trim();
                if (part.Length == 0)
                    continue;

                string[] fields = part.Split(':');
                int portNumber;
                int targetPortNumber;
                string protocol = "TCP";

                if (fields.Length == 1)
                {
                    if (!int.TryParse(fields[0].Trim(), out portNumber))
                        throw new ArgumentException("Formato de porta inválido: " + part);
                    targetPortNumber = portNumber;
                }
                else if (fields.Length == 2)
                {
                    if (!int.TryParse(fields[0].Trim(), out portNumber))
                        throw new ArgumentException("Formato de porta inválido: " + part);

                    if (int.TryParse(fields[1].Trim(), out targetPortNumber))
                    {
                        protocol = "TCP";
                    }
                    else
                    {
                        targetPortNumber = portNumber;
                        protocol = fields[1].Trim().ToUpperInvariant();
                    }
                }
                else
                {
                    if (!int.TryParse(fields[0].Trim(), out portNumber) || !int.TryParse(fields[1].Trim(), out targetPortNumber))
                        throw new ArgumentException("Formato de porta inválido: " + part);

                    protocol = fields[2].Trim().ToUpperInvariant();
                }

                if (protocol != "TCP" && protocol != "UDP")
                    protocol = "TCP";

                Dictionary<string, object> port = new Dictionary<string, object>();
                port["name"] = "port-" + index;
                port["port"] = portNumber;
                port["targetPort"] = targetPortNumber;
                port["protocol"] = protocol;
                ports.Add(port);
                index++;
            }

            return ports;
        }

        private string NormalizeServiceType(string serviceType)
        {
            string value = serviceType.Trim();

            if (string.Equals(value, "NodePort", StringComparison.OrdinalIgnoreCase))
                return "NodePort";

            if (string.Equals(value, "LoadBalancer", StringComparison.OrdinalIgnoreCase))
                return "LoadBalancer";

            return "ClusterIP";
        }

        private List<KubernetesKeyValue> GetKeyValueList(JsonElement parent, string propertyName)
        {
            JsonElement obj;
            if (!parent.TryGetProperty(propertyName, out obj) || obj.ValueKind != JsonValueKind.Object)
                return new List<KubernetesKeyValue>();

            return ObjectToKeyValueList(obj);
        }

        private List<KubernetesKeyValue> GetKeyValueListFromObject(JsonElement parent, string propertyName)
        {
            JsonElement obj;
            if (!parent.TryGetProperty(propertyName, out obj) || obj.ValueKind != JsonValueKind.Object)
                return new List<KubernetesKeyValue>();

            return ObjectToKeyValueList(obj);
        }

        private List<KubernetesKeyValue> ObjectToKeyValueList(JsonElement obj)
        {
            List<KubernetesKeyValue> result = new List<KubernetesKeyValue>();

            foreach (JsonProperty property in obj.EnumerateObject())
                result.Add(new KubernetesKeyValue { Chave = property.Name, Valor = GetElementText(property.Value) });

            return result;
        }

        private string JoinKeyValues(List<KubernetesKeyValue> values)
        {
            if (values == null || values.Count == 0)
                return "";

            return string.Join(", ", values.Select(v => v.Chave + "=" + v.Valor).ToArray());
        }

        private string GetStringValue(JsonElement element, string propertyName)
        {
            JsonElement value;
            if (!element.TryGetProperty(propertyName, out value))
                return "";

            return GetElementText(value);
        }

        private string GetScalarText(JsonElement element, string propertyName)
        {
            JsonElement value;
            if (!element.TryGetProperty(propertyName, out value))
                return "";

            return GetElementText(value);
        }

        private string GetElementText(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString() ?? "";
                case JsonValueKind.Number:
                    return element.ToString();
                case JsonValueKind.True:
                    return "True";
                case JsonValueKind.False:
                    return "False";
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "";
                default:
                    return element.ToString();
            }
        }
    }
}
