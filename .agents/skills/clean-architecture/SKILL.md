---
name: jason-taylor-clean-architecture
description: Use this skill for C# ASP.NET Core projects based on Jason Taylor's CleanArchitecture template. Use it when adding or modifying features, commands, queries, validators, endpoints, EF Core entities, migrations, tests, authentication, authorization, or Aspire configuration.
---

# Jason Taylor CleanArchitecture Skill

Use this skill when working in a C# ASP.NET Core solution generated from Jason Taylor's CleanArchitecture template.

The goal is to preserve the template's architecture, conventions, vertical-slice structure, MediatR request pipeline, validation, authorization, testing style, and dependency direction.

## First inspect the solution

Before changing code:

* Identify the solution name and actual project folders.
* Inspect `src/Domain`, `src/Application`, `src/Infrastructure`, `src/Web`, and `tests`.
* Follow existing naming, namespaces, folder structure, and endpoint style.
* Prefer existing patterns over introducing new abstractions.
* Do not add a repository layer unless the project already uses one.
* Do not put business logic in endpoints.
* Do not put HTTP concerns in Application or Domain.
* Do not make Application depend on Web or Infrastructure.

## Architecture rules

Follow the Clean Architecture dependency rule:

* `Domain` is the inner layer.
* `Application` depends on `Domain`.
* `Infrastructure` implements interfaces defined by `Application`.
* `Web` is the presentation layer and composes the app through dependency injection.

Do not break dependency direction.

## Domain layer rules

Use `src/Domain` for:

* Entities
* Value objects
* Domain events
* Enumerations
* Domain-level business rules and invariants

Rules:

* Keep Domain free of HTTP, database, EF configuration, UI, and infrastructure concerns.
* Add domain events when a business-relevant state change should trigger follow-up work.
* Put entity invariants near the entity when they are truly domain rules.
* Avoid anemic changes when the logic clearly belongs in the domain model.

## Application layer rules

Use `src/Application` for use cases.

Every feature should be organized as a vertical slice:

* `Application/<Feature>/Commands/<UseCase>/`
* `Application/<Feature>/Queries/<UseCase>/`

A command changes state.
A query reads state and must not mutate state.

Prefer this structure:

* `<UseCase>Command.cs` or `<UseCase>Query.cs`
* `<UseCase>CommandHandler.cs` or `<UseCase>QueryHandler.cs`
* `<UseCase>CommandValidator.cs` when validation is needed
* DTO/view model files only when needed by that use case

Rules:

* Use MediatR request/handler patterns already present in the project.
* Use `IApplicationDbContext` for database access from handlers.
* Use `CancellationToken` in async calls.
* Use `AsNoTracking()` for read-only EF Core queries.
* Use AutoMapper projection when the project already uses it.
* Keep handlers focused on one use case.
* Do not create large shared service classes for unrelated use cases.
* Put request validation in FluentValidation validators, not in endpoints.
* Use `[Authorize]` on commands/queries when authorization belongs to the use case.

## Scaffolding rule

When adding a new command or query and the `ca-usecase` template is available, prefer scaffolding from `src/Application`.

Example command:

```
cd src/Application
dotnet new ca-usecase --name CreateProduct --feature-name Products --usecase-type command --return-type int
```

Example query:

```
cd src/Application
dotnet new ca-usecase --name GetProduct --feature-name Products --usecase-type query --return-type ProductDto
```

After scaffolding:

* Implement the handler.
* Complete the validator.
* Add or update the endpoint in `src/Web/Endpoints`.
* Add tests.

If `ca-usecase` is unavailable, manually create the same folder and file structure used by the existing code.

## Infrastructure layer rules

Use `src/Infrastructure` for:

* EF Core `ApplicationDbContext`
* Entity type configurations
* Identity implementation
* External services
* File storage
* Email
* Third-party API clients
* Background infrastructure concerns

Rules:

* Implement interfaces that are defined in Application.
* Keep EF Core configuration in Infrastructure.
* Add `DbSet<T>` to `IApplicationDbContext` and `ApplicationDbContext` when a new aggregate/entity must be queried or persisted by use cases.
* Add `IEntityTypeConfiguration<T>` for entity mapping.
* Add migrations only when schema changes require them.
* Do not place infrastructure implementation details in Application.

## Web layer rules

Use `src/Web` for presentation.

Rules:

* Add Minimal API endpoints under `src/Web/Endpoints`.
* Endpoints should call `ISender.Send(...)`.
* Keep endpoints thin.
* Put validation in Application validators.
* Put authorization requirements in Application where the template pattern supports it, or follow existing endpoint authorization patterns if already used.
* Preserve existing OpenAPI/Scalar conventions.
* Do not put business rules in endpoint classes.

Typical endpoint flow:

* Receive route/body parameters.
* Create command or query.
* Send it through `ISender`.
* Return typed results consistent with existing endpoints.

## Pipeline behaviour rules

Remember that commands and queries pass through the existing MediatR pipeline.

Do not manually duplicate cross-cutting behaviour that is already handled by the pipeline, such as:

* Logging
* Unhandled exception handling
* Authorization
* Validation
* Performance monitoring

Use FluentValidation validators for validation so the pipeline can handle them consistently.

## Data access rules

* Use async EF Core methods.
* Pass `CancellationToken`.
* Use `AsNoTracking()` for queries.
* Avoid N+1 queries.
* Avoid leaking EF entities as API response models unless the project already does so.
* Prefer DTOs or view models for query responses.
* Do not expose internal domain objects unnecessarily.
* Do not modify connection strings or secrets unless explicitly asked.

## Testing rules

When changing behavior, add or update tests in the existing test projects.

Prefer the existing test style and helpers.

For a new use case, consider:

* Application-level command/query tests.
* Validator tests.
* Functional/API tests for endpoints.
* Authorization tests when access rules change.
* Persistence tests when EF mappings or migrations change.

Before finishing, run the most relevant commands available for this repo:

```
dotnet restore
dotnet build
dotnet test
```

If the project uses Aspire and app host orchestration, inspect the existing run/test commands before inventing new ones.

## Safe-change rules

* Make the smallest cohesive change.
* Keep public API contracts stable unless the task explicitly asks to change them.
* Do not introduce new NuGet packages unless necessary and justified.
* Do not rewrite architecture.
* Do not move files across layers unless needed to fix an architecture violation.
* Do not add generic repository/unit-of-work abstractions over EF Core unless the project already uses them.
* Do not bypass the MediatR pipeline from Web for normal use cases.
* Do not hard-code environment-specific configuration.

## Completion checklist

Before the final response:

* Confirm which layer(s) changed.
* Confirm which command/query/endpoint/entity/test files changed.
* Run build and tests when possible.
* Report any command that failed and why.
* Mention any migration, API contract, authorization, or database-impacting change.
* Mention any risk or follow-up work.

Final response format:

* Summary
* Files changed
* Tests run
* Notes or risks
