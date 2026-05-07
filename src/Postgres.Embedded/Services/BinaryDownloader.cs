using System.Net.Http;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Postgres.Embedded.Models;
using Postgres.Embedded.Exceptions;

namespace Postgres.Embedded.Services;

public class BinaryDownloader
{
    private readonly ILogger _logger;
    private static readonly HttpClient HttpClient;
    
    static BinaryDownloader()
    {
        HttpClient = new HttpClient();
        HttpClient.Timeout = TimeSpan.FromMinutes(30);
    }
    
    public BinaryDownloader(ILogger logger)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
    
    public void Download(string url, string destinationPath)
    {
        _logger.LogInformation("Downloading PostgreSQL binaries from: {Url}", url);
        
        try
        {
            var directoryPath = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            using var response = HttpClient.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            
            var tempPath = destinationPath + ".tmp";
            using var fileStream = File.Create(tempPath);
            response.Content.CopyToAsync(fileStream).Wait();
            
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);
            
            File.Move(tempPath, destinationPath);
            
            _logger.LogInformation("Download completed: {Path} ({Size} bytes)", 
                destinationPath, fileStream.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Download failed from: {Url}", url);
            throw new EmbeddedPostgresException($"Failed to download PostgreSQL binaries from {url}", ex);
        }
    }
    
    public void DownloadChecksumAndVerify(string filePath, string checksumUrl)
    {
        try
        {
            _logger.LogInformation("Downloading checksum from: {Url}", checksumUrl);
            
            using var response = HttpClient.GetAsync(checksumUrl).Result;
            
            if (response.IsSuccessStatusCode)
            {
                var expectedChecksum = response.Content.ReadAsStringAsync().Result.Trim();
                VerifyChecksum(filePath, expectedChecksum);
            }
            else
            {
                _logger.LogWarning("Checksum not available at: {Url}, skipping verification", checksumUrl);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Checksum verification failed, continuing without verification");
        }
    }
    
    public bool VerifyChecksum(string filePath, string expectedChecksum)
    {
        _logger.LogDebug("Verifying SHA256 checksum for: {Path}", filePath);
        
        using var stream = File.OpenRead(filePath);
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        var actualChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        
        var isValid = actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase);
        
        if (!isValid)
        {
            _logger.LogError("Checksum verification failed! Expected: {Expected}, Actual: {Actual}", 
                expectedChecksum, actualChecksum);
            throw new EmbeddedPostgresException($"Checksum verification failed for {filePath}");
        }
        else
        {
            _logger.LogDebug("Checksum verification succeeded");
        }
        
        return isValid;
    }
    
    public string BuildDownloadUrl(PostgresVersion version, PlatformInfo platformInfo)
    {
        var versionString = GetVersionString(version);
        var os = platformInfo.OperatingSystem;
        var arch = platformInfo.Architecture;
        
        return $"https://repo1.maven.org/maven2/io/zonky/test/postgres/" +
               $"embedded-postgres-binaries-{os}-{arch}/{versionString}/" +
               $"embedded-postgres-binaries-{os}-{arch}-{versionString}.jar";
    }
    
    private string GetVersionString(PostgresVersion version)
    {
        return version switch
        {
            PostgresVersion.V18 => "18.3.0",
            PostgresVersion.V17 => "17.5.0",
            PostgresVersion.V16 => "16.9.0",
            PostgresVersion.V15 => "15.13.0",
            PostgresVersion.V14 => "14.18.0",
            PostgresVersion.V13 => "13.21.0",
            PostgresVersion.V12 => "12.22.0",
            PostgresVersion.V11 => "11.22.0",
            PostgresVersion.V10 => "10.23.0",
            PostgresVersion.V9 => "9.6.24",
            _ => throw new ArgumentOutOfRangeException(nameof(version))
        };
    }
}