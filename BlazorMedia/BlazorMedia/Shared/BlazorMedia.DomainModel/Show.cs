//using FluentValidation;

using FluentValidation;

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

        public List<Season> Seasons { get; set; } = new List<Season>();
    }

    public class Season
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

    public class ShowValidator : AbstractValidator<Show>
    {
        public ShowValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Please enter a name"); 
            RuleFor(x => x.Description).NotEmpty().WithMessage("Please enter a description"); 
            RuleFor(x => x.Platform).NotEmpty().WithMessage("Please enter a platform"); 
            RuleFor(x => x.Genre).NotEmpty().WithMessage("Please enter a genre"); 
            RuleFor(x => x.Rating).NotEmpty().WithMessage("Please enter a rating");
            RuleFor(x => x.Seasons).NotEmpty().WithMessage("Please add at least one season");
        }
    }

    public class SeasonValidator : AbstractValidator<Season>
    {
        public SeasonValidator()
        {
            RuleFor(x => x.Number).NotEmpty().WithMessage("Please enter a season number");
            RuleFor(x => x.EpisodeCount).GreaterThan(0).WithMessage("Please enter an episode count");
            RuleFor(x => x.Year).GreaterThan(0).WithMessage("Please enter a year for the season");
        }
    }
}