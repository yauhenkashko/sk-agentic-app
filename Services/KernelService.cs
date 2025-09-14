using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SK.Agentic.Application.Configuration;
using SK.Agentic.Application.Plugins;

namespace SK.Agentic.Application.Services
{
    public class KernelService : IKernelService
    {
        private readonly Kernel _kernel;
        private readonly ILogger<KernelService> _logger;

        public KernelService(IOptions<AzureOpenAISettings> settings, IDevelopmentPlugin plugin, ILogger<KernelService> logger)
        {
            _logger = logger;
            var config = settings.Value;
            config.Validate();

            _logger.LogInformation("Initializing Semantic Kernel with Azure OpenAI");

            var builder = Kernel.CreateBuilder();
            builder.AddAzureOpenAIChatCompletion(
                deploymentName: config.DeploymentName,
                endpoint: config.Endpoint,
                apiKey: config.ApiKey
            );

            _kernel = builder.Build();
            _logger.LogInformation("Kernel initialized successfully");
        }

        public Kernel GetKernel() => _kernel.Clone();
    }
}
