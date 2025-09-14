using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using SK.Agentic.Application.Plugins;
using SK.Agentic.Application.Services;

namespace SK.Agentic.Application.Agents
{
    public class AgentFactory : IAgentFactory
    {
        private readonly IKernelService _kernelService;
        private readonly IAgentDefinitions _definitions;
        private readonly IDevelopmentPlugin _plugin;
        private readonly ILogger<AgentFactory> _logger;

        public AgentFactory(
            IKernelService kernelService,
            IAgentDefinitions definitions,
            IDevelopmentPlugin plugin,
            ILogger<AgentFactory> logger)
        {
            _kernelService = kernelService;
            _definitions = definitions;
            _plugin = plugin;
            _logger = logger;
        }
        public ChatCompletionAgent CreateBusinessAnalystAgent()
        {
            _logger.LogDebug("Creating {Agent} agent", _definitions.BusinessAnalystName);
            var kernel = _kernelService.GetKernel();

            return new ChatCompletionAgent
            {
                Name = _definitions.BusinessAnalystName,
                Instructions = _definitions.GetBusinessAnalystInstructions(),
                Kernel = kernel
            };
        }

        public ChatCompletionAgent CreateDeveloperAgent()
        {
            _logger.LogDebug("Creating {Agent} agent", _definitions.DeveloperName);
            var kernel = _kernelService.GetKernel();

            return new ChatCompletionAgent
            {
                Name = _definitions.DeveloperName,
                Instructions = _definitions.GetDeveloperInstructions(),
                Kernel = kernel
            };
        }

        public ChatCompletionAgent CreateTechLeadAgent()
        {
            _logger.LogDebug("Creating {Agent} agent", _definitions.TechLeadName);
            var kernel = _kernelService.GetKernel();

            return new ChatCompletionAgent
            {
                Name = _definitions.TechLeadName,
                Instructions = _definitions.GetTechLeadInstructions(),
                Kernel = kernel
            };
        }

        public ChatCompletionAgent CreateTeamLeadAgent()
        {
            _logger.LogDebug("Creating {Agent} agent with Development plugin", _definitions.TeamLeadName);
            var kernel = _kernelService.GetKernel();

            if (!kernel.Plugins.Any(p => p.Name.Equals("DevelopmentPlugin")))
            {
                kernel.Plugins.AddFromObject(_plugin, "DevelopmentPlugin");
            }

            return new ChatCompletionAgent
            {
                Name = _definitions.TeamLeadName,
                Instructions = _definitions.GetTeamLeadInstructions(),
                Kernel = kernel,
                Arguments = new KernelArguments(new PromptExecutionSettings
                {
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
                })
            };
        }
    }
}
