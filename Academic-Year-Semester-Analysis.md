# Academic Year & Semester Management — Feature Analysis

**Analysis status:** Existing implementation review only. No source code, configuration, migration, seed data, or database was changed.

**Target framework:** `net10.0`, verified in `SchoolSystem.Api/SchoolSystem.Api.csproj:7`.

## 1. Executive Summary

The repository does not contain `AcademicYear` or `Semester` domain entities, `DbSet`s, services, specifications, DTOs, controllers, mappings, exceptions, migrations, or seed files for those concepts. The only explicit academic-year concept is `Class.AcademicYear`, a required free-form `string`, exposed by `Shared.ClassDto` and populated in every inspected class seed record as `"2024/2025"`.

The current model therefore supports an academic-year label on a class, but it does not support authoritative year periods, active/current state, semester periods, overlap validation, year/semester foreign keys, or reliable historical assignment of exams, attendance, results, students, and enrollments. Existing records can sometimes be associated indirectly through `ClassId` or `ExamId`, but that is not equivalent to a formal academic-year relationship and does not provide semester identity.

The recommended direction is to introduce `AcademicYear` and `Semester` as first-class entities, with `Semester` owned by an academic year. The preferred source of truth for class placement and historical academic context is an explicit `Class.AcademicYearId`; student enrollment should be modeled per year through an actual year-aware enrollment record rather than through `Student.EnrollmentDate`. Exams should reference a semester directly (and derive the year through the semester), while attendance and grade results can derive year and semester through their existing class/exam paths unless reporting or validation requirements require direct foreign keys.

Several choices are not confirmed by the current code and require product approval: whether exactly one year and one semester may be active, whether semesters are always two, whether periods may overlap, what “current” means, and whether old records should be backfilled. These are identified as recommendations or open decisions, not existing rules.

## 2. Existing Project Architecture and Relevant Components

The architecture and the principal implementation locations are confirmed by `SchoolSystem.Api/AGENTS.md:7-15`:

- **Domain:** `Core/Domain` contains entities, domain exceptions, repository contracts, and specification contracts.
- **Service abstraction:** `Core/ServiceAbstraction` contains service interfaces and `IServiceManager`.
- **Service:** `Core/Service` contains concrete services, specifications, and AutoMapper profiles.
- **Persistence:** `Infrastructure/Persistence` contains `MainContext`, repository and UnitOfWork implementations, EF migrations, and JSON seeding.
- **Presentation:** `Infrastructure/Presentation` contains controllers discovered through the application part configured in `SchoolSystem.Api/Program.cs:24`.
- **Shared contracts:** `Shared/Shared` contains DTOs, request DTOs, query parameter classes, and paginated result types.
- **Host:** `SchoolSystem.Api` contains startup and exception middleware.

Data access uses the existing generic repository/specification/UnitOfWork design. `Core/Domain/Contracts/IGenericRepository.cs:5-16` exposes asynchronous add, get, list, count, update, and delete operations, including specification overloads. `Core/Domain/Contracts/Specifications.cs:5-25` supports criteria, includes, and database-side pagination. `Core/Domain/Contracts/IUnitOfWork.cs:5-10` exposes repositories and `SaveChangesAsync`. `Infrastructure/Persistence/Contracts/GenericRepository.cs:10-41` applies specifications to EF `IQueryable`; `Infrastructure/Persistence/Contracts/UnitOfWork.cs:8-25` caches repositories and saves through `MainContext`.

Analogous features inspected:

- **Grade:** `Core/ServiceAbstraction/IGradeService.cs`, `Core/Service/GradeService/GradeService.cs`, `Core/Service/ConcreteSpecifications/GradeSpecifications.cs`, `Core/Service/MappingProfiles/GradeProfile.cs`, and `Infrastructure/Presentation/GradesController.cs`.
- **Exam:** `Core/ServiceAbstraction/IExamService.cs:9-16`, `Core/Service/ExamService/ExamService.cs:13-153`, `Core/Service/ConcreteSpecifications/ExamSpecifications.cs:8-40`, `Core/Service/MappingProfiles/ExamProfile.cs`, `Shared/ExamDto.cs`, `Shared/CreateExamDto.cs`, `Shared/UpdateExamDto.cs`, `Shared/Params/ExamsParams.cs`, and `Infrastructure/Presentation/ExamsController.cs:8-29`.
- **GradeResult:** `Core/ServiceAbstraction/IGradeResultService.cs`, `Core/Service/GradeResultService/GradeResultService.cs`, `Core/Service/ConcreteSpecifications/GradeResultSpecifications.cs:6-43`, `Core/Service/MappingProfiles/GradeResultProfile.cs`, and `Infrastructure/Presentation/GradeResultsController.cs`.
- **Attendance:** `Core/ServiceAbstraction/IAttendanceService.cs`, `Core/Service/AttendanceService/AttendanceService.cs:8-158`, `Core/Service/ConcreteSpecifications/AttendanceSpecifications.cs:6-56`, `Core/Service/MappingProfiles/AttendanceProfile.cs`, and `Infrastructure/Presentation/AttendancesController.cs:8-35`.
- **Student:** `Core/Domain/Entities/Student.cs`, `Core/Service/student/StudentService.cs`, `Core/Service/ConcreteSpecifications/StudentSpecifications.cs:8-33`, `Core/Service/MappingProfiles/StudentProfiles.cs`, `Shared/StudentDto.cs`, and `Infrastructure/Presentation/StudentController.cs`.
- **Class:** `Core/Domain/Entities/Class.cs`, `Shared/ClassDto.cs`, `Core/Service/ConcreteSpecifications/ClassSpecifications.cs`, and the class configuration in `Infrastructure/Persistence/MainContext.cs:77-81`.

`SchoolSystem.Api/Program.cs:18-27` registers AutoMapper, `IDbInitializer`, `IUnitOfWork`, `IServiceManager`, EF SQL Server, and Identity. `Service.ServiceManger` in `Core/Service/ServiceManager.cs:14-44` constructs the existing services lazily. No academic-year or semester service is registered or exposed.

Controllers generally derive from `Presentation.ApiBaseController`, whose route is `api/[controller]` (`Infrastructure/Presentation/ApiBaseController.cs:9-13`), inject `IServiceManager`, and delegate to services. The current Exam and Attendance controllers use explicit `Task<ActionResult<T>>` return types and expression-bodied delegation (`Infrastructure/Presentation/ExamsController.cs:10-28`, `AttendancesController.cs:10-33`). The Student controller is an older inconsistent implementation using `IActionResult`, manual `ModelState`, `CreatedAtAction`, and `NoContent` (`Infrastructure/Presentation/StudentController.cs:23-53`); it should not be used as the convention for the proposed feature.

## 3. Current Domain Model

### Entities and relationships

- `Core/Domain/Entities/Class.cs:7-20` defines `Class : BaseEntity<int>` with `Name`, `AcademicYear` (`string`), `GradeId`, `Grade`, and collections for students, class subjects, class teachers, attendance, and exams.
- `Core/Domain/Entities/Student.cs:8-29` defines `Student : BaseEntity<int>` with personal data, `EnrollmentDate`, `GradeId`, `ClassId`, and navigations to `Grade`, `Class`, `Parent`, `User`, `Attendance`, and `GradeResults`.
- `Core/Domain/Entities/Enrollment.cs:8-15` has only `StudentId`, `ClassId`, and navigations to `Student` and `Class`. It has no academic-year ID, semester ID, enrollment date, status, or service/controller use discovered in the project.
- `Core/Domain/Entities/Exam.cs:7-24` has `ExamDate`, subject, class, grade, and grade-result relationships. It has no academic-year or semester foreign key.
- `Core/Domain/Entities/Attendance.cs:7-17` references `Student` and `Class`, with `Date` and `Status`; it has no year or semester foreign key.
- `Core/Domain/Entities/GradeResult.cs:7-19` references `Exam`, `Student`, and `Grade`, with `Score`; it has no year or semester foreign key.
- `Core/Domain/Entities/Grade.cs:7-14` is a grade-level lookup with collections for students, classes, exams, and results; it is not an academic year.

`Infrastructure/Persistence/MainContext.cs:13-28` exposes `DbSet`s for these entities, including `Enrollments`, but no `AcademicYears` or `Semesters`. `OnModelCreating` configures grade relationships, student/class, join entities, and Identity relationships (`MainContext.cs:35-126`).

### Current historical support

The model cannot reliably distinguish current and previous academic years. `Class.AcademicYear` is a label, not a foreign key or date period. `Student.EnrollmentDate` is a single date on the student record, not a year-specific enrollment. Exams and attendance have dates but no formal period association, and grade results have no date of their own. There is no current/active flag, unique constraint, year navigation, or semester navigation.

The current model can preserve historical class labels if classes are never overwritten, but it does not establish a normalized academic-year record or prevent reuse of the same label with conflicting dates. It also cannot represent a student changing class between years without either creating another student record or using the currently under-specified `Enrollment` entity.

## 4. Existing Academic Year and Semester Support

### Academic year

The only verified implementation is:

- `Class.AcademicYear` in `Core/Domain/Entities/Class.cs:9-12`.
- `ClassDto.AcademicYear` in `Shared/Shared/ClassDto.cs:7-18`.
- `AcademicYear` as a required `nvarchar(max)` column in the migration/model snapshot, for example `Infrastructure/Persistence/Migrations/20251221173135_DB-init.cs:71-84` and `Infrastructure/Persistence/Migrations/MainContextModelSnapshot.cs:78-102`.
- `Classes.json:3-120`, where all 24 class seed records use the label `2024/2025`.

No academic-year management behavior was found: there is no CRUD service, current-year query, date range, active flag, overlap validation, dedicated exception, or API endpoint.

### Semester

No `Semester` type or equivalent term/period entity was found in the inspected source, project file lists, migration snapshot, or seed files. No semester property appears on the inspected exam, attendance, result, class, student, or enrollment entities. No semester-specific API or filtering contract exists.

### Existing behavior versus needed behavior

The existing class label may be used as transitional data when introducing a normalized year table, but it is not sufficient to implement the requested feature. A direct migration from the string to a foreign key requires an explicit backfill policy; source data only demonstrates the label `2024/2025`, not authoritative start/end dates or historical assignments for every related record.

## 5. Identified Gaps

1. No `AcademicYear` entity, table, service, DTO, controller, mapping, specification, exception, or seed data exists.
2. No `Semester` entity or table exists, and no parent-child year relationship exists.
3. `Class.AcademicYear` is a required unbounded string rather than a foreign key to a defined period (`Core/Domain/Entities/Class.cs:9-12`; snapshot `:86-88`).
4. There are no start or end dates, active/current flags, display labels with normalized semantics, or overlap constraints.
5. Classes, enrollments, exams, attendance, and grade results cannot be consistently filtered by year or semester at database level.
6. `Enrollment` does not model an academic year and is not used by the initializer or a dedicated service (`Core/Domain/Entities/Enrollment.cs`; `Infrastructure/Persistence/Contracts/DbInitializer.cs:56-65`).
7. Students have one current `ClassId` and one `EnrollmentDate`, which is insufficient for year-to-year history (`Student.cs:13-25`).
8. Exams can be date-filtered but cannot identify a semester; grade results derive exam context but have no period contract.
9. Attendance can be date-filtered, but the code does not enforce that its date belongs to a formal academic year or semester.
10. Database delete behavior is not a suitable lifecycle policy for new year records until deletion rules are decided. The snapshot currently shows cascade relationships for attendance, exams, grade results, and enrollments in several paths (`MainContextModelSnapshot.cs:591-676`, `:678-735`), while `MainContext.cs` explicitly configures some relationships as restrict and does not configure all of those paths. This discrepancy must be resolved through the normal model/migration process, not assumed away.
11. Existing API conventions are mixed. New work should follow the documented project instructions and newer Exam/Attendance patterns, not reproduce the legacy Student controller behavior.

## 6. Proposed Domain Model

The following is a recommendation, not an existing implementation.

### `AcademicYear`

Suggested entity: `Core/Domain/Entities/AcademicYear.cs`, inheriting `BaseEntity<int>`.

Suggested properties:

- `int Id` from `BaseEntity<int>`.
- `string Name` or `Label`, required and bounded; example `2024/2025`. The exact naming should be approved because `Class` currently uses `AcademicYear` as a string.
- `DateTime StartDate`, required.
- `DateTime EndDate`, required.
- `bool IsActive` or `IsCurrent`, required if the product wants stored activation state. A single authoritative meaning must be selected; a calculated “current by today's date” rule is an alternative.
- `ICollection<Semester> Semesters`.
- `ICollection<Class> Classes` if classes receive a direct `AcademicYearId`.
- Potentially `ICollection<Enrollment> Enrollments` if enrollment is made year-aware.

Recommended constraints: `StartDate < EndDate`; normalized label uniqueness if labels are business identifiers; a non-overlap rule for periods; an index on `(StartDate, EndDate)`; and a filtered/controlled uniqueness rule for one active year only if that rule is approved and supported consistently by SQL Server and the service layer.

### `Semester`

Suggested entity: `Core/Domain/Entities/Semester.cs`, inheriting `BaseEntity<int>`.

Suggested properties:

- `int Id`.
- `int AcademicYearId`, required.
- `AcademicYear AcademicYear`, required navigation.
- `string Name` or `Code`, required, such as `First`/`Second` or `Fall`/`Spring`.
- `DateTime StartDate`, required.
- `DateTime EndDate`, required.
- `bool IsActive`/`IsCurrent` only if activation is a stored business state rather than a calculated date result.
- Optional `int DisplayOrder` if the number of terms is configurable.
- Optional `ICollection<Exam> Exams` if exams receive `SemesterId`.

Recommended constraints: `StartDate < EndDate`; `AcademicYearId` foreign key; no semester period outside the parent year; no overlapping semesters within a year; and uniqueness of `(AcademicYearId, Name)` or `(AcademicYearId, DisplayOrder)` according to the approved naming model. Do not enforce exactly two semesters unless that decision is confirmed.

### Existing entity changes that may be required

- `Class`: replace or transition `AcademicYear` string to `AcademicYearId` plus `AcademicYear` navigation. Retain the old label only temporarily during migration if needed.
- `Exam`: add `SemesterId` if semester is an explicit exam concept. The academic year can then be derived through `Semester.AcademicYearId`; alternatively add `AcademicYearId` as a denormalized/reporting key only with a consistency rule.
- `Enrollment`: add `AcademicYearId`; consider `EnrollmentDate`, status, and a uniqueness rule such as `(StudentId, AcademicYearId)` only if one enrollment per student/year is intended. Its current name and use should be reviewed before relying on it.
- `Student`: do not use the single `EnrollmentDate` as a substitute for historical enrollment. A direct `AcademicYearId` on Student would represent only the current year and should not replace year-aware enrollment history.
- `Attendance`: prefer deriving year through the class/year relationship and validating `Date` against the class's year. Add direct `SemesterId` only if attendance reporting requires an immutable period classification or classes can span terms in a way that cannot be derived from date.
- `GradeResult`: derive year/semester through `Exam` where possible. A direct period key is optional and creates a consistency obligation.

## 7. Business Rules and Open Decisions

The current code confirms none of the following as academic-year business rules. Each recommendation requires approval.

| Rule | Why it matters | Current support | Recommendation / decision status |
|---|---|---|---|
| Only one academic year active/current | Prevents ambiguous defaults and conflicting operational context | No active/current property or query exists | Recommended if the application has one global operating year; must be approved. A database/service invariant is needed. |
| Only one semester current | Prevents ambiguous semester-dependent operations | No semester exists | Recommended within the active year if “current” is global; must be approved. |
| Academic-year periods cannot overlap | Makes date-based current lookup deterministic and prevents double classification | No dates or validation exist | Strong recommendation. Validate in the service and protect with an appropriate database strategy where possible. |
| Semester periods cannot overlap within a year | Prevents a date mapping to multiple semesters | No semester support exists | Strong recommendation. Decide whether adjacent boundaries are allowed and whether end dates are inclusive. |
| Semester must be entirely inside parent year | Preserves referential temporal integrity | No parent or date ranges exist | Strong recommendation; validate on create/update and when parent dates change. |
| Exactly two semesters or configurable count | Determines schema/API rules and future flexibility | No requirement is encoded | Keep a configurable number by default. Confirm whether the institution requires exactly two. |
| Delete an academic year with related records | Deleting a year could destroy history or orphan classes/results | No year relationships; existing relationships include cascade paths in the snapshot | Recommend preventing deletion when related classes, enrollments, exams, attendance, or results exist. Prefer an explicit archive/deactivate lifecycle, but soft delete is out of scope. |
| Delete a semester with related exams/results | Prevents loss of assessment history | No semester exists | Recommend blocking deletion when dependent records exist. A semester can be renamed/closed instead. |
| What happens when a year ends | Controls current state and historical reporting | No year state transition exists | Recommend closing/deactivating the year and creating a new year; do not rewrite historical records. Approval required. |
| Historical records when moving to a new year | Determines whether records remain reportable | Current class/student pointers do not preserve full history | Preserve prior year records and create new class/enrollment associations. Do not overwrite historical period keys. |
| Class and enrollment year association | Determines whether class membership is historical | Class has only a string; Enrollment has no year | Recommend explicit `Class.AcademicYearId` and year-aware enrollment. Confirm whether classes are recreated per year. |
| Date/time boundary and timezone | Affects current/overlap calculations | Existing fields are `DateTime` with no timezone policy | Approve date-only or normalized UTC policy. Academic periods are likely date-based; implementation must define inclusive/exclusive boundaries. |

## 8. Relationships and Data Integrity

### AcademicYear ↔ Semester

This should be a required one-to-many relationship: one academic year contains many semesters; every semester has exactly one `AcademicYearId`. The foreign key should use restrict/no-action semantics for deleting a parent with children, subject to the selected lifecycle policy. Semester dates must be checked against the parent year in service validation because a simple foreign key cannot enforce temporal containment.

### AcademicYear ↔ Class

The current relationship is a string label only (`Class.AcademicYear`). A direct `Class.AcademicYearId` is recommended because a class is the natural context for attendance and many exams. It avoids repeatedly parsing a label and allows historical classes to remain tied to an immutable year. This is a schema change and requires a backfill plan for the existing `2024/2025` labels.

### AcademicYear ↔ Student enrollment

`Student.ClassId` is a current/single relationship and `Student.EnrollmentDate` is a single date. `Enrollment` currently links only Student and Class. The most coherent historical model is `Enrollment.AcademicYearId` plus Student/Class foreign keys, allowing a student to have one or more year-specific records according to the approved policy. This is preferable to a direct year on Student because students span multiple years.

### AcademicYear ↔ Exam

`Exam` currently belongs to `Class`, `Grade`, and `Subject`, and its date is `Exam.ExamDate`. If `Class.AcademicYearId` is authoritative, the year can be derived through `Exam.Class`. For semester reporting, `Exam.SemesterId` is recommended because an exam is a period-specific assessment and a date alone does not provide a stable named-semester association. If a direct `AcademicYearId` is added to Exam, the service must verify it matches the semester/class year.

### AcademicYear ↔ Attendance

Attendance currently references Student and Class (`Attendance.cs:9-16`). The year can be derived through Class, and the attendance date can be checked against the class's year. A direct year key is not required for the first design if classes are year-specific. A direct semester key is optional and should be added only if attendance can be reported by semester without a reliable date-to-semester calculation.

### AcademicYear ↔ GradeResult

GradeResult references Exam, Student, and Grade (`GradeResult.cs:9-18`). Its academic context should normally be derived from Exam, then Semester, then AcademicYear. Adding another direct year key would be redundant and could become inconsistent. It may be justified for immutable reporting snapshots, but that is not supported by current requirements and would need explicit approval.

### Referential integrity and delete behavior

`MainContext.cs:59-87` explicitly uses `DeleteBehavior.Restrict` for Grade relationships and Student-to-Class. The model snapshot records several relationships as cascade, including Attendance-to-Class/Student, Enrollment-to-Class/Student, Exam-to-Class/Subject, and GradeResult-to-Exam (`MainContextModelSnapshot.cs:591-711`). New year/semester relationships should not cascade into historical academic records. The preferred behavior is to block deletion when dependents exist and expose a validation error through the service. Existing snapshot/context differences must be investigated during implementation planning before generating a migration.

## 9. API Endpoints Proposal

These endpoints are proposed only; none currently exists. They follow `ApiBaseController`'s `api/[controller]` convention and the newer explicit concrete return type pattern.

### Academic years controller (`api/academic-years`)

| Method | Route | Purpose | Request | Response |
|---|---|---|---|---|
| GET | `/api/academic-years` | Retrieve paginated/filterable years | `AcademicYearsParams` | `PaginatedResultDto<AcademicYearDto>` |
| GET | `/api/academic-years/{id:int}` | Retrieve a year and optionally its semesters | `id`; optional include query only if approved | `AcademicYearDto` |
| GET | `/api/academic-years/current` | Retrieve the current/active year | No body; current-state rule required | `AcademicYearDto` |
| POST | `/api/academic-years` | Create a year | `CreateAcademicYearDto` | `AcademicYearDto`; existing requested convention is `200 OK`, not `CreatedAtAction` |
| PUT | `/api/academic-years` | Update a year | `UpdateAcademicYearDto` containing `Id` | Updated `AcademicYearDto` with `200 OK` |
| PATCH/POST | `/api/academic-years/{id:int}/activate` | Activate a year if explicit state transitions are selected | `id` or an approved action DTO | `AcademicYearDto`; route/action choice requires approval |
| DELETE | `/api/academic-years/{id:int}` | Delete only when no dependent records exist | `id` | Meaningful `string` with `200 OK`; service throws validation error when blocked |

Prefer a single approved activation operation rather than exposing both active flags and action endpoints without a defined state model. If deactivation is needed, use a corresponding approved action or an update request; do not introduce authorization changes in this feature.

### Semesters controller (`api/semesters`)

| Method | Route | Purpose | Request | Response |
|---|---|---|---|---|
| GET | `/api/semesters` | Retrieve paginated/filterable semesters | `SemestersParams` | `PaginatedResultDto<SemesterDto>` |
| GET | `/api/semesters/{id:int}` | Retrieve a semester | `id` | `SemesterDto` |
| GET | `/api/academic-years/{academicYearId:int}/semesters` | Retrieve semesters belonging to a year | `academicYearId`, pagination/filter parameters | `PaginatedResultDto<SemesterDto>` or an approved collection DTO |
| GET | `/api/semesters/current` | Retrieve current semester | No body | `SemesterDto`; only if a current rule is approved |
| POST | `/api/semesters` | Create a semester under a year | `CreateSemesterDto` containing `AcademicYearId` | `SemesterDto` with `200 OK` |
| PUT | `/api/semesters` | Update a semester | `UpdateSemesterDto` containing `Id` | Updated `SemesterDto` with `200 OK` |
| DELETE | `/api/semesters/{id:int}` | Delete an unused semester | `id` | Meaningful `string` with `200 OK`; block if dependent exams/results exist |

Controllers should inject `IServiceManager`, delegate directly to the new services, avoid manual ModelState checks and business logic, and use explicit `Task<ActionResult<T>>` return types. The existing `ExamsController` is the closest template; `StudentController`'s legacy `IActionResult`, `CreatedAtAction`, and `NoContent` behavior should not be copied.

## 10. DTOs and Query Parameters Proposal

All proposed contracts belong in `Shared/Shared`, matching the existing DTO placement.

### Academic year DTOs

- `AcademicYearDto`: `int Id`, `string Name`/`Label`, `DateTime StartDate`, `DateTime EndDate`, `bool IsActive`/`IsCurrent`, and optionally `int SemesterCount` or `IReadOnlyCollection<ShortSemesterDto> Semesters` depending on endpoint payload size.
- `CreateAcademicYearDto`: required `Name`/`Label`, `StartDate`, `EndDate`, and optional initial active state only if creation can activate a year.
- `UpdateAcademicYearDto`: required `int Id`, required name/label, start/end dates, and active state only if direct state updates are approved. The ID should be in the DTO, matching the project instruction and `UpdateExamDto.Id` (`Shared/UpdateExamDto.cs:8-20`).
- `AcademicYearsParams`: `Search`/`Name`, `DateTime? FromStartDate`, `DateTime? ToEndDate`, `bool? IsActive`, `bool? IsCurrent`, `PageSize`, and `PageIndex` using the pattern in `Shared/Params/ExamsParams.cs:7-32`.

### Semester DTOs

- `SemesterDto`: `int Id`, `int AcademicYearId`, `string AcademicYear` or a compact parent DTO, `string Name`/`Code`, `DateTime StartDate`, `DateTime EndDate`, `bool IsActive`/`IsCurrent`, and optional `int DisplayOrder`.
- `CreateSemesterDto`: `AcademicYearId`, name/code, start/end dates, and optional order/state.
- `UpdateSemesterDto`: `Id`, `AcademicYearId`, name/code, start/end dates, and optional order/state.
- `SemestersParams`: `int? AcademicYearId`, `Search`/name, `DateTime? FromDate`, `DateTime? ToDate`, `bool? IsActive`, `bool? IsCurrent`, and pagination.

Concrete response types should not expose unrestricted EF navigation graphs. A compact semester DTO should be used inside an academic-year response if including children is approved. Empty collections should return an empty paginated result, consistent with the requested project conventions.

## 11. Specifications and Filtering Strategy

The existing specification design applies criteria and pagination at the database query level. `Specifications<T>` (`Core/Domain/Contracts/Specifications.cs:5-25`) stores criteria, includes, and `Skip`/`Take`; `GenericRepository` applies these through `SpecificationEvaluator` (`Infrastructure/Persistence/Contracts/GenericRepository.cs:30-41`). The new feature should reuse this design and should not add a new repository abstraction.

Recommended specifications:

- `AcademicYearSpecifications(AcademicYearsParams parameters, bool applyPagination, bool includeSemesters)` with optional name/label search, start/end range filters, active/current filtering, overlap-aware date predicates where appropriate, ordering, and pagination.
- `AcademicYearSpecifications(int id, bool includeSemesters)` for by-ID retrieval.
- `SemesterSpecifications(SemestersParams parameters, bool applyPagination, bool includeAcademicYear)` with `AcademicYearId`, name/code, date range, active/current, and pagination filters.
- `SemesterSpecifications(int id, bool includeRelated)` for by-ID retrieval.
- A dedicated existence/overlap specification may be used by services to count conflicting periods. It must handle inclusive/exclusive boundary rules consistently.

Filtering recommendations:

- **Year name/label:** case-insensitive search consistent with `ExamSpecifications` (`Core/Service/ConcreteSpecifications/ExamSpecifications.cs:10-17`). Prefer a normalized database column or a provider-safe comparison if search volume grows; do not assume `ToUpper()` is optimal without a database decision.
- **Year dates:** filter by start/end date, with clear semantics for containing versus intersecting a requested range.
- **Active/current:** use stored active state or calculate by date, not both ambiguously. A `current=true` filter should be defined as active, date-containing-today, or both.
- **Semester:** filter by `AcademicYearId`, name/code, period, and current state.
- **Related feature queries:** extend `ExamsParams`, `GradeResultsParams`, `AttendancesParams`, and student/class parameters only after deciding whether their period is direct or derived. For exams, `SemesterId` is the cleanest database filter. For attendance/results, year and semester filters can use navigations through Class/Exam if EF translation remains acceptable.

Count and page queries should remain separate as in `ExamService.GetExamsAsync` (`Core/Service/ExamService/ExamService.cs:24-35`). No specification should load all records into memory for filtering or pagination.

## 12. Database Schema and Migration Assessment

No database change is needed for the current, limited behavior of displaying the existing `Class.AcademicYear` string. Implementing the requested management feature, however, requires schema changes.

### Necessary changes for a normalized implementation

1. **New `AcademicYears` table:** integer identity primary key, required label/name, required start/end date columns, and an active/current representation if approved.
2. **New `Semesters` table:** integer identity primary key, required `AcademicYearId`, name/code, start/end dates, optional order/state, foreign key to `AcademicYears`, and an index on `AcademicYearId`.
3. **Class relationship:** add `AcademicYearId` to `Classes`, create a foreign key/index, and decide whether the existing `AcademicYear` string is retained temporarily, replaced, or removed after backfill. This cannot be safely decided from the source data alone.
4. **Enrollment relationship:** add `AcademicYearId` to `Enrollments` if historical enrollment is in scope, plus an index and any approved uniqueness constraint. Existing `Enrollments` currently have only `StudentId` and `ClassId` in the snapshot (`MainContextModelSnapshot.cs:137-157`).
5. **Semester-dependent records:** add `SemesterId` to `Exams` if semester is explicit. Attendance and grade result foreign keys are optional and should not be added merely for symmetry if they can derive context reliably.
6. **Indexes/constraints:** indexes for foreign keys and period queries; unique `(AcademicYearId, SemesterName/Code)` or order if approved; overlap constraints generally require service validation and may require a SQL Server-specific strategy because ordinary unique indexes do not prevent arbitrary date-range overlap.
7. **Delete behaviors:** configure restrict/no-action for year and semester dependents if historical records must be preserved. Do not use cascade from an academic year into classes, exams, attendance, or results.

### Migration and compatibility impact

The checked-in migrations include the existing class label as `nvarchar(max)` (`Infrastructure/Persistence/Migrations/20251221173135_DB-init.cs:71-84`) and the snapshot has no AcademicYear or Semester entity. A future implementation would require a new migration; this analysis intentionally does not create one or update the database.

Backfilling `AcademicYears` from `Classes.AcademicYear` can create a `2024/2025` row, but start/end dates cannot be derived authoritatively from the label. Backfilling class foreign keys is possible only after a date policy is approved. Existing exams date in January 2025 and existing attendance date in December 2025 do not by themselves prove the intended academic-year boundaries. Historical assignments for each record must not be invented.

A current-year unique rule is optional and database-provider-specific. If only one active year is allowed, enforce it in the service and consider a filtered unique index if the chosen SQL Server model supports the desired nullable/boolean representation. The report does not recommend a particular migration syntax.

## 13. Existing Data and Seed Analysis

`Infrastructure/Persistence/Contracts/DbInitializer.cs:48-80` initializes data in this order: Admin, Parent, Teacher, Grade, Subject, Class, TeacherSubject, Student, Attendance, Exam, and GradeResult. It has no AcademicYear or Semester initialization and no Enrollment initialization despite `MainContext` exposing `DbSet<Enrollment>`.

Verified seed facts:

- `Infrastructure/Persistence/Seeding/Classes.json:3-120` contains 24 classes, all labeled `2024/2025`, with grade IDs 1 through 12.
- `Exams.json:3-90` contains exam dates from January 15 through January 24, 2025, and links each exam to subject, grade, and class IDs. There is no year or semester field.
- `Attendance.json:3-60` contains December 1, 2025 attendance rows with student and class IDs. There is no year or semester field.
- `GradeResults.json:3-60` contains student/exam/grade/score values. There is no date, year, or semester field.
- The inspected seed file list contains no AcademicYears, Semesters, or Enrollments JSON file.

These records contain implicit assumptions but do not provide enough evidence to assign authoritative year/semester periods. In particular, the class label says `2024/2025`, but no start/end dates are supplied; the different exam and attendance dates cannot safely establish whether they are in one year, two years, or a seed-data inconsistency. No historical academic-year assignment should be invented.

Adding seed data would be necessary only if the feature requires a deterministic development/test dataset. It would require an approved date policy and explicit backfill/association decisions. This analysis does not modify any seed file.

## 14. Exceptions and Validation Strategy

The project uses domain exceptions and centralized middleware. `Core/Domain/Exceptions/NotFoundException.cs:7-11` is the base exception; `ExamNotFoundException(int id)` in `Core/Domain/Exceptions/NotFoundExceptions/ExamNotFoundException.cs:1-5` is the analogous entity-specific form. `ExceptionHandlerMiddelware` maps `NotFoundException` to 404 and `ValidationException` to 400 (`SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs:28-53`).

Recommended new exceptions, if the feature is approved:

- `AcademicYearNotFoundException(int id)`.
- `SemesterNotFoundException(int id)`.

They should accept only the ID and follow existing entity-specific naming. A semester lookup through a year route should still distinguish whether the parent year or semester is missing, according to the service contract.

Validation should live in services, not controllers:

- Required label/name and sensible length bounds.
- `StartDate < EndDate` for both entities.
- No overlapping academic years if approved.
- No overlapping semesters within one academic year.
- Semester dates entirely within parent year.
- Parent academic year existence before semester create/update.
- Active/current invariant, if selected.
- Prevent deletion of years/semesters with dependent classes, enrollments, exams, attendance, or results according to the approved policy.
- When updating, retrieve the entity by `UpdateDto.Id`, verify existence first, and reuse that entity, matching the project instructions and analogous services such as `ExamService.UpdateExamAsync` (`Core/Service/ExamService/ExamService.cs:71-93`).

DataAnnotations on request DTOs should handle basic request shape, while cross-record and period validation belongs in the service. Do not introduce FluentValidation or a new validation layer; `SchoolSystem.Api/AGENTS.md:77-80` explicitly excludes that architectural change.

## 15. Expected Files to Create or Modify

No files in this section were created or modified by this analysis. The following is an implementation inventory only.

### Expected files to create

- `Core/Domain/Entities/AcademicYear.cs` — year entity.
- `Core/Domain/Entities/Semester.cs` — semester entity.
- `Core/Domain/Exceptions/NotFoundExceptions/AcademicYearNotFoundException.cs` — year not-found exception.
- `Core/Domain/Exceptions/NotFoundExceptions/SemesterNotFoundException.cs` — semester not-found exception.
- `Core/ServiceAbstraction/IAcademicYearService.cs` — year service contract.
- `Core/ServiceAbstraction/ISemesterService.cs` — semester service contract.
- `Core/Service/AcademicYearService/AcademicYearService.cs` — year CRUD/current/validation behavior.
- `Core/Service/SemesterService/SemesterService.cs` — semester CRUD/current/validation behavior.
- `Core/Service/ConcreteSpecifications/AcademicYearSpecifications.cs` — year filtering and retrieval.
- `Core/Service/ConcreteSpecifications/SemesterSpecifications.cs` — semester filtering and retrieval.
- `Core/Service/MappingProfiles/AcademicYearProfile.cs` — year mappings.
- `Core/Service/MappingProfiles/SemesterProfile.cs` — semester mappings.
- `Shared/Shared/AcademicYearDto.cs`, `CreateAcademicYearDto.cs`, `UpdateAcademicYearDto.cs`.
- `Shared/Shared/SemesterDto.cs`, `CreateSemesterDto.cs`, `UpdateSemesterDto.cs`.
- `Shared/Shared/Params/AcademicYearsParams.cs`, `SemestersParams.cs`.
- `Infrastructure/Presentation/AcademicYearsController.cs` and `SemestersController.cs`.
- A future EF migration generated only after model and data decisions are approved.

### Expected files to modify

- `Core/Domain/Entities/Class.cs` — replace/transition the string year relationship.
- `Core/Domain/Entities/Enrollment.cs` — add year context if historical enrollment is approved.
- `Core/Domain/Entities/Exam.cs` — add `SemesterId` if direct semester association is approved.
- `Core/ServiceAbstraction/IServiceManager.cs` — expose both new services.
- `Core/Service/ServiceManager.cs` — construct and expose both services.
- `Core/Service/MappingProfiles/*` — update mappings for changed class/exam/enrollment contracts as needed.
- `Shared/Shared/ClassDto.cs` and related create/update class DTOs if present in the implementation scope.
- `Shared/Shared/ExamDto.cs`, create/update DTOs, and exam params if semester filtering is approved.
- `Infrastructure/Persistence/MainContext.cs` — DbSets, relationships, indexes, constraints, and delete behaviors.
- `Infrastructure/Persistence/Contracts/DbInitializer.cs` — seed order and data only after explicit approval.
- `Infrastructure/Persistence/Seeding/*` — only if approved seed records are defined.
- A new migration file and model snapshot generated by EF; existing migrations must not be edited.

No authentication, authorization, JWT, soft-delete, or unrelated refactoring should be included.

## 16. Implementation Plan

1. **Confirm business decisions:** choose year label semantics, date boundary policy, current/active semantics, overlap rules, semester count, deletion policy, and historical/backfill strategy.
2. **Model the domain:** add `AcademicYear` and `Semester`; decide and document direct versus derived relationships for Class, Enrollment, Exam, Attendance, and GradeResult.
3. **Update EF configuration:** add DbSets and explicit relationships/indexes/delete behaviors in `Infrastructure/Persistence/MainContext.cs`; resolve current context/snapshot delete-behavior discrepancies as part of the approved model change.
4. **Define shared contracts:** add DTOs and params with DataAnnotations for basic shape, retaining IDs in update DTOs.
5. **Add specifications:** implement database-level filtering, includes, counts, pagination, and overlap checks using the existing specification contracts.
6. **Implement services:** add existence checks, reference validation, overlap validation, active/current transitions, delete guards, mapping, and UnitOfWork saves using async conventions.
7. **Register services:** extend `IServiceManager` and `ServiceManger` while preserving lazy service construction and the existing dependency graph.
8. **Add mappings:** create profiles for year and semester and update existing profiles only for approved relationship changes.
9. **Add controllers:** use `ApiBaseController`, `IServiceManager`, direct expression-bodied delegation, concrete `Task<ActionResult<T>>`, `200 OK` for POST/PUT, and meaningful `200 OK` delete responses.
10. **Plan data transition:** create an approved backfill for the existing `Class.AcademicYear` values; do not infer missing dates or historical semester assignments.
11. **Add migration:** generate a new migration after model and backfill decisions; do not alter or run existing migrations as part of feature design.
12. **Seed intentionally:** add year/semester seed data only when dates and associations are confirmed, preserving initializer dependency order.
13. **Validate behavior:** build the solution and run relevant tests after implementation; test overlap, containment, current selection, dependent deletes, empty pages, and historical queries.

## 17. Risks and Potential Breaking Changes

- Replacing `Class.AcademicYear` string with a foreign key changes the database schema and class DTO/API contract. A compatibility period may be required.
- Existing seed data has only a label, so automatic date assignment can create false historical facts.
- Adding a semester foreign key to existing exams requires a backfill; exams have dates but no authoritative semester assignment.
- Adding year-aware enrollment changes how student/class history is queried and may conflict with the current direct `Student.ClassId` behavior.
- A direct year key on multiple records increases consistency risk if derived relationships are also retained.
- Deletion rules may make previously possible deletes fail once dependents are formally related; this is intentional if history is protected but is a behavioral change.
- Current date/time and timezone semantics are unspecified; an incorrect boundary policy can classify records differently around midnight or year boundaries.
- The model snapshot currently reports cascade behavior for relationships that `MainContext.OnModelCreating` partially configures as restrict. New implementation work must verify the actual model and migration state before relying on either representation.
- Current API conventions are inconsistent, especially in `StudentController.cs`; adopting the documented conventions may make the new endpoints behave differently from older endpoints, but avoids introducing additional legacy patterns.
- “Current” may be calculated by dates or stored as a flag. Supporting both without a single source of truth can produce multiple current years/semesters.

## 18. Questions Requiring User Decisions

1. Is an academic year identified by a unique label such as `2024/2025`, by dates, or by both?
2. What exact start and end dates should be used for the existing `2024/2025` class data, if it is migrated?
3. Can academic-year periods overlap, or must they be globally non-overlapping?
4. Is exactly one academic year active at a time, and is “active” different from “current by today's date”?
5. Is exactly one semester current at a time, and only within the active year?
6. Must every academic year contain exactly two semesters, or should the data model support a configurable number?
7. Are semester boundaries inclusive, exclusive, or represented as a start-inclusive/end-exclusive interval?
8. Should classes be recreated for each academic year with a required `AcademicYearId`, or can one class span multiple years?
9. Should `Enrollment` become the authoritative year-specific student/class relationship, and can a student have multiple enrollments in one year?
10. Should exams reference `SemesterId` directly, or should semester be derived from `ExamDate` and the class/year relationship?
11. Should attendance and grade results receive direct year/semester foreign keys, or should their context remain derived through Class and Exam?
12. May an academic year or semester be deleted after any dependent records exist, or must deletion always be blocked?
13. When a year ends, should it be deactivated/closed, and may historical records ever be edited after closure?
14. Is seed data required for an initial year and semesters, and what authoritative dates/associations should it use?
15. Should current academic-year and semester endpoints return 404 when no current period exists, or an empty/nullable response contract?
16. Should class/exam/student list endpoints gain year and semester filters in the same implementation, or should those be delivered separately?

## 19. Final Recommendations

1. Treat the current `Class.AcademicYear` string as transitional legacy data, not as the completed feature.
2. Add first-class `AcademicYear` and `Semester` entities with required date validation and a required semester-to-year relationship.
3. Make `Class.AcademicYearId` authoritative for class-based history and use a year-aware `Enrollment` for student history instead of relying on `Student.EnrollmentDate`.
4. Associate exams directly with semesters if semester-specific assessment reporting is required; derive grade-result context through exams and avoid redundant period keys unless a reporting requirement proves necessary.
5. Derive attendance year through its class initially, with direct semester storage only if the approved reporting model requires it.
6. Keep period filtering, overlap checks, counts, and pagination in specifications/services at database level using the existing GenericRepository and UnitOfWork architecture.
7. Block deletion of periods with historical dependents and use explicit active/closed lifecycle decisions rather than cascading deletion into school records.
8. Resolve date boundaries, current-state semantics, semester count, and historical backfill before implementation or migration generation.
9. Do not invent dates or historical semester assignments for the existing seed data. The available source supports the `2024/2025` label only.
10. Implement the feature in the existing layers and conventions, without CQRS, MediatR, FluentValidation, new repository abstractions, new packages, authentication changes, authorization changes, soft delete, or unrelated refactoring.
11. After approval, create one new migration through the normal EF workflow; do not modify existing migrations or update the database as part of analysis.

**Inspection limitation:** This report is based on the checked-in source files, migrations, model snapshot, seed JSON, and configuration inspected in the workspace. No live database schema/data inspection was performed, and no database-changing command was run. Consequently, the actual deployed database state, historical production data, and runtime business policies are unknown and require verification before implementation.
