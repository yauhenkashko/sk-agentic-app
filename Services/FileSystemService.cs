using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SK.Agentic.Application.Configuration;

namespace SK.Agentic.Application.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly ApplicationSettings _settings;
        private readonly ILogger<FileSystemService> _logger;

        public FileSystemService(IOptions<ApplicationSettings> settings, ILogger<FileSystemService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            EnsureDirectoryExists(_settings.OutputDirectory);
        }

        public async Task<string> CreateProjectDirectoryAsync(string projectName)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var projectPath = Path.Combine(_settings.OutputDirectory, projectName, timestamp);

            EnsureDirectoryExists(projectPath);
            _logger.LogInformation("Created project directory: {Path}", projectPath);

            return await Task.FromResult(projectPath);
        }

        public async Task SaveFileAsync(string directory, string fileName, string content)
        {
            try
            {
                EnsureDirectoryExists(directory);
                var filePath = Path.Combine(directory, fileName);

                await File.WriteAllTextAsync(filePath, content);
                _logger.LogDebug("Saved file: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file {FileName} to {Directory}", fileName, directory);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> SaveFilesAsync(string directory, Dictionary<string, string> files)
        {
            var savedFiles = new Dictionary<string, string>();

            foreach (var (fileName, content) in files)
            {
                await SaveFileAsync(directory, fileName, content);
                savedFiles[fileName] = Path.Combine(directory, fileName);
            }

            _logger.LogInformation("Saved {Count} files to {Directory}", files.Count, directory);
            return savedFiles;
        }

        public async Task<string> ReadFileAsync(string path)
        {
            return await File.ReadAllTextAsync(path);
        }

        private void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }
        }
    }
}
