namespace SK.Agentic.Application
{
    public interface IApplication
    {
        Task RunAsync();
        Task RunTaskAsync(string taskDescription);
    }
}