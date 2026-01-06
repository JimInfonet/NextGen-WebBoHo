namespace InfonetCSCR.Core.Dtos
{
    public class TenderData
    {
        public string? Tender { get; set; }
        public int Count { get; set; } = default!;
        public decimal Amount { get; set; }
        public decimal Used { get; set; }
    }
}