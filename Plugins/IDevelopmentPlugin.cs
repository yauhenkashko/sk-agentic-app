namespace SK.Agentic.Application.Plugins
{
    public interface IDevelopmentPlugin
    {
        Task<string> SaveProjectFilesAsync(string projectName, string filesJson, string language);
    }
}
