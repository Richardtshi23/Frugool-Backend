using HolyWater.Server.Models;
using HolyWater.Server.services;

namespace HolyWater.Server.Interfaces
{
    public interface IProductsService
    {
        ProductDto[] GetProductsByCategory(string category);
        Product? GetProduct(int id);
        ProductDto[] GetAllProducts();
    }
}
