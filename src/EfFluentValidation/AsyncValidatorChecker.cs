using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

static class AsyncValidatorChecker
{
    public static bool IsAsync(IValidator validator, IValidationContext context)
    {
        if (validator is IEnumerable<IValidationRule> rules)
        {
            context.RootContextData["__FV_IsAsyncExecution"] = true;
            return rules.Any(IsAsync);
        }

        return false;
    }

    static bool IsAsync(IValidationRule validationRule)
    {
        return validationRule.Components.Any(x => x.HasAsyncCondition);
    }

    public static Task<ValidationResult> ValidateEx(this IValidator validator, IValidationContext validationContext)
    {
        if (IsAsync(validator, validationContext))
        {
            return validator.ValidateAsync(validationContext);
        }

        return Task.FromResult(validator.Validate(validationContext));
    }
}