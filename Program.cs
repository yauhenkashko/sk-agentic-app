using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SK.Agentic.Application.Agents;
using SK.Agentic.Application.Configuration;
using SK.Agentic.Application.Plugins;
using SK.Agentic.Application.Services;
using AgentFactory = SK.Agentic.Application.Agents.AgentFactory;

#pragma warning disable SKEXP0110
namespace SK.Agentic.Application
{
    /// <summary>
    /// Semantic Kernel Agent Framework: https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting Multi-Agent application");
                Console.WriteLine("========================================");

                var host = CreateHostBuilder(args).Build();

                var application = host.Services.GetRequiredService<IApplication>();

                // Task 1: Find Maximum in Array
                await application.RunTaskAsync("Create a method to find the maximum number in an array of integers");

                // Task 2: Reverse a String
                await application.RunTaskAsync("Reverse a String using C#");

                Console.WriteLine();
                Log.Information("All tasks completed successfully!");

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.AddUserSecrets<Program>(optional: true);
                }

                var builtConfig = config.Build();

                var keyVaultEndpoint = builtConfig["AzureKeyVault:Endpoint"];
                if (!string.IsNullOrEmpty(keyVaultEndpoint))
                {
                    Log.Information("Configuring Azure Key Vault: {Endpoint}", keyVaultEndpoint);

                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions());

                    config.AddAzureKeyVault(new Uri(keyVaultEndpoint), credential);
                }
            })
            .UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext())
            .ConfigureServices((context, services) =>
            {
                // Register configuration
                services.Configure<AzureOpenAISettings>(context.Configuration.GetSection("AzureOpenAI"));
                services.Configure<ApplicationSettings>(context.Configuration.GetSection("Application"));

                // Register core services
                services.AddSingleton<IApplication, Application>();
                services.AddSingleton<IKernelService, KernelService>();
                services.AddSingleton<IFileSystemService, FileSystemService>();

                // Register plugins
                services.AddSingleton<IDevelopmentPlugin, DevelopmentPlugin>();

                // Register agent system
                services.AddSingleton<IAgentDefinitions, AgentDefinitions>();
                services.AddSingleton<IAgentFactory, AgentFactory>();
                services.AddSingleton<IAgentOrchestrator, AgentOrchestrator>();
            });
        }
    }
}
