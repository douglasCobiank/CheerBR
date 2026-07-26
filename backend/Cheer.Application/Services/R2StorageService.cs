using System;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Cheer.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cheer.Application.Services;

/// <summary>
/// Implementação de <see cref="IStorageService"/> usando Cloudflare R2
/// (compatível com a API S3). Fallback: se as env vars R2 não estiverem
/// configuradas, grava em disco local (UPLOADS_PATH).
///
/// R2 free tier: 10 GB armazenamento, egress gratuito, ~1M operações
/// de leitura/mês. Perfeito para logos de equipe.
///
/// Env vars necessárias (no .env ou environment):
///   R2_ACCOUNT_ID       — ID da conta Cloudflare (ex: "abc123def456")
///   R2_ACCESS_KEY_ID    — Token de acesso R2
///   R2_SECRET_ACCESS_KEY— Secret do token
///   R2_BUCKET_NAME      — Nome do bucket (ex: "cheerbr-logos")
///   R2_PUBLIC_URL       — URL pública do bucket (ex: "https://logos.cheerbr.com")
///
/// Se ausentes, usa o disco local em UPLOADS_PATH (<see cref="LocalFallback"/>).
/// </summary>
public class R2StorageService : IStorageService
{
    private const string FallbackPrefix = "/uploads/";

    private readonly IAmazonS3? _s3Client;
    private readonly string? _bucketName;
    private readonly string? _publicUrl;
    private readonly string? _localPath;
    private readonly ILogger<R2StorageService> _logger;
    private readonly bool _useR2;

    public R2StorageService(ILogger<R2StorageService> logger)
    {
        _logger = logger;

        var accountId = Environment.GetEnvironmentVariable("R2_ACCOUNT_ID");
        var accessKey = Environment.GetEnvironmentVariable("R2_ACCESS_KEY_ID");
        var secretKey = Environment.GetEnvironmentVariable("R2_SECRET_ACCESS_KEY");
        var bucket = Environment.GetEnvironmentVariable("R2_BUCKET_NAME");
        var publicUrl = Environment.GetEnvironmentVariable("R2_PUBLIC_URL");

        _useR2 = !string.IsNullOrWhiteSpace(accountId)
                 && !string.IsNullOrWhiteSpace(accessKey)
                 && !string.IsNullOrWhiteSpace(secretKey)
                 && !string.IsNullOrWhiteSpace(bucket)
                 && !string.IsNullOrWhiteSpace(publicUrl);

        if (_useR2)
        {
            var endpoint = $"https://{accountId}.r2.cloudflarestorage.com";
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                RegionEndpoint = RegionEndpoint.USEast1, // R2 ignora região
                ForcePathStyle = true,
            };

            _s3Client = new AmazonS3Client(credentials, config);
            _bucketName = bucket;
            _publicUrl = publicUrl!.TrimEnd('/');

            _logger.LogInformation("R2Storage ativado: bucket={Bucket}, publicUrl={Url}", _bucketName, _publicUrl);
        }
        else
        {
            _localPath = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("UPLOADS_PATH"))
                ? Environment.GetEnvironmentVariable("UPLOADS_PATH")!
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(_localPath)) Directory.CreateDirectory(_localPath);

            _logger.LogWarning("R2 nao configurado. Usando disco local em {Path}", _localPath);
        }
    }

    public async Task<string> UploadAsync(string key, Stream content, string contentType)
    {
        if (_useR2 && _s3Client != null)
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = $"logos/{key}",
                InputStream = content,
                ContentType = contentType,
                DisablePayloadSigning = true,
            };

            await _s3Client.PutObjectAsync(request);
            _logger.LogInformation("Upload R2: logos/{Key}", key);

            return GetPublicUrl(key);
        }

        // Fallback: disco local
        var filePath = Path.Combine(_localPath!, key);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await content.CopyToAsync(fileStream);
        }
        _logger.LogInformation("Upload local: {Path}", filePath);

        return $"{FallbackPrefix}{key}";
    }

    public Task DeleteAsync(string key)
    {
        if (_useR2 && _s3Client != null)
        {
            return _s3Client.DeleteObjectAsync(_bucketName, $"logos/{key}");
        }

        var filePath = Path.Combine(_localPath!, key);
        if (File.Exists(filePath))
        {
            try { File.Delete(filePath); } catch { /* best-effort */ }
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string key)
    {
        if (_useR2 && _publicUrl != null)
            return $"{_publicUrl}/logos/{key}";

        return $"{FallbackPrefix}{key}";
    }

    public string? ExtractKey(string? publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl)) return null;

        if (_useR2 && _publicUrl != null)
        {
            var prefix = $"{_publicUrl}/logos/";
            if (!publicUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
            var key = publicUrl.Substring(prefix.Length);
            return RejectPathTraversal(key) ? null : key;
        }

        // Fallback: local /uploads/
        if (!publicUrl.StartsWith(FallbackPrefix, StringComparison.OrdinalIgnoreCase)) return null;
        var localKey = publicUrl.Substring(FallbackPrefix.Length);
        return RejectPathTraversal(localKey) ? null : localKey;
    }

    private static bool RejectPathTraversal(string s) =>
        s.Contains("..") || s.Contains('/') || s.Contains('\\');
}
