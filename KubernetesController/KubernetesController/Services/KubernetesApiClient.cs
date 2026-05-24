using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KubernetesController.Services
{
    public class KubernetesApiClient
    {
        private readonly HttpClient _httpClient;
        private const int MaxRetries = 15;
        private const int RetryDelayMilliseconds = 2000;

        public KubernetesApiClient(string baseUrl, string token = null)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(60)
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token.Trim());
            }
        }

        public Task<string> GetAsync(string endpoint)
        {
            return SendWithRetryAsync(HttpMethod.Get, endpoint, null);
        }

        public Task<string> PostAsync(string endpoint, string jsonBody)
        {
            return SendWithRetryAsync(HttpMethod.Post, endpoint, jsonBody);
        }

        public Task<string> DeleteAsync(string endpoint)
        {
            return SendWithRetryAsync(HttpMethod.Delete, endpoint, "{}");
        }

        private async Task<string> SendWithRetryAsync(HttpMethod method, string endpoint, string jsonBody)
        {
            string cleanEndpoint = endpoint.TrimStart('/');
            Exception lastException = null;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    using (HttpRequestMessage request = new HttpRequestMessage(method, cleanEndpoint))
                    {
                        request.Version = HttpVersion.Version11;
                        request.Headers.ConnectionClose = true;

                        if (jsonBody != null)
                            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                        using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                        {
                            string content = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                                return content;

                            if (method == HttpMethod.Delete && response.StatusCode == HttpStatusCode.NotFound)
                                return content;

                            if (IsTemporaryStatusCode(response.StatusCode) && attempt < MaxRetries)
                            {
                                await Task.Delay(RetryDelayMilliseconds);
                                continue;
                            }

                            throw new Exception(
                                "Erro " + method.Method + " /" + cleanEndpoint + ": " + response.StatusCode + "\n" + content
                            );
                        }
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;

                    if (!IsTemporaryException(ex) || attempt >= MaxRetries)
                        break;

                    await Task.Delay(RetryDelayMilliseconds);
                }
            }

            throw new Exception(
                "Erro " + method.Method + " /" + cleanEndpoint + " depois de várias tentativas.\n" +
                (lastException != null ? lastException.Message : "Sem detalhe adicional.")
            );
        }

        private bool IsTemporaryStatusCode(HttpStatusCode statusCode)
        {
            int code = (int)statusCode;

            return code == 408 ||
                   code == 429 ||
                   code == 500 ||
                   code == 502 ||
                   code == 503 ||
                   code == 504;
        }

        private bool IsTemporaryException(Exception ex)
        {
            if (ex is HttpRequestException || ex is TaskCanceledException)
                return true;

            if (ex.InnerException != null)
                return IsTemporaryException(ex.InnerException);

            string message = ex.Message == null ? "" : ex.Message.ToLowerInvariant();

            return message.Contains("sending") ||
                   message.Contains("receiving") ||
                   message.Contains("ligação") ||
                   message.Contains("connection") ||
                   message.Contains("remoto") ||
                   message.Contains("remote") ||
                   message.Contains("peer") ||
                   message.Contains("closed") ||
                   message.Contains("timeout") ||
                   message.Contains("temporarily") ||
                   message.Contains("serviceunavailable") ||
                   message.Contains("starting");
        }
    }
}
