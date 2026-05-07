using System.IO.Compression;
using Microsoft.Extensions.Logging;
using SharpCompress.Common;
using SharpCompress.Readers;
using Postgres.Embedded.Exceptions;

namespace Postgres.Embedded.Services;

public class ArchiveExtractor
{
    private readonly ILogger _logger;
    
    public ArchiveExtractor(ILogger logger)
    {
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }
    
    public void ExtractTarXz(string archivePath, string destinationPath)
    {
        _logger.LogInformation("Extracting tar.xz archive: {Archive} to {Destination}", 
            archivePath, destinationPath);
        
        try
        {
            Directory.CreateDirectory(destinationPath);
            
            using var archiveStream = File.OpenRead(archivePath);
            using var reader = ReaderFactory.OpenReader(archiveStream);
            
            while (reader.MoveToNextEntry())
            {
                if (!reader.Entry.IsDirectory)
                {
                    var extractionOptions = new ExtractionOptions
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    };
                    
                    reader.WriteEntryToDirectory(destinationPath, extractionOptions);
                }
            }
            
            _logger.LogInformation("Extraction completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract tar.xz archive: {Archive}", archivePath);
            throw new EmbeddedPostgresException($"Failed to extract PostgreSQL binaries from {archivePath}", ex);
        }
    }
    
    public void ExtractJar(string jarPath, string destinationPath)
    {
        _logger.LogInformation("Extracting JAR archive: {Jar} to find .txz file", jarPath);
        
        try
        {
            using var archive = ZipFile.OpenRead(jarPath);
            
            foreach (var entry in archive.Entries)
            {
                if (entry.Name.EndsWith(".txz"))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? destinationPath);
                    entry.ExtractToFile(destinationPath, overwrite: true);
                    
                    _logger.LogDebug("Extracted .txz file: {File}", destinationPath);
                    return;  // 只提取第一个 .txz 文件
                }
            }
            
            throw new BinaryNotFoundException(jarPath, $"No .txz file found in JAR archive: {jarPath}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract JAR archive: {Jar}", jarPath);
            throw;
        }
    }
}