namespace ECommerceApi.DTOs.Payment
{
    /// <summary>
    /// Payload SePay POST tới webhook. JSON gửi dạng camelCase (id, transferAmount...),
    /// binding của ASP.NET không phân biệt hoa/thường nên map thẳng PascalCase.
    /// </summary>
    public class SepayWebhookPayload
    {
        public long Id { get; set; }
        public string? Gateway { get; set; }
        public string? TransactionDate { get; set; }
        public string? AccountNumber { get; set; }
        public string? Code { get; set; }
        public string? Content { get; set; }
        public string? TransferType { get; set; }   // "in" / "out"
        public decimal TransferAmount { get; set; }
        public decimal Accumulated { get; set; }
        public string? SubAccount { get; set; }
        public string? ReferenceCode { get; set; }
    }
}
