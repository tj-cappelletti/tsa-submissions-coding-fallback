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

        if (!await db.Problems.AnyAsync())
            {
                var problem = new Problem
                {
                    Title = "FizzBuzz Extended",
                    DescriptionMarkdown = "# FizzBuzz Extended\n\nWrite a program that prints numbers 1-100.\n\n- Multiples of 3 print `Fizz`\n- Multiples of 5 print `Buzz`\n- Multiples of both print `FizzBuzz`"
                };

                problem.StarterCodes = new List<ProblemStarterCode>
            {
                new() { Language = SupportedLanguage.Cpp, Code = "#include <iostream>\nint main(){\n    // TODO\n    return 0;\n}" },
                new() { Language = SupportedLanguage.CSharp, Code = "using System;\nclass Program{\n    static void Main(){\n        // TODO\n    }\n}" },
                new() { Language = SupportedLanguage.Java, Code = "public class Main {\n    public static void main(String[] args) {\n        // TODO\n    }\n}" },
                new() { Language = SupportedLanguage.JavaScript, Code = "function main() {\n  // TODO\n}\nmain();" },
                new() { Language = SupportedLanguage.Python, Code = "def main():\n    # TODO\n    pass\n\nif __name__ == '__main__':\n    main()" }
            };

                db.Problems.Add(problem);
            }

            await db.SaveChangesAsync();
        }
    }
}
