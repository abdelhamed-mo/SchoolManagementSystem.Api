.github/copilot-instructions.md
# GitHub Copilot instructions — School Management System

Follow these instructions for all code suggestions, new files, and modifications.

## Project Coding Standards

### Architecture and project structure

- Preserve the existing architecture and project boundaries:
  `Core/Domain`, `Core/ServiceAbstraction`, `Core/Service`, `Infrastructure/*`,
  `Shared/Shared`, and `SchoolSystem.Api`.
- Follow the established flow:
  `Controller -> IServiceManager -> service abstraction -> concrete service -> IUnitOfWork -> generic repository -> specifications -> MainContext`.
- Reuse the GenericRepository, UnitOfWork, specification, AutoMapper, and centralized exception-handling patterns.
- Do not introduce CQRS, MediatR, FluentValidation, a new service-resolution pattern, or third-party packages without explicit approval.
- Do not refactor unrelated code, rename existing public types or members, or change authentication or authorization configuration unless explicitly requested.

### Controller standards

- All controllers must inherit from `ApiBaseController` and follow its routing conventions. `ApiBaseController` already supplies `[ApiController]` and `[Route("api/[controller]")]`.
- Resolve application behavior through `IServiceManager` and the appropriate service abstraction.
- Keep controllers thin. Business logic, validation rules, database access, and entity existence checks belong in services.
- Do not add redundant manual `ModelState` validation checks. `[ApiController]` handles request-model validation; business validation remains in the service layer.
- Avoid unnecessary local variables, conditions, response branching, and repetitive code.
- Follow the API response standards below consistently.

### Exception standards

- Use the existing custom exception hierarchy and the centralized exception middleware in `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs`.
- Entity-specific not-found exceptions must inherit from `Domain.Exceptions.NotFoundException`.
- Each entity-specific not-found exception must accept only the entity ID in its constructor and follow the established message convention, for example `ExamNotFoundException(int id)`.
- Before implementing a feature, identify the required domain exceptions. If an appropriate exception is missing, create it in `Core/Domain/Exceptions/NotFoundExceptions/` or the established exception directory using the existing naming, constructor, and message conventions.
- Do not create duplicate or redundant exceptions. Reuse an existing exception when it correctly represents the situation.
- Use `Domain.Exceptions.ValidationException` for expected business-rule violations.
- Never use a generic exception for an expected business error when an appropriate custom exception is available.
- Never return `null` or an empty success response when an individual entity is expected but does not exist. Throw the corresponding entity-specific not-found exception instead.
- Do not throw a not-found exception for an empty collection; return the empty collection or empty paginated result.

### API response standards

- Never use `NoContent()` or return HTTP 204 responses.
- GET collection endpoints must return an empty collection or empty paginated result when no records match.
- GET-by-ID endpoints must return the requested resource or throw its corresponding entity-specific not-found exception.
- POST endpoints must return HTTP 201 Created with the created resource and its location, normally using `CreatedAtAction`.
- PUT endpoints must return HTTP 200 OK with the updated resource when appropriate. The service should return the updated DTO when the API contract requires it.
- DELETE endpoints must return HTTP 200 OK with a meaningful success response.
- Keep response formatting in controllers limited to the established API contract; do not move business decisions into controllers.

### Service standards

- Services own business logic, business validation, and entity existence checks.
- Use asynchronous methods consistently and follow existing cancellation-token conventions when a surrounding API already supports them.
- Use `IUnitOfWork`, generic repositories, and specifications for data access. Do not bypass these abstractions with direct `DbContext` access from services.
- Apply filtering before pagination. Use the same filtering specification for count queries and returned-page queries, with pagination disabled for the count.
- Avoid unnecessary database queries and reuse entities already retrieved during the operation.
- Use the correct entity-specific not-found exception for every missing required entity.
- Keep methods focused and avoid duplicated validation or data-access logic.
- Do not unintentionally overwrite navigation properties or collections during updates.

### DTOs, mappings, and dependency injection

- Inspect existing DTOs, parameter objects, and mapping profiles before adding new contracts.
- Place shared request/response DTOs and query parameters under `Shared/Shared` using the existing naming and namespace conventions.
- Place AutoMapper profiles under `Core/Service/MappingProfiles/`. The host currently scans the Service assembly through `Service.AssemblyReference`; do not add duplicate map definitions or unnecessary `ReverseMap` configurations.
- Avoid unsafe collection-to-scalar mappings and map related data explicitly when required by the response contract.
- Expose new services through `IServiceManager` and resolve them using the existing `ServiceManger` lazy-service pattern unless the architecture is explicitly changed.
- Register dependencies through the existing DI configuration in `SchoolSystem.Api/Program.cs`; do not introduce a parallel registration or resolution mechanism.

### General code quality

- Follow the actual formatting, naming, namespace, accessibility, and asynchronous style of the surrounding project.
- Prefer simple, readable, maintainable implementations over unnecessary abstractions.
- Avoid redundant variables, unnecessary conditions, duplicated logic, and speculative extensibility.
- Preserve existing authentication and authorization behavior. Authorization changes are out of scope unless explicitly requested.
- Do not modify database relationships, create migrations, or change persistence behavior without an identified requirement and approval.

### Autonomous feature-development workflow

For every future feature or cross-cutting modification:

1. Inspect the complete relevant project structure and representative implementations.
2. Identify the exact reusable controllers, services, interfaces, specifications, repositories, DTOs, mappings, exceptions, and registrations.
3. Identify missing components and prepare a concise implementation plan before editing when the work is multi-file or uncertain.
4. Create all required components using the established architecture and standards.
5. Ensure service-manager, AutoMapper, and dependency-injection wiring is complete.
6. Review the complete implementation for consistency, response behavior, exception behavior, and unintended relationship changes.
7. Build the solution.
8. Run applicable tests when available.
9. Fix errors caused by the implementation and report warnings separately.
10. Report any unresolved issue that requires a business decision; do not silently apply a data-loss or architecture-changing workaround.

### Continuous standards enforcement

- These standards apply to every future feature and code modification, not only to the task that introduced this section.
- Automatically inspect newly written code for violations of these standards and correct violations before considering the work complete.
- Do not ask the user to perform simple corrections that can be made directly in the workspace.
- If an existing implementation conflicts with these standards, preserve unrelated legacy code unless the user requests remediation, but ensure all newly written or modified code follows these standards.

## Existing project references

Inspect these files before implementing similar functionality:

- Controller flow: `Infrastructure/Presentation/ApiBaseController.cs`, `Infrastructure/Presentation/StudentController.cs`, `Infrastructure/Presentation/ParentsController.cs`, `Infrastructure/Presentation/AdminController.cs`, and `Infrastructure/Presentation/AuthenticationController.cs`.
- Student flow: `Core/Service/student/StudentService.cs`, `Core/Service/ConcreteSpecifications/StudentSpecifications.cs`, and `Infrastructure/Presentation/StudentController.cs`.
- Parent flow: `Core/Service/ParentService/ParentService.cs` and `Infrastructure/Presentation/ParentsController.cs`.
- Service resolution: `Core/Service/ServiceManager.cs`, `Core/ServiceAbstraction/IServiceManager.cs`, and `SchoolSystem.Api/Program.cs`.
- Exceptions: `Core/Domain/Exceptions/NotFoundException.cs`, `Core/Domain/Exceptions/NotFoundExceptions/`, `Core/Domain/Exceptions/ValidationException.cs`, and `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs`.
- Data access: `Core/Domain/Contracts/IGenericRepository.cs`, `Core/Domain/Contracts/Specifications.cs`, `Infrastructure/Persistence/Contracts/GenericRepository.cs`, and `Infrastructure/Persistence/Contracts/SpecificationEvaluator.cs`.
- Mapping: `Core/Service/MappingProfiles/` and AutoMapper registration in `SchoolSystem.Api/Program.cs`.

## Known legacy conventions that conflict with these standards

These are observed in existing source and must not be copied into new or modified code:

- `Infrastructure/Presentation/StudentController.cs`, `ParentsController.cs`, and the current `ExamsController.cs` contain manual `ModelState` checks even though `ApiBaseController` uses `[ApiController]`.
- Existing Student, Parent, and Exam update/delete actions return `NoContent()`/HTTP 204, while these standards require HTTP 200 responses with an appropriate result.
- Some legacy controllers use direct response branching or variables instead of delegating directly to services.
- `Core/Service/ExamService/ExamService.cs` currently uses generic `NotFoundException` for exams and related entities; new work must use the appropriate entity-specific exception, creating it if needed.
- `SchoolSystem.Api/Program.cs` registers `ExceptionHandlerMiddleware` after `MapControllers`; the middleware remains the centralized handler, but pipeline ordering should be reviewed before changing middleware behavior.
- `ParentsController` and the original instructions include authorization attributes, while authentication and authorization configuration remain unchanged and authorization is only added when explicitly required.
