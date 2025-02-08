using System.ComponentModel.DataAnnotations;
using System.IO;

namespace ExcelVbaSearch
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class DirectoryExistsAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Jeżeli pole jest puste lub nie jest typu string, zwracamy błąd (lub można dopuścić puste).
            if (value is not string path || string.IsNullOrWhiteSpace(path))
            {
                return new ValidationResult("Ścieżka nie może być pusta.");
            }

            if (!Directory.Exists(path))
            {
                return new ValidationResult($"Folder '{path}' nie istnieje.");
            }

            // Jeśli wszystko OK, zwracamy sukces
            return ValidationResult.Success;
        }
    }
}
