using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;
using HolyWater.Server.services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HolyWater.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    { 
        readonly IProductsService _productService;
        private readonly AppDbContext _db;
        public ProductsController(IProductsService productService, AppDbContext db)
        {
            _productService = productService;
            _db = db;
        }

        [HttpGet("GetAllProducts")]
        public IActionResult GetProducts([FromQuery] string param)
        {
            var products = _productService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("CheckOnline")]
        public IActionResult CheckOnline()
        {
           var response = new { Message = "API is online and responsive!" };
            return Ok(response);
        }

        [HttpGet("GetProducts")]
        public IActionResult GetProduct([FromQuery] int id)
        {
            var response = _productService.GetProduct(id);
            return Ok(response);
        }

        [HttpGet("GetProductsByCategory")]
        public IActionResult GetProductByCategory([FromQuery] string category)
        {
            var response = _productService.GetProductsByCategory(category);
            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string term)
        {
            // Basic validation: if the term is empty or too short, return an empty list.
            if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            {
                return Ok(new List<object>());
            }

            var searchTerm = term.ToLower().Trim();

            // The simple way: Using EF Core's Contains() method.
            // This translates to a SQL `LIKE '%term%'` query.
            var products = await _db.Products
                .Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Description.ToLower().Contains(searchTerm) // Optional: search in description too
                )
                .Take(10) // IMPORTANT: Always limit the number of results for a live search!
                .Select(p => new // Use a DTO to send only the data you need
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Image
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}
