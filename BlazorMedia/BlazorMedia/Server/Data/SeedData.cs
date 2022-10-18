using System.Formats.Asn1;
using System.Globalization;

using CsvHelper;

namespace BlazorMedia.Server.Data
{
    public class SeedData
    {
        public static void EnsureSeeded(IServiceProvider services)
        {
            var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            using var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (db.Database.EnsureCreated())
            {
                var dir = Path.GetDirectoryName(typeof(SeedData).Assembly.Location)!;
                using var fileReader = (TextReader)File.OpenText(Path.Combine(dir, "Data", "reviewers.csv"));
                using var csv = new CsvReader(fileReader, CultureInfo.InvariantCulture);
                var count = 0;
                while (csv.Read())
                {
                    count++;

                    db.Reviewers.Add(new Reviewer
                    {
                        ReviewerId = count,
                        Platform = csv.GetField<string>(0),
                        Genre = csv.GetField<string>(1),
                        Name = csv.GetField<string>(2),
                        Location = csv.GetField<string>(3),
                        Revenue = (long)(csv.GetField<double>(4) * 100),
                        ModifiedTicks = count,
                        Reviews = (int)Math.Round(700 * Random.Shared.NextDouble() * Random.Shared.NextDouble() * Random.Shared.NextDouble()),
                    });

                    if (count % 1000 == 0)
                    {
                        Console.WriteLine($"Seeded item {count}...");
                    }
                }

                db.SaveChanges();
            }
        }
    }
}
