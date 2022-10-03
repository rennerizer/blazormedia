namespace BlazorMedia.DomainModel
{
    public class Show
    {
        public int Id { get; set; }

        public string Name { get; set; } = default!;

        public string Description { get; set; } = default!;

        public string Image { get; set; } = default!;

        public double Rating { get; set; } = default!;

        public string Genre { get; set; } = default!;

        public string Platform { get; set; } = default!;

        public IEnumerable<Seasons> Seasons { get; set; } = Enumerable.Empty<Seasons>();
    }

    public class Seasons
    {
        public int Number { get; set; }

        public int Year { get; set; }

        public int EpisodeCount { get; set; }
    }

    public enum Platform
    {
        Netflix,
        HBOMax,
        Hulu,
        ParamountPlus,
        Peacock
    }

    public enum Genre
    {
        Comedy,
        Drama,
        Horror,
        SciFi,
    }
}