namespace ECommerceApi.Models
{
    /// <summary>
    /// Log mỗi giao dịch webhook SePay gửi tới — chống xử lý trùng (unique SepayId) + audit.
    /// </summary>
    public class SepayTransaction : BaseEntity
    {
        public long SepayId { get; set; }               // id giao dịch từ SePay (duy nhất)
        public string Gateway { get; set; } = "";       // tên ngân hàng
        public string? Content { get; set; }            // nội dung chuyển khoản
        public string TransferType { get; set; } = "";  // "in" (tiền vào) / "out"
        public decimal TransferAmount { get; set; }
        public string? ReferenceCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? OrderId { get; set; }               // đơn đã khớp (null nếu không khớp đơn nào)
    }
}
