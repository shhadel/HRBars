using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Infrastructure.Data;

namespace HRBars.Infrastructure.Services
{
    public class PdfService : IPdfService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PdfService> _logger;
        private readonly string _logoPath;

        public PdfService(AppDbContext context, ILogger<PdfService> logger)
        {
            _context = context;
            _logger = logger;
            _logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "BarsLogo.png");

            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Генерация карточки кандидата
        /// </summary>
        public async Task<byte[]> GenerateCandidateCardAsync(Guid candidateId)
        {
            var candidate = await _context.Candidates
                .Include(c => c.WorkExperiences)
                .Include(c => c.Educations)
                .FirstOrDefaultAsync(c => c.Id == candidateId);

            if (candidate == null)
                throw new InvalidOperationException("Кандидат не найден");

            var fullName = $"{candidate.LastName} {candidate.FirstName} {candidate.MiddleName}".Trim();

            var educationText = string.Join("\n", candidate.Educations.Select(e =>
                $"{e.Institution}" +
                (!string.IsNullOrEmpty(e.Faculty) ? $", {e.Faculty}" : "") +
                (!string.IsNullOrEmpty(e.Degree) ? $", {e.Degree}" : "") +
                (e.StartYear.HasValue ? $", {e.StartYear.Value}" : "") +
                (e.EndYear.HasValue ? $"-{e.EndYear.Value}" : "-н.в.")
            ));

            var workExperienceText = string.Join("\n\n", candidate.WorkExperiences.Select(w =>
                $"{w.Company}\n{w.Position}\n{w.StartDate:yyyy} – {(w.EndDate.HasValue ? w.EndDate.Value.ToString("yyyy") : "н.в.")}" +
                (!string.IsNullOrEmpty(w.Description) ? $"\n{w.Description}" : "")
            ));

            var totalExperienceYears = candidate.WorkExperiences.Sum(w =>
                (w.EndDate ?? DateTime.Now).Year - w.StartDate.Year);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    // ---------- Заголовок ----------
                    page.Header().ShowOnce().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            if (File.Exists(_logoPath))
                                row.ConstantItem(120).Image(_logoPath);
                            row.RelativeItem();
                        });

                        column.Item().PaddingTop(30);
                        column.Item()
                            .Text("КАРТОЧКА КАНДИДАТА")
                            .FontSize(24)
                            .Bold()
                            .AlignCenter();
                    });

                    // ---------- Основное содержимое ----------
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().PaddingTop(20);

                        column.Item().Text("ЛИЧНАЯ ИНФОРМАЦИЯ").FontSize(18).Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(170);
                                columns.RelativeColumn();
                            });

                            void Row(string title, string value)
                            {
                                table.Cell().BorderBottom(1).Padding(5).Text(title).Bold();
                                table.Cell().BorderBottom(1).Padding(5).Text(value);
                            }

                            Row("ФИО", fullName);
                            Row("Телефон", candidate.Phone);
                            Row("Email", candidate.Email ?? string.Empty);
                            Row("Город", candidate.City ?? string.Empty);
                            Row("Желаемая должность", candidate.DesiredVacancy.Title ?? "Не указана");
                            Row("Опыт работы", $"{totalExperienceYears} лет");
                        });

                        if (!string.IsNullOrEmpty(educationText))
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("ОБРАЗОВАНИЕ").FontSize(18).Bold();
                            column.Item().Text(educationText);
                        }

                        if (!string.IsNullOrEmpty(candidate.Skills))
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("КЛЮЧЕВЫЕ НАВЫКИ").FontSize(18).Bold();
                            column.Item().Text(candidate.Skills);
                        }

                        if (!string.IsNullOrEmpty(workExperienceText))
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("ОПЫТ РАБОТЫ").FontSize(18).Bold();
                            column.Item().Text(workExperienceText);
                        }

                        // ---------- Подпись ----------
                        column.Item().PaddingTop(30);
                        column.Item().ShowEntire().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Подпись HR");
                                col.Item().PaddingTop(20);
                                col.Item().Text("______________________");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignRight().Text($"Дата: {DateTime.Now:dd.MM.yyyy}");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Генерация протокола собеседования
        /// </summary>
        public async Task<byte[]> GenerateInterviewProtocolAsync(Guid interviewId)
        {
            var interview = await _context.Interviews
                .Include(i => i.Application)
                    .ThenInclude(a => a.Candidate)
                .Include(i => i.Application)
                    .ThenInclude(a => a.Vacancy)
                .Include(i => i.CreatedByUser)
                .Include(i => i.CompetencyScores)
                    .ThenInclude(cs => cs.Competency)
                .FirstOrDefaultAsync(i => i.Id == interviewId);

            if (interview == null)
                throw new InvalidOperationException("Собеседование не найдено");

            var application = interview.Application;
            var candidate = application.Candidate;
            var vacancy = application.Vacancy;

            var candidateName = $"{candidate.LastName} {candidate.FirstName} {candidate.MiddleName}".Trim();
            var recruiter = interview.CreatedByUser != null
                ? $"{interview.CreatedByUser.LastName} {interview.CreatedByUser.FirstName}".Trim()
                : "Не указан";

            var format = interview.Format switch
            {
                1 => "Очно",
                2 => "Онлайн",
                _ => "Не указан"
            };

            var result = interview.Result switch
            {
                1 => "Рекомендован",
                2 => "Не рекомендован",
                3 => "На рассмотрении",
                _ => "Не определён"
            };

            var matchIndex = interview.CompetencyScores.Any()
                ? (int)Math.Round(interview.CompetencyScores.Average(c => c.Score) * 10)
                : 0;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    // ---------- Заголовок ----------
                    page.Header().ShowOnce().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            if (File.Exists(_logoPath))
                                row.ConstantItem(120).Image(_logoPath);
                            row.RelativeItem();
                        });

                        column.Item().PaddingTop(30);
                        column.Item()
                            .Text("ПРОТОКОЛ ОЦЕНКИ КАНДИДАТА")
                            .FontSize(24)
                            .Bold()
                            .AlignCenter();
                    });

                    // ---------- Основное содержимое ----------
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().PaddingTop(20);

                        column.Item().Text("ИНФОРМАЦИЯ О КАНДИДАТЕ").FontSize(18).Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(200);
                                columns.RelativeColumn();
                            });

                            void Row(string title, string value)
                            {
                                table.Cell().BorderBottom(1).Padding(5).Text(title).Bold();
                                table.Cell().BorderBottom(1).Padding(5).Text(value);
                            }

                            Row("ФИО", candidateName);
                            Row("Вакансия", vacancy?.Title ?? "Не указана");
                            Row("Отдел", vacancy?.Department ?? "Не указан");
                            Row("HR-менеджер", recruiter);
                            Row("Дата интервью", $"{interview.InterviewDate:dd.MM.yyyy HH:mm}");
                            Row("Формат", format);
                            Row("Место", interview.Location ?? "Не указано");
                            Row("Индекс совпадения", $"{matchIndex}%");
                            Row("Результат", result);
                        });

                        if (!string.IsNullOrEmpty(interview.Plan))
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("ПЛАН СОБЕСЕДОВАНИЯ").FontSize(18).Bold();
                            column.Item().Text(interview.Plan);
                        }

                        if (interview.CompetencyScores.Any())
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("ОЦЕНКА КОМПЕТЕНЦИЙ").FontSize(18).Bold();

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn(3);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Компетенция").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Оценка").Bold().AlignCenter();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Комментарий").Bold();
                                });

                                foreach (var score in interview.CompetencyScores)
                                {
                                    var name = score.Competency?.Name ?? "Неизвестная компетенция";
                                    table.Cell().BorderBottom(1).Padding(5).Text(name);
                                    table.Cell().BorderBottom(1).Padding(5).AlignCenter().Text($"{score.Score}/10");
                                    table.Cell().BorderBottom(1).Padding(5).Text(score.Comment ?? string.Empty);
                                }
                            });
                        }

                        if (!string.IsNullOrEmpty(interview.DecisionComment))
                        {
                            column.Item().PaddingTop(10);
                            column.Item().Text("КОММЕНТАРИЙ К РЕШЕНИЮ").FontSize(18).Bold();
                            column.Item().Text(interview.DecisionComment);
                        }

                        // ---------- Подпись ----------
                        column.Item().PaddingTop(30);
                        column.Item().ShowEntire().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("HR-менеджер");
                                col.Item().PaddingTop(20);
                                col.Item().Text("______________________");
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().AlignRight().Text($"Дата: {DateTime.Now:dd.MM.yyyy}");
                            });
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }

        /// <summary>
        /// Генерация оффера или отказа
        /// </summary>
        public async Task<byte[]> GenerateOfferAsync(Guid applicationId, bool isAccepted)
        {
            var application = await _context.Applications
                .Include(a => a.Candidate)
                .Include(a => a.Vacancy)
                .Include(a => a.CreatedByUser)
                .FirstOrDefaultAsync(a => a.Id == applicationId);

            if (application == null)
                throw new InvalidOperationException("Отклик не найден");

            var candidate = application.Candidate;
            var vacancy = application.Vacancy;

            var candidateName = $"{candidate.LastName} {candidate.FirstName} {candidate.MiddleName}".Trim();
            var recruiter = application.CreatedByUser != null
                ? $"{application.CreatedByUser.LastName} {application.CreatedByUser.FirstName}".Trim()
                : "Не указан";

            var employmentType = vacancy?.EmploymentType switch
            {
                1 => "Полная занятость",
                2 => "Частичная занятость",
                3 => "Проектная работа",
                4 => "Стажировка",
                _ => "Не указан"
            };

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    // ---------- Заголовок ----------
                    page.Header().Element(header =>
                    {
                        header.ShowOnce().Column(column =>
                        {
                            column.Item().Row(row =>
                            {
                                if (File.Exists(_logoPath))
                                    row.ConstantItem(120).Image(_logoPath);

                                row.RelativeItem().AlignRight().Column(col =>
                                {
                                    col.Item().AlignRight().Text($"Дата: {DateTime.Now:dd.MM.yyyy}");
                                    col.Item().AlignRight().Text($"Кандидат: {candidateName}");
                                    col.Item().AlignRight().Text($"Вакансия: {vacancy?.Title ?? "Не указана"}");
                                    col.Item().AlignRight().Text($"HR-менеджер: {recruiter}");
                                });
                            });

                            column.Item().PaddingTop(40);
                            column.Item()
                                .Text(isAccepted ? "ОФФЕР" : "УВЕДОМЛЕНИЕ ОБ ОТКАЗЕ")
                                .FontSize(30)
                                .Bold()
                                .AlignCenter();
                        });
                    });

                    // ---------- Основная часть ----------
                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().PaddingTop(20);

                        if (isAccepted)
                        {
                            var salaryText = vacancy?.SalaryFrom.HasValue == true && vacancy?.SalaryTo.HasValue == true
                                ? $"{vacancy.SalaryFrom.Value:N0} - {vacancy.SalaryTo.Value:N0} рублей в месяц"
                                : vacancy?.SalaryFrom.HasValue == true
                                    ? $"от {vacancy.SalaryFrom.Value:N0} рублей в месяц"
                                    : "по результатам собеседования";

                            column.Item().Text(
$@"Уважаемый(ая) {candidateName}!

Благодарим Вас за участие в конкурсе на должность «{vacancy?.Title ?? "Не указана"}»
в отдел {vacancy?.Department ?? "Не указан"}.

По результатам рассмотрения Вашей кандидатуры
АО «БАРС Груп» рада предложить Вам работу.

Мы высоко оценили Ваш профессиональный опыт,
технические знания и личные качества.

Предлагаем Вам присоединиться к нашей команде
на следующих условиях:

• Должность: {vacancy?.Title ?? "Не указана"}
• Отдел: {vacancy?.Department ?? "Не указан"}
• Тип занятости: {employmentType}
• Заработная плата: {salaryText}

Просим подтвердить согласие на трудоустройство
в течение 5 рабочих дней.");
                        }
                        else
                        {
                            column.Item().Text(
$@"Уважаемый(ая) {candidateName}!

Благодарим Вас за участие в собеседовании
на должность «{vacancy?.Title ?? "Не указана"}»
в отдел {vacancy?.Department ?? "Не указан"}.

К сожалению, по итогам рассмотрения
было принято решение выбрать другого кандидата.

Мы благодарим Вас за интерес,
проявленный к АО «БАРС Груп».

Желаем Вам успехов
в дальнейшей профессиональной деятельности.");
                        }

                        column.Item().PaddingTop(30);

                        // ---------- Детали ----------
                        column.Item().Text("Основные сведения").FontSize(18).Bold();
                        column.Item().PaddingTop(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(160);
                                columns.RelativeColumn(1);
                            });

                            void Row(string label, string value)
                            {
                                table.Cell().BorderBottom(1).Padding(5).Text(label).Bold();
                                table.Cell().BorderBottom(1).Padding(5).Text(value);
                            }

                            Row("Компания", "АО «БАРС Груп»");
                            Row("Кандидат", candidateName);
                            Row("Вакансия", vacancy?.Title ?? "Не указана");
                            Row("Отдел", vacancy?.Department ?? "Не указан");
                            Row("HR-менеджер", recruiter);
                            Row("Дата оффера", $"{DateTime.Now:dd.MM.yyyy}");
                            Row("Тип занятости", employmentType);

                            if (vacancy?.SalaryFrom.HasValue == true && vacancy?.SalaryTo.HasValue == true)
                                Row("Заработная плата", $"{vacancy.SalaryFrom.Value:N0} - {vacancy.SalaryTo.Value:N0} руб.");
                            else if (vacancy?.SalaryFrom.HasValue == true)
                                Row("Заработная плата", $"от {vacancy.SalaryFrom.Value:N0} руб.");

                            Row("Статус", isAccepted ? "Предложение действительно" : "Отказ");
                        });

                        column.Item().PaddingTop(30);

                        // ---------- Подписи ----------
                        column.Item().Text("Подписи").FontSize(18).Bold();
                        column.Item().PaddingTop(15);

                        column.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Работодатель");
                                col.Item().PaddingTop(20);
                                col.Item().Text("________________________");
                            });

                            if (isAccepted)
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().AlignRight().Text("Кандидат");
                                    col.Item().PaddingTop(20);
                                    col.Item().AlignRight().Text("________________________");
                                });
                            }
                        });

                        column.Item().PaddingTop(30);
                        column.Item().Text(text =>
                        {
                            text.Span("Документ сформирован автоматически ");
                            text.Span($"{DateTime.Now:dd.MM.yyyy HH:mm}").SemiBold();
                        });
                    });

                    // ---------- Подвал ----------
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("АО «БАРС Груп»   |   Страница ");
                        text.CurrentPageNumber();
                        text.Span(" из ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();
        }
    }
}