using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reportly.Models;
using RestSharp;

namespace Reportly.Services
{
    public class FacebookService
    {
        private readonly HttpClient _http;
        private readonly string _apiVersion;

        private readonly string _apiID;


        public FacebookService(HttpClient http, AppSettings config)
        {
            _http = http;
            _apiID = config.Facebook.AppId;
            _apiVersion = config.Facebook.ApiVersion;
            _http.BaseAddress = new Uri($"https://graph.facebook.com/{_apiVersion}/");
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", config.Facebook.AccessToken);
        }

        public async Task<List<AdCampaignPerformance>> GetCampaignsAsync()
        {
            var response = await _http.GetAsync($"act_{_apiID}/campaigns");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookCampaignsResponse>(content)?.Data ?? new();
        }

        public static FacebookAdMetrics FetchFacebookMetrics()
        {
            const string accessToken = "YOUR_ACCESS_TOKEN";
            const string adAccountId = "act_YOUR_AD_ACCOUNT_ID";

            var client = new RestClient("https://graph.facebook.com/v19.0/");
            var request = new RestRequest($"{adAccountId}/insights", Method.Get);
            request.AddParameter("date_preset", "last_30d");
            request.AddParameter("fields", "spend,purchases,purchase_value,actions");

            var response = client.Execute(request);
            if (!response.IsSuccessful) throw new Exception($"API Error: {response.ErrorMessage}");

            var insight = JObject.Parse(response.Content)["data"][0];
            return new FacebookAdMetrics(insight);
        }
    }

    public class FacebookCampaignsResponse
    {
        [JsonProperty("data")]
        public List<AdCampaignPerformance> Data { get; set; }
    }

    public class FacebookMetricsResponse
    {
        [JsonProperty("data")]
        public List<FacebookMetrics> Data { get; set; }
    }
}