using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reportly.Models;

namespace Reportly.Services
{
    public class ShopifyService
    {
        private readonly HttpClient _http;
        private readonly string _apiVersion = "2023-10";

        public ShopifyService(HttpClient http, AppSettings config)
        {
            _http = http;
            _http.BaseAddress = new Uri($"https://{config.Shopify.StoreName}.myshopify.com/admin/api/{_apiVersion}/");
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{config.Shopify.ApiKey}:{config.Shopify.Password}")));
        }

        public async Task<List<ShopifyProduct>> GetProductsAsync()
        {
            var response = await _http.GetAsync("products.json");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShopifyProductsResponse>(content)?.Products ?? new();
        }



        public async Task<ShopifyMetrics> GetStoreMetrics(DateTime startDate, DateTime endDate)
        {
            var metrics = new ShopifyMetrics
            {
                StartDate = startDate,
                EndDate = endDate
            };

            // Get orders for the period
            var orders = await GetOrdersInDateRange(startDate, endDate);

            metrics.TotalOrders = orders.Count;
            metrics.TotalRevenue = orders.Sum(o => o.TotalPrice);
            metrics.AverageOrderValue = metrics.TotalOrders > 0
                ? metrics.TotalRevenue / metrics.TotalOrders
                : 0;

            // Get customer counts (implementation depends on your Shopify API version)
            metrics.NewCustomers = await GetNewCustomerCount(startDate, endDate);

            // Get top products
            metrics.TopProducts = await GetTopProducts(startDate, endDate);

            // Conversion rate would typically come from analytics integration
            // This is a placeholder implementation
            metrics.ConversionRate = await CalculateConversionRate(startDate, endDate, metrics);

            return metrics;
        }

        public async Task<ShopifyMetrics> GetStoreMetrics()
        {
            var metrics = new ShopifyMetrics
            {
            };

            // Get orders for the period
            var orders = await GetOrders();

            metrics.TotalOrders = orders.Count;
            metrics.TotalRevenue = orders.Sum(o => o.TotalPrice);
            metrics.AverageOrderValue = metrics.TotalOrders > 0
                ? metrics.TotalRevenue / metrics.TotalOrders
                : 0;

            // Get customer counts (implementation depends on your Shopify API version)
            metrics.NewCustomers = await GetNewCustomerCount();

            // Get top products
            metrics.TopProducts = await GetTopProducts();

            // Conversion rate would typically come from analytics integration
            // This is a placeholder implementation
            metrics.ConversionRate = await CalculateConversionRate(metrics);

            return metrics;
        }

        private async Task<List<Models.ShopifyOrder>> GetOrdersInDateRange(DateTime startDate, DateTime endDate)
        {
            // Implementation depends on your Shopify API version
            // This is a simplified example
            var response = await _http.GetAsync(
                $"orders.json?created_at_min={startDate:yyyy-MM-dd}&created_at_max={endDate:yyyy-MM-dd}&status=any");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ShopifyOrdersResponse>(content)?.Orders ?? new();
        }

        private async Task<List<Models.ShopifyOrder>> GetOrders()
        {
            // Implementation depends on your Shopify API version
            // This is a simplified example
            var response = await _http.GetAsync(
                $"orders.json?status=any");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ShopifyOrdersResponse>(content)?.Orders ?? new();
            return result;
        }

        private async Task<int> GetNewCustomerCount(DateTime startDate, DateTime endDate)
        {
            var response = await _http.GetAsync(
                $"customers/count.json?created_at_min={startDate:yyyy-MM-dd}&created_at_max={endDate:yyyy-MM-dd}");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            return json["count"]?.Value<int>() ?? 0;
        }

        private async Task<int> GetNewCustomerCount()
        {
            var response = await _http.GetAsync(
                $"customers/count.json");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            return json["count"]?.Value<int>() ?? 0;
        }

        private async Task<List<ProductPerformance>> GetTopProducts(DateTime startDate, DateTime endDate)
        {
            // This is a simplified approach - in production you might need to analyze line items
            var orders = await GetOrdersInDateRange(startDate, endDate);

            return orders
                .SelectMany(o => o.LineItems)
                .GroupBy(li => new { li.ProductId, li.Title })
                .Select(g => new ProductPerformance
                {
                    ProductId = g.Key.ProductId,
                    Title = g.Key.Title,
                    UnitsSold = g.Sum(li => li.Quantity),
                    Revenue = g.Sum(li => li.Price * li.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToList();
        }

        private async Task<List<ProductPerformance>> GetTopProducts()
        {
            // This is a simplified approach - in production you might need to analyze line items
            var orders = await GetOrders();

            return orders
                .SelectMany(o => o.LineItems)
                .GroupBy(li => new { li.ProductId, li.Title })
                .Select(g => new ProductPerformance
                {
                    ProductId = g.Key.ProductId,
                    Title = g.Key.Title,
                    UnitsSold = g.Sum(li => li.Quantity),
                    Revenue = g.Sum(li => li.Price * li.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(5)
                .ToList();
        }

        private async Task<decimal> CalculateConversionRate(DateTime startDate, DateTime endDate, ShopifyMetrics metrics)
        {
            // Placeholder - in a real app you'd integrate with Shopify Analytics or Google Analytics
            // This is a simplified example using session counts from the API (if available)
            try
            {
                var sessionsResponse = await _http.GetAsync(
                    $"analytics/sessions.json?start_date={startDate:yyyy-MM-dd}&end_date={endDate:yyyy-MM-dd}");

                if (sessionsResponse.IsSuccessStatusCode)
                {
                    var content = await sessionsResponse.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    var sessions = json["sessions"]?.Value<int>() ?? 0;

                    return sessions > 0
                        ? (decimal)metrics.TotalOrders / sessions * 100
                        : 0;
                }
            }
            catch { /* API might not support this endpoint */ }

            return 0; // Default value if we can't calculate
        }

        private async Task<decimal> CalculateConversionRate(ShopifyMetrics metrics)
        {
            // Placeholder - in a real app you'd integrate with Shopify Analytics or Google Analytics
            // This is a simplified example using session counts from the API (if available)
            try
            {
                var sessionsResponse = await _http.GetAsync(
                    $"analytics/sessions.json");

                if (sessionsResponse.IsSuccessStatusCode)
                {
                    var content = await sessionsResponse.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    var sessions = json["sessions"]?.Value<int>() ?? 0;

                    return sessions > 0
                        ? (decimal)metrics.TotalOrders / sessions * 100
                        : 0;
                }
            }
            catch { /* API might not support this endpoint */ }

            return 0; // Default value if we can't calculate
        }
    }

    public class ShopifyProductsResponse
    {
        [JsonProperty("products")]
        public List<ShopifyProduct> Products { get; set; }
    }
    
    public class ShopifyOrder
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        
        [JsonProperty("total_price")]
        public decimal TotalPrice { get; set; }
        
        [JsonProperty("line_items")]
        public List<LineItem> LineItems { get; set; }
    }

    public class LineItem
    {
        [JsonProperty("product_id")]
        public long ProductId { get; set; }
        
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("price")]
        public decimal Price { get; set; }
        
        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}