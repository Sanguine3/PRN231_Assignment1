namespace EstoreMVC.Models
{
    public class OrderDTO
    {
        public int OrderDetailId { get; set; }
        public int MemberId { get; set; }
        public string ProductName { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int Discount { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime ShippedDate { get; set; }
        public int? Freight { get; set; }
    }
}
