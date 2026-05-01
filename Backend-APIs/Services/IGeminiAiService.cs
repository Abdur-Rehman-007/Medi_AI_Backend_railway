using Backend_APIs.DTOs;

namespace Backend_APIs.Services
{
    public interface IGeminiAiService
    {
        Task<AiAnalyzeResultDto> AnalyzeAsync(AiAnalyzeRequestDto request, CancellationToken cancellationToken = default);
    }
}
