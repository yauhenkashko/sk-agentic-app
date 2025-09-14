using Microsoft.SemanticKernel;

namespace SK.Agentic.Application.Services
{
    public interface IKernelService
    {
        Kernel GetKernel();
    }
}