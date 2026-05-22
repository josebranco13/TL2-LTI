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
                JsonElement items = doc.RootElement.GetProperty("items");

                foreach (JsonElement ns in items.EnumerateArray())
                {
                    JsonElement metadata = ns.GetProperty("metadata");
                    JsonElement status = ns.GetProperty("status");

                    int labelsCount = 0;
                    if (metadata.TryGetProperty("labels", out JsonElement labels))
                        labelsCount = CountProperties(labels);

                    string finalizers = "";
                    if (ns.TryGetProperty("spec", out JsonElement spec))
                        finalizers = GetArrayAsText(spec, "finalizers");

                    namespaces.Add(new KubernetesNamespaceSummary
                    {
                        Name = GetStringValue(metadata, "name"),
                        Phase = GetStringValue(status, "phase"),
                        CreationTimestamp = GetStringValue(metadata, "creationTimestamp"),
                        ResourceVersion = GetStringValue(metadata, "resourceVersion"),
                        Uid = GetStringValue(metadata, "uid"),
                        LabelsCount = labelsCount,
                        Finalizers = finalizers,
                        ManagedBy = GetManagedBy(metadata)
                    });
                }
            }

            return namespaces;
        }

        public async Task<List<KubernetesNamespaceDetails>> GetNamespaceDetailsAsync()
        {
            string json = await api.GetAsync("/api/v1/namespaces");
            List<KubernetesNamespaceDetails> result = new List<KubernetesNamespaceDetails>();

            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                JsonElement items = doc.RootElement.GetProperty("items");

                foreach (JsonElement ns in items.EnumerateArray())
                {
                    JsonElement metadata = ns.GetProperty("metadata");
                    JsonElement status = ns.GetProperty("status");

                    string finalizersText = "";
                    if (ns.TryGetProperty("spec", out JsonElement spec))
                        finalizersText = GetArrayAsText(spec, "finalizers");

                    KubernetesNamespaceDetails details = new KubernetesNamespaceDetails
                    {
                        Name = GetStringValue(metadata, "name"),
                        Phase = GetStringValue(status, "phase"),
                        Uid = GetStringValue(metadata, "uid"),
                        CreationTimestamp = GetStringValue(metadata, "creationTimestamp"),
                        ResourceVersion = GetStringValue(metadata, "resourceVersion"),
                        FinalizersText = finalizersText,
                        ManagedBy = GetManagedBy(metadata),
                        Labels = GetLabels(metadata),
                        Finalizers = GetFinalizers(ns),
                        ManagedFields = GetManagedFields(metadata)
                    };

                    result.Add(details);
                }
            }

            return result;
        }

        public async Task CreateNamespaceAsync(string name, string labelsText)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Indica o nome do namespace.");

            string safeName = name.Trim();
            string labelsJson = BuildLabelsJson(labelsText);

            string json = @"
{
  ""apiVersion"": ""v1"",
  ""kind"": ""Namespace"",
  ""metadata"": {
    ""name"": """ + safeName + @"""" + labelsJson + @"
  }
}";

            await api.PostAsync("/api/v1/namespaces", json);
        }

        public async Task DeleteNamespaceAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Seleciona um namespace para eliminar.");

            string safeName = Uri.EscapeDataString(name);
            await api.DeleteAsync("/api/v1/namespaces/" + safeName);
        }

        private string BuildLabelsJson(string labelsText)
        {
            if (string.IsNullOrWhiteSpace(labelsText))
                return "";

            StringBuilder sb = new StringBuilder();
            sb.Append(", \"labels\": {");

            string[] pairs = labelsText.Split(',');
            bool first = true;

            for (int i = 0; i < pairs.Length; i++)
            {
                string[] parts = pairs[i].Split('=');

                if (parts.Length != 2)
                    continue;

                string key = parts[0].Trim();
                string value = parts[1].Trim();

                if (key == "" || value == "")
                    continue;

                if (!first)
                    sb.Append(",");

                sb.Append("\"" + EscapeJson(key) + "\": \"" + EscapeJson(value) + "\"");
                first = false;
            }

            sb.Append("}");

            if (first)
                return "";

            return sb.ToString();
        }

        private string EscapeJson(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        private string GetStringValue(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement property))
                return "";

            return property.GetString() ?? "";
        }

        private int CountProperties(JsonElement element)
        {
            int count = 0;

            foreach (JsonProperty property in element.EnumerateObject())
                count++;

            return count;
        }

        private string GetArrayAsText(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement array))
                return "";

            List<string> values = new List<string>();

            foreach (JsonElement item in array.EnumerateArray())
                values.Add(item.GetString() ?? "");

            return string.Join(", ", values);
        }

        private string GetManagedBy(JsonElement metadata)
        {
            if (!metadata.TryGetProperty("managedFields", out JsonElement managedFields))
                return "";

            foreach (JsonElement field in managedFields.EnumerateArray())
            {
                string manager = GetStringValue(field, "manager");
                if (!string.IsNullOrWhiteSpace(manager))
                    return manager;
            }

            return "unknown";
        }

        private List<KubernetesKeyValue> GetLabels(JsonElement metadata)
        {
            List<KubernetesKeyValue> result = new List<KubernetesKeyValue>();

            if (!metadata.TryGetProperty("labels", out JsonElement labels))
                return result;

            foreach (JsonProperty property in labels.EnumerateObject())
            {
                result.Add(new KubernetesKeyValue
                {
                    Chave = property.Name,
                    Valor = property.Value.ToString()
                });
            }

            return result;
        }

        private List<KubernetesNamespaceFinalizer> GetFinalizers(JsonElement ns)
        {
            List<KubernetesNamespaceFinalizer> result = new List<KubernetesNamespaceFinalizer>();

            if (!ns.TryGetProperty("spec", out JsonElement spec))
                return result;

            if (!spec.TryGetProperty("finalizers", out JsonElement finalizers))
                return result;

            foreach (JsonElement finalizer in finalizers.EnumerateArray())
            {
                result.Add(new KubernetesNamespaceFinalizer
                {
                    Nome = finalizer.GetString() ?? ""
                });
            }

            return result;
        }

        private List<KubernetesNamespaceManagedField> GetManagedFields(JsonElement metadata)
        {
            List<KubernetesNamespaceManagedField> result = new List<KubernetesNamespaceManagedField>();

            if (!metadata.TryGetProperty("managedFields", out JsonElement managedFields))
                return result;

            foreach (JsonElement field in managedFields.EnumerateArray())
            {
                result.Add(new KubernetesNamespaceManagedField
                {
                    Manager = GetStringValue(field, "manager"),
                    Operation = GetStringValue(field, "operation"),
                    ApiVersion = GetStringValue(field, "apiVersion"),
                    Time = GetStringValue(field, "time"),
                    FieldsType = GetStringValue(field, "fieldsType")
                });
            }

            return result;
        }
    }
}
