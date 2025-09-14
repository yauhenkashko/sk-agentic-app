using Microsoft.Extensions.Logging;
using SK.Agentic.Application.Agents;

namespace SK.Agentic.Application
{
    public class Application : IApplication
    {
        private readonly IAgentOrchestrator _orchestrator;
        private readonly ILogger<Application> _logger;

        public Application(IAgentOrchestrator orchestrator, ILogger<Application> logger)
        {
            _orchestrator = orchestrator;
            _logger = logger;
        }

        public async Task RunAsync()
        {
            Console.WriteLine("Enter a task description to process and specify language: 'Write C# code...'");
            Console.WriteLine("Type 'exit' or 'quit' to terminate");
            Console.WriteLine();

            while (true)
            {
                Console.WriteLine();
                Console.Write("Enter task: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                    input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    await _orchestrator.ProcessTaskAsync(input);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process task");
                }
            }

            _logger.LogInformation("Application shutting down");
        }

        public async Task RunTaskAsync(string taskDescription)
        {
            try
            {
                await _orchestrator.ProcessTaskAsync(taskDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process task");
            }
        }
    }
}
