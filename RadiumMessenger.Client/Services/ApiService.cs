using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace RadiumMessenger.Client.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://127.0.0.1:7199/")
            };
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            return await _httpClient.GetFromJsonAsync<T>(url);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(
            string url,
            TRequest data)
        {
            var response = await _httpClient.PostAsJsonAsync(url, data);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TResponse>();
        }

        public async Task PostAsync(string url)
        {
            var response = await _httpClient.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(string url)
        {
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
    }
}