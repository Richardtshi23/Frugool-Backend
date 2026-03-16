using HolyWater.Server.Models;
using HolyWater.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PaymentsController(AppDbContext context)
        {
                _context = context;
        }

        [HttpPost("paystack-webhook")]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaystackWebhook()
        {
            
            // 1. Read the body
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            // 2. Validate the Paystack signature
            var signature = Request.Headers["x-paystack-signature"];
            var secret = "sk_test_4dffb697c2841bc2d4630e4e905662f9a639a608";

            var computed = new HMACSHA512(Encoding.UTF8.GetBytes(secret))
                .ComputeHash(Encoding.UTF8.GetBytes(json));

            var computedHex = BitConverter.ToString(computed).Replace("-", "").ToLower();

            if (computedHex != signature)
                return Unauthorized(); // signature mismatch

            // 3. Parse event
            dynamic evt = JsonConvert.DeserializeObject(json);

            string refer = evt.data.reference;
            var existingOrder = await _context.Orders.FirstOrDefaultAsync(o => o.Reference == refer);

            if (existingOrder != null)
            {
                return Ok();
            }

            if (evt.@event == "charge.success")
            {
                string reference = evt.data.reference;
                int amount = evt.data.amount;
                string? userId = evt?.data?.metadata?.userId ?? 0;
                var cartItems = evt.data.metadata?.cartItems;
                List<CartItem> items = new();

                foreach (var item in cartItems.items)
                {
                    items.Add(new CartItem
                    {
                        Id = item.id,
                        Qty = item.qty,
                        Name = item.name,
                        Category = item.category,
                        Image = item.image
                    });
                }

                var order = new Orders
                {
                    UserId = Convert.ToInt32(userId),
                    TotalAmount = amount / 100m, // convert cents to rands
                    Reference = reference,
                    OrderDate = DateTime.UtcNow,
                    Status = evt.@event,
                    ShippingAddress = evt.data.metadata.address,
                    ShippingCity = evt.data.metadata.city,
                    ShippingPostalCode = evt.data.metadata.postal,
                    PaymentMethod = "Paystack",
                    ShippingCountry = "SA",
                    //ShippingCountry = evt.data.metadata.country,    
                    Items = items.Select(i => new OrderItem
                    {
                        ProductId = i.Id ?? 0,
                        Quantity = i.Qty,
                        ProductName = i.Name,
                        Category = i.Category,
                        ProductImage = i.Image
                    }).ToList(),
                };

                _context.Orders.Add(order);
                _context.SaveChanges();
            }

            return Ok();
        }
            
        [HttpPost("initiate-payment")]
        public async Task<IActionResult> InitiatePayment([FromBody]PaymentRequest request)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "sk_test_4dffb697c2841bc2d4630e4e905662f9a639a608");

            var data = new
            {
                email = request.Email,
                amount = request.Amount * 100, // Paystack uses cents
                callback_url = "http://localhost:4200/shopping-cart",
                metadata = new
                {
                    userId = request.UserId ?? 0,
                    cartItems = request.Metadata,
                    address = request.Metadata.Address,
                    city = request.Metadata.City,
                    postal = request.Metadata.Postal,
                    phone = request.Metadata.Phone,
                    name = request.Metadata.Name,
                }
            };

            var response = await client.PostAsJsonAsync(
                "https://api.paystack.co/transaction/initialize",
                data
            );

            var json = await response.Content.ReadFromJsonAsync<PaystackInitResponse>();

            return Ok(json.Data.authorization_url);
        }
    }
    public class PaystackInitResponse
    {
        public bool Status { get; set; } 
        public string Message { get; set; }
        public PaystackInitData Data { get; set; }
    }

    public class PaystackInitData
    {
        public string authorization_url { get; set; }
        public string access_code { get; set; }
        public string Reference { get; set; }
    }

    public class PaymentRequest
    {
        public string Email { get; set; } = string.Empty;   // guest or logged-in user email
        public int Amount { get; set; }                     // total amount (Rands, not cents)
        public int? UserId { get; set; }                    // null for guest, number for logged in user
        public Metadata Metadata { get; set; }                     // cart being checked out
    }

    public class Metadata
    {
        public List<CartItem> Items { get; set; } = new();
        public string City { get; set; }
        public string Postal { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Name { get; set; }
    }

    public class CartItem
    {
        public int? Id { get; set; }
        public int Qty { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
    }

}
