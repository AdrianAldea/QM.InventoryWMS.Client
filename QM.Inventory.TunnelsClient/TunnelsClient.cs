using System.Collections;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tunnels.Core.Models;
using Tunnels.Core.Views;

namespace QM.Inventory.TunnelsClient {
    public class TunnelsClient {
        private static readonly string _url = ConfigurationManager.AppSettings["TunnelsURL"];
        public static string Url => _url;
        private static HttpClient? _httpClient = null;
        public static HttpClient _HttpClient {
            get {
                if (_httpClient == null) {
                    _httpClient = new HttpClient {
                        BaseAddress = new Uri(uriString: Url)
                    };
                    _httpClient.Timeout = TimeSpan.FromHours(1);
                    _httpClient.DefaultRequestHeaders.Accept.Clear();
                    _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                return _httpClient;
            }
            set { _httpClient = value; }
        }

        public static async Task<User?> ValidateUsernameAndPassword(string username, string password) {
            HttpResponseMessage response = await _HttpClient.GetAsync($"api/users/validate?username={username}&password={password}");
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            User? user = JsonSerializer.Deserialize<User?>(
                responseStream,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            if (user == null) {
                return await Task.FromResult<User?>(result: null);
            }
            else {
                return await Task.FromResult<User?>(result: user);
            }
        }

        public static async Task<List<User>> GetAllUsersAsync() {
            HttpResponseMessage response = await _HttpClient.GetAsync($"api/users");
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var users = JsonSerializer.Deserialize<List<User>>(
                responseStream,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            return users.Any() ? await Task.FromResult(result: users) : await Task.FromResult(result: new List<User>());
        }

        public static async Task<List<OrdersWithProductsView>> GetAllOrdersWithProductsByFilterAsync(OrdersWithProductsFilterRequest ordersWithProductsFilter) {
           HttpResponseMessage response = await _HttpClient.PostAsJsonAsync($"api/orders/GetAllOrdersWithProductsByFilterAsync", ordersWithProductsFilter);
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var orders = JsonSerializer.Deserialize<List<OrdersWithProductsView>>(
                responseStream,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            return orders.Any() ? await Task.FromResult(result: orders) : await Task.FromResult(result: new List<OrdersWithProductsView>());
        }

        public static async Task<double> GetSumOfOrders(OrdersWithProductsFilterRequest ordersWithProductsFilter)
        {
            HttpResponseMessage response = await _HttpClient.PostAsJsonAsync($"api/orders/GetSumOfOrders", ordersWithProductsFilter);
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var sum = JsonSerializer.Deserialize<double>(
                responseStream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            return sum;
        }

        public static async Task<List<Product>> GetAllProductsAsync(bool? isActive) {
            HttpResponseMessage response = await _HttpClient.GetAsync($"api/products?isActive={isActive}");
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var products = JsonSerializer.Deserialize<List<Product>>(
                responseStream,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            return products.Any() ? await Task.FromResult(result: products) : await Task.FromResult(result: new List<Product>());
        }

        public static async Task UpdateAllProductsAsync(IEnumerable<Product> products)
        {
            HttpResponseMessage response = await _HttpClient.PostAsJsonAsync($"api/products", products);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<Order> CreateOrderWithProductAsync(Order product) {
            HttpResponseMessage response = await _HttpClient.PostAsJsonAsync($"api/orders", product);
            response.EnsureSuccessStatusCode();

            Stream responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var ordertResponse = JsonSerializer.Deserialize<Order>(
                responseStream,
                new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true,
                    DefaultBufferSize = 128
                });

            return ordertResponse
                ?? await Task.FromResult(result: new Order());
        }

        public static async Task CreateUser(User user) {
            HttpResponseMessage response = await _HttpClient.PostAsJsonAsync($"api/users", user);
            response.EnsureSuccessStatusCode();

            await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        public static async Task DeleteProductById(int productId) {
            HttpResponseMessage response = await _HttpClient.DeleteAsync($"api/products/{productId}");
            response.EnsureSuccessStatusCode();
        }

        public static async Task InactivateOrder(int orderId) {
            HttpResponseMessage response = await _HttpClient.DeleteAsync($"api/orders/{orderId}");
            response.EnsureSuccessStatusCode();
        }
    }
}