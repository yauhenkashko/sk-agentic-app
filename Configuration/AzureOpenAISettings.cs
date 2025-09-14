namespace SK.Agentic.Application.Configuration
{
    public class AzureOpenAISettings
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string DeploymentName { get; set; } = "gpt-4o";

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Endpoint))
            {
                throw new InvalidOperationException("Azure OpenAI Endpoint is not configured");
            }

            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                throw new InvalidOperationException("Azure OpenAI API Key is not configured");
            }

            if (!Uri.TryCreate(Endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            {
                throw new InvalidOperationException("Azure OpenAI Endpoint must be a valid HTTPS URL");
            }
        }
    }
}