using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesApiClient
    {
        private readonly HttpClient _httpClient;

        public KubernetesApiClient(string baseUrl, string token = null)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Trim());
            }
        }

        public async Task<string> GetAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint.TrimStart('/'));
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro GET " + endpoint + ": " + response.StatusCode + "\n" + content);

            return content;
        }

        public async Task<string> PostAsync(string endpoint, string jsonBody)
        {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint.TrimStart('/'), content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Erro POST " + endpoint + ": " + response.StatusCode + "\n" + responseContent);

            return responseContent;
        }

        public async Task<string> DeleteAsync(string endpoint)
        {
            for (int attempt = 1; attempt <= 5; attempt++)
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint.TrimStart('/'));
                string content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                    return content;

                if ((int)response.StatusCode == 503 && attempt < 5)
                {
                    await Task.Delay(2000);
                    continue;
                }

                throw new Exception(
                    $"Erro DELETE {endpoint}: {response.StatusCode}\n{content}"
                );
            }

            throw new Exception("Erro inesperado ao executar DELETE.");
        }
    }
}
