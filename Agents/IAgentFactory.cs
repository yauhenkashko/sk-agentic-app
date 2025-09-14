using Microsoft.SemanticKernel.Agents;

namespace SK.Agentic.Application.Agents
{
    public interface IAgentFactory
    {
        ChatCompletionAgent CreateBusinessAnalystAgent();
        ChatCompletionAgent CreateDeveloperAgent();
        ChatCompletionAgent CreateTechLeadAgent();
        ChatCompletionAgent CreateTeamLeadAgent();
    }
}