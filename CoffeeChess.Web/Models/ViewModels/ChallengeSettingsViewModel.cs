using System.ComponentModel.DataAnnotations;
using CoffeeChess.Domain.Matchmaking.Enums;

namespace CoffeeChess.Web.Models.ViewModels;

public class ChallengeSettingsViewModel : IValidatableObject
{
    [Display(Name = "minutes")]
    [Required(ErrorMessage = "The number of {0} for the time control is required.")] 
    [Range(1, 180,ErrorMessage = "The number of {0} should be in the range between {1} and {2}.")]
    public int Minutes { get; init; }
    
    [Display(Name = "increment")]
    [Required(ErrorMessage = "The {0} for the time control is required.")]
    [Range(0, 59, ErrorMessage = "The {0} should be in the range between {1} and {2}.")]
    public int Increment { get; init; }
    
    [Display(Name = "color preference")]
    [EnumDataType(typeof(ColorPreference), ErrorMessage = "The {0} should be either White, Black or Any.")]
    public ColorPreference ColorPreference { get; init; } = ColorPreference.Any;
    
    [Range(0, 4000, ErrorMessage = "The {0} for the rating range preference " +
                                   "should be in the range between {1} and {2}.")]
    [Display(Name = "min rating")]
    public int MinRating { get; init; } = 0;
    
    [Range(0, 4000, ErrorMessage = "The {0} for the rating range preference " +
                                   "should be in the range between {1} and {2}.")]
    [Display(Name = "max rating")]
    public int MaxRating { get; init; } = 4000;


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinRating > MaxRating)
            yield return new ValidationResult(
                $"The {GetDisplayName(nameof(MinRating))} for the rating range preference " +
                $"should be less than or equal to the {GetDisplayName(nameof(MaxRating))}.",
                [nameof(MinRating)]);
    }
    
    private static string GetDisplayName(string propertyName)
    {
        var prop = typeof(ChallengeSettingsViewModel).GetProperty(propertyName);
        if (prop == null) 
            return propertyName;

        var attr = prop
            .GetCustomAttributes(typeof(DisplayAttribute), false)
            .Cast<DisplayAttribute>()
            .FirstOrDefault();

        return attr?.Name ?? propertyName;
    }
}