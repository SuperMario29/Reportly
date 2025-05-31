using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reportly.Models;

namespace Reportly.Services
{
    public class KlaviyoService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private readonly KlaviyoService _klaviyoService;


        public KlaviyoService(HttpClient http, AppSettings config)
        {
            _http = http;
            _apiKey = config.Klaviyo.ApiKey;
            _http.BaseAddress = new Uri(config.Klaviyo.BaseUrl);
            _http.DefaultRequestHeaders.Add("Authorization", $"Klaviyo-API-Key {_apiKey}");
        }


        public KlaviyoService(KlaviyoService klaviyoService)
        {
            _klaviyoService = klaviyoService;
        }

        public async Task<List<KlaviyoListsResponse>> GetListsAsync()
        {
            var response = await _http.GetAsync("lists");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<KlaviyoListsResponse>(content)?.Data ?? new();
        }

        public async Task<KlaviyoMetrics> GetEmailMetrics(DateTime startDate, DateTime endDate)
        {
            var metrics = new KlaviyoMetrics();
            
            // Get list metrics
            var lists = await _klaviyoService.GetListsAsync();
            metrics.TotalSubscribers = lists.Sum(l => l.ListSize);
            metrics.ActiveSubscribers = lists.Sum(l => l.ActiveCount);

            // Get campaign metrics
            var campaigns = await GetCampaignsInDateRange(startDate, endDate);
            metrics.RecentCampaigns = campaigns
                .OrderByDescending(c => c.SendTime)
                .Take(3)
                .ToList();

            if (campaigns.Any())
            {
                metrics.OpenRate = campaigns.Average(c => c.Opens);
                metrics.ClickRate = campaigns.Average(c => c.Clicks);
                metrics.ConversionRate = campaigns.Average(c => c.Conversions);
                metrics.Unsubscribes = campaigns.Sum(c => c.Unsubscribes);
            }

            return metrics;
        }
        
        public async Task<KlaviyoMetrics> GetEmailMetrics()
        {
            var metrics = new KlaviyoMetrics();
            
            // Get list metrics
            var lists = await _klaviyoService.GetListsAsync();
            metrics.TotalSubscribers = lists.Sum(l => l.ListSize);
            metrics.ActiveSubscribers = lists.Sum(l => l.ActiveCount);

            // Get campaign metrics
            var campaigns = await GetCampaigns();
            metrics.RecentCampaigns = campaigns
                .OrderByDescending(c => c.SendTime)
                .Take(3)
                .ToList();

            if (campaigns.Any())
            {
                metrics.OpenRate = campaigns.Average(c => c.Opens);
                metrics.ClickRate = campaigns.Average(c => c.Clicks);
                metrics.ConversionRate = campaigns.Average(c => c.Conversions);
                metrics.Unsubscribes = campaigns.Sum(c => c.Unsubscribes);
            }

            return metrics;
        }

        private async Task<List<CampaignPerformance>> GetCampaignsInDateRange(DateTime startDate, DateTime endDate)
        {
            // Klaviyo API endpoint for metrics
            var response = await _klaviyoService.Http.GetAsync(
                $"campaigns?filter=greater-than(send_time,{startDate:yyyy-MM-dd})" +
                $"&filter=less-than(send_time,{endDate:yyyy-MM-dd})" +
                "&include=metrics");

            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);

            return json["data"]?
                .Select(item => new CampaignPerformance
                {
                    CampaignId = item["id"]?.ToString(),
                    Name = item["attributes"]?["name"]?.ToString(),
                    Recipients = item["attributes"]?["recipients"]?["count"]?.Value<int>() ?? 0,
                    Opens = GetMetricValue(item, "opened"),
                    Clicks = GetMetricValue(item, "clicked"),
                    Conversions = GetMetricValue(item, "converted"),
                    Unsubscribes = GetMetricValue(item, "unsubscribed")
                })
                .ToList() ?? new List<CampaignPerformance>();
        }

      private async Task<List<CampaignPerformance>> GetCampaigns()
        {
            // Klaviyo API endpoint for metrics
            var response = await _klaviyoService.Http.GetAsync(
                $"campaigns?include=metrics");
            
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            
            return json["data"]?
                .Select(item => new CampaignPerformance
                {
                    CampaignId = item["id"]?.ToString(),
                    Name = item["attributes"]?["name"]?.ToString(),
                    Recipients = item["attributes"]?["recipients"]?["count"]?.Value<int>() ?? 0,
                    Opens = GetMetricValue(item, "opened"),
                    Clicks = GetMetricValue(item, "clicked"),
                    Conversions = GetMetricValue(item, "converted"),
                    Unsubscribes = GetMetricValue(item, "unsubscribed")
                })
                .ToList() ?? new List<CampaignPerformance>();
        }

        private int GetMetricValue(JToken campaignItem, string metricType)
        {
            return campaignItem["relationships"]?["metrics"]?["data"]?
                .FirstOrDefault(m => m["type"]?.ToString() == metricType)?
                ["value"]?.Value<int>() ?? 0;
        }
    }

    public class KlaviyoListsResponse
    {
        [JsonProperty("data")]
        public List<KlaviyoListsResponse> Data { get; set; }

        public int ListSize { get; set; }

        public int ActiveCount { get; set; }
    }

    
}