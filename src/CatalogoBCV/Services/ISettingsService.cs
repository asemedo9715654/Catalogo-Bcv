namespace CatalogoBCV.Services
{
    public interface ISettingsService
    {
        Task<string> GetSettingAsync(string key, string defaultValue = "");
        Task<T> GetSettingAsync<T>(string key, T defaultValue);
    }
}
