# Attendance Management Feature Analysis

**Analysis-only result:** No existing source or project files were modified, created, deleted, renamed, or refactored.

## 1. Executive Summary

Attendance is partially represented in the existing system but does not have a dedicated Attendance Management feature.

### Existing Attendance support

Verified existing components:

- Attendance domain entity.
- `Attendance` database table.
- Student and Class relationships.
- Attendance seed data.
- Existing `AttendanceDto`.
- Existing AutoMapper mapping inside `StudentProfiles`.
- Student-specific attendance retrieval method:
  - `StudentService.GetMyAttendanceAsync(int studentId)`
- Student controller endpoint:
  - `GET /api/Students/{studentId}/attendance`

### Missing dedicated Attendance feature components

The following components do not currently exist:

- Dedicated `AttendanceService`.
- `IAttendanceService`.
- `AttendanceSpecifications`.
- `AttendanceProfile`.
- `AttendanceController`.
- `AttendanceNotFoundException`.
- Create Attendance DTO.
- Update Attendance DTO.
- Attendance query parameters.
- Attendance registration in `IServiceManager`.
- Attendance registration in `ServiceManger`.
- Dedicated Attendance CRUD operations.
- Database-level duplicate attendance constraint.
- Attendance-specific validation service logic.

### Recommended architectural direction

Attendance should follow the existing Exam and Grade architecture:

```text
AttendanceController
	-> IServiceManager
	-> IAttendanceService
	-> AttendanceService
	-> IUnitOfWork
	-> GenericRepository<Attendance, int>
	-> AttendanceSpecifications
	-> MainContext
```

The Exam feature is the primary controller and service reference. The Grade feature is the primary pagination reference. The older Student attendance method should not be copied because it loads all Attendance records and filters them in memory.

---

## 2. Current Attendance Entity Structure

### Entity path

`Core/Domain/Entities/Attendance.cs`

### Verified entity

```csharp
public class Attendance : BaseEntity<int>
{
	public int StudentId { get; set; }
	public Student Student { get; set; }

	public int ClassId { get; set; }
	public Class Class { get; set; }

	public DateTime Date { get; set; }
	public string Status { get; set; }
}
```

### Properties

| Property | Type | Required by current model | Purpose |
|---|---:|---:|---|
| `Id` | `int` | Yes | Primary key inherited from `BaseEntity<int>` |
| `StudentId` | `int` | Yes | Foreign key to `Student` |
| `Student` | `Student` | Yes by relationship | Student navigation |
| `ClassId` | `int` | Yes | Foreign key to `Class` |
| `Class` | `Class` | Yes by relationship | Class navigation |
| `Date` | `DateTime` | Yes | Attendance date and time |
| `Status` | `string` | Yes | Attendance status |

### Important model limitations

The entity does not contain:

- GradeId.
- SubjectId.
- SessionId.
- Session name.
- Attendance time separate from Date.
- Status enum.
- Excused reason.
- Notes.
- Attendance author or teacher ID.
- Created or updated timestamps.
- Soft-delete fields.
- Historical class identifier separate from `ClassId`.

The current model stores a full `DateTime`, even though all current seed values use the same time of `08:00:00`.

---

## 3. Database Relationships and Constraints

### MainContext

Path:

`Infrastructure/Persistence/MainContext.cs`

`MainContext` exposes:

```csharp
public DbSet<Attendance> Attendances => Set<Attendance>();
```

There is no explicit Attendance configuration in `OnModelCreating`.

The relationships are therefore represented through conventions and appear in the migrations.

### Attendance-to-Class relationship

Migration metadata confirms:

```csharp
b.HasOne("Domain.Entities.Class", "Class")
	.WithMany("Attendances")
	.HasForeignKey("ClassId")
	.OnDelete(DeleteBehavior.Cascade)
	.IsRequired();
```

Relationship:

```text
Class 1 ──── * Attendance
```

Deleting a Class currently cascades to its Attendance records.

### Attendance-to-Student relationship

Migration metadata confirms:

```csharp
b.HasOne("Domain.Entities.Student", "Student")
	.WithMany("Attendances")
	.HasForeignKey("StudentId")
	.OnDelete(DeleteBehavior.Cascade)
	.IsRequired();
```

Relationship:

```text
Student 1 ──── * Attendance
```

Deleting a Student currently cascades to its Attendance records.

### Attendance-to-Grade relationship

There is no direct Attendance-to-Grade relationship.

Grade is related indirectly through:

```text
Attendance -> Student -> Grade
Attendance -> Class -> Grade
```

The Attendance entity itself does not store `GradeId`.

### Primary key

The migration creates:

```csharp
table.PrimaryKey("PK_Attendances", x => x.Id);
```

### Indexes

The migration creates indexes on:

```text
IX_Attendances_ClassId
IX_Attendances_StudentId
```

No index currently exists on:

- `Date`.
- `Status`.
- `(StudentId, Date)`.
- `(ClassId, Date)`.
- `(StudentId, ClassId, Date)`.

### Unique constraints

No unique Attendance constraint currently exists.

Therefore, the database currently permits multiple Attendance records for the same student and date.

### Migration paths

Relevant migration files include:

- `Infrastructure/Persistence/Migrations/20251221173135_DB-init.cs`
- `Infrastructure/Persistence/Migrations/20251221173135_DB-init.Designer.cs`
- `Infrastructure/Persistence/Migrations/20251224041421_Modify-parent-student-rel.Designer.cs`
- `Infrastructure/Persistence/Migrations/20251225105922_refactor-db.Designer.cs`
- `Infrastructure/Persistence/Migrations/20251227120352_Class-Subject-Teacher-Rel.Designer.cs`
- `Infrastructure/Persistence/Migrations/MainContextModelSnapshot.cs`

The initial migration creates the Attendance table with:

```text
Id int identity primary key
StudentId int not null
ClassId int not null
Date datetime2 not null
Status nvarchar(max) not null
```

---

## 4. Existing Attendance Implementation Inventory

### Attendance entity

**Exists**

Path:

`Core/Domain/Entities/Attendance.cs`

Status:

- Complete as a basic persistence entity.
- Does not support historical class snapshots.
- Does not support session-based attendance.
- Does not define an enum or constrained status type.

### Attendance DTO

**Exists**

Path:

`Shared/Shared/AttendanceDto.cs`

Current implementation:

```csharp
public class AttendanceDto
{
	public DateTime Date { get; set; }
	public bool IsPresent { get; set; }
	public string SubjectName { get; set; }
}
```

Status:

- Incomplete for dedicated Attendance Management.
- Does not expose Attendance ID.
- Does not expose StudentId.
- Does not expose ClassId.
- Does not expose Status directly.
- Does not expose Student name.
- Does not expose Class name.
- Does not represent Absent or Late distinctly.
- `SubjectName` is not a property of Attendance.
- The current mapping derives `SubjectName` through a class-subject collection.

### Create Attendance DTO

**Missing**

No file matching a Create Attendance DTO was found.

Recommended future path:

`Shared/Shared/CreateAttendanceDto.cs`

### Update Attendance DTO

**Missing**

No file matching an Update Attendance DTO was found.

Recommended future path:

`Shared/Shared/UpdateAttendanceDto.cs`

### Attendance query parameters

**Missing**

No Attendance-specific parameter class was found.

Recommended future path:

`Shared/Shared/Params/AttendanceParams.cs`

### Attendance service abstraction

**Missing**

No `IAttendanceService` was found.

Recommended future path:

`Core/ServiceAbstraction/IAttendanceService.cs`

### Attendance service implementation

**Missing**

No dedicated Attendance service was found.

The only Attendance-related service method is in:

`Core/Service/student/StudentService.cs`

Current method:

```csharp
public async Task<IEnumerable<AttendanceDto>> GetMyAttendanceAsync(int studentId)
{
	var attendance = await unitOfWork
		.GetRepository<Attendance, int>()
		.GetAllAsync();

	var myAttendance = attendance
		.Where(a => a.StudentId == studentId);

	return mapper.Map<IEnumerable<AttendanceDto>>(myAttendance);
}
```

Status:

- Existing but incomplete and not suitable as the dedicated feature implementation.

Problems:

- Loads the entire Attendance table.
- Filters in memory.
- Does not use a specification.
- Does not verify that the student exists.
- Does not use pagination.
- Does not filter at database level.
- Uses the older Student feature response contract.
- Depends on a mapping that may require navigation data not loaded.

### Existing Student controller endpoint

**Exists**

Path:

`Infrastructure/Presentation/StudentController.cs`

Current endpoint:

```csharp
[HttpGet("{studentId:int}/attendance")]
public async Task<IActionResult> GetMyAttendance(int studentId)
{
	var attendance = await serviceManager.StudentService.GetMyAttendanceAsync(studentId);
	return Ok(attendance);
}
```

Route:

```text
GET /api/Students/{studentId}/attendance
```

Status:

- Legacy implementation.
- Uses `IActionResult`.
- Uses a method body.
- Contains an intermediate variable.
- Manually returns `Ok`.
- Does not use pagination.
- Does not use a dedicated Attendance response contract.
- Does not throw a Student-specific exception if the student does not exist through this method.

### Attendance specifications

**Missing**

No `AttendanceSpecifications` class was found.

Recommended future path:

`Core/Service/ConcreteSpecifications/AttendanceSpecifications.cs`

### Attendance AutoMapper profile

**Missing as a dedicated profile**

The existing mapping is inside:

`Core/Service/MappingProfiles/StudentProfiles.cs`

Current mapping:

```csharp
CreateMap<Attendance, AttendanceDto>()
	.ForMember(d => d.IsPresent, opt => opt.MapFrom(s => s.Status == "Present"))
	.ForMember(d => d.SubjectName, opt => opt.MapFrom(s => s.Class.ClassSubjects.Select(s => s.Subject.Name)))
	.ReverseMap();
```

Status:

- Existing but unsuitable as the dedicated mapping.
- `IsPresent` collapses all statuses other than `"Present"` into `false`.
- `SubjectName` maps a collection projection into a scalar string.
- Attendance has no direct Subject relationship.
- Mapping depends on `Class.ClassSubjects` and nested Subject data.
- `ReverseMap()` is unnecessary and may create unsafe reverse mappings.
- The current mapping does not include Attendance ID, Student ID, Class ID, or Status.

### Attendance controller

**Missing**

No dedicated Attendance controller was found.

The only Attendance route is in:

`Infrastructure/Presentation/StudentController.cs`

Status:

- Existing student-specific endpoint, not a complete Attendance Management API.

### Attendance-specific exception

**Missing**

No `AttendanceNotFoundException` was found.

Recommended future path:

`Core/Domain/Exceptions/NotFoundExceptions/AttendanceNotFoundException.cs`

### ServiceManager registration

**Missing**

`IServiceManager` currently exposes:

- `IAdminService AdminService { get; }`
- `IStudentService StudentService { get; }`
- `IAuthenticationService AuthenticationService { get; }`
- `IParentService ParentService { get; }`
- `IGradeService GradeService { get; }`
- `IExamService ExamService { get; }`
- `IGradeResultService GradeResultService { get; }`

No Attendance service property exists.

### `ServiceManger` registration

**Missing**

`Core/Service/ServiceManager.cs` currently contains lazy services for Admin, Student, Authentication, Parent, Grade, Exam, and GradeResult.

No Attendance lazy field, construction, or property exists.

### Dependency injection

There is no direct `IAttendanceService` registration in `SchoolSystem.Api/Program.cs`.

The project registers the service manager:

```csharp
builder.Services.AddScoped<IServiceManager, ServiceManger>();
```

The existing architecture resolves concrete services lazily through `ServiceManger`, so Attendance should follow that pattern rather than adding a separate resolution mechanism.

### Attendance repository methods

**Missing**

There are no Attendance-specific repository methods.

The generic repository supports all required basic operations:

```csharp
Task AddAsync(TEntity entity);
void Update(TEntity entity);
void Delete(TEntity entity);
Task<TEntity> GetAsync(TKey Id);
Task<IEnumerable<TEntity>> GetAllAsync();
Task<TEntity> GetAsync(Specifications<TEntity> specifications);
Task<IEnumerable<TEntity>> GetAllAsync(Specifications<TEntity> specifications);
Task<int> CountAsync();
Task<int> CountAsync(Specifications<TEntity> specifications);
```

### Attendance validation

**Missing**

No dedicated Attendance validation exists.

The current Student attendance method performs no Attendance-specific validation.

---

## 5. Seed Data Analysis

### Attendance seed file

Path:

`Infrastructure/Persistence/Seeding/Attendance.json`

### Number of records

There are 10 Attendance records.

### Seed records

All records use:

```text
Date: 2025-12-01T08:00:00
```

| Record | StudentId | ClassId | Status |
|---:|---:|---:|---|
| 1 | 1 | 1 | Present |
| 2 | 2 | 3 | Present |
| 3 | 3 | 5 | Absent |
| 4 | 4 | 7 | Present |
| 5 | 5 | 9 | Late |
| 6 | 6 | 11 | Present |
| 7 | 7 | 13 | Present |
| 8 | 8 | 15 | Present |
| 9 | 9 | 17 | Absent |
| 10 | 10 | 19 | Present |

### Student/Class consistency

The Student seed data assigns:

| StudentId | Student GradeId | Student ClassId |
|---:|---:|---:|
| 1 | 1 | 1 |
| 2 | 2 | 3 |
| 3 | 3 | 5 |
| 4 | 4 | 7 |
| 5 | 5 | 9 |
| 6 | 6 | 11 |
| 7 | 7 | 13 |
| 8 | 8 | 15 |
| 9 | 9 | 17 |
| 10 | 10 | 19 |

Each Attendance record’s `ClassId` matches the related Student’s `ClassId`.

### Class existence

The Class seed contains classes covering IDs 1 through 24.

The Attendance ClassIds are:

```text
1, 3, 5, 7, 9, 11, 13, 15, 17, 19
```

All referenced classes exist.

### Grade consistency

The Attendance entity does not have GradeId.

Consistency can only be inferred through:

- Student.GradeId.
- Class.GradeId.

The seed records match the Student/Class relationships:

- Student 1 and Class 1 belong to Grade 1.
- Student 2 and Class 3 belong to Grade 2.
- Student 3 and Class 5 belong to Grade 3.
- This pattern continues through Student 10 and Class 19, which belong to Grade 10.

### Status values

The seed data contains:

- `Present`
- `Absent`
- `Late`

It does not contain:

- `Excused`
- `Leave`
- `Partial`
- `EarlyDeparture`

Only the three observed values should be treated as currently supported.

### Date validity

All seed dates are valid `DateTime` values.

The entity stores date and time, but the current seed uses a common attendance time of `08:00:00`.

### Duplicate records

There are no duplicate `(StudentId, Date)` pairs in the seed data.

However, no database constraint prevents duplicates.

### Seed inconsistencies

No Attendance seed inconsistencies were found.

Verified:

- All StudentIds reference existing students.
- All ClassIds reference existing classes.
- Student/Class relationships match.
- All statuses are among the observed status values.
- All dates are valid.
- No duplicate student/date pairs exist.

### Seed initialization order

Path:

`Infrastructure/Persistence/Contracts/DbInitializer.cs`

Attendance is initialized after Classes and Students:

```csharp
await Init<Grade>("Grades.json");
await Init<Subject>("Subjects.json");
await Init<Class>("Classes.json");
await Init<TeacherSubject>("TeacherSubjects.json");
await Init<Student>("Students.json");
await Init<Attendance>("Attendance.json");
await Init<Exam>("Exams.json");
await Init<GradeResult>("GradeResults.json");
```

This order satisfies the current Attendance foreign keys.

### Seed data should not be modified

The task explicitly prohibits modifying seed data. No seed changes are recommended because the inspected Attendance seed data is internally consistent.

---

## 6. Existing Architecture and Reference Implementations

### Primary reference: Exam Management

Relevant files:

- `Infrastructure/Presentation/ExamsController.cs`
- `Core/ServiceAbstraction/IExamService.cs`
- `Core/Service/ExamService/ExamService.cs`
- `Core/Service/ConcreteSpecifications/ExamSpecifications.cs`
- `Core/Service/MappingProfiles/ExamProfile.cs`
- `Shared/Shared/ExamDto.cs`
- `Shared/Shared/CreateExamDto.cs`
- `Shared/Shared/UpdateExamDto.cs`
- `Shared/Shared/Params/ExamsParams.cs`

The Exam controller currently follows the preferred style:

```csharp
public class ExamsController(IServiceManager serviceManager) : ApiBaseController
{
	[HttpGet]
	public async Task<ActionResult<PaginatedResultDto<ExamDto>>> Get(
		[FromQuery] ExamsParams _params)
		=> await serviceManager.ExamService.GetExamsAsync(_params);

	[HttpGet("{id:int}")]
	public async Task<ActionResult<ExamDto>> GetById(int id)
		=> await serviceManager.ExamService.GetExamByIdAsync(id);

	[HttpPost]
	public async Task<ActionResult<ExamDto>> Create(
		[FromBody] CreateExamDto dto)
		=> await serviceManager.ExamService.CreateExamAsync(dto);

	[HttpPut]
	public async Task<ActionResult<ExamDto>> Update(
		[FromBody] UpdateExamDto dto)
		=> await serviceManager.ExamService.UpdateExamAsync(dto);

	[HttpDelete("{id:int}")]
	public async Task<ActionResult<string>> Delete(int id)
		=> await serviceManager.ExamService.DeleteExamAsync(id);
}
```

This is the recommended Attendance controller style.

### Pagination reference: Grade Management

Relevant files:

- `Infrastructure/Presentation/GradesController.cs`
- `Core/ServiceAbstraction/IGradeService.cs`
- `Core/Service/GradeService/GradeService.cs`
- `Core/Service/ConcreteSpecifications/GradeSpecifications.cs`

Grade uses separate specifications for:

- Count without pagination.
- Page data with pagination.

Current pattern:

```csharp
var specForCount = new GradeSpecifications(
	_params,
	applyPagination: false);

var specForPage = new GradeSpecifications(
	_params,
	applyPagination: true);

var total = await repository.CountAsync(specForCount);
var grades = await repository.GetAllAsync(specForPage);
```

Attendance should use the same pattern.

### Student Management

Relevant files:

- `Infrastructure/Presentation/StudentController.cs`
- `Core/ServiceAbstraction/student/IStudentService.cs`
- `Core/Service/student/StudentService.cs`
- `Core/Service/ConcreteSpecifications/StudentSpecifications.cs`
- `Core/Service/MappingProfiles/StudentProfiles.cs`
- `Shared/Shared/AttendanceDto.cs`

Student contains the only existing Attendance read operation, but it is not the preferred reference because it:

- Loads all Attendance records.
- Filters in memory.
- Does not use specifications.
- Uses a legacy controller style.
- Does not support pagination.
- Does not verify Student existence.
- Uses an incomplete Attendance DTO.

### ServiceManager

Files:

- `Core/Service/ServiceManager.cs`
- `Core/ServiceAbstraction/IServiceManager.cs`

The existing pattern is:

```csharp
readonly Lazy<IExamService> examService;

examService = new Lazy<IExamService>(
	() => new Service.ExamService.ExamService(unitOfWork, mapper));

public IExamService ExamService => examService.Value;
```

Attendance should be added using the same lazy pattern.

### Repository and specification architecture

Files:

- `Core/Domain/Contracts/IGenericRepository.cs`
- `Core/Domain/Contracts/IUnitOfWork.cs`
- `Core/Domain/Contracts/Specifications.cs`
- `Infrastructure/Persistence/Contracts/GenericRepository.cs`
- `Infrastructure/Persistence/Contracts/SpecificationEvaluator.cs`

The specification evaluator applies:

1. Criteria.
2. Includes.
3. Pagination.

```csharp
if (specifications.Criteria is not null)
	query = query.Where(specifications.Criteria);

query = specifications.Includes.Aggregate(
	query,
	(current, increment) => current.Include(increment));

if (specifications.IsPaginated)
{
	query = query
		.Skip(specifications.Skip)
		.Take(specifications.Take);
}
```

Attendance filters should therefore be placed in the specification criteria to ensure database-level filtering.

### ApiBaseController

Path:

`Infrastructure/Presentation/ApiBaseController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class ApiBaseController : ControllerBase
{
}
```

Manual ModelState validation is not needed for new Attendance controllers.

### Exceptions and middleware

Relevant files:

- `Core/Domain/Exceptions/NotFoundException.cs`
- `Core/Domain/Exceptions/NotFoundExceptions/StudentNotFoundException.cs`
- `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs`

Entity-specific exceptions follow this pattern:

```csharp
public class StudentNotFoundException(int id)
	: NotFoundException($"Student with Id [[{id}]] is not found.")
{
}
```

The Attendance feature should use:

```csharp
AttendanceNotFoundException(int id)
```

The middleware maps `NotFoundException` to HTTP 404 and `ValidationException` to HTTP 400.

### AutoMapper registration

Path:

`SchoolSystem.Api/Program.cs`

```csharp
builder.Services.AddAutoMapper(
	cfg => { },
	typeof(Service.AssemblyReference).Assembly);
```

A dedicated Attendance profile under `Core/Service/MappingProfiles` would be discovered automatically.

### Authentication and authorization

Authentication and authorization should remain unchanged.

No Attendance authorization attributes should be added because authorization is intentionally postponed.

---

## 7. Missing Components

The following components should be proposed for a future implementation.

| Component | Recommended path | Current status |
|---|---|---|
| Attendance response DTO | `Shared/Shared/AttendanceDto.cs` | Existing but incomplete and used by Student |
| Create DTO | `Shared/Shared/CreateAttendanceDto.cs` | Missing |
| Update DTO | `Shared/Shared/UpdateAttendanceDto.cs` | Missing |
| Query parameters | `Shared/Shared/Params/AttendanceParams.cs` | Missing |
| Service interface | `Core/ServiceAbstraction/IAttendanceService.cs` | Missing |
| Service implementation | `Core/Service/AttendanceService/AttendanceService.cs` | Missing |
| Specification | `Core/Service/ConcreteSpecifications/AttendanceSpecifications.cs` | Missing |
| Mapping profile | `Core/Service/MappingProfiles/AttendanceProfile.cs` | Missing |
| Controller | `Infrastructure/Presentation/AttendancesController.cs` | Missing |
| Not-found exception | `Core/Domain/Exceptions/NotFoundExceptions/AttendanceNotFoundException.cs` | Missing |
| ServiceManager property | `Core/ServiceAbstraction/IServiceManager.cs` | Missing |
| Lazy service registration | `Core/Service/ServiceManager.cs` | Missing |

The existing `AttendanceDto` should either be expanded for the dedicated feature or replaced by a dedicated response contract after checking compatibility with `StudentService.GetMyAttendanceAsync`.

---

## 8. Business Rules and Open Decisions

### A. Attendance statuses

#### Verified current representation

Attendance uses:

```csharp
public string Status { get; set; }
```

The seed data uses:

- `Present`
- `Absent`
- `Late`

The existing mapping determines presence using:

```csharp
s.Status == "Present"
```

#### Currently supported statuses

Only these statuses are verified as existing:

```text
Present
Absent
Late
```

#### Not verified

The current project does not verify support for:

- Excused.
- Leave.
- Partial.
- EarlyDeparture.

#### Recommendation

Initially validate against the existing statuses:

```text
Present
Absent
Late
```

Do not add an enum or additional statuses without approval.

A future status enum could improve consistency, but changing from a string representation may require migration and compatibility handling.

### B. Duplicate attendance records

#### Current behavior

The database has no unique constraint on:

```text
StudentId + Date
```

The current seed contains one record per student on one date, but this is only seed behavior, not an enforced rule.

#### Option 1: One record per student per day

Benefits:

- Simple daily attendance reporting.
- Easy duplicate prevention.
- Clear daily status.

Risks:

- Does not support multiple class sessions in one day.
- Cannot represent morning and afternoon attendance separately.
- Requires a unique constraint or service-level duplicate check.

#### Option 2: Multiple records per student per day

Benefits:

- Supports multiple sessions.
- Supports subject-specific or period-specific attendance if extended later.

Risks:

- Requires session information to distinguish records.
- Daily reporting becomes more complex.
- Multiple records without session metadata are ambiguous.

#### Recommendation

If the school records daily attendance only, use one record per student per day.

If attendance is session-based, the current entity is insufficient. A session identifier, subject, period, or time range would be required.

This is a business decision and should not be silently enforced.

### C. Attendance date and time

#### Current model

```csharp
public DateTime Date { get; set; }
```

The model supports both date and time.

#### Current seed behavior

All records use:

```text
2025-12-01T08:00:00
```

#### Current limitations

The entity has no:

- Separate date-only field.
- Session ID.
- Period number.
- Start/end time.
- Attendance event type.

#### Recommendation

For daily attendance, normalize the date component and define whether the time should be ignored.

For session attendance, add explicit session information in a future schema change.

No current model property proves that attendance is subject-specific or session-specific.

### D. Class and Student consistency

#### Current model

Attendance stores both:

```csharp
StudentId
ClassId
```

Student also stores:

```csharp
Student.ClassId
Student.GradeId
```

#### Current seed behavior

All Attendance ClassIds match the related Student ClassIds.

#### Recommended validation

When creating or updating Attendance:

```text
Attendance.ClassId == Student.ClassId
```

This prevents attendance from being recorded against a class unrelated to the student.

#### Historical class issue

Because Attendance stores `ClassId`, it can preserve the class at the time of recording only if the service intentionally stores the current class value and the student later changes class.

However, no explicit historical policy exists.

If Student.ClassId changes later:

- Existing Attendance.ClassId remains unchanged.
- The Attendance record can preserve the old class.
- A later validation comparing Attendance.ClassId to the student’s current ClassId may falsely treat historical records as inconsistent.

#### Recommendation

Use the supplied or derived Attendance ClassId as the historical class at creation time, but define update behavior explicitly.

Do not automatically rewrite historical Attendance.ClassId when a student changes class.

### E. Attendance updates and deletion

#### Current behavior

No dedicated update or delete behavior exists.

Database behavior:

- Deleting a Student cascades to Attendance.
- Deleting a Class cascades to Attendance.

#### Recommendation

For dedicated API operations:

- Update should retrieve the Attendance entity by DTO ID.
- Missing Attendance should throw `AttendanceNotFoundException(id)`.
- Update should validate Student/Class consistency.
- Update should modify only scalar and foreign-key properties.
- Delete should retrieve the entity first.
- Missing Attendance should throw `AttendanceNotFoundException(id)`.
- Delete should use the generic repository and UnitOfWork.
- Delete should return a meaningful string response with HTTP 200.

Do not introduce:

- Soft delete.
- Archive fields.
- Global filters.
- New deletion policies.

The existing cascade relationships should remain unchanged unless a separate approved persistence change is requested.

### F. Attendance reports

#### Currently supportable with the current model

The current entity can support:

- Daily attendance by Date.
- Attendance by StudentId.
- Attendance by ClassId.
- Attendance by Grade through Student or Class relationships.
- Date-range filtering.
- Status filtering.
- Present/Absent/Late counts.

These can be implemented with database-level specifications and aggregation queries, subject to repository limitations.

#### Not directly supported

The current model does not directly support:

- Subject attendance.
- Session attendance.
- Period-level attendance.
- Teacher-specific attendance records.
- Excused reason reporting.
- Historical class enrollment snapshots.
- Attendance percentage over a defined enrollment period.

#### Reporting recommendation

Start with filtered paginated records. Add aggregate reporting only after the basic feature and status/duplicate policies are approved.

---

## 9. Proposed DTO Design

### AttendanceDto

Recommended path:

`Shared/Shared/AttendanceDto.cs`

The existing DTO is insufficient for dedicated Attendance Management.

Recommended properties based on actual entity fields and related navigations:

| Property | Type | Required | Purpose |
|---|---|---:|---|
| `Id` | `int` | Yes | Attendance identifier |
| `StudentId` | `int` | Yes | Related Student identifier |
| `StudentName` | `string` | Response | Related Student display name |
| `ClassId` | `int` | Yes | Related Class identifier |
| `ClassName` | `string` | Response | Related Class display name |
| `Date` | `DateTime` | Yes | Attendance date/time |
| `Status` | `string` | Yes | Raw status value |

Optional derived response field:

| Property | Type | Required | Purpose |
|---|---|---:|---|
| `GradeName` | `string` | Optional | Derived through Class.Grade or Student.Grade |

Do not include SubjectName because Attendance has no Subject relationship.

Do not claim a direct GradeId property exists on Attendance.

### CreateAttendanceDto

Recommended path:

`Shared/Shared/CreateAttendanceDto.cs`

Proposed properties:

| Property | Type | Required | Purpose |
|---|---|---:|---|
| `StudentId` | `int` | Yes | Student receiving attendance |
| `ClassId` | `int` | Business-required | Class associated with attendance |
| `Date` | `DateTime` | Yes | Attendance date/time |
| `Status` | `string` | Yes | Present, Absent, or Late initially |

Open decision:

- Should ClassId be supplied by the client?
- Or should it always be derived from the student’s current class?

Recommended approach:

- Accept ClassId if historical class recording is required.
- Validate it against the student’s class according to the approved business policy.
- Otherwise derive ClassId from Student.ClassId to prevent client-controlled inconsistencies.

### UpdateAttendanceDto

Recommended path:

`Shared/Shared/UpdateAttendanceDto.cs`

Proposed properties:

| Property | Type | Required | Purpose |
|---|---|---:|---|
| `Id` | `int` | Yes | Existing Attendance identifier |
| `StudentId` | `int` | Yes or immutable | Student reference |
| `ClassId` | `int` | Business-required | Class reference |
| `Date` | `DateTime` | Yes | Attendance date/time |
| `Status` | `string` | Yes | Attendance status |

The update endpoint should accept the DTO body directly:

```text
PUT /api/Attendances
```

The ID should be read from `UpdateAttendanceDto.Id`.

Open decision:

- Should StudentId and ClassId be editable after creation?
- Or should only Date and Status be editable?

Recommended initial policy:

- Allow updates only to Date and Status.
- Keep StudentId and ClassId stable after creation unless an explicit correction workflow is required.

### AttendanceParams

Recommended path:

`Shared/Shared/Params/AttendanceParams.cs`

Proposed properties:

| Property | Type | Required | Purpose |
|---|---|---:|---|
| `StudentId` | `int?` | Optional | Filter by student |
| `ClassId` | `int?` | Optional | Filter by class |
| `GradeId` | `int?` | Optional | Filter through Student or Class |
| `Status` | `string?` | Optional | Filter by status |
| `FromDate` | `DateTime?` | Optional | Inclusive lower date boundary |
| `ToDate` | `DateTime?` | Optional | Inclusive upper date boundary |
| `PageSize` | `int` | Optional | Existing pagination convention |
| `PageIndex` | `int` | Optional | Existing pagination convention |

There is no verified Attendance SubjectId relationship, so SubjectId should not be included unless the domain is expanded.

---

## 10. Proposed API Endpoints

All proposed endpoints should follow the current `ExamsController` style:

- Typed `Task<ActionResult<T>>`.
- Expression-bodied methods.
- Direct IServiceManager delegation.
- No manual ModelState checks.
- No `IActionResult`.
- No `CreatedAtAction`.
- No HTTP 201.
- No HTTP 204.

### Get all attendance records

```text
GET /api/Attendances
```

Recommended signature:

```csharp
Task<ActionResult<PaginatedResultDto<AttendanceDto>>>
```

Recommended query parameters:

```text
StudentId
ClassId
GradeId
Status
FromDate
ToDate
PageIndex
PageSize
```

Expected behavior:

- Apply filters in the database.
- Apply filters before pagination.
- Return filtered total count.
- Return an empty paginated result when no records match.
- Include Student and Class as required for mapping.

### Get attendance by ID

```text
GET /api/Attendances/{id}
```

Recommended signature:

```csharp
Task<ActionResult<AttendanceDto>>
```

Expected behavior:

- Retrieve Attendance by ID.
- Include Student and Class.
- Return the DTO.
- Throw `AttendanceNotFoundException(id)` if missing.

### Create attendance

```text
POST /api/Attendances
```

Recommended signature:

```csharp
Task<ActionResult<AttendanceDto>>
```

Expected behavior according to current controller standards:

- Return 200 OK with the created Attendance DTO.
- Do not use `CreatedAtAction`.
- Do not return 201.
- Service validates Student, Class, status, date, and any approved duplicate policy.

Possible controller shape:

```csharp
[HttpPost]
public async Task<ActionResult<AttendanceDto>> Create(
	[FromBody] CreateAttendanceDto dto)
	=> await serviceManager.AttendanceService.CreateAttendanceAsync(dto);
```

### Update attendance

```text
PUT /api/Attendances
```

Recommended signature:

```csharp
Task<ActionResult<AttendanceDto>>
```

Expected behavior:

- Receive one `UpdateAttendanceDto`.
- Retrieve by `dto.Id`.
- Throw `AttendanceNotFoundException(dto.Id)` if missing.
- Validate the update.
- Return updated DTO with HTTP 200.

Possible controller shape:

```csharp
[HttpPut]
public async Task<ActionResult<AttendanceDto>> Update(
	[FromBody] UpdateAttendanceDto dto)
	=> await serviceManager.AttendanceService.UpdateAttendanceAsync(dto);
```

### Delete attendance

```text
DELETE /api/Attendances/{id}
```

Recommended signature:

```csharp
Task<ActionResult<string>>
```

Expected behavior:

- Retrieve Attendance by ID.
- Throw `AttendanceNotFoundException(id)` if missing.
- Delete through repository and UnitOfWork.
- Return a meaningful success string with HTTP 200.

Possible controller shape:

```csharp
[HttpDelete("{id:int}")]
public async Task<ActionResult<string>> Delete(int id)
	=> await serviceManager.AttendanceService.DeleteAttendanceAsync(id);
```

### Get attendance by student

This can be represented by the collection endpoint:

```text
GET /api/Attendances?StudentId={studentId}
```

Recommended approach:

- Avoid duplicating a dedicated route initially.
- Reuse the same specification and pagination logic.
- Verify Student existence if the endpoint is intended to represent a specific student resource rather than a filter.

Optional route:

```text
GET /api/Attendances/students/{studentId}
```

This should only be added if the API requires a distinct route contract.

### Get attendance by class

Recommended through filters:

```text
GET /api/Attendances?ClassId={classId}
```

Optional dedicated route:

```text
GET /api/Attendances/classes/{classId}
```

A dedicated route is not necessary if query filtering is sufficient.

### Get attendance by date/date range

Recommended through:

```text
GET /api/Attendances?FromDate=...&ToDate=...
```

Inclusive boundary behavior should be specified:

```text
Date >= FromDate
Date <= ToDate
```

If the API should treat dates as date-only values, the service/specification must normalize time boundaries.

### Reporting endpoints

Possible future endpoints:

```text
GET /api/Attendances/reports/daily
GET /api/Attendances/reports/students/{studentId}
GET /api/Attendances/reports/classes/{classId}
```

These are not required for the initial CRUD feature and require decisions about:

- Aggregation shape.
- Date-only versus DateTime.
- Status categories.
- Empty result behavior.
- Whether reporting belongs in the Attendance service or a separate reporting service.

No new architectural abstraction should be introduced without approval.

---

## 11. Proposed Service and Specification Design

### IAttendanceService

Recommended path:

`Core/ServiceAbstraction/IAttendanceService.cs`

Proposed methods:

```csharp
Task<PaginatedResultDto<AttendanceDto>> GetAttendancesAsync(
	AttendanceParams parameters);

Task<AttendanceDto> GetAttendanceByIdAsync(int id);

Task<AttendanceDto> CreateAttendanceAsync(
	CreateAttendanceDto dto);

Task<AttendanceDto> UpdateAttendanceAsync(
	UpdateAttendanceDto dto);

Task<string> DeleteAttendanceAsync(int id);
```

### AttendanceService

Recommended path:

`Core/Service/AttendanceService/AttendanceService.cs`

Responsibilities:

- Retrieve paginated Attendance data.
- Validate Student existence.
- Validate Class existence.
- Validate Student/Class consistency.
- Validate status.
- Validate date.
- Apply approved duplicate policy.
- Retrieve entity before update.
- Retrieve entity before delete.
- Throw `AttendanceNotFoundException`.
- Map entities to DTOs.
- Save through UnitOfWork.

The service should not:

- Access MainContext directly.
- Contain controller response handling.
- Return HTTP-specific errors.
- Use null-forgiving operators.
- Load all Attendance data for filtering.
- Modify unrelated entities.

### AttendanceSpecifications

Recommended path:

`Core/Service/ConcreteSpecifications/AttendanceSpecifications.cs`

Recommended constructors:

```csharp
AttendanceSpecifications(
	AttendanceParams parameters,
	bool applyPagination = true,
	bool includeRelated = true)
```

And:

```csharp
AttendanceSpecifications(
	int id,
	bool includeRelated = true)
```

Criteria should support:

```text
StudentId
ClassId
Status
FromDate
ToDate
GradeId through Student.GradeId or Class.GradeId
```

Example conceptual criteria:

```csharp
attendance =>
	(!parameters.StudentId.HasValue ||
		attendance.StudentId == parameters.StudentId) &&
	(!parameters.ClassId.HasValue ||
		attendance.ClassId == parameters.ClassId) &&
	(!parameters.GradeId.HasValue ||
		attendance.Student.GradeId == parameters.GradeId) &&
	(string.IsNullOrWhiteSpace(parameters.Status) ||
		attendance.Status == parameters.Status) &&
	(!parameters.FromDate.HasValue ||
		attendance.Date >= parameters.FromDate) &&
	(!parameters.ToDate.HasValue ||
		attendance.Date <= parameters.ToDate)
```

Includes should be added only when needed:

```csharp
AddInclude(attendance => attendance.Student);
AddInclude(attendance => attendance.Class);
```

If GradeName is returned, related Grade data may need to be loaded through Student or Class. The existing specification API supports simple includes but does not expose a `ThenInclude` abstraction.

### Pagination

Use two specifications:

```csharp
var countSpecification = new AttendanceSpecifications(
	parameters,
	applyPagination: false,
	includeRelated: false);

var pageSpecification = new AttendanceSpecifications(
	parameters,
	applyPagination: true,
	includeRelated: true);
```

Then:

```csharp
var total = await repository.CountAsync(countSpecification);
var attendances = await repository.GetAllAsync(pageSpecification);
```

The total must represent filtered records before pagination.

### AttendanceProfile

Recommended path:

`Core/Service/MappingProfiles/AttendanceProfile.cs`

The current `StudentProfiles` mapping should not be copied because it maps a collection to a scalar and collapses statuses into a Boolean.

A dedicated profile should explicitly map:

- `Id`.
- `StudentId`.
- Student name.
- `ClassId`.
- Class name.
- `Date`.
- `Status`.

If a DTO exposes `IsPresent`, it should be clearly defined and should not replace the raw status unless approved.

### AttendanceNotFoundException

Recommended path:

`Core/Domain/Exceptions/NotFoundExceptions/AttendanceNotFoundException.cs`

Recommended shape:

```csharp
public class AttendanceNotFoundException(int id)
	: NotFoundException($"Attendance with Id [[{id}]] is not found.")
{
}
```

It must:

- Inherit from `NotFoundException`.
- Accept only the Attendance ID.
- Be thrown by GetById, Update, and Delete when required.
- Be handled by the existing exception middleware.

### ServiceManager integration

Required future changes:

`Core/ServiceAbstraction/IServiceManager.cs`

```csharp
IAttendanceService AttendanceService { get; }
```

`Core/Service/ServiceManager.cs`

```csharp
readonly Lazy<IAttendanceService> attendanceService;
```

Constructor initialization:

```csharp
attendanceService = new Lazy<IAttendanceService>(
	() => new Service.AttendanceService.AttendanceService(
		unitOfWork,
		mapper));
```

Property:

```csharp
public IAttendanceService AttendanceService
	=> attendanceService.Value;
```

---

## 12. Validation Requirements

### Student existence

**Why needed:** Attendance requires a valid Student foreign key.

**Layer:** Attendance service.

**Model support:** Yes. `StudentId` is a required foreign key.

**Recommended behavior:**

```csharp
throw new StudentNotFoundException(studentId);
```

### Class existence

**Why needed:** Attendance requires a valid Class foreign key.

**Layer:** Attendance service.

**Model support:** Yes. `ClassId` is a required foreign key.

**Required exception:** A `ClassNotFoundException` already exists and should be reused.

### Student/Class consistency

**Why needed:** The Attendance record should not associate a student with an unrelated class.

**Layer:** Attendance service.

**Model support:** Yes. Student stores `ClassId`.

**Business decision:** The exact historical behavior must be approved.

Potential rule:

```text
Attendance.ClassId == Student.ClassId
```

Potential validation message:

```text
The selected class does not belong to the selected student.
```

### Valid status

**Why needed:** Current `Status` is an unrestricted string.

**Layer:** Attendance service.

**Model support:** The string property allows validation, but the database does not constrain values.

**Verified statuses:**

- `Present`
- `Absent`
- `Late`

**Business decision:** Whether to allow `Excused` or other statuses.

### Valid date

**Why needed:** `Date` is required and should represent a valid attendance event.

**Layer:** Attendance service.

**Model support:** Yes, `DateTime` is non-nullable.

Potential rules requiring approval:

- Reject default `DateTime`.
- Reject future dates.
- Normalize to date-only.
- Allow future attendance entry for scheduled classes.

### Duplicate prevention

**Why needed:** The current database permits duplicates.

**Layer:** Service for application-level checking; database if permanent uniqueness is approved.

**Model support:** No unique constraint exists.

**Business decision:** One record per student/date versus multiple sessions per day.

### Update existence

**Why needed:** Avoid updating a missing entity.

**Layer:** Attendance service.

**Required process:**

1. Retrieve using `UpdateAttendanceDto.Id`.
2. Check for null.
3. Throw `AttendanceNotFoundException(id)`.
4. Reuse retrieved instance.
5. Apply validated changes.

### Delete existence

**Why needed:** Avoid deleting a missing entity or returning a false success.

**Layer:** Attendance service.

**Required process:**

1. Retrieve by ID.
2. Check for null.
3. Throw `AttendanceNotFoundException(id)`.
4. Delete retrieved entity.
5. Save changes.

### Date-range boundaries

Recommended initial behavior:

```text
Date >= FromDate
Date <= ToDate
```

If date-only filtering is required, the API must define how time components are handled.

---

## 13. Database and Migration Impact

### Can be implemented without migration

The following can be implemented without schema changes:

- Attendance CRUD.
- Student and Class existence validation.
- Student/Class consistency validation.
- Status validation in the service.
- Date validation in the service.
- Pagination.
- Student, Class, Grade, and date filtering.
- Attendance-specific exception.
- Attendance DTOs and mappings.
- Attendance specifications.
- ServiceManager registration.
- Attendance controller.
- Dynamic response mapping.

### Requires a migration if approved

The following would require schema changes:

- Unique constraint on `(StudentId, Date)`.
- Date-only column replacing DateTime.
- Session identifier.
- SubjectId.
- TeacherId.
- Excused reason.
- Audit fields.
- Soft-delete fields.
- Historical enrollment snapshot fields.
- Status enum conversion if represented as a database enum or constrained value.
- Additional indexes for high-volume reporting.

### Requires business decision before migration

- Whether multiple daily sessions are supported.
- Whether one student can have multiple records on a date.
- Whether historical class membership must be preserved independently.
- Whether statuses include Excused.
- Whether Attendance needs subject/session context.
- Whether attendance should be immutable after finalization.
- Whether deletion should remain cascading.

### Existing deletion risks

Current database behavior:

- Deleting Student cascades to Attendance.
- Deleting Class cascades to Attendance.

This may remove historical attendance records automatically.

The task explicitly prohibits introducing soft delete or changing deletion behavior at this stage, so the existing behavior should be documented and preserved until a separate decision is made.

---

## 14. Risks and Recommendations

### Risk: Existing AttendanceDto is incomplete

The current DTO is:

```csharp
public class AttendanceDto
{
	public DateTime Date { get; set; }
	public bool IsPresent { get; set; }
	public string SubjectName { get; set; }
}
```

It is not suitable for complete Attendance CRUD because it lacks:

- ID.
- Student ID.
- Class ID.
- Status.
- Student name.
- Class name.

Recommendation: establish a dedicated DTO contract while reviewing compatibility with the Student attendance endpoint.

### Risk: Existing mapping is unsafe

Current mapping:

```csharp
.ForMember(
	d => d.SubjectName,
	opt => opt.MapFrom(
		s => s.Class.ClassSubjects.Select(s => s.Subject.Name)))
```

This maps a collection projection to a scalar property and depends on nested navigation loading.

Recommendation: do not reuse this mapping for the dedicated feature. Add an explicit Attendance profile.

### Risk: No database status constraint

`Status` is stored as unrestricted `nvarchar(max)`.

Recommendation: validate allowed status values in the service until a database constraint or enum strategy is approved.

### Risk: No duplicate constraint

The database allows duplicate student/date records.

Recommendation: do not impose uniqueness until the session policy is approved.

### Risk: Class changes over time

Student.ClassId may change, while existing Attendance.ClassId remains unchanged.

Recommendation: treat Attendance.ClassId as a historical value after creation unless the business explicitly requires reassignment.

### Risk: Cascade deletion

Deleting Student or Class can remove Attendance history.

Recommendation: preserve current behavior for now, but obtain approval before changing deletion behavior.

### Risk: Legacy Student endpoint

The existing Student endpoint:

```text
GET /api/Students/{studentId}/attendance
```

uses a non-paginated DTO and in-memory filtering.

Recommendation:

- Keep it unchanged in the initial Attendance feature unless integration requires updates.
- Consider internally delegating it to the Attendance service in a separate approved change.
- Do not refactor Student as part of the initial Attendance implementation without explicit scope.

### Risk: No reporting abstraction

The existing repository provides basic specifications and counts, but not aggregate query methods.

Recommendation:

- Implement filtered paginated records first.
- Add reporting only after the required aggregation response contracts are approved.
- Avoid introducing a new repository abstraction solely for reporting without approval.

### Risk: Current exception middleware ordering

`Program.cs` currently invokes:

```csharp
app.MapControllers();
app.UseMiddleware<ExceptionHandlerMiddleware>();
```

The project instructions identify middleware ordering as an unresolved existing issue.

Recommendation: do not modify it as part of Attendance analysis or implementation unless separately approved.

---

## 15. Recommended Implementation Sequence

1. Approve Attendance status values.
2. Decide whether Attendance is daily or session-based.
3. Decide whether duplicate Student/date records are allowed.
4. Decide whether ClassId is supplied or derived.
5. Decide whether historical class membership must be preserved.
6. Decide whether only Date/Status can be updated or Student/Class can also change.
7. Confirm whether the existing Student attendance endpoint should remain unchanged.
8. Define the final Attendance response DTO.
9. Define create and update DTO properties.
10. Add `AttendanceParams`.
11. Add `AttendanceNotFoundException`.
12. Add `AttendanceSpecifications`.
13. Add a dedicated `AttendanceProfile`.
14. Add `IAttendanceService`.
15. Implement `AttendanceService`.
16. Add `IAttendanceService` to `IServiceManager`.
17. Add lazy Attendance resolution to `ServiceManger`.
18. Add `AttendancesController` using the exact ExamsController style.
19. Verify database-level filtering and filtered pagination count.
20. Verify Student/Class existence checks.
21. Verify status/date validation.
22. Verify update/delete existence behavior.
23. Build the full solution.
24. Run available tests.
25. Review the final diff and confirm no authentication, authorization, migration, seed, or unrelated changes were introduced.

---

## 16. Questions Requiring My Approval

1. **Allowed statuses**
   - Keep only `Present`, `Absent`, and `Late`?
   - Add `Excused`?
   - Add other statuses?

2. **Duplicate attendance**
   - Enforce one Attendance record per Student/date?
   - Allow multiple records per day for future sessions?

3. **Date semantics**
   - Treat `Date` as date-only?
   - Preserve time?
   - Add session support later?

4. **Class consistency**
   - Must Attendance.ClassId always equal the student’s current ClassId?
   - Should ClassId be derived automatically?
   - Should historical ClassId be preserved if a student later changes class?

5. **Update policy**
   - Allow updates to Date and Status only?
   - Allow changing StudentId and ClassId?
   - Should records become immutable after finalization?

6. **Deletion policy**
   - Keep current explicit delete behavior and existing cascade behavior?
   - Should attendance deletion be restricted later?
   - Should historical records eventually be archived or soft-deleted?

7. **DTO compatibility**
   - Replace or expand the existing `AttendanceDto`?
   - Preserve the existing Student attendance endpoint contract separately?

8. **Reporting scope**
   - Is paginated CRUD sufficient initially?
   - Are daily, student, class, grade, date-range, and status statistics required now?

9. **Database constraints**
   - Should a unique `(StudentId, Date)` index be added after the duplicate policy is approved?
   - Should indexes be added for Date, Status, or reporting combinations?

10. **Historical data**
	- Should the existing Attendance seed structure remain unchanged?
	- Should future implementation validate existing records against current Student/Class relationships?

---

## Final status

This report is analysis-only. No files were modified during the Attendance inspection. Implementation should remain paused until the business decisions above are approved.
