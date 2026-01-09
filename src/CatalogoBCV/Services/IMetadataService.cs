using CatalogoBCV.Models;

namespace CatalogoBCV.Services
{
    public interface IMetadataService
    {
        Task<List<Table>> GetMetadataAsync(string connectionString);
        Task<bool> TestConnectionAsync(string connectionString);
    }
}
