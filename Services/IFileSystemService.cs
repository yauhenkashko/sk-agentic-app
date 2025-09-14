namespace SK.Agentic.Application.Services
{
    public interface IFileSystemService
    {
        Task<string> CreateProjectDirectoryAsync(string projectName);
        Task SaveFileAsync(string directory, string fileName, string content);
        Task<Dictionary<string, string>> SaveFilesAsync(string directory, Dictionary<string, string> files);
        Task<string> ReadFileAsync(string path);
    }
}