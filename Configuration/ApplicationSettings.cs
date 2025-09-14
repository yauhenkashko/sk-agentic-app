namespace SK.Agentic.Application.Configuration
{
    public class ApplicationSettings
    {
        public string OutputDirectory { get; set; } = "output";
        public int MaxIterations { get; set; } = 10;
        public int HistoryTruncation { get; set; } = 6;
        public bool EnableDetailedLogging { get; set; } = false;
    }
}
