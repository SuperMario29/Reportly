using System.Linq;
using Newtonsoft.Json.Linq;

namespace Reportly.Models
{
    public class FacebookAdMetrics
    {
        public decimal Spend { get; }
        public int Purchases { get; }
        public decimal PurchaseValue { get; }
        public int NewCustomers { get; }
        public decimal AOV => Purchases > 0 ? PurchaseValue / Purchases : 0;
        public decimal ROAS => Spend > 0 ? PurchaseValue / Spend : 0;
        public decimal NCPA => NewCustomers > 0 ? Spend / NewCustomers : 0;
        public decimal NetProfit => (PurchaseValue * 0.3m) - Spend; // 30% margin
        public decimal NetROAS => Spend > 0 ? (PurchaseValue * 0.3m) / Spend : 0;

        public FacebookAdMetrics(JToken insight)
        {
            Spend = decimal.Parse(insight["spend"].ToString());
            Purchases = int.Parse(insight["purchases"].ToString());
            PurchaseValue = decimal.Parse(insight["purchase_value"].ToString());
            NewCustomers = insight["actions"]?
                .FirstOrDefault(a => a["action_type"].ToString() == "new_customer")?["value"]?.ToObject<int>() ?? 0;
        }

    }
}


