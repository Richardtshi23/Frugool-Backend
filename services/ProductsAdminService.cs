using HolyWater.Server.Interfaces;
using HolyWater.Server.Models;

namespace HolyWater.Server.services
{
    public class ProductsAdminService : IProductsAdminService
    {
        private readonly AppDbContext _context;
        public ProductsAdminService(AppDbContext context)
        {
            _context = context;
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }
        public Product GetProductById(int id)
        {
            return _context.Products.Where(x => x.Id == id).FirstOrDefault();
        }
        public void AddProduct(Product product)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product, int id)
        {
            var item = _context.Products.Where(x => x.Id == id).FirstOrDefault(); 
            if (item == null) return;
            product = item;
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            var item = _context.Products.Where(x => x.Id == id).FirstOrDefault();
            if (item == null) return;
            _context.Products.Remove(item);
            _context.SaveChanges();
        }

    }
}
