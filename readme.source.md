# <img src="/src/icon.png" height="30px"> EfFluentValidation

[![Build status](https://ci.appveyor.com/api/projects/status/rj0vpbi5nd17se7h?svg=true)](https://ci.appveyor.com/project/SimonCropp/effluentvalidation)
[![NuGet Status](https://img.shields.io/nuget/v/EfFluentValidation.svg)](https://www.nuget.org/packages/EfFluentValidation/)

Adds [FluentValidation](https://fluentvalidation.net/) support to [EntityFramework](https://docs.microsoft.com/en-us/ef/core/).

Support is available via a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-effluentvalidation?utm_source=nuget-effluentvalidation&utm_medium=referral&utm_campaign=enterprise).


## NuGet package

 * https://nuget.org/packages/EfFluentValidation/


## Usage


### Define Validators

snippet: Employee.cs

See [Creating a validator](https://docs.fluentvalidation.net/en/latest/start.html).


### Context

Extra context is passed through FluentValidations [CustomContext](https://docs.fluentvalidation.net/en/latest/custom-validators.html#writing-a-custom-validator).

Data:

snippet: EfContext

Usage:

snippet: ValidatorWithContext.cs


### ValidationFinder

ValidationFinder wraps `FluentValidation.AssemblyScanner.FindValidatorsInAssembly` to provide convenience methods for scanning Assemblies for validators.

snippet: FromAssemblyContaining


## DbContextValidator

`DbContextValidator` performs the validation a DbContext. It has two method:


### TryValidate

snippet: TryValidateSignature


### Validate

snippet: ValidateSignature


### ValidatorTypeCache

`ValidatorTypeCache` creates and caches `IValidator` instances against their corresponding entity type.

It can only be used against validators that have a public default constructor (i.e. no parameters).

snippet: ValidatorTypeCacheUsage


### ValidatorFactory

Many APIs take a validation factory with the signature `Func<Type, IEnumerable<IValidator>>` where `Type` is the entity type and `IEnumerable<IValidator>` is all validators for that entity type.

This approach allows a flexible approach on how Validators can be instantiated.


#### DefaultValidatorFactory

`DefaultValidatorFactory` combines [ValidatorTypeCache](#ValidatorTypeCache) and [ValidationFinder](#ValidationFinder).

It assumes that all validators for a DbContext exist in the same assembly as the DbContext and have public default constructors.

Implementation:

snippet: DefaultValidatorFactory.cs


## DbContext

There are several approaches to adding validation to a DbContext


### ValidatingDbContext

`ValidatingDbContext` provides a base class with validation already implemented in `SaveChnages` and `SaveChangesAsync`

snippet: SampleDbContext.cs


### DbContext as a base

In some scenarios it may not be possible to use a custom base class, I thise case `SaveChnages` and `SaveChangesAsync` can be overridden.

snippet: CustomDbContext


## Security contact information

To report a security vulnerability, use the [Tidelift security contact](https://tidelift.com/security). Tidelift will coordinate the fix and disclosure.


## Icon

[Database](https://thenounproject.com/term/database/310841/) designed by [Creative Stall](https://thenounproject.com/creativestall/) from [The Noun Project](https://thenounproject.com/creativepriyanka).