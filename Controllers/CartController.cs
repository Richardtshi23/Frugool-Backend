using HolyWater.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        // This endpoint runs when the user logs in or refreshes their cart
        //[HttpPost("sync")]
        //public async Task<IActionResult> SyncCart([FromBody] CartSyncRequest request)
        //{
        //    // 1. Get this user’s cart from DB, including its items
        //    var cart = await _context.Carts
        //        .Include(c => c.Items)
        //        .FirstOrDefaultAsync(c => c.UserId == request.UserId);

        //    // 2. If user doesn’t have a cart yet → create one
        //    if (cart == null)
        //    {
        //        cart = new Cart
        //        {
        //            UserId = request.UserId
        //        };

        //        _context.Carts.Add(cart);
        //    }

        //    // 3. Now merge items from frontend into the database cart
        //    foreach (var incomingItem in request.Items)
        //    {
        //        // see if this product already exists in the cart
        //        var existingItem = cart.Items
        //            .FirstOrDefault(ci => ci.ProductId == incomingItem.ProductId);

        //        if (existingItem != null)
        //        {
        //            // Increase quantity if product already exists
        //            existingItem.Quantity += incomingItem.Quantity;
        //        }
        //        else
        //        {
        //            // Add new product to cart
        //            cart.Items.Add(new CartItem
        //            {
        //                Id = incomingItem.ProductId,
        //                Qty= incomingItem.Quantity
        //            });
        //        }
        //    }

        //    // 4. Save everything
        //    await _context.SaveChangesAsync();
        //    return Ok(cart);
        //}
    }
}
