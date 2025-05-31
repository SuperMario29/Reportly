using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reportly.Models
{
    public class ShopifyMetrics
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int NewCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ConversionRate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Product performance metrics
        public List<ProductPerformance> TopProducts { get; set; } = new();
    }

    public class ProductPerformance
    {
        public long ProductId { get; set; }
        public string Title { get; set; }
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ShopifyOrdersResponse
    {
        [JsonProperty("orders")]
        public List<ShopifyOrder> Orders { get; set; }
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

    public class ShopifyProduct
    {
        
    }
}