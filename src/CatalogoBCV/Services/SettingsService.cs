using CatalogoBCV.Data;
using Microsoft.EntityFrameworkCore;

namespace CatalogoBCV.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly CatalogContext _context;

        public SettingsService(CatalogContext context)
        {
            _context = context;
        }

        public async Task<string> GetSettingAsync(string key, string defaultValue = "")
        {
            var setting = await _context.SystemSettings.FindAsync(key);
            return setting?.Value ?? defaultValue;
        }

        public async Task<T> GetSettingAsync<T>(string key, T defaultValue)
        {
            var value = await GetSettingAsync(key);
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
