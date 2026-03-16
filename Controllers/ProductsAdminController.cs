using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
namespace HolyWater.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsAdminController : ControllerBase
    {
        readonly IProductsAdminService _productsAdminService;
        private readonly Cloudinary _cloudinary;
        public ProductsAdminController(IProductsAdminService productsAdminService, Cloudinary cloudinary)
        {
            _productsAdminService = productsAdminService;
            _cloudinary = cloudinary;
        }

        [HttpGet("get-All-Products")]
        public IActionResult GetProduct()
        {
            var products = _productsAdminService.GetAllProducts();
            return Ok(products);
        }

        [HttpGet("getProductById")]
        public IActionResult GetProduct([FromQuery] int id)
        {
            var product = _productsAdminService.GetProductById(id);
            return Ok(product);
        }

        [HttpDelete("deleteProductById/{id}")]
        public IActionResult RemoveProduct([FromRoute] int id)
        {
            _productsAdminService.DeleteProduct(id);
            return Ok();
        }
        [HttpPut("updateProduct-with-image")]
        public async Task<IActionResult> UpdateProduct([FromForm] ProductDTO product, [FromQuery] int id)
        {
            try
            {
                // 1. Get the existing product from the DB first
                var existingProduct = _productsAdminService.GetProductById(id);
                if (existingProduct == null) return NotFound("Product not found");

                string imagePath = existingProduct.Image; // Default to the current image URL

                // 2. Only upload if a NEW file was provided
                if (product.Image != null && product.Image.Length > 0)
                {
                    using var stream = product.Image.OpenReadStream();
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(product.Image.FileName, stream),
                        Transformation = new Transformation().Height(500).Width(500).Crop("fill"),
                        Folder = "products"
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                        throw new Exception(uploadResult.Error.Message);

                    imagePath = uploadResult.SecureUrl.ToString();

                    // OPTIONAL: Delete the old image from Cloudinary here if you want to save space
                    // You would need to parse the PublicId from the old URL
                }

                // 3. Map the updated details
                var prod = new Product()
                {
                    Name = product.Name,
                    Title = product.Title,
                    Price = (decimal)product.Price,
                    OldPrice = (decimal)product.OldPrice,
                    Category = product.Category,
                    Description = product.Description,
                    InStock = product.InStock,
                    Onsale = product.OnSale,
                    Qty = product.Qty,
                    Image = imagePath // Uses new URL or keeps the old one
                };

                _productsAdminService.UpdateProduct(prod, id);

                var products = _productsAdminService.GetAllProducts();
                return Ok(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UPDATE ERROR: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("AddProduct-with-image")]
        public async Task<IActionResult> AddProduct([FromForm] ProductDTO product)
        {
            try
            {
                string imageUrl = string.Empty;

                if (product.Image != null && product.Image.Length > 0)
                {
                    // 1. Open the file stream
                    using var stream = product.Image.OpenReadStream();

                    // 2. Setup upload parameters
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(product.Image.FileName, stream),
                        // This automatically resizes/crops the image on upload (Optional)
                        Transformation = new Transformation().Height(500).Width(500).Crop("fill")
                    };

                    // 3. Upload to Cloudinary
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.Error != null)
                        throw new Exception(uploadResult.Error.Message);

                    // 4. Get the permanent URL
                    imageUrl = uploadResult.SecureUrl.ToString();
                }

                var prod = new Product()
                {
                    Name = product.Name,
                    Title = product.Title,
                    Price = (decimal)product.Price,
                    OldPrice = (decimal)product.OldPrice,
                    Category = product.Category,
                    Description = product.Description,
                    InStock = product.InStock,
                    Onsale = product.OnSale,
                    Qty = product.Qty,
                    Image = imageUrl 
                };

                _productsAdminService.AddProduct(prod);
                return Ok(new { message = "Product added successfully", url = imageUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Obsolete("This method is no longer used. Image uploads are now handled directly with Cloudinary in the AddProduct and UpdateProduct methods.")]
        private async Task<string> CreatedImagePathAsync(ProductDTO product)
        {
            // Use Path.Combine for Linux/Windows compatibility
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.Image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // SAVE ASYNC: Critical for Render's stability
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await product.Image.CopyToAsync(fileStream);
            }

            return $"/images/{uniqueFileName}";
        }
    }
}