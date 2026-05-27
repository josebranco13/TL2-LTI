using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesDeploymentsService
    {
        private readonly KubernetesApiClient api;

        public KubernetesDeploymentsService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<List<KubernetesDeploymentSummary>> GetDeploymentsAsync()
        {
            List<KubernetesDeploymentDetails> details = await GetDeploymentDetailsAsync();
            List<KubernetesDeploymentSummary> deployments = new List<KubernetesDeploymentSummary>();

            foreach (KubernetesDeploymentDetails deployment in details)
            {
                deployments.Add(new KubernetesDeploymentSummary
                {
                    Name = deployment.Name,
                    Namespace = deployment.Namespace,
                    Status = deployment.Status,
                    Ready = deployment.ReadyText,
                    Replicas = deployment.Replicas,
                    Available = deployment.AvailableReplicas,
                    Updated = deployment.UpdatedReplicas,
                    Containers = deployment.ContainersText,
                    Images = deployment.ImagesText,
                    Strategy = deployment.StrategyType,
                    CreatedAt = deployment.CreationTimestamp,
                    Revision = deployment.Revision
                });
            }

            return deployments;
        }

        public async Task<List<KubernetesDeploymentDetails>> GetDeploymentDetailsAsync()
        {
            string json = await api.GetAsync("/apis/apps/v1/deployments");
            List<KubernetesDeploymentDetails> deployments = new List<KubernetesDeploymentDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items;
                if (!doc.RootElement.TryGetProperty("items", out items) || items.ValueKind != JsonValueKind.Array)
                    return deployments;

                foreach (JsonElement deployment in items.EnumerateArray())
                    deployments.Add(ParseDeploymentDetails(deployment));
            }

            return deployments;
        }

        public async Task CreateDeploymentAsync(string name, string containersText, string replicasText, string namespaceText, string labelsText)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do deployment não pode estar vazio.");

            if (string.IsNullOrWhiteSpace(containersText))
                throw new ArgumentException("Indica pelo menos um container no formato nome:imagem. Exemplo: web:nginx");

            int replicas = 1;
            if (!string.IsNullOrWhiteSpace(replicasText) && !int.TryParse(replicasText.Trim(), out replicas))
                throw new ArgumentException("O número de réplicas tem de ser um número inteiro.");

            if (replicas < 0)
                throw new ArgumentException("O número de réplicas não pode ser negativo.");

            string cleanName = name.Trim();
            string cleanNamespace = string.IsNullOrWhiteSpace(namespaceText) ? "default" : namespaceText.Trim();

            Dictionary<string, string> labels = ParseLabels(labelsText);
            if (labels.Count == 0)
                labels["app"] = cleanName;

            List<Dictionary<string, object>> containers = ParseContainers(containersText);

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = cleanName;
            metadata["namespace"] = cleanNamespace;
            metadata["labels"] = labels;

            Dictionary<string, object> selector = new Dictionary<string, object>();
            selector["matchLabels"] = labels;

            Dictionary<string, object> templateMetadata = new Dictionary<string, object>();
            templateMetadata["labels"] = labels;

            Dictionary<string, object> templateSpec = new Dictionary<string, object>();
            templateSpec["containers"] = containers;

            Dictionary<string, object> template = new Dictionary<string, object>();
            template["metadata"] = templateMetadata;
            template["spec"] = templateSpec;

            Dictionary<string, object> spec = new Dictionary<string, object>();
            spec["replicas"] = replicas;
            spec["selector"] = selector;
            spec["template"] = template;

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["apiVersion"] = "apps/v1";
            body["kind"] = "Deployment";
            body["metadata"] = metadata;
            body["spec"] = spec;

            string json = JsonSerializer.Serialize(body);
            string encodedNamespace = Uri.EscapeDataString(cleanNamespace);
            await api.PostAsync("/apis/apps/v1/namespaces/" + encodedNamespace + "/deployments", json);
        }

        public async Task DeleteDeploymentAsync(string namespaceName, string deploymentName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
                throw new ArgumentException("Namespace inválido.");

            if (string.IsNullOrWhiteSpace(deploymentName))
                throw new ArgumentException("Deployment inválido.");

            string encodedNamespace = Uri.EscapeDataString(namespaceName.Trim());
            string encodedName = Uri.EscapeDataString(deploymentName.Trim());

            await api.DeleteAsync("/apis/apps/v1/namespaces/" + encodedNamespace + "/deployments/" + encodedName);
            await Task.Delay(1500);
        }

        private KubernetesDeploymentDetails ParseDeploymentDetails(JsonElement deployment)
        {
            KubernetesDeploymentDetails details = new KubernetesDeploymentDetails();

            JsonElement metadata;
            if (deployment.TryGetProperty("metadata", out metadata))
            {
                details.Name = GetStringValue(metadata, "name");
                details.Namespace = GetStringValue(metadata, "namespace");
                details.Uid = GetStringValue(metadata, "uid");
                details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                details.Labels = GetKeyValueList(metadata, "labels");
                details.Annotations = GetKeyValueList(metadata, "annotations");
                details.Revision = GetAnnotationValue(details.Annotations, "deployment.kubernetes.io/revision");
                details.ManagedBy = GetManagedBy(metadata);
                details.ManagedFields = GetManagedFields(metadata);
                details.LabelsText = ToText(details.Labels);
            }

            JsonElement spec;
            if (deployment.TryGetProperty("spec", out spec))
            {
                details.Replicas = GetIntValue(spec, "replicas");
                details.Selector = GetSelector(spec);
                details.SelectorText = ToText(details.Selector);
                details.Strategy = GetStrategy(spec);
                details.StrategyType = GetStrategyType(spec);
                details.MaxUnavailable = GetRollingUpdateValue(spec, "maxUnavailable");
                details.MaxSurge = GetRollingUpdateValue(spec, "maxSurge");
                details.Containers = GetContainers(spec);
                details.ContainersText = string.Join(", ", details.Containers.Select(c => c.Nome).ToArray());
                details.ImagesText = string.Join(", ", details.Containers.Select(c => c.Imagem).Distinct().ToArray());
            }

            JsonElement status;
            if (deployment.TryGetProperty("status", out status))
            {
                details.ReadyReplicas = GetIntValue(status, "readyReplicas");
                details.AvailableReplicas = GetIntValue(status, "availableReplicas");
                details.UpdatedReplicas = GetIntValue(status, "updatedReplicas");
                details.UnavailableReplicas = GetIntValue(status, "unavailableReplicas");
                details.TerminatingReplicas = GetIntValue(status, "terminatingReplicas");

                int statusReplicas = GetIntValue(status, "replicas");
                if (details.Replicas == 0 && statusReplicas > 0)
                    details.Replicas = statusReplicas;

                details.Conditions = GetConditions(status);
            }

            details.ReadyText = details.ReadyReplicas + "/" + details.Replicas;
            details.Status = GetDeploymentStatus(details);

            return details;
        }

        private string GetDeploymentStatus(KubernetesDeploymentDetails deployment)
        {
            if (deployment.Replicas == 0)
                return "Sem réplicas";

            if (deployment.AvailableReplicas >= deployment.Replicas && deployment.ReadyReplicas >= deployment.Replicas)
                return "Disponível";

            if (deployment.UpdatedReplicas > 0 || deployment.ReadyReplicas > 0)
                return "Em progresso";

            return "Indisponível";
        }

        private List<Dictionary<string, object>> ParseContainers(string containersText)
        {
            List<Dictionary<string, object>> containers = new List<Dictionary<string, object>>();
            string[] items = containersText.Split(',');

            foreach (string item in items)
            {
                if (string.IsNullOrWhiteSpace(item))
                    continue;

                string[] parts = item.Split(new char[] { ':' }, 2);
                if (parts.Length != 2)
                    throw new ArgumentException("Os containers devem estar no formato nome:imagem. Exemplo: web:nginx");

                string name = parts[0].Trim();
                string image = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(image))
                    throw new ArgumentException("Existe um container com nome ou imagem vazia.");

                Dictionary<string, object> container = new Dictionary<string, object>();
                container["name"] = name;
                container["image"] = image;
                container["imagePullPolicy"] = "IfNotPresent";
                containers.Add(container);
            }

            if (containers.Count == 0)
                throw new ArgumentException("Indica pelo menos um container no formato nome:imagem. Exemplo: web:nginx");

            return containers;
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
                    throw new ArgumentException("As labels devem estar no formato chave=valor. Exemplo: app=web,ambiente=teste");

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Existe uma label sem chave.");

                labels[key] = value;
            }

            return labels;
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

        private int GetIntValue(JsonElement element, string propertyName)
        {
            JsonElement property;
            if (!element.TryGetProperty(propertyName, out property))
                return 0;

            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value))
                return value;

            return 0;
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
                    Valor = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() ?? "" : property.Value.ToString()
                });
            }

            return values;
        }

        private string GetAnnotationValue(List<KubernetesKeyValue> annotations, string key)
        {
            KubernetesKeyValue item = annotations.FirstOrDefault(a => a.Chave == key);
            return item == null ? "" : item.Valor;
        }

        private string ToText(List<KubernetesKeyValue> values)
        {
            if (values == null || values.Count == 0)
                return "";

            return string.Join(", ", values.Select(v => v.Chave + "=" + v.Valor).ToArray());
        }

        private List<KubernetesKeyValue> GetSelector(JsonElement spec)
        {
            List<KubernetesKeyValue> selector = new List<KubernetesKeyValue>();

            JsonElement selectorElement;
            if (!spec.TryGetProperty("selector", out selectorElement))
                return selector;

            JsonElement matchLabels;
            if (selectorElement.TryGetProperty("matchLabels", out matchLabels) && matchLabels.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in matchLabels.EnumerateObject())
                {
                    selector.Add(new KubernetesKeyValue
                    {
                        Chave = property.Name,
                        Valor = property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() ?? "" : property.Value.ToString()
                    });
                }
            }

            return selector;
        }

        private List<KubernetesKeyValue> GetStrategy(JsonElement spec)
        {
            List<KubernetesKeyValue> strategy = new List<KubernetesKeyValue>();
            string type = GetStrategyType(spec);
            if (!string.IsNullOrWhiteSpace(type))
                strategy.Add(new KubernetesKeyValue { Chave = "type", Valor = type });

            string maxUnavailable = GetRollingUpdateValue(spec, "maxUnavailable");
            if (!string.IsNullOrWhiteSpace(maxUnavailable))
                strategy.Add(new KubernetesKeyValue { Chave = "maxUnavailable", Valor = maxUnavailable });

            string maxSurge = GetRollingUpdateValue(spec, "maxSurge");
            if (!string.IsNullOrWhiteSpace(maxSurge))
                strategy.Add(new KubernetesKeyValue { Chave = "maxSurge", Valor = maxSurge });

            return strategy;
        }

        private string GetStrategyType(JsonElement spec)
        {
            JsonElement strategy;
            if (!spec.TryGetProperty("strategy", out strategy))
                return "";

            return GetStringValue(strategy, "type");
        }

        private string GetRollingUpdateValue(JsonElement spec, string propertyName)
        {
            JsonElement strategy;
            if (!spec.TryGetProperty("strategy", out strategy))
                return "";

            JsonElement rollingUpdate;
            if (!strategy.TryGetProperty("rollingUpdate", out rollingUpdate))
                return "";

            return GetStringValue(rollingUpdate, propertyName);
        }

        private List<KubernetesDeploymentContainer> GetContainers(JsonElement spec)
        {
            List<KubernetesDeploymentContainer> containers = new List<KubernetesDeploymentContainer>();

            JsonElement template;
            if (!spec.TryGetProperty("template", out template))
                return containers;

            JsonElement templateSpec;
            if (!template.TryGetProperty("spec", out templateSpec))
                return containers;

            JsonElement containersElement;
            if (!templateSpec.TryGetProperty("containers", out containersElement) || containersElement.ValueKind != JsonValueKind.Array)
                return containers;

            foreach (JsonElement container in containersElement.EnumerateArray())
            {
                KubernetesDeploymentContainer item = new KubernetesDeploymentContainer();
                item.Nome = GetStringValue(container, "name");
                item.Imagem = GetStringValue(container, "image");
                item.ImagePullPolicy = GetStringValue(container, "imagePullPolicy");
                item.Ports = GetPorts(container);
                item.Requests = GetResources(container, "requests");
                item.Limits = GetResources(container, "limits");
                item.Args = GetStringArray(container, "args");
                item.Env = GetEnv(container);
                containers.Add(item);
            }

            return containers;
        }

        private string GetPorts(JsonElement container)
        {
            List<string> ports = new List<string>();
            JsonElement portsElement;
            if (!container.TryGetProperty("ports", out portsElement) || portsElement.ValueKind != JsonValueKind.Array)
                return "";

            foreach (JsonElement port in portsElement.EnumerateArray())
            {
                string name = GetStringValue(port, "name");
                string number = GetStringValue(port, "containerPort");
                string protocol = GetStringValue(port, "protocol");
                string text = string.IsNullOrWhiteSpace(name) ? number : name + ":" + number;
                if (!string.IsNullOrWhiteSpace(protocol))
                    text += "/" + protocol;
                ports.Add(text);
            }

            return string.Join(", ", ports.ToArray());
        }

        private string GetResources(JsonElement container, string propertyName)
        {
            JsonElement resources;
            if (!container.TryGetProperty("resources", out resources))
                return "";

            JsonElement obj;
            if (!resources.TryGetProperty(propertyName, out obj) || obj.ValueKind != JsonValueKind.Object)
                return "";

            List<string> values = new List<string>();
            foreach (JsonProperty property in obj.EnumerateObject())
                values.Add(property.Name + "=" + (property.Value.ValueKind == JsonValueKind.String ? property.Value.GetString() : property.Value.ToString()));

            return string.Join(", ", values.ToArray());
        }

        private string GetStringArray(JsonElement element, string propertyName)
        {
            JsonElement array;
            if (!element.TryGetProperty(propertyName, out array) || array.ValueKind != JsonValueKind.Array)
                return "";

            List<string> values = new List<string>();
            foreach (JsonElement item in array.EnumerateArray())
                values.Add(item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : item.ToString());

            return string.Join(" ", values.ToArray());
        }

        private string GetEnv(JsonElement container)
        {
            JsonElement array;
            if (!container.TryGetProperty("env", out array) || array.ValueKind != JsonValueKind.Array)
                return "";

            List<string> values = new List<string>();
            foreach (JsonElement item in array.EnumerateArray())
            {
                string name = GetStringValue(item, "name");
                string value = GetStringValue(item, "value");
                if (string.IsNullOrWhiteSpace(value) && item.TryGetProperty("valueFrom", out JsonElement valueFrom))
                    value = valueFrom.ToString();
                values.Add(name + "=" + value);
            }

            return string.Join(", ", values.ToArray());
        }

        private List<KubernetesDeploymentCondition> GetConditions(JsonElement status)
        {
            List<KubernetesDeploymentCondition> conditions = new List<KubernetesDeploymentCondition>();
            JsonElement array;
            if (!status.TryGetProperty("conditions", out array) || array.ValueKind != JsonValueKind.Array)
                return conditions;

            foreach (JsonElement condition in array.EnumerateArray())
            {
                conditions.Add(new KubernetesDeploymentCondition
                {
                    Type = GetStringValue(condition, "type"),
                    Status = GetStringValue(condition, "status"),
                    Reason = GetStringValue(condition, "reason"),
                    Message = GetStringValue(condition, "message"),
                    LastUpdateTime = GetStringValue(condition, "lastUpdateTime"),
                    LastTransitionTime = GetStringValue(condition, "lastTransitionTime")
                });
            }

            return conditions;
        }

        private List<KubernetesDeploymentManagedField> GetManagedFields(JsonElement metadata)
        {
            List<KubernetesDeploymentManagedField> fields = new List<KubernetesDeploymentManagedField>();
            JsonElement managedFields;
            if (!metadata.TryGetProperty("managedFields", out managedFields) || managedFields.ValueKind != JsonValueKind.Array)
                return fields;

            foreach (JsonElement field in managedFields.EnumerateArray())
            {
                fields.Add(new KubernetesDeploymentManagedField
                {
                    Manager = GetStringValue(field, "manager"),
                    Operation = GetStringValue(field, "operation"),
                    ApiVersion = GetStringValue(field, "apiVersion"),
                    Time = GetStringValue(field, "time"),
                    FieldsType = GetStringValue(field, "fieldsType")
                });
            }

            return fields;
        }

        private string GetManagedBy(JsonElement metadata)
        {
            JsonElement managedFields;
            if (!metadata.TryGetProperty("managedFields", out managedFields) || managedFields.ValueKind != JsonValueKind.Array)
                return "unknown";

            foreach (JsonElement field in managedFields.EnumerateArray())
            {
                string manager = GetStringValue(field, "manager");
                if (!string.IsNullOrWhiteSpace(manager))
                    return manager;
            }

            return "unknown";
        }
    }
}
