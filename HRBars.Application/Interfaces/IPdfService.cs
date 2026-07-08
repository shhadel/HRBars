using Microsoft.Extensions.Logging;

namespace HRBars.Application.Interfaces
{
    public interface IPdfService
    {
        /// <summary>
        /// Генерация карточки кандидата
        /// </summary>
        Task<byte[]> GenerateCandidateCardAsync(Guid candidateId);

        /// <summary>
        /// Генерация протокола собеседования
        /// </summary>
        Task<byte[]> GenerateInterviewProtocolAsync(Guid interviewId);

        /// <summary>
        /// Генерация оффера или отказа
        /// </summary>
        Task<byte[]> GenerateOfferAsync(Guid applicationId, bool isAccepted);
    }
}