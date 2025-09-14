using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SK.Agentic.Application.Configuration;
using SK.Agentic.Application.Services;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0110
namespace SK.Agentic.Application.Agents
{
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly IAgentFactory _agentFactory;
        private readonly IAgentDefinitions _definitions;
        private readonly IKernelService _kernelService;
        private readonly ApplicationSettings _settings;
        private readonly ILogger<AgentOrchestrator> _logger;

        public AgentOrchestrator(
            IAgentFactory agentFactory,
            IAgentDefinitions definitions,
            IKernelService kernelService,
            IOptions<ApplicationSettings> settings,
            ILogger<AgentOrchestrator> logger)
        {
            _agentFactory = agentFactory;
            _definitions = definitions;
            _kernelService = kernelService;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task ProcessTaskAsync(string taskDescription)
        {
            var taskId = Guid.NewGuid().ToString("N")[..8];

            if (_settings.EnableDetailedLogging)
            {
                _logger.LogDebug("User task: {Task}", taskDescription);
            }

            try
            {
                var chat = CreateAgentGroupChat();
                chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, taskDescription));

                var iteration = 0;
                await foreach (ChatMessageContent response in chat.InvokeAsync())
                {
                    iteration++;

                    _logger.LogInformation("Agent {Agent} responded in iteration {Iteration} for task {TaskId}",
                        response.AuthorName, iteration, taskId);

                    if (_settings.EnableDetailedLogging)
                    {
                        _logger.LogDebug("Response content: {Content}", response.Content);
                    }
                }

                _logger.LogInformation("Task {TaskId} completed after {Iterations} iterations", taskId, iteration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process task {TaskId}", taskId);
                throw;
            }
        }

        private AgentGroupChat CreateAgentGroupChat()
        {
            _logger.LogDebug("Creating new agent group chat");

            var businessAnalyst = _agentFactory.CreateBusinessAnalystAgent();
            var developer = _agentFactory.CreateDeveloperAgent();
            var techLead = _agentFactory.CreateTechLeadAgent();
            var teamLead = _agentFactory.CreateTeamLeadAgent();

            var kernel = _kernelService.GetKernel();

            var selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
                _definitions.GetSelectionStrategy(),
                safeParameterNames: "history");

            var selectionStrategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
            {
                InitialAgent = businessAnalyst,
                ResultParser = (result) => result.GetValue<string>()?.Trim() ?? "",
                HistoryVariableName = "history",
                HistoryReducer = new ChatHistoryTruncationReducer(_settings.HistoryTruncation)
            };

            var terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
                _definitions.GetTerminationStrategy(),
                safeParameterNames: "history");

            var terminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
            {
                Agents = [teamLead],
                ResultParser = (result) =>
                    result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                HistoryVariableName = "history",
                HistoryReducer = new ChatHistoryTruncationReducer(2),
                MaximumIterations = _settings.MaxIterations
            };

            return new AgentGroupChat(businessAnalyst, developer, techLead, teamLead)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = selectionStrategy,
                    TerminationStrategy = terminationStrategy
                }
            };
        }
    }
}