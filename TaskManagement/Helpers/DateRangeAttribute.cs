using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Helpers
{
    public class DateRangeAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var fromDateProperty = validationContext.ObjectType.GetProperty("FromDate");
            var toDateProperty = validationContext.ObjectType.GetProperty("ToDate");

            if (fromDateProperty == null || toDateProperty == null)
            {
                return ValidationResult.Success;
            }

            var fromDate = (DateTime?)fromDateProperty.GetValue(validationContext.ObjectInstance);
            var toDate = (DateTime?)toDateProperty.GetValue(validationContext.ObjectInstance);

            if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
            {
                return new ValidationResult("FromDate must be less than or equal to ToDate.");
            }

            return ValidationResult.Success;
        }
    }
}
