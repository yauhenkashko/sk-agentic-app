namespace SK.Agentic.Application.Agents
{
    public interface IAgentOrchestrator
    {
        Task ProcessTaskAsync(string taskDescription);
    }
}
