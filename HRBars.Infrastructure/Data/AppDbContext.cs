using Microsoft.EntityFrameworkCore;
using HRBars.Domain.Entities;

namespace HRBars.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
    public DbSet<Role> Roles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<WorkExperience> WorkExperiences { get; set; }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Vacancy> Vacancies { get; set; }
    public DbSet<CompetencyMatrixTemplate> CompetencyMatrixTemplates { get; set; }
    public DbSet<Competency> Competencies { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<InterviewCompetencyScore> InterviewCompetencyScores { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(up => new { up.UserId, up.PermissionId });
            
            entity.HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(up => up.GrantedByUser)
                .WithMany()
                .HasForeignKey(up => up.GrantedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasOne(i => i.CreatedByUser)
                .WithMany(u => u.CreatedInterviews)
                .HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(i => i.DecidedByUser)
                .WithMany(u => u.DecidedInterviews)
                .HasForeignKey(i => i.DecidedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        modelBuilder.Entity<InterviewCompetencyScore>()
            .HasIndex(ics => new { ics.InterviewId, ics.CompetencyId })
            .IsUnique();
        
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
            
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Admin", Description = "Full access" },
            new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "HR", Description = "Human Resources" },
            new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Reshala", Description = "Decision maker" }
        );
            
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "candidates.view", Description = "View candidates", Category = "candidates" },
            new Permission { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "candidates.create", Description = "Create candidates", Category = "candidates" },
            new Permission { Id = Guid.Parse("66666666-6666-6666-6666-666666666666"), Name = "interviews.view", Description = "View interviews", Category = "interviews" },
            new Permission { Id = Guid.Parse("77777777-7777-7777-7777-777777777777"), Name = "interviews.make_decision", Description = "Make decision", Category = "interviews" }
        );
    }
}