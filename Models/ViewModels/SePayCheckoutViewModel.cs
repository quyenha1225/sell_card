using System.Collections.Generic;

namespace BBEcom.Models.ViewModels
{
    public class SePayCheckoutViewModel
    {
        public string CheckoutUrl { get; set; }
        public IList<KeyValuePair<string, string>> Fields { get; set; }
        public string Signature { get; set; }
    }

    public class SePayIpnRequest
    {
        public long timestamp { get; set; }
        public string notification_type { get; set; }
        public SePayIpnOrder order { get; set; }
    }

    public class SePayIpnOrder
    {
        public string order_status { get; set; }
        public string order_currency { get; set; }
        public string order_amount { get; set; }
        public string order_invoice_number { get; set; }
    }

    public class SePayOrderListResponse
    {
        public List<SePayOrderSummary> data { get; set; }
    }

    public class SePayOrderSummary
    {
        public string order_status { get; set; }
        public string order_currency { get; set; }
        public string order_amount { get; set; }
        public string order_invoice_number { get; set; }
    }
}
