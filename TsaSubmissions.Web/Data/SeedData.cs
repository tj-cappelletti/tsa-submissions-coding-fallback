using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TsaSubmissions.Web.Models;

namespace TsaSubmissions.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync())
        {
            var hasher = new PasswordHasher<AppUser>();

            var firstJudge = new AppUser { Username = "judge01", Role = AppRole.Judge };
            firstJudge.PasswordHash = hasher.HashPassword(firstJudge, "judge123!");

            var secondJudge = new AppUser { Username = "judge02", Role = AppRole.Judge };
            secondJudge.PasswordHash = hasher.HashPassword(secondJudge, "judge123!");

            db.Users.AddRange(firstJudge, secondJudge);
        }

        await db.SaveChangesAsync();
    }
}
