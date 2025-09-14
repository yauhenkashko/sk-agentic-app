using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SK.Agentic.Application.Services;
using System.ComponentModel;
using System.Text.Json;

namespace SK.Agentic.Application.Plugins
{
    public class DevelopmentPlugin : IDevelopmentPlugin
    {
        private readonly IFileSystemService _fileSystem;
        private readonly ILogger<DevelopmentPlugin> _logger;

        public DevelopmentPlugin(IFileSystemService fileSystem, ILogger<DevelopmentPlugin> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        [KernelFunction("SaveProjectFiles")]
        [Description("Saves project files to disk")]
        public async Task<string> SaveProjectFilesAsync(
            [Description("Name of the project")] string projectName,
            [Description("JSON dictionary that represents filename to written code content")] string filesJson,
            [Description("The programming language in which the code is written")] string language)
        {
            try
            {
                var files = JsonSerializer.Deserialize<Dictionary<string, string>>(filesJson);

                if (files == null || files.Count == 0)
                {
                    return "Error: No files provided";
                }

                var projectPath = await _fileSystem.CreateProjectDirectoryAsync(projectName);
                await _fileSystem.SaveFilesAsync(projectPath, files);

                _logger.LogInformation("Saved {Count} files for {Language} project {Project} at {Path}",
                    files.Count, language, projectName, projectPath);

                return $"Success: Saved {files.Count} files to {projectPath}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save project files");
                return $"Error: {ex.Message}";
            }
        }
    }
}