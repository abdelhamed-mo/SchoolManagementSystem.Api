.github/copilot-instructions.md
# GitHub Copilot instructions — School Management System (concise)

Follow these strict rules when suggesting code or new files:

1. Preserve the implemented architecture
   - Keep layers: `Core/Domain`, `Core/ServiceAbstraction`, `Core/Service`, `Infrastructure/*`, `Shared/Shared`, `SchoolSystem.Api`.
   - Reuse patterns: GenericRepository + UnitOfWork + Specifications; service interfaces in `Core/ServiceAbstraction`, implementations in `Core/Service`.

2. Controllers (confirmed)
   - Verified controller locations:
     - `Infrastructure/Presentation/StudentController.cs`
     - `Infrastructure/Presentation/ParentsController.cs`
     - `Infrastructure/Presentation/AdminController.cs`
     - `Infrastructure/Presentation/AuthenticationController.cs`
     - `Infrastructure/Presentation/ApiBaseController.cs`
   - Route base: `ApiBaseController` uses `[ApiController]` and `[Route("api/[controller]")]`.
   - Use `IServiceManager` and service layer methods (e.g., `serviceManager.StudentService`, `serviceManager.ParentService`, `serviceManager.AdminService`, `serviceManager.AuthenticationService`).

3. AutoMapper (verified)
   - Profiles exist at:
     - `Core/Service/MappingProfiles/AdminProfile.cs`
     - `Core/Service/MappingProfiles/StudentProfiles.cs`
   - Before generating mapping code:
     - Inspect these profiles for required CreateMap directions and potential gaps (collection→scalar mappings).
     - If creating new profiles, place them in `Core/Service/MappingProfiles/` and register via the Service assembly (host registration already uses `Service.AssemblyReference`).

4. Validation & errors
   - Use DataAnnotations on request DTOs in `Shared/Shared/*`.
   - For business-rule violations throw `Core/Domain/Exceptions/ValidationException`.
   - Use `NotFoundException` for missing resources and rely on centralized middleware for HTTP mapping.

5. Authentication & Authorization
   - Token generation exists (`Core/Service/AuthenticationService.cs`) but confirm intended JWT middleware in `SchoolSystem.Api/Program.cs` before emitting `AddAuthentication`/`UseAuthentication` code.
   - Use `[Authorize]` and role-based `[Authorize(Roles = "Admin")]` as seen in `ParentsController`.

6. No new libraries without approval
   - Do not add MediatR/CQRS, FluentValidation, or other third-party frameworks without explicit maintainer approval.

7. Mandatory workflow (generate a plan before code)
   - Identify an existing feature to copy pattern from (list exact files).
   - Draft a minimal plan (files to add/change, mapping checks, tests).
   - Ask for approval.
   - Implement only the approved scope, build, test, and document.

8. Safety-first completions
   - If required pieces are missing or ambiguous (mappings, controller discovery, JWT config), generate a short verification task listing files to inspect rather than code that assumes missing pieces.

References (examples to inspect before coding)
- Student flow: `Core/Service/student/StudentService.cs`, `Core/Service/ConcreteSpecifications/StudentSpecifications.cs`, `Core/Service/MappingProfiles/StudentProfiles.cs`, `Infrastructure/Presentation/StudentController.cs`.
- Admin flow: `Core/Service/AdminService/*`, `Core/Service/MappingProfiles/AdminProfile.cs`, `Infrastructure/Presentation/AdminController.cs`.

When in doubt, ask for confirmation.