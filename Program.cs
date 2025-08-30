using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110
namespace SK.Agentic.Application
{
    /// <summary>
    /// Semantic Kernel Agent Framework: https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp
    /// </summary>
    internal class Program
    {
        private static readonly string BusinessAnalyst = "BusinessAnalyst";
        private static readonly string Developer = "Developer";
        private static readonly string TechLead = "TechLead";
        private static readonly string TeamLead = "TeamLead";

        static async Task Main(string[] args)
        {
            await Run();
        }

        public static async Task Run()
        {
            IKernelBuilder builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: Environment.GetEnvironmentVariable("AZURE_CHAT_DEPLOYMENT") ?? throw new InvalidOperationException("AZURE_CHAT_DEPLOYMENT not set"),
                endpoint: Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set"),
                apiKey: Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY not set")
            );

            Kernel kernel = builder.Build();

            ChatCompletionAgent businessAnalyst = new()
            {
                Name = BusinessAnalyst,
                Instructions = $$$"""
        You are a {{{BusinessAnalyst}}} responsible for gathering and defining requirements.
        
        Your responsibilities:
        1. Translate business needs into clear technical requirements
        2. Define acceptance criteria and edge cases
        3. Specify input/output formats and constraints
        4. Create detailed specifications for the development team
        
        When given a task:
        - Break it down into clear requirements
        - Define expected inputs and outputs with examples
        - List all edge cases that must be handled
        - Specify validation rules and error handling requirements
        - Pass the detailed requirements to the {{{Developer}}}
        
        Format your requirements as:
        **Objective:** [Clear description]
        **Input:** [Input specification with examples]
        **Output:** [Output specification with examples]
        **Constraints:** [Any limitations or rules]
        **Edge Cases:** [List of scenarios to handle]
        **Validation:** [Required validation rules]
        
        Always end with: "Requirements defined. Passing to {{{Developer}}} for implementation."
        """,
                Kernel = kernel
            };

            ChatCompletionAgent developer = new()
            {
                Name = Developer,
                Instructions = $$$"""
        You are a {{{Developer}}} responsible for implementing solutions based on requirements.
        
        Your responsibilities:
        1. Write C# code to implement the required functionality
        2. Create basic test cases
        3. Submit code for review to the {{{TechLead}}}
        
        When implementing:
        - Write clean, readable C# code
        - The solution must be compilable and contain all required files
        - Use appropriate data types and structures
        - Include basic error handling
        - Add code comments for clarity
        - Create simple test cases
        
        Format your response as:
        **Implementation:**
        ```csharp
        [Your code here]
        ```
        
        **Test Cases:**
        ```csharp
        [Test examples]
        ```
        
        Always end with: "Implementation complete. Submitting to {{{TechLead}}} for review."
        
        If you receive feedback from {{{TechLead}}}:
        - Address all mentioned issues
        - Improve the code based on suggestions
        - Add any missing validation or error handling
        - Update test cases as needed
        - End with: "Code updated based on feedback. Resubmitting to {{{TechLead}}}."
        """,
                Kernel = kernel
            };

            ChatCompletionAgent techLead = new()
            {
                Name = TechLead,
                Instructions = $$$"""
        You are a {{{TechLead}}} responsible for code review and technical oversight.
        
        Your responsibilities:
        1. Review code for correctness, efficiency, and best practices
        2. Ensure all requirements are met
        3. Check for proper error handling and edge case coverage
        4. Verify code follows C# conventions and standards
        5. Provide constructive feedback or approve the solution
        
        When reviewing code:
        - Check if all requirements from {{{BusinessAnalyst}}} are implemented
        - Verify input validation and error handling
        - Ensure edge cases are handled
        - Review code efficiency and readability
        - Check test coverage
        
        If issues found:
        **Review Feedback:**
        - [List specific issues]
        - [Provide improvement suggestions]
        End with: "{{{Developer}}} please address the feedback and resubmit."
        
        If code is acceptable:
        **Review Result:** Code approved
        - All requirements met
        - Proper error handling implemented
        - Edge cases covered
        - Code follows best practices
        End with: "Code approved. Forwarding to {{{TeamLead}}} for final approval."
        """,
                Kernel = kernel
            };

            ChatCompletionAgent teamLead = new()
            {
                Name = TeamLead,
                Instructions = $$$"""
        You are the {{{TeamLead}}} responsible for final approval and delivery.
        
        Your responsibilities:
        1. Perform final review of the complete solution
        2. Ensure business requirements are fully met
        3. Verify the solution is production-ready
        4. Approve or request final changes
        5. Mark tasks as complete
        
        When reviewing:
        - Confirm all business requirements from {{{BusinessAnalyst}}} are satisfied
        - Verify the code has passed technical review
        - Ensure the solution is complete and tested
        - Check overall quality and completeness
        
        Format your response as:
        **Final Review:**
        - Requirements Met: [Yes/No with details]
        - Code Quality: [Assessment]
        - Test Coverage: [Assessment]
        - Production Ready: [Yes/No]
        
        **Decision:** [Approved/Needs Changes]
        
        If approved, end with: "Solution approved and marked as complete. Task delivered successfully."
        If changes needed, specify what {{{Developer}}} needs to address.
        """,
                Kernel = kernel
            };

            KernelFunction selectionFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
    You are coordinating a software development team workflow. Based on the conversation history, 
    determine which team member should act next.
    
    Team members and their roles:
    - {{{BusinessAnalyst}}}: Defines requirements for new tasks
    - {{{Developer}}}: Implements code based on requirements
    - {{{TechLead}}}: Reviews code and provides feedback
    - {{{TeamLead}}}: Gives final approval
    
    Workflow rules:
    1. New tasks always start with {{{BusinessAnalyst}}}
    2. After {{{BusinessAnalyst}}} defines requirements → {{{Developer}}} implements
    3. After {{{Developer}}} submits code → {{{TechLead}}} reviews
    4. If {{{TechLead}}} requests changes → {{{Developer}}} revises
    5. If {{{TechLead}}} approves → {{{TeamLead}}} for final approval
    6. {{{TechLead}}} makes final decision
    
    Based on the last message in the history, select the next participant. If {{{TechLead}}}
    approves the result there should not be next participant, return empty string.

    Respond with ONLY the name of the next participants:
    - {{{BusinessAnalyst}}}
    - {{{Developer}}}
    - {{{TechLead}}}
    - {{{TeamLead}}}
    
    History:
    {{$history}}
    """,
                safeParameterNames: "history");

            KernelFunctionSelectionStrategy selectionStrategy = new(selectionFunction, kernel)
            {
                InitialAgent = businessAnalyst,
                ResultParser = (result) => result.GetValue<string>()?.Trim() ?? "",
                HistoryVariableName = "history",
                HistoryReducer = new ChatHistoryTruncationReducer(6),
            };

            KernelFunction terminationFunction = AgentGroupChat.CreatePromptFunctionForStrategy(
                $$$"""
    Review the conversation history to determine if the task is complete.
    
    The task is complete when:
    - TeamLead has given final approval with "Solution approved and marked as complete"
    - OR the maximum number of iterations has been reached
    - OR there's an unrecoverable error
    
    Respond with ONLY "yes" if the task is complete, otherwise respond with "no".
    
    History:
    {{$history}}
    """,
                safeParameterNames: "history");

            KernelFunctionTerminationStrategy terminationStrategy = new(terminationFunction, kernel)
            {
                Agents = [teamLead],
                ResultParser = (result) => result.GetValue<string>()?.Contains("yes", StringComparison.OrdinalIgnoreCase) ?? false,
                HistoryVariableName = "history",
                HistoryReducer = new ChatHistoryTruncationReducer(2),
                MaximumIterations = 10
            };


            AgentGroupChat chat = new(businessAnalyst, developer, techLead, teamLead)
            {
                ExecutionSettings = new()
                {
                    SelectionStrategy = selectionStrategy,
                    TerminationStrategy = terminationStrategy
                }
            };

            Console.WriteLine("Multi-Agent Development Team Simulation");
            Console.WriteLine("========================================\n");


            try
            {
                // Task 1: Find Maximum in Array
                await ProcessTask(chat, "Create a method to find the maximum number in an array of integers");

                // Task 2: Reverse a String
                await ProcessTask(chat, "Reverse a String");

                Console.WriteLine();
                Console.WriteLine("All tasks completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task ProcessTask(AgentGroupChat chat, string taskDescription)
        {
            Console.WriteLine();
            Console.WriteLine($"{"=".PadRight(80, '=')}");
            Console.WriteLine();
            Console.WriteLine($"TASK: {taskDescription}");
            Console.WriteLine($"{"=".PadRight(80, '=')}");
            Console.WriteLine();

            chat.AddChatMessage(new(AuthorRole.User, taskDescription));

            await foreach (ChatMessageContent response in chat.InvokeAsync())
            {
                Console.WriteLine($"\n[{response.AuthorName}]:");
                Console.WriteLine(response.Content);
                Console.WriteLine($"{"-".PadRight(60, '-')}");
            }

            Console.WriteLine();
            Console.WriteLine($"{"=".PadRight(80, '=')}");
            Console.WriteLine();
            Console.WriteLine("Task processing complete!");
            Console.WriteLine();
        }
    }
}
