# C# .NET Clean Architecture Agent Instructions

## Core Principles
- Be extremely concise. Focus strictly on providing code. Skip theoretical explanations unless explicitly asked.
- Only provide code for the specifically requested layer. Do not generate the entire flow if the task is only to add a single Entity or feature component.
- Write clean code that adheres to SOLID principles.

## Architecture Layers (Jason Taylor Template)
1. **Domain Layer (`src/Domain`)**:
   - Must NOT contain any external dependencies.
   - Contains only Entities, Enums, Exceptions, and Value Objects.

2. **Application Layer (`src/Application`)**:
   - Implements the CQRS pattern using MediatR.
   - Contains Interfaces, Models/DTOs, and Validators (FluentValidation).
   - Must NOT reference the Infrastructure layer or have dependencies on external frameworks (except MediatR/FluentValidation).

3. **Infrastructure Layer (`src/Infrastructure`)**:
   - Contains Entity Framework Core DbContext, Configurations, and Migrations.
   - Implements interfaces defined in the Application layer (e.g., calling external APIs, file system access, authentication providers).

4. **Web/API Layer (`src/Web`)**:
   - Contains Minimal APIs or Controllers.
   - Its sole responsibility is to map incoming HTTP requests to MediatR Commands/Queries. 
   - Must NOT contain any business logic.

## Token Saving & Workflow Mode
- **Step-by-Step Execution:** When implementing a new feature, ALWAYS ask me which layer to work on first. Do not auto-generate code across all layers at once.
- **Diff-Only Output:** When refactoring or updating existing code, ONLY output the specific code blocks or methods that are modified. DO NOT reprint the entire file.