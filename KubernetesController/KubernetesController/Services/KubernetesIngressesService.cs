using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesIngressesService
    {
        private readonly KubernetesApiClient api;

        public KubernetesIngressesService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<List<KubernetesIngressSummary>> GetIngressesAsync()
        {
            List<KubernetesIngressDetails> details = await GetIngressDetailsAsync();
            return details.Select(i => ToSummary(i)).ToList();
        }

        public async Task<List<KubernetesIngressDetails>> GetIngressDetailsAsync()
        {
            string json = await api.GetAsync("/apis/networking.k8s.io/v1/ingresses");
            List<KubernetesIngressDetails> ingresses = new List<KubernetesIngressDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items;
                if (!doc.RootElement.TryGetProperty("items", out items) || items.ValueKind != JsonValueKind.Array)
                    return ingresses;

                foreach (JsonElement ingress in items.EnumerateArray())
                    ingresses.Add(ParseIngressDetails(ingress));
            }

            return ingresses;
        }

        public async Task CreateIngressAsync(string ingressName, string namespaceName, string host, string path, string serviceName, string servicePort)
        {
            if (string.IsNullOrWhiteSpace(ingressName))
                throw new ArgumentException("O nome do ingress não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(namespaceName))
                namespaceName = "default";

            if (string.IsNullOrWhiteSpace(path))
                path = "/";

            if (string.IsNullOrWhiteSpace(serviceName))
                throw new ArgumentException("Indica o service de destino do ingress.");

            int portNumber;
            if (!int.TryParse(servicePort.Trim(), out portNumber))
                throw new ArgumentException("A porta do service deve ser numérica.");

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = ingressName.Trim();
            metadata["namespace"] = namespaceName.Trim();

            Dictionary<string, object> servicePortDict = new Dictionary<string, object>();
            servicePortDict["number"] = portNumber;

            Dictionary<string, object> serviceDict = new Dictionary<string, object>();
            serviceDict["name"] = serviceName.Trim();
            serviceDict["port"] = servicePortDict;

            Dictionary<string, object> backend = new Dictionary<string, object>();
            backend["service"] = serviceDict;

            Dictionary<string, object> pathItem = new Dictionary<string, object>();
            pathItem["path"] = path.Trim();
            pathItem["pathType"] = "Prefix";
            pathItem["backend"] = backend;

            Dictionary<string, object> http = new Dictionary<string, object>();
            http["paths"] = new List<Dictionary<string, object>> { pathItem };

            Dictionary<string, object> rule = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(host))
                rule["host"] = host.Trim();
            rule["http"] = http;

            Dictionary<string, object> spec = new Dictionary<string, object>();
            spec["rules"] = new List<Dictionary<string, object>> { rule };

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["apiVersion"] = "networking.k8s.io/v1";
            body["kind"] = "Ingress";
            body["metadata"] = metadata;
            body["spec"] = spec;

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string json = JsonSerializer.Serialize(body);

            await api.PostAsync("/apis/networking.k8s.io/v1/namespaces/" + encodedNamespace + "/ingresses", json);
        }

        public async Task DeleteIngressAsync(string namespaceName, string ingressName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Seleciona um namespace válido.");

            if (string.IsNullOrWhiteSpace(ingressName))
                throw new ArgumentException("Seleciona um ingress válido.");

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string encodedIngress = Uri.EscapeDataString(ingressName.Trim());

            await api.DeleteAsync("/apis/networking.k8s.io/v1/namespaces/" + encodedNamespace + "/ingresses/" + encodedIngress);
            await Task.Delay(1000);
        }

        private KubernetesIngressSummary ToSummary(KubernetesIngressDetails ingress)
        {
            return new KubernetesIngressSummary
            {
                Name = ingress.Name,
                Namespace = ingress.Namespace,
                ClassName = ingress.ClassName,
                Hosts = ingress.HostsText,
                Paths = ingress.PathsText,
                Services = ingress.ServicesText,
                Address = ingress.Address,
                CreatedAt = ingress.CreationTimestamp,
                ManagedBy = ingress.ManagedBy
            };
        }

        private KubernetesIngressDetails ParseIngressDetails(JsonElement ingress)
        {
            KubernetesIngressDetails details = new KubernetesIngressDetails();

            JsonElement metadata;
            if (ingress.TryGetProperty("metadata", out metadata))
            {
                details.Name = GetStringValue(metadata, "name");
                details.Namespace = GetStringValue(metadata, "namespace");
                details.Uid = GetStringValue(metadata, "uid");
                details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                details.Labels = GetKeyValueList(metadata, "labels");
                details.Annotations = GetKeyValueList(metadata, "annotations");
                details.ManagedFields = GetManagedFields(metadata);
                details.ManagedBy = GetManagedBy(details.ManagedFields);
            }

            JsonElement spec;
            if (ingress.TryGetProperty("spec", out spec))
            {
                details.ClassName = GetStringValue(spec, "ingressClassName");
                details.Rules = GetRules(spec);
                details.Tls = GetTls(spec);
                details.HostsText = JoinDistinct(details.Rules.Select(r => string.IsNullOrWhiteSpace(r.Host) ? "*" : r.Host));
                details.PathsText = JoinDistinct(details.Rules.Select(r => r.Path));
                details.ServicesText = JoinDistinct(details.Rules.Select(r => r.Service + ":" + r.ServicePort));
            }

            JsonElement status;
            if (ingress.TryGetProperty("status", out status))
            {
                details.LoadBalancerIngresses = GetLoadBalancerIngresses(status);
                details.Address = GetAddress(details.LoadBalancerIngresses);
            }

            if (string.IsNullOrWhiteSpace(details.Address))
                details.Address = "-";

            return details;
        }

        private List<KubernetesIngressRule> GetRules(JsonElement spec)
        {
            List<KubernetesIngressRule> rulesList = new List<KubernetesIngressRule>();

            JsonElement rules;
            if (!spec.TryGetProperty("rules", out rules) || rules.ValueKind != JsonValueKind.Array)
                return rulesList;

            foreach (JsonElement rule in rules.EnumerateArray())
            {
                string host = GetStringValue(rule, "host");

                JsonElement http;
                if (!rule.TryGetProperty("http", out http) || http.ValueKind != JsonValueKind.Object)
                    continue;

                JsonElement paths;
                if (!http.TryGetProperty("paths", out paths) || paths.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (JsonElement pathItem in paths.EnumerateArray())
                {
                    KubernetesIngressRule item = new KubernetesIngressRule();
                    item.Host = host;
                    item.Path = GetStringValue(pathItem, "path");
                    item.PathType = GetStringValue(pathItem, "pathType");

                    JsonElement backend;
                    if (pathItem.TryGetProperty("backend", out backend))
                    {
                        JsonElement service;
                        if (backend.TryGetProperty("service", out service))
                        {
                            item.Service = GetStringValue(service, "name");

                            JsonElement port;
                            if (service.TryGetProperty("port", out port))
                            {
                                string number = GetScalarText(port, "number");
                                string name = GetScalarText(port, "name");
                                item.ServicePort = !string.IsNullOrWhiteSpace(number) ? number : name;
                            }
                        }
                    }

                    rulesList.Add(item);
                }
            }

            return rulesList;
        }

        private List<KubernetesIngressTls> GetTls(JsonElement spec)
        {
            List<KubernetesIngressTls> result = new List<KubernetesIngressTls>();

            JsonElement tls;
            if (!spec.TryGetProperty("tls", out tls) || tls.ValueKind != JsonValueKind.Array)
                return result;

            foreach (JsonElement item in tls.EnumerateArray())
            {
                result.Add(new KubernetesIngressTls
                {
                    Hosts = JoinArrayStrings(item, "hosts"),
                    SecretName = GetStringValue(item, "secretName")
                });
            }

            return result;
        }

        private List<KubernetesIngressLoadBalancerIngress> GetLoadBalancerIngresses(JsonElement status)
        {
            List<KubernetesIngressLoadBalancerIngress> result = new List<KubernetesIngressLoadBalancerIngress>();

            JsonElement lb;
            if (!status.TryGetProperty("loadBalancer", out lb) || lb.ValueKind != JsonValueKind.Object)
                return result;

            JsonElement ingress;
            if (!lb.TryGetProperty("ingress", out ingress) || ingress.ValueKind != JsonValueKind.Array)
                return result;

            foreach (JsonElement item in ingress.EnumerateArray())
            {
                result.Add(new KubernetesIngressLoadBalancerIngress
                {
                    IP = GetStringValue(item, "ip"),
                    Hostname = GetStringValue(item, "hostname")
                });
            }

            return result;
        }

        private string GetAddress(List<KubernetesIngressLoadBalancerIngress> ingresses)
        {
            if (ingresses == null || ingresses.Count == 0)
                return "-";

            return string.Join(", ", ingresses.Select(i => !string.IsNullOrWhiteSpace(i.IP) ? i.IP : i.Hostname).Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
        }

        private List<KubernetesIngressManagedField> GetManagedFields(JsonElement metadata)
        {
            List<KubernetesIngressManagedField> fields = new List<KubernetesIngressManagedField>();

            JsonElement array;
            if (!metadata.TryGetProperty("managedFields", out array) || array.ValueKind != JsonValueKind.Array)
                return fields;

            foreach (JsonElement item in array.EnumerateArray())
            {
                fields.Add(new KubernetesIngressManagedField
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

        private string GetManagedBy(List<KubernetesIngressManagedField> fields)
        {
            if (fields == null || fields.Count == 0)
                return "unknown";

            KubernetesIngressManagedField first = fields.FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Manager));
            return first == null ? "unknown" : first.Manager;
        }

        private List<KubernetesKeyValue> GetKeyValueList(JsonElement parent, string propertyName)
        {
            JsonElement obj;
            if (!parent.TryGetProperty(propertyName, out obj) || obj.ValueKind != JsonValueKind.Object)
                return new List<KubernetesKeyValue>();

            List<KubernetesKeyValue> result = new List<KubernetesKeyValue>();
            foreach (JsonProperty property in obj.EnumerateObject())
                result.Add(new KubernetesKeyValue { Chave = property.Name, Valor = GetElementText(property.Value) });

            return result;
        }

        private string JoinArrayStrings(JsonElement parent, string propertyName)
        {
            JsonElement array;
            if (!parent.TryGetProperty(propertyName, out array) || array.ValueKind != JsonValueKind.Array)
                return "";

            List<string> values = new List<string>();
            foreach (JsonElement item in array.EnumerateArray())
                values.Add(GetElementText(item));

            return string.Join(", ", values.ToArray());
        }

        private string JoinDistinct(IEnumerable<string> values)
        {
            if (values == null)
                return "";

            return string.Join(", ", values.Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToArray());
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
