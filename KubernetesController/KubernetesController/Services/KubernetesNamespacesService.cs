using KubernetesController.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesNamespacesService
    {
        private readonly KubernetesApiClient api;

        public KubernetesNamespacesService(KubernetesApiClient api)
        {
            this.api = api;
        }

        public async Task<List<KubernetesNamespaceSummary>> GetNamespacesAsync()
        {
            string json = await api.GetAsync("/api/v1/namespaces");
            List<KubernetesNamespaceSummary> namespaces = new List<KubernetesNamespaceSummary>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (!doc.RootElement.TryGetProperty("items", out JsonElement items))
                    return namespaces;

                foreach (JsonElement ns in items.EnumerateArray())
                {
                    KubernetesNamespaceDetails details = ParseNamespaceDetails(ns);

                    namespaces.Add(new KubernetesNamespaceSummary
                    {
                        Name = details.Name,
                        Status = details.Phase,
                        CreatedAt = details.CreationTimestamp,
                        ResourceVersion = details.ResourceVersion,
                        Uid = details.Uid,
                        Labels = details.Labels.Count,
                        Finalizers = details.FinalizersText,
                        ManagedBy = details.ManagedBy
                    });
                }
            }

            return namespaces;
        }

        public async Task<List<KubernetesNamespaceDetails>> GetNamespaceDetailsAsync()
        {
            string json = await api.GetAsync("/api/v1/namespaces");
            List<KubernetesNamespaceDetails> namespaces = new List<KubernetesNamespaceDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (!doc.RootElement.TryGetProperty("items", out JsonElement items))
                    return namespaces;

                foreach (JsonElement ns in items.EnumerateArray())
                    namespaces.Add(ParseNamespaceDetails(ns));
            }

            return namespaces;
        }

        public async Task<string> GetNamespaceJsonAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Seleciona um namespace válido.");

            string cleanName = name.Trim();
            string encodedName = Uri.EscapeDataString(cleanName);
            string json = await api.GetAsync("/api/v1/namespaces/" + encodedName);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonSerializerOptions options = new JsonSerializerOptions();
                options.WriteIndented = true;
                return JsonSerializer.Serialize(doc.RootElement, options);
            }
        }

        public async Task<string> GetNamespaceYamlAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Seleciona um namespace válido.");

            string json = await GetNamespaceJsonAsync(name);

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                StringBuilder yaml = new StringBuilder();
                WriteYamlElement(yaml, doc.RootElement, 0, null);
                return yaml.ToString();
            }
        }

        public async Task CreateNamespaceAsync(string name, string labelsText)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("O nome do namespace não pode estar vazio.");

            Dictionary<string, object> metadata = new Dictionary<string, object>();
            metadata["name"] = name.Trim();

            Dictionary<string, string> labels = ParseLabels(labelsText);
            if (labels.Count > 0)
                metadata["labels"] = labels;

            Dictionary<string, object> body = new Dictionary<string, object>();
            body["apiVersion"] = "v1";
            body["kind"] = "Namespace";
            body["metadata"] = metadata;

            string json = JsonSerializer.Serialize(body);
            await api.PostAsync("/api/v1/namespaces", json);
        }

        public async Task DeleteNamespaceAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Seleciona um namespace válido.");

            string cleanName = name.Trim();
            string encodedName = Uri.EscapeDataString(cleanName);

            await api.DeleteAsync("/api/v1/namespaces/" + encodedName);

            // O K3s pode demorar alguns segundos a finalizar o namespace e, por vezes,
            // responde temporariamente com 503 "starting" logo após o DELETE.
            await Task.Delay(2000);
        }

        private KubernetesNamespaceDetails ParseNamespaceDetails(JsonElement ns)
        {
            KubernetesNamespaceDetails details = new KubernetesNamespaceDetails();

            JsonElement metadata;
            if (ns.TryGetProperty("metadata", out metadata))
            {
                details.Name = GetStringValue(metadata, "name");
                details.Uid = GetStringValue(metadata, "uid");
                details.ResourceVersion = GetStringValue(metadata, "resourceVersion");
                details.CreationTimestamp = GetStringValue(metadata, "creationTimestamp");
                details.ManagedBy = GetManagedBy(metadata);
                details.Labels = GetKeyValueList(metadata, "labels");
                details.ManagedFields = GetManagedFields(metadata);
            }

            JsonElement status;
            if (ns.TryGetProperty("status", out status))
                details.Phase = GetStringValue(status, "phase");

            JsonElement spec;
            if (ns.TryGetProperty("spec", out spec))
            {
                details.Finalizers = GetFinalizers(spec);
                details.FinalizersText = GetFinalizersText(details.Finalizers);
            }

            return details;
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
                    throw new ArgumentException("As labels devem estar no formato chave=valor, exemplo: ambiente=teste,app=web");

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Existe uma label sem chave.");

                labels[key] = value;
            }

            return labels;
        }

        private void WriteYamlElement(StringBuilder yaml, JsonElement element, int indent, string key)
        {
            string currentIndent = new string(' ', indent);

            if (!string.IsNullOrEmpty(key))
            {
                if (element.ValueKind == JsonValueKind.Object || element.ValueKind == JsonValueKind.Array)
                {
                    yaml.Append(currentIndent).Append(FormatYamlKey(key)).AppendLine(":");
                    WriteYamlElement(yaml, element, indent + 2, null);
                    return;
                }

                yaml.Append(currentIndent)
                    .Append(FormatYamlKey(key))
                    .Append(": ")
                    .AppendLine(FormatYamlScalar(element));
                return;
            }

            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                    WriteYamlElement(yaml, property.Value, indent, property.Name);

                return;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in element.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        yaml.Append(currentIndent).AppendLine("-");

                        foreach (JsonProperty property in item.EnumerateObject())
                            WriteYamlElement(yaml, property.Value, indent + 2, property.Name);
                    }
                    else if (item.ValueKind == JsonValueKind.Array)
                    {
                        yaml.Append(currentIndent).AppendLine("-");
                        WriteYamlElement(yaml, item, indent + 2, null);
                    }
                    else
                    {
                        yaml.Append(currentIndent)
                            .Append("- ")
                            .AppendLine(FormatYamlScalar(item));
                    }
                }

                return;
            }

            yaml.Append(currentIndent).AppendLine(FormatYamlScalar(element));
        }

        private string FormatYamlKey(string key)
        {
            return QuoteYamlString(key);
        }

        private string FormatYamlScalar(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return QuoteYamlString(element.GetString() ?? "");
                case JsonValueKind.Number:
                    return element.ToString();
                case JsonValueKind.True:
                    return "true";
                case JsonValueKind.False:
                    return "false";
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return "null";
                default:
                    return QuoteYamlString(element.ToString());
            }
        }

        private string QuoteYamlString(string value)
        {
            if (value == null)
                value = "";

            return "\"" + value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n") + "\"";
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

        private List<KubernetesNamespaceFinalizer> GetFinalizers(JsonElement spec)
        {
            List<KubernetesNamespaceFinalizer> finalizers = new List<KubernetesNamespaceFinalizer>();

            JsonElement array;
            if (!spec.TryGetProperty("finalizers", out array) || array.ValueKind != JsonValueKind.Array)
                return finalizers;

            foreach (JsonElement item in array.EnumerateArray())
            {
                finalizers.Add(new KubernetesNamespaceFinalizer
                {
                    Nome = item.ValueKind == JsonValueKind.String ? item.GetString() ?? "" : item.ToString()
                });
            }

            return finalizers;
        }

        private string GetFinalizersText(List<KubernetesNamespaceFinalizer> finalizers)
        {
            List<string> values = new List<string>();

            foreach (KubernetesNamespaceFinalizer finalizer in finalizers)
                values.Add(finalizer.Nome);

            return string.Join(", ", values);
        }

        private List<KubernetesNamespaceManagedField> GetManagedFields(JsonElement metadata)
        {
            List<KubernetesNamespaceManagedField> fields = new List<KubernetesNamespaceManagedField>();

            JsonElement managedFields;
            if (!metadata.TryGetProperty("managedFields", out managedFields) || managedFields.ValueKind != JsonValueKind.Array)
                return fields;

            foreach (JsonElement field in managedFields.EnumerateArray())
            {
                fields.Add(new KubernetesNamespaceManagedField
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
