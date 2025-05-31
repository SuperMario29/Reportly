using System;
using System.Collections.Generic;

namespace Reportly.Models
{
    public class KlaviyoMetrics
    {
        public int TotalSubscribers { get; set; }
        public int ActiveSubscribers { get; set; }
        public double OpenRate { get; set; }
        public double ClickRate { get; set; }
        public double ConversionRate { get; set; }
        public int Unsubscribes { get; set; }
        
        // Campaign performance
        public List<CampaignPerformance> RecentCampaigns { get; set; } = new();
    }

    public class CampaignPerformance
    {
        public string CampaignId { get; set; }
        public string Name { get; set; }
        public int Recipients { get; set; }
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public int Conversions { get; set; }
        public int Unsubscribes { get; internal set; }

        public DateTime SendTime { get; set; }
    }
}