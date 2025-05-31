using System;
using System.Collections.Generic;

namespace Reportly.Models
{
    public class FacebookMetrics
    {
        public decimal TotalSpend { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public decimal CTR { get; set; } // Click-through rate
        public decimal CPC { get; set; } // Cost per click
        public int Conversions { get; set; }
        public decimal CPA { get; set; } // Cost per acquisition
        public decimal ROAS { get; set; } // Return on ad spend

        // Campaign performance
        public List<AdCampaignPerformance> TopCampaigns { get; set; } = new();

        // Ad set performance
        public List<AdSetPerformance> TopAdSets { get; set; } = new();
    }

    public class AdCampaignPerformance
    {
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public decimal Spend { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public decimal ROAS { get; set; }
    }

    public class AdSetPerformance
    {
        public string AdSetId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public decimal Spend { get; set; }
        public int Impressions { get; set; }
        public int Clicks { get; set; }
        public decimal CTR { get; set; }
    }
}