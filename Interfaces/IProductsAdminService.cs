using HolyWater.Server.Models;

namespace HolyWater.Server.Interfaces
{
    public interface IProductsAdminService
    {
        void AddProduct(Product product);
        List<Product> GetAllProducts();
        Product GetProductById(int id);
        void UpdateProduct(Product product, int id);
        void DeleteProduct(int id);
    }
}
