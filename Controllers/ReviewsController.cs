using HolyWater.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ReviewsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("add-review")]
        public async Task<IActionResult> AddReview([FromBody] Reviewdto review)
         {
           var reviews = new Review
            {
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                ProductId = review.ProductId,
                rating = review.Rating,
                Name = review.Name
            };

            _db.Reviews.Add(reviews);
            await _db.SaveChangesAsync();
            return Ok(review);
        }

        [HttpDelete("delete-review")]
        public async Task<IActionResult> AddReview([FromQuery] int id)
        {
            var review = _db.Reviews.Where(x => x.Id == id);
            _db.Reviews.Remove((Review)review);

            return Ok(review);
        }

        [HttpGet("get-reviews")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _db.Reviews
                                .Where(r => r.ProductId == productId)
                                .OrderByDescending(r => r.CreatedAt)
                                .ToListAsync();
            return Ok(reviews);
        }
        public class Reviewdto
        {
            public int ProductId { get; set; } 
            public string Name { get; set; }
            public string Comment { get; set; }
            public int Rating { get; set; } 
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        }
    }
}
