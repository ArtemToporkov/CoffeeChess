using System.ComponentModel.DataAnnotations;
using CoffeeChess.Domain.Matchmaking.Enums;

namespace CoffeeChess.Web.Models.ViewModels;

public class ChallengeSettingsViewModel : IValidatableObject
{
    [Required] 
    [Range(1, 180)]
    public int Minutes { get; set; }
    
    [Required]
    [Range(0, 59)]
    public int Increment { get; set; }
    
    [EnumDataType(typeof(ColorPreference))]
    public ColorPreference ColorPreference { get; set; } = ColorPreference.Any;
    
    [Range(0, 4000)]
    [Display(Name = "Min rating")]
    public int MinRating { get; set; } = 0;
    
    [Range(0, 4000)]
    [Display(Name = "Max rating")]
    public int MaxRating { get; set; } = int.MaxValue;


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinRating > MaxRating)
            yield return new ValidationResult(
                $"{GetDisplayName(nameof(MinRating))} should be less than " +
                $"or equal to {GetDisplayName(nameof(MaxRating))}",
                [nameof(MinRating), nameof(MaxRating)]);
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