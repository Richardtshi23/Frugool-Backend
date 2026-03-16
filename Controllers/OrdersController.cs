using HolyWater.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Make sure you have this using statement

namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // This endpoint is for the customer-facing "My Account" page
        [HttpGet("getOrders")]
        public IActionResult GetOrders([FromQuery] int userId)
        {
            var ordersAndItems = _context.Orders
                .Where(o => o.UserId == userId)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Username = o.Username,
                    Reference = o.Reference,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    ShippingCity = o.ShippingCity,
                    ShippingPostalCode = o.ShippingPostalCode,
                    ShippingCountry = o.ShippingCountry,
                    isFullfilled = o.isFulfilled,
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new OrderItemDto
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            ProductImage = oi.ProductImage,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            Subtotal = oi.Subtotal
                        }).ToList()
                })
                .ToList();

            return Ok(ordersAndItems);
        }

        // --- ADMIN ENDPOINTS ---

        // CORRECTED: This now gets ALL orders for the admin page and uses the DTO
        // In OrdersController.cs

        [HttpGet("getAllOrders")]
        public IActionResult GetAllOrders()
        {
            var allOrdersWithItems = _context.Orders
                .Select(o => new OrderDto // Projecting to the OrderDto
                {
                    // --- All the main order properties ---
                    Id = o.Id,
                    UserId = o.UserId,
                    Username = o.Username,
                    Reference = o.Reference,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress, // Make sure to include all necessary fields for the modal
                    ShippingCity = o.ShippingCity,
                    ShippingPostalCode = o.ShippingPostalCode,
                    ShippingCountry = o.ShippingCountry,
                    isFullfilled = o.isFulfilled,

                    // --- THIS IS THE ADDED PART ---
                    // Now, for each order, we also get its items and project them to the DTO.
                    Items = _context.OrderItems
                        .Where(oi => oi.OrderId == o.Id)
                        .Select(oi => new OrderItemDto
                        {
                            Id = oi.Id,
                            ProductId = oi.ProductId,
                            ProductName = oi.ProductName,
                            ProductImage = oi.ProductImage,
                            Quantity = oi.Quantity,
                            UnitPrice = oi.UnitPrice,
                            Subtotal = oi.Subtotal
                        }).ToList()
                    // --- END OF ADDED PART ---
                })
                .OrderByDescending(o => o.OrderDate) // Show newest orders first
                .ToList();

            return Ok(allOrdersWithItems);
        }

        // CORRECTED: The verb is now [HttpPut] for updating
        [HttpPut("updateOrder/{id}")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound("Order not found.");
            }

            order.isFulfilled = request.IsFulfilled;
            order.Status = request.IsFulfilled ? "Fulfilled" : "Processing"; // Automatically update status text

            await _context.SaveChangesAsync();
            return Ok(order);
        }

        // NEW: The endpoint for the "smart" bulk-action button
        [HttpPut("bulkMarkAsFulfilled")]
        public async Task<IActionResult> BulkMarkAsFulfilled([FromBody] List<int> orderIds)
        {
            if (orderIds == null || !orderIds.Any())
            {
                return BadRequest("No order IDs provided.");
            }

            var ordersToUpdate = await _context.Orders
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync();

            foreach (var order in ordersToUpdate)
            {
                order.isFulfilled = true;
                order.Status = "Fulfilled";
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"{ordersToUpdate.Count} orders marked as fulfilled." });
        }

        // This endpoint is correct, no changes needed
        [HttpDelete("deleteOrders/{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- DTOs for this controller ---

        public class OrderItemDto
        {
            public int Id { get; set; }
            public int? ProductId { get; set; }
            public string ProductName { get; set; }
            public string? ProductImage { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Subtotal { get; set; }
        }

        public class OrderDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string? Username { get; set; }
            public string Reference { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string? Status { get; set; }
            public string ShippingAddress { get; set; }
            public string ShippingCity { get; set; }
            public string ShippingPostalCode { get; set; }
            public string ShippingCountry { get; set; }
            public bool isFullfilled { get; set; }

            // The list of items will use the clean DTO, not the EF model
            public List<OrderItemDto> Items { get; set; } = new();
        }
        // New DTO for the simple status update
        public class UpdateOrderStatusRequest
        {
            public bool IsFulfilled { get; set; }
        }
    }
}