using HRBars.Application.Interfaces;
using HRBars.Infrastructure;
using HRBars.WebAPI.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRBars.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PdfController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(IPdfService pdfService, ILogger<PdfController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        /// <summary>
        /// Скачать карточку кандидата
        /// </summary>
        [HttpGet("candidate/{candidateId:guid}")]
        ///[RequirePermission("candidates.view")]
        public async Task<IActionResult> DownloadCandidateCard(Guid candidateId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateCandidateCardAsync(candidateId);
                return File(pdfBytes, "application/pdf", $"Карточка_кандидата_{candidateId}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации карточки кандидата {CandidateId}", candidateId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Скачать протокол собеседования
        /// </summary>
        [HttpGet("interview/{interviewId:guid}")]
        ///[RequirePermission("interviews.view")]
        public async Task<IActionResult> DownloadInterviewProtocol(Guid interviewId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateInterviewProtocolAsync(interviewId);
                return File(pdfBytes, "application/pdf", $"Протокол_собеседования_{interviewId}.pdf");
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации протокола собеседования {InterviewId}", interviewId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Скачать приглашение или отказ по ID вакансии
        /// </summary>
        [HttpGet("vacancy/{vacancyId:guid}/offer")]
        public async Task<IActionResult> DownloadOfferByVacancy(Guid vacancyId)
        {
            try
            {
                // isAccepted передаётся из запроса или определяется отдельно
                var isAccepted = true; // TODO: Определять по статусу

                var pdfBytes = await _pdfService.GenerateOfferByVacancyAsync(vacancyId, isAccepted);

                var fileName = isAccepted
                    ? $"Приглашение_по_вакансии_{vacancyId}.pdf"
                    : $"Отказ_по_вакансии_{vacancyId}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации оффера/отказа для вакансии {VacancyId}", vacancyId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}