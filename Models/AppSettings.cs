using System;

namespace Reportly.Models
{
    public class AppSettings
    {
        public ShopifyConfig Shopify { get; set; }
        public FacebookConfig Facebook { get; set; }
        public KlaviyoConfig Klaviyo { get; set; }
    }

    public class ShopifyConfig
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string StoreName { get; set; }
        public string ApiKey { get; set; }
        public string Password { get; set; }
            public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
    }

    public class FacebookConfig
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string AccessToken { get; set; }
        public string AppId { get; set; }
        public string ApiVersion { get; set; } = "v16.0";
        
            public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
    }

    public class KlaviyoConfig
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "https://a.klaviyo.com/api/";
        
            public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    public string CreatedBy
    {
        get; set;
    }

    public string UpdateBy { get; set; }
    }
}