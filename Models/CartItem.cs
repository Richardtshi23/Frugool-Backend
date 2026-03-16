namespace HolyWater.Server.Models
{
    public class CartItem
    {
        public int? Id { get; set; }
        public int Qty { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
    }
}
