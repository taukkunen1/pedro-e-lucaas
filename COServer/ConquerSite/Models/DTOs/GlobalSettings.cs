using Core;
using Microsoft.Extensions.Configuration;

namespace ConquerSite.Models.DTOs
{
    public class GlobalSettings
    {
        private readonly IConfiguration _config;

        public GlobalSettings(IConfiguration config)
        {
            _config = config;
        }

        public void Init()
        {
            string apiPortConfStr = _config.GetSection("Settings:ApiPort").Value;
            if (apiPortConfStr != null)
            {
                if (!ushort.TryParse(apiPortConfStr, out ushort ApiPort))
                {
                    ApiPort = 8080; // Default port if parsing fails
                }
                RestApiHelper.ApiPort = ApiPort;
                RestApiHelper.ApiRequestBaseURI = $"http://localhost:{ApiPort}/api/"; // Regenerate the base URI with the port
            }
        }
    }
}
