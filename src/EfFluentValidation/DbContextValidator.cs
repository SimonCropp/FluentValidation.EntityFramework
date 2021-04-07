using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EfFluentValidation
{
    public static class DbContextValidator
    {
        #region TryValidateSignature

        /// <summary>
        /// Validates a <see cref="DbContext"/> an relies on the caller to handle those results.
        /// </summary>
        /// <param name="dbContext">
        /// The <see cref="DbContext"/> to validate.
        /// </param>
        /// <param name="validatorFactory">
        /// A factory that accepts a entity type and returns
        /// a list of corresponding <see cref="IValidator"/>.
        /// </param>
        public static async Task<(bool isValid, IReadOnlyList<EntityValidationFailure> failures)> TryValidate(
                DbContext dbContext,
                Func<Type, IEnumerable<IValidator>> validatorFactory)

            #endregion

        {
            Guard.AgainstNull(dbContext, nameof(dbContext));
            Guard.AgainstNull(validatorFactory, nameof(validatorFactory));
            var entityFailures = await InnerVerify(dbContext, validatorFactory).ToAsyncList();
            return (!entityFailures.Any(), entityFailures);
        }

        static async IAsyncEnumerable<EntityValidationFailure> InnerVerify(DbContext dbContext, Func<Type, IEnumerable<IValidator>> validatorFactory)
        {
            static void AddFailures(List<TypeValidationFailure> failures, IEnumerable<ValidationFailure> errors, IValidator validator)
            {
                failures.AddRange(errors.Select(failure => new TypeValidationFailure(validator.GetType(), failure)));
            }

            foreach (var entry in dbContext.AddedEntries())
            {
                List<TypeValidationFailure> validationFailures = new();
                var clrType = entry.Metadata.ClrType;
                var validationContext = BuildValidationContext(dbContext, entry);
                foreach (var validator in validatorFactory(clrType))
                {
                    var result = await validator.ValidateEx(validationContext);
                    AddFailures(validationFailures, result.Errors, validator);
                }

                if (validationFailures.Any())
                {
                    yield return new(entry.Entity, clrType, validationFailures);
                }
            }

            foreach (var entry in dbContext.ModifiedEntries())
            {
                List<TypeValidationFailure> validationFailures = new();
                var clrType = entry.Metadata.ClrType;
                var validationContext = BuildValidationContext(dbContext, entry);
                var changedProperties = entry.ChangedProperties().ToList();
                foreach (var validator in validatorFactory(clrType))
                {
                    var result = await validator.ValidateEx(validationContext);
                    var errors = result.Errors.Where(x => changedProperties.Contains(x.PropertyName));

                    AddFailures(validationFailures, errors, validator);
                }

                if (validationFailures.Any())
                {
                    yield return new(entry.Entity, clrType, validationFailures);
                }
            }
        }

        static IValidationContext BuildValidationContext(DbContext dbContext, EntityEntry entry)
        {
            //TODO: cache
            var validationContextType = typeof(ValidationContext<>).MakeGenericType(entry.Metadata.ClrType);
            var validationContext = (IValidationContext) Activator.CreateInstance(validationContextType, entry.Entity);
            validationContext.RootContextData.Add("EfContext", new EfContext(dbContext, entry));
            return validationContext;
        }

        #region ValidateSignature

        /// <summary>
        /// Validates a <see cref="DbContext"/> and throws a <see cref="MessageValidationException"/>
        /// if any changed entity is not valid.
        /// </summary>
        /// <param name="dbContext">
        /// The <see cref="DbContext"/> to validate.
        /// </param>
        /// <param name="validatorFactory">
        /// A factory that accepts a entity type and returns a
        /// list of corresponding <see cref="IValidator"/>.
        /// </param>
        public static async Task Validate(
                DbContext dbContext,
                Func<Type, IEnumerable<IValidator>> validatorFactory)

            #endregion

        {
            var (isValid, failures) = await TryValidate(dbContext, validatorFactory);
            if (!isValid)
            {
                throw new EntityValidationException(failures);
            }
        }
    }
}