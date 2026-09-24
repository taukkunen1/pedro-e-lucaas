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

        private string Get(string key)
        {
            return _config.GetSection("Settings:" + key).Value ?? "";
        }

        public string GetClientMegaUrl() => Get("DownloadClientMegaUrl");
        public string GetClientMediafireUrl() => Get("DownloadClientMediafireUrl");
        public string GetLauncherUrl() => Get("DownloadLauncherUrl");
        public string GetLastPatch() => Get("LastPatch");
        public string GetClientVersion() => Get("ClientVersion");
        public string GetClientSize() => Get("ClientSize");
        public string GetClientSha256() => Get("ClientSha256");
        public string GetPublishedDate() => Get("ClientPublishedDate");

        public bool HasFullClientDownload =>
            !string.IsNullOrWhiteSpace(GetClientMegaUrl()) ||
            !string.IsNullOrWhiteSpace(GetClientMediafireUrl());

        public bool HasLauncherDownload => !string.IsNullOrWhiteSpace(GetLauncherUrl());
        public bool HasChecksum => !string.IsNullOrWhiteSpace(GetClientSha256());
    }
}
