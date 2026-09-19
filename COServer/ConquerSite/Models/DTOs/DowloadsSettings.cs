using Microsoft.Extensions.Configuration;

namespace ConquerSite.Models.DTOs
{
    public class DowloadsSettings
    {
        private readonly IConfiguration _config;
        public DowloadsSettings(IConfiguration config)
        {
            _config = config;
        }
        public string GetClientMegaUrl()
        {
            return _config.GetSection("Settings:DownloadClientMegaUrl").Value;
        }
        public string GetClientMediafireUrl()
        {
            return _config.GetSection("Settings:DownloadClientMediafireUrl").Value;
        }
        public string GetLastPatch()
        {
            return _config.GetSection("Settings:LastPatch").Value;
        }
    }
}
