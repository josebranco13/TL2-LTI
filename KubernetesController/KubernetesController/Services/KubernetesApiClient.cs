using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace KubernetesController.Services
{
    internal class KubernetesApiClient
    {
        private readonly HttpClient _httpClient;

        public KubernetesApiClient(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/")
            };
        }

        public async Task<string> GetAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(endpoint.TrimStart('/'));
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro GET {endpoint}: {response.StatusCode}\n{content}");

            return content;
        }

        public async Task<string> PostAsync(string endpoint, string jsonBody)
        {
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _httpClient.PostAsync(endpoint.TrimStart('/'), content);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro POST {endpoint}: {response.StatusCode}\n{responseContent}");

            return responseContent;
        }

        public async Task<string> DeleteAsync(string endpoint)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync(endpoint.TrimStart('/'));
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Erro DELETE {endpoint}: {response.StatusCode}\n{content}");

            return content;
        }
    }
}
