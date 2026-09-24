namespace TrinityConquerLoader.Updater
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class UpdateProgress
    {
        public string Stage { get; set; }          // "checking" | "downloading" | "applying" | "done"
        public string CurrentFile { get; set; }
        public int FilesDone { get; set; }
        public int FilesTotal { get; set; }
        public long BytesDone { get; set; }
        public long BytesTotal { get; set; }
    }

    /// <summary>
    /// Confere a versao do cliente contra o manifesto publicado pela API, baixa so os
    /// arquivos que faltam ou mudaram (por SHA-256, nao por data) e aplica de forma
    /// atomica (baixa em .part e so substitui o arquivo final se o hash bater).
    /// Sem dependencia de WinForms: pode ser testado isoladamente.
    /// </summary>
    public class UpdateManager
    {
        private readonly string _clientDir;
        private readonly string _manifestUrl;
        private readonly HttpClient _http;

        public UpdateManager(string clientDir, string manifestUrl, HttpClient httpClient = null)
        {
            _clientDir = clientDir;
            _manifestUrl = manifestUrl;
            _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        public async Task<UpdateManifest> FetchManifestAsync()
        {
            string json = await _http.GetStringAsync(_manifestUrl).ConfigureAwait(false);
            var manifest = JsonConvert.DeserializeObject<UpdateManifest>(json);
            if (manifest == null || string.IsNullOrEmpty(manifest.Version) || string.IsNullOrEmpty(manifest.BaseUrl))
                throw new InvalidDataException("Manifest invalido ou incompleto.");
            return manifest;
        }

        /// <summary>Arquivos que faltam ou tem hash diferente do local. Nunca deleta nada por conta propria.</summary>
        public List<ManifestFile> GetPendingFiles(UpdateManifest manifest)
        {
            var pending = new List<ManifestFile>();
            foreach (var file in manifest.Files)
            {
                string localPath = ToLocalPath(file.Path);
                if (!File.Exists(localPath))
                {
                    pending.Add(file);
                    continue;
                }
                if (new FileInfo(localPath).Length != file.Size)
                {
                    pending.Add(file);
                    continue;
                }
                if (!string.Equals(Sha256Of(localPath), file.Sha256, StringComparison.OrdinalIgnoreCase))
                    pending.Add(file);
            }
            return pending;
        }

        public string ToLocalPath(string relativePath)
        {
            // O manifesto sempre usa "/"; Path.Combine cuida da barra certa no Windows.
            string[] parts = relativePath.Split('/');
            return Path.Combine(new[] { _clientDir }.Concat(parts).ToArray());
        }

        private static string Sha256Of(string path)
        {
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                byte[] hash = sha.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Baixa cada arquivo pendente para um .part ao lado do destino, confere o SHA-256
        /// e so entao substitui o arquivo final. Um arquivo corrompido/interrompido nunca
        /// fica pela metade no lugar do arquivo de verdade.
        /// </summary>
        public async Task ApplyUpdatesAsync(UpdateManifest manifest, List<ManifestFile> pending,
            IProgress<UpdateProgress> progress, CancellationToken cancellationToken)
        {
            long totalBytes = pending.Sum(f => f.Size);
            long bytesDone = 0;
            int fileIndex = 0;

            foreach (var file in pending)
            {
                cancellationToken.ThrowIfCancellationRequested();
                fileIndex++;

                progress?.Report(new UpdateProgress
                {
                    Stage = "downloading",
                    CurrentFile = file.Path,
                    FilesDone = fileIndex - 1,
                    FilesTotal = pending.Count,
                    BytesDone = bytesDone,
                    BytesTotal = totalBytes,
                });

                string localPath = ToLocalPath(file.Path);
                string tempPath = localPath + ".part";
                Directory.CreateDirectory(Path.GetDirectoryName(localPath) ?? _clientDir);

                string url = manifest.BaseUrl.TrimEnd('/') + "/" + file.Path;
                using (var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                    {
                        await httpStream.CopyToAsync(fileStream).ConfigureAwait(false);
                    }
                }

                string gotHash = Sha256Of(tempPath);
                if (!string.Equals(gotHash, file.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(tempPath);
                    throw new IOException($"Falha na verificacao do arquivo baixado: {file.Path} (hash nao confere).");
                }

                if (File.Exists(localPath))
                    File.Delete(localPath);
                File.Move(tempPath, localPath);

                bytesDone += file.Size;
            }

            // Grava a versao aplicada, para o proximo start comparar rapido (antes de buscar o manifest inteiro).
            File.WriteAllText(Path.Combine(_clientDir, "version.txt"), manifest.Version);

            progress?.Report(new UpdateProgress
            {
                Stage = "done",
                FilesDone = pending.Count,
                FilesTotal = pending.Count,
                BytesDone = totalBytes,
                BytesTotal = totalBytes,
            });
        }

        public string GetLocalVersion()
        {
            string versionFile = Path.Combine(_clientDir, "version.txt");
            return File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : "";
        }
    }
}
