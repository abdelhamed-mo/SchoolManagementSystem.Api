AGENTS.md
# Agents guidance — School Management System

Purpose
- Brief: instruct agents/contributors how to add or change features while preserving the implemented architecture and conventions.

Confirmed conventions (must preserve)
- Layered projects:
  - `Core/Domain` — entities, exceptions, repository & specification contracts (e.g., `Core/Domain/Entities/*`, `Core/Domain/Exceptions/*`).
  - `Core/ServiceAbstraction` — service interfaces (`Core/ServiceAbstraction/*`).
  - `Core/Service` — concrete services and AutoMapper profiles (`Core/Service/*`, `Core/Service/MappingProfiles/*`).
  - `Infrastructure/Persistence` — EF Core `DbContext`, GenericRepository, UnitOfWork, SpecificationEvaluator, migrations, seeding (`Infrastructure/Persistence/*`).
  - `Infrastructure/Presentation` — controllers discovered via application part (`Infrastructure/Presentation/*`).
  - `Shared/Shared` — DTOs, params, shared helpers (`Shared/Shared/*`).
  - Host: `SchoolSystem.Api` — program, middleware (`SchoolSystem.Api/Program.cs`, `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs`).
- Data access pattern (do not change):
  - GenericRepository + UnitOfWork + Specification pattern:
    - `Core/Domain/Contracts/IGenericRepository.cs`
    - `Core/Domain/Contracts/Specifications.cs`
    - `Infrastructure/Persistence/Contracts/GenericRepository.cs`
    - `Infrastructure/Persistence/Contracts/UnitOfWork.cs`
    - `Infrastructure/Persistence/Contracts/SpecificationEvaluator.cs`
  - Services obtain repositories via `unitOfWork.GetRepository<TEntity, TKey>()` and call `unitOfWork.SaveChangesAsync()`.

Controllers (verified paths and conventions)
- Controllers use `ApiBaseController` route base `api/[controller]`:
  - `Infrastructure/Presentation/ApiBaseController.cs`
- Verified controllers and patterns:
  - `Infrastructure/Presentation/AdminController.cs`
    - Uses `IServiceManager` via primary-constructor syntax; action routes like `[HttpGet("parents")]`.
    - Returns service results (Task<ActionResult<...>>).
  - `Infrastructure/Presentation/AuthenticationController.cs`
    - Exposes `Register` and `Login` that call `serviceManager.AuthenticationService`.
  - `Infrastructure/Presentation/ParentsController.cs`
    - Decorated with `[Authorize]` at class level; uses `[Authorize(Roles = "Admin")]` for admin-only endpoints.
    - Uses `CreatedAtAction`, `NoContent`, and standard REST status patterns.
  - `Infrastructure/Presentation/StudentController.cs`
    - Uses primary constructor; checks `ModelState` on Create and returns `CreatedAtAction` or `NoContent`.
- Routing & model validation:
  - `[ApiController]` on `ApiBaseController` enables automatic model validation (400 responses) — explicit ModelState checks are redundant but allowed.
- Dependency injection:
  - Controllers consume `IServiceManager` (and via it call `AdminService`, `ParentService`, `StudentService`, `AuthenticationService`).

AutoMapper (verified profiles)
- Profiles present:
  - `Core/Service/MappingProfiles/AdminProfile.cs`
  - `Core/Service/MappingProfiles/StudentProfiles.cs`
- AutoMapper registered in host:
  - `SchoolSystem.Api/Program.cs` — `builder.Services.AddAutoMapper(..., typeof(Service.AssemblyReference).Assembly);`
- Mapping notes (verify before relying):
  - Profiles map navigation properties to DTO collections (e.g., `Teacher.TeacherSubjects.Select(...)` → `TeacherDto.Subjects`).
  - Some mappings convert collections to scalar DTO properties (e.g., mapping `Class.ClassSubjects.Select(... )` to `AttendanceDto.SubjectName` or `EnrollmentDto.SubjectId`), which may result in type mismatches or unintended behavior. Verify each mapping source/destination types and run tests.
  - Some maps use `ReverseMap()`; others do not — check directionality requirements before relying on ReverseMap.

DTOs (location & conventions)
- All DTOs in `Shared/Shared/*`:
  - Examples: `Shared/Shared/StudentDto.cs`, `Shared/Shared/CreateStudentDto.cs`, `Shared/Shared/ParentDto.cs`, `Shared/Shared/PaginatedResultDto.cs`, `Shared/Shared/UserRegisterDTO.cs`
- Conventions:
  - Use `*Dto` / `*DTO` suffix for DTOs.
  - Use DataAnnotations on request DTOs for model validation.

Exception handling
- Centralized exception middleware:
  - `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs` maps domain exceptions to HTTP codes and returns `Shared/ErrorModels/ErrorDto`.
  - Services throw domain exceptions (e.g., `Core/Domain/Exceptions/ValidationException`, `NotFoundException`, `UnAuthorizedException`).

Unresolved verification items (must confirm before change)
- JWT pipeline activation:
  - Token creation exists in `Core/Service/AuthenticationService.cs`, but `Program.cs` does not show `AddAuthentication().AddJwtBearer(...)` or `app.UseAuthentication()`. Confirm intended JWT setup before adding/assuming protected endpoints behavior.
- Exception middleware ordering:
  - `Program.cs` currently invokes `app.UseMiddleware<ExceptionHandlerMiddleware>()` after `MapControllers()`. Confirm intended ordering and run tests before reordering pipeline.
- Mapping correctness:
  - Verify mappings that map collections to scalar DTO properties (see `Core/Service/MappingProfiles/*`). Fix or clarify mapping intent only after approval.
- Controller locations and discovery:
  - Controllers were found in `Infrastructure/Presentation/*`. If adding controllers, confirm presentation assembly is the intended location and that `Program.cs` application-part registration covers it.

Hard rules (do not violate)
- Preserve Service Layer, Generic Repository, UnitOfWork, Specification Pattern, DTO conventions, and centralized exception handling.
- Do not introduce CQRS/MediatR, FluentValidation, or new third-party libraries without explicit maintainer approval.
- Do not rename public types or refactor cross-cutting code without maintainers' approval.

Mandatory change workflow (must be followed)
1. Inspect: locate an existing similar feature and list exact file paths to use as references (service, specification, mapping, controller).
   - Example references: `Core/Service/student/StudentService.cs`, `Core/Service/ConcreteSpecifications/StudentSpecifications.cs`, `Core/Service/MappingProfiles/StudentProfiles.cs`, `Infrastructure/Presentation/StudentController.cs`.
2. Propose: short plan listing files to add/change (explicit paths), mapping changes (verify current mapping profile contents), required DI updates, and tests to add.
3. Obtain explicit approval from a maintainer.
4. Implement only the approved scope.
5. Build & test (full solution build and any existing tests).
6. Summarize changes in PR description and changelog, including where mappings/controllers/authentication/exception handling were verified.
7. Push: small, focused commits with descriptive messages.

Where to look first (quick reference)
- DTOs: `Shared/Shared/`
- Student reference: `Core/Service/student/StudentService.cs`, `Core/Service/ConcreteSpecifications/StudentSpecifications.cs`, `Core/Service/MappingProfiles/StudentProfiles.cs`
- Admin reference: `Core/Service/AdminService/*`, `Core/Service/MappingProfiles/AdminProfile.cs`
- Repos/specifications: `Infrastructure/Persistence/Contracts/*`
- Host & middleware: `SchoolSystem.Api/Program.cs`, `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs`

If uncertain, stop and ask; do not guess.