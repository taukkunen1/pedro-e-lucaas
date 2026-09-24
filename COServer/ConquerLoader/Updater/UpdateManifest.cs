namespace TrinityConquerLoader.Updater
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>Corresponde ao JSON servido por GET /api/clientupdate/manifest.</summary>
    public class UpdateManifest
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>Prefixo para montar a URL de download de cada arquivo (baseUrl + "/" + path).</summary>
        [JsonProperty("baseUrl")]
        public string BaseUrl { get; set; }

        [JsonProperty("files")]
        public List<ManifestFile> Files { get; set; } = new List<ManifestFile>();
    }

    public class ManifestFile
    {
        /// <summary>Caminho relativo ao diretorio do client, sempre com "/" (ex.: "Sound/click.wav").</summary>
        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }
    }
}
