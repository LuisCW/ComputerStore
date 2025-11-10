namespace ComputerStore.Models
{
    public class Pedido
    {
        public string ReferenceCode { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
    }
}
