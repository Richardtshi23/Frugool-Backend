namespace HolyWater.Server.Models
{
    public class CartSyncRequest
    {
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }
}
