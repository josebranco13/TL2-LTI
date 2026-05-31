using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace KubernetesController
{
    public partial class DashboardForm
    {
        private string ShowExportFormatDialog(string title)
        {
            string selectedFormat = null;

            using (Form formatForm = new Form())
            {
                formatForm.Text = title;
                formatForm.StartPosition = FormStartPosition.CenterParent;
                formatForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                formatForm.MaximizeBox = false;
                formatForm.MinimizeBox = false;
                formatForm.ShowInTaskbar = false;
                formatForm.ClientSize = new Size(330, 130);

                Label lblQuestion = new Label();
                lblQuestion.Text = "Escolhe o formato de exportação:";
                lblQuestion.Location = new Point(25, 22);
                lblQuestion.Size = new Size(280, 25);
                lblQuestion.TextAlign = ContentAlignment.MiddleCenter;
                formatForm.Controls.Add(lblQuestion);

                Button btnYaml = new Button();
                btnYaml.Text = "YAML";
                btnYaml.Location = new Point(55, 70);
                btnYaml.Size = new Size(95, 35);
                btnYaml.Click += delegate
                {
                    selectedFormat = "yaml";
                    formatForm.DialogResult = DialogResult.OK;
                    formatForm.Close();
                };
                formatForm.Controls.Add(btnYaml);

                Button btnJson = new Button();
                btnJson.Text = "JSON";
                btnJson.Location = new Point(180, 70);
                btnJson.Size = new Size(95, 35);
                btnJson.Click += delegate
                {
                    selectedFormat = "json";
                    formatForm.DialogResult = DialogResult.OK;
                    formatForm.Close();
                };
                formatForm.Controls.Add(btnJson);

                formatForm.AcceptButton = btnYaml;
                formatForm.ShowDialog(this);
            }

            return selectedFormat;
        }

        private async Task ExportResourceAsync(string endpoint, string defaultFileName, string title)
        {
            if (api == null)
                return;

            string format = ShowExportFormatDialog(title);
            if (string.IsNullOrWhiteSpace(format))
                return;

            string json = await api.GetAsync(endpoint);
            string content = BuildExportContent(json, format);
            string safeBaseName = SanitizeExportFileName(defaultFileName);

            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Title = title;
                dialog.FileName = safeBaseName + "." + format;
                dialog.DefaultExt = format;
                dialog.AddExtension = true;
                dialog.OverwritePrompt = true;
                dialog.Filter = format == "yaml"
                    ? "YAML (*.yaml)|*.yaml"
                    : "JSON (*.json)|*.json";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                File.WriteAllText(dialog.FileName, content, new UTF8Encoding(false));
            }

            MessageBox.Show("Exportação concluída com sucesso.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private string BuildExportContent(string json, string format)
        {
            using (JsonDocument doc = JsonDocument.Parse(json))
            {
                if (format == "json")
                {
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.WriteIndented = true;
                    return JsonSerializer.Serialize(doc.RootElement, options);
                }

                return ConvertJsonToYaml(doc.RootElement);
            }
        }

        private string ConvertJsonToYaml(JsonElement element)
        {
            StringBuilder builder = new StringBuilder();
            WriteYamlValue(builder, element, 0);
            return builder.ToString();
        }

        private void WriteYamlValue(StringBuilder builder, JsonElement element, int indent)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                foreach (JsonProperty property in element.EnumerateObject())
                    WriteYamlProperty(builder, property, indent);
                return;
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                WriteYamlArray(builder, element, indent);
                return;
            }

            builder.Append(new string(' ', indent));
            builder.AppendLine(FormatYamlScalar(element));
        }

        private void WriteYamlProperty(StringBuilder builder, JsonProperty property, int indent)
        {
            JsonElement value = property.Value;
            string spacing = new string(' ', indent);

            if (value.ValueKind == JsonValueKind.Object)
            {
                if (!HasObjectProperties(value))
                {
                    builder.AppendLine(spacing + property.Name + ": {}");
                    return;
                }

                builder.AppendLine(spacing + property.Name + ":");
                WriteYamlValue(builder, value, indent + 2);
                return;
            }

            if (value.ValueKind == JsonValueKind.Array)
            {
                if (value.GetArrayLength() == 0)
                {
                    builder.AppendLine(spacing + property.Name + ": []");
                    return;
                }

                builder.AppendLine(spacing + property.Name + ":");
                WriteYamlArray(builder, value, indent + 2);
                return;
            }

            builder.AppendLine(spacing + property.Name + ": " + FormatYamlScalar(value));
        }

        private void WriteYamlArray(StringBuilder builder, JsonElement array, int indent)
        {
            string spacing = new string(' ', indent);

            foreach (JsonElement item in array.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    if (!HasObjectProperties(item))
                    {
                        builder.AppendLine(spacing + "- {}");
                    }
                    else
                    {
                        builder.AppendLine(spacing + "-");
                        WriteYamlValue(builder, item, indent + 2);
                    }
                }
                else if (item.ValueKind == JsonValueKind.Array)
                {
                    if (item.GetArrayLength() == 0)
                    {
                        builder.AppendLine(spacing + "- []");
                    }
                    else
                    {
                        builder.AppendLine(spacing + "-");
                        WriteYamlArray(builder, item, indent + 2);
                    }
                }
                else
                {
                    builder.AppendLine(spacing + "- " + FormatYamlScalar(item));
                }
            }
        }

        private bool HasObjectProperties(JsonElement element)
        {
            foreach (JsonProperty ignored in element.EnumerateObject())
                return true;

            return false;
        }

        private string FormatYamlScalar(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String)
                return QuoteYamlString(element.GetString() ?? "");

            if (element.ValueKind == JsonValueKind.Number)
                return element.ToString();

            if (element.ValueKind == JsonValueKind.True)
                return "true";

            if (element.ValueKind == JsonValueKind.False)
                return "false";

            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
                return "null";

            return QuoteYamlString(element.ToString());
        }

        private string QuoteYamlString(string value)
        {
            return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n") + "\"";
        }

        private string SanitizeExportFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "export";

            string clean = fileName.Trim();
            char[] invalid = Path.GetInvalidFileNameChars();

            foreach (char c in invalid)
                clean = clean.Replace(c, '-');

            return clean;
        }
    }
}
