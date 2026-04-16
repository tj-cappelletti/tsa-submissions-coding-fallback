using Microsoft.EntityFrameworkCore;
using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<ProblemStarterCode> ProblemStarterCodes => Set<ProblemStarterCode>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<ParticipantSession> ParticipantSessions => Set<ParticipantSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<ProblemStarterCode>()
            .HasIndex(s => new { s.ProblemId, s.Language })
            .IsUnique();

        modelBuilder.Entity<ParticipantSession>()
            .HasIndex(s => s.ParticipantId)
            .IsUnique();

        modelBuilder.Entity<Submission>()
            .Property(s => s.UploadedAtUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}
