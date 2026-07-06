using HRBars.Application.DTOs.InterviewScore;
using HRBars.Application.Interfaces;
using HRBars.Domain.Entities;
using HRBars.Domain.Enums;
using HRBars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HRBars.Application.Services;

public class InterviewCompetencyScoreService : IInterviewCompetencyScoreService
{
	private readonly AppDbContext _context;

	public InterviewCompetencyScoreService(AppDbContext context)
	{
		_context = context;
	}

	public async Task<List<InterviewCompetencyScoreResponse>> GetScoresAsync(Guid interviewId)
	{
		return await _context.InterviewCompetencyScores
			.AsNoTracking()
			.Where(s => s.InterviewId == interviewId)
			.Include(s => s.Competency)
			.OrderBy(s => s.Competency.Name)
			.Select(s => new InterviewCompetencyScoreResponse
			{
				Id = s.Id,
				CompetencyId = s.CompetencyId,
				CompetencyName = s.Competency.Name,
				Score = s.Score,
				Weight = s.Weight,
				MaxScore = s.MaxScore,
				Comment = s.Comment
			})
			.ToListAsync();
	}

	public async Task<List<InterviewCompetencyScoreResponse>> SaveScoresAsync(
		Guid interviewId,
		SaveInterviewScoresRequest request)
	{
		var interview = await _context.Interviews
			.FirstOrDefaultAsync(i => i.Id == interviewId);

		if (interview == null)
			throw new KeyNotFoundException("Собеседование не найдено.");

		if (interview.Status == (short)InterviewStatus.Completed)
		{
			throw new InvalidOperationException(
				"Нельзя изменить оценки завершенного собеседования.");
		}

		var competencyIds = request.Scores
			.Select(s => s.CompetencyId)
			.ToHashSet();

		var competenciesCount = await _context.Competencies
			.CountAsync(c => competencyIds.Contains(c.Id));

		if (competenciesCount != competencyIds.Count)
			throw new InvalidOperationException(
				"Одна или несколько компетенций не существуют.");

		var existingScores = await _context.InterviewCompetencyScores
			.Where(s => s.InterviewId == interviewId)
			.ToListAsync();

		var existingDictionary = existingScores
			.ToDictionary(s => s.CompetencyId);

		foreach (var score in request.Scores)
		{
			if (existingDictionary.TryGetValue(score.CompetencyId, out var existing))
			{
				existing.Score = score.Score;
				existing.Weight = score.Weight;
				existing.MaxScore = score.MaxScore;
				existing.Comment = score.Comment;
			}
			else
			{
				await _context.InterviewCompetencyScores.AddAsync(
					new InterviewCompetencyScore
					{
						Id = Guid.NewGuid(),
						InterviewId = interviewId,
						CompetencyId = score.CompetencyId,
						Score = score.Score,
						Weight = score.Weight,
						MaxScore = score.MaxScore,
						Comment = score.Comment
					});
			}
		}

		var requestIds = competencyIds;

		var removedScores = existingScores
			.Where(s => !requestIds.Contains(s.CompetencyId));

		_context.InterviewCompetencyScores.RemoveRange(removedScores);

		await _context.SaveChangesAsync();

		return await GetScoresAsync(interviewId);
	}
}