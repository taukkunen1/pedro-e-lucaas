using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Serve o manifesto de atualizacao do cliente (launcher). Os arquivos em si
    /// (client, patches) ficam em wwwroot/client-updates e sao servidos estaticamente
    /// por app.UseStaticFiles(); esta controller so expoe o manifest.json de forma
    /// controlada (com validacao e um endpoint de "versao atual" simples).
    /// </summary>
    [Route("api/clientupdate")]
    [ApiController]
    public class ClientUpdateController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public ClientUpdateController(IWebHostEnvironment env)
        {
            _env = env;
        }

        private string ManifestPath => Path.Combine(_env.WebRootPath ?? "wwwroot", "client-updates", "manifest.json");

        /// <summary>
        /// O manifesto completo: {"version": "...", "baseUrl": "...", "files": [{"path","sha256","size"}, ...]}.
        /// Gerado por tools/build_client_manifest.py, nunca escrito pela API.
        /// </summary>
        [HttpGet("manifest")]
        public IActionResult GetManifest()
        {
            if (!System.IO.File.Exists(ManifestPath))
                return NotFound(new { error = "manifest not published yet" });

            // Repassa o arquivo como esta gravado; a API nao reprocessa o JSON.
            return PhysicalFile(ManifestPath, "application/json");
        }

        /// <summary>Atalho leve para o launcher checar so a versao publicada, sem baixar o manifesto inteiro.</summary>
        [HttpGet("version")]
        public IActionResult GetVersion()
        {
            if (!System.IO.File.Exists(ManifestPath))
                return NotFound(new { error = "manifest not published yet" });

            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(ManifestPath));
            string version = doc.RootElement.TryGetProperty("version", out var v) ? v.GetString() : null;
            return Ok(new { version });
        }
    }
}
