using System.ComponentModel.DataAnnotations;

namespace ForexGround.ApiService.Providers.Frankfurter;

[DateRange]
public class ForexHistoryRequest
{
    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }


    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (ForexHistoryRequest)validationContext.ObjectInstance;
            if (model.EndDate.HasValue && model.StartDate > model.EndDate)
            {
                return new ValidationResult("StartDate must be earlier than EndDate.");
            }

            return ValidationResult.Success;
        }
    }
}
