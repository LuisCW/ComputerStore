using ComputerStore.Models;

namespace ComputerStore.Services
{
    public interface IProductService
    {
        // Métodos para la página principal
        Task<List<Products>> GetProductosDestacadosAsync();
        Task<List<Products>> GetNuevosLanzamientosAsync();
        Task<List<Products>> GetMasVendidosAsync();
        
        // Métodos básicos CRUD
        Task<List<Products>> GetAllProductsAsync();
        Task<Products?> GetProductByIdAsync(int id);
        Task<List<Products>> GetProductsByCategoryAsync(string category);
        Task<List<Products>> SearchProductsAsync(string searchTerm);
        
        // Métodos para el admin
        Task<Products> CreateProductAsync(Products product);
        Task<Products> UpdateProductAsync(Products product);
        Task<bool> DeleteProductAsync(Products product);
        Task<bool> DeleteProductAsync(int id);
        
        // Métodos optimizados con paginación
        Task<(List<Products> Products, int TotalCount)> GetProductsPagedAsync(int pageNumber, int pageSize);
        Task<(List<Products> Products, int TotalCount)> GetProductsByCategoryPagedAsync(string category, int pageNumber, int pageSize);
        Task<(List<Products> Products, int TotalCount)> SearchProductsPagedAsync(string searchTerm, int pageNumber, int pageSize);
        Task<List<string>> GetCategoriesAsync();
        Task<List<string>> GetBrandsAsync();
        Task<List<string>> GetColorsAsync();
    }
}