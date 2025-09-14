namespace SK.Agentic.Application.Agents
{
    public interface IAgentDefinitions
    {
        string BusinessAnalystName { get; }
        string DeveloperName { get; }
        string TechLeadName { get; }
        string TeamLeadName { get; }

        string GetBusinessAnalystInstructions();
        string GetDeveloperInstructions();
        string GetTechLeadInstructions();
        string GetTeamLeadInstructions();

        string GetSelectionStrategy();
        string GetTerminationStrategy();
    }
}
