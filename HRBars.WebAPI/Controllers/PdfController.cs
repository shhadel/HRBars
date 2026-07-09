using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRBars.Application.Interfaces;
using HRBars.Infrastructure;
using HRBars.WebAPI.Attributes;

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
        [RequirePermission("reports.download_candidate_card")]
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
        [RequirePermission("reports.download_interview_protocol")]
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
        /// Скачать приглашение или отказ
        /// </summary>
        [HttpGet("application/{applicationId:guid}/result")]
        [RequirePermission("reports.offer")]
        public async Task<IActionResult> DownloadOfferResult(Guid applicationId)
        {
            try
            {
                // Определяем статус отклика через отдельный запрос к БД
                // или передаём параметр из фронтенда
                var isAccepted = true; // TODO: Определять из статуса Application

                var pdfBytes = await _pdfService.GenerateOfferAsync(applicationId, isAccepted);

                var fileName = isAccepted
                    ? $"Приглашение_{applicationId}.pdf"
                    : $"Отказ_{applicationId}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации оффера/отказа для {ApplicationId}", applicationId);
                return StatusCode(500, new { message = "Внутренняя ошибка сервера" });
            }
        }
    }
}