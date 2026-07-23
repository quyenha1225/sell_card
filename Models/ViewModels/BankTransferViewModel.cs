namespace BBEcom.Models.ViewModels
{
    public class BankTransferViewModel
    {
        public int OrderId { get; set; }

        public string OrderCode { get; set; }

        public decimal Amount { get; set; }

        public string BankName { get; set; }

        public string AccountNumber { get; set; }

        public string AccountName { get; set; }

        public string QrUrl { get; set; }
    }
}