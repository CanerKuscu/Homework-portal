using System;
using System.ComponentModel.DataAnnotations;

namespace Homework_portal.Utility
{
    /// <summary>
    /// Validates that a DateTime value is in the future (greater than current time).
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        public FutureDateAttribute() : base("Tarih bugünden sonra olmalýdýr.")
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dateTime)
            {
                if (dateTime <= DateTime.Now)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }

            return ValidationResult.Success;
        }
    }
}
