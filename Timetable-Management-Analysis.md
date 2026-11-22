# Timetable & Weekly Schedule Management Audit

## 1. Executive Summary

This audit examines the existing `SchoolSystem.Api` solution for the approved Timetable & Weekly Schedule Management feature. The solution uses the required layered architecture: controllers delegate through `IServiceManager`, concrete services apply business rules, repositories are obtained through `IUnitOfWork`, and specifications shape queries. The existing model contains useful academic relationships—`ClassSubject`, `TeacherSubject`, `AcademicYear`, and `Semester`—but it has no timetable, weekly schedule, planned lesson, holiday, execution, or schedule-history implementation.

**Verified facts:** the existing `Attendance` record is keyed around a student, class, date, and status; it is not session-based. `Exam` and `GradeResult` represent academic assessment and do not reference attendance or lessons. `AcademicYearService` and `SemesterService` provide manually managed active-period operations. The repository and transaction abstractions needed by a scheduling feature already exist.

**Recommendation:** add a versioned recurring timetable model that is copied into independent weekly schedules, then represent every planned lesson as a weekly child record. Store holiday dates as separate calendar exceptions and record every weekly lesson change as an immutable change log. Add an explicit actual-lesson execution record before linking scheduling to attendance. Do not repurpose `ClassTeacher`, add `ClassSubjectTeacher`, add teacher status fields, alter authentication, add soft delete, or rewrite historical academic records.

No source implementation was performed by this audit.

## 2. Existing Solution Architecture

### Verified architecture

| Area | Finding | Status | Evidence |
|---|---|---|---|
| Domain | Entities, repository contracts, specifications, and domain exceptions are in the domain project. | Implemented | `Core/Domain/Entities/*`; `Core/Domain/Contracts/IGenericRepository.cs`, `IUnitOfWork.cs`, and `Specifications.cs`. |
| Service abstractions | Feature contracts are exposed through interfaces and aggregated by `IServiceManager`. | Implemented | `Core/ServiceAbstraction/IServiceManager.cs:10-23`; existing interfaces under `Core/ServiceAbstraction`. |
| Concrete services | Services obtain repositories from `IUnitOfWork`, validate references, save asynchronously, and map entities to DTOs. | Implemented | `Core/Service/AttendanceService/AttendanceService.cs:20-101`; `Core/Service/ExamService/ExamService.cs:48-95`. |
| Service manager | `ServiceManger` lazily constructs concrete services and exposes them through `IServiceManager`. | Implemented | `Core/Service/ServiceManager.cs:14-53`. |
| Persistence | `GenericRepository` supports CRUD, specifications, includes, pagination, and counts. | Implemented | `Infrastructure/Persistence/Contracts/GenericRepository.cs:10-42`; `Core/Domain/Contracts/IGenericRepository.cs:5-16`. |
| Transactions | Unit of work exposes `ExecuteInTransactionAsync`; it begins, commits, and rolls back an EF transaction. | Implemented | `Core/Domain/Contracts/IUnitOfWork.cs:5-10`; `Infrastructure/Persistence/Contracts/UnitOfWork.cs:22-35`. |
| Mapping | AutoMapper profiles are used by services. | Implemented | `Core/Service/MappingProfiles/AttendanceProfile.cs:7-16`; registration in `SchoolSystem.Api/Program.cs:18`. |
| Presentation | Controllers inherit `ApiBaseController`, use `IServiceManager`, and delegate to services. | Implemented | `Infrastructure/Presentation/ApiBaseController.cs`; `Infrastructure/Presentation/AttendancesController.cs:8-33`. |
| Exception handling | Domain exceptions are translated centrally to HTTP responses. | Implemented | `SchoolSystem.Api/MiddleWares/ExceptionHandlerMiddelware.cs:9-64`. |
| Scheduling architecture | No scheduling-specific service, abstraction, DTO, controller, specification, or persistence model exists. | Missing | Project inventory and targeted search for timetable, schedule, lesson, weekly, holiday, calendar, generation, substitution, and change-log terms found no implementation. |

### Conventions to preserve

A future implementation should follow `Controller → IServiceManager → service abstraction → concrete service → UnitOfWork → GenericRepository/Specifications`. Controllers should remain thin and should not perform conflict or relationship validation. The service layer should own validation, entity existence checks, transaction coordination, and history creation. New DTOs, specifications, mappings, services, controllers, entities, and exception types should be added only as part of an approved implementation; this report does not add them.

The repository contract uses `GetRepository<TEntity,TKey>()`, and the current unit of work already provides a transaction boundary. Specifications support criteria, includes, and pagination but do not currently provide a unique-constraint or locking abstraction; persistence constraints and transaction behavior will therefore be important for duplicate weekly generation.

## 3. Existing Domain Model and Relationships

The following findings are based on the actual source files, not assumed seed data.

### Scheduling-related academic entities

| Entity / member | Status | Verified evidence and scheduling relevance |
|---|---|---|
| `Teacher` | Implemented | `Core/Domain/Entities/Teacher.cs:8-17` has `Id`, `FullName`, `Specialization`, `HireDate`, `UserId`, `TeacherSubjects`, and `TeacherClasses`. There is no teacher status or availability property. |
| `TeacherSubject` | Implemented | `Core/Domain/Entities/TeacherSubject.cs:7-13` is a composite qualification link between `TeacherId` and `SubjectId`. This is suitable for the qualification check only. |
| `ClassTeacher` | Implemented | `Core/Domain/Entities/ClassTeacher.cs:7-12` links a teacher and class. `MainContext` configures a composite key at `Infrastructure/Persistence/MainContext.cs:192-203`. It is not a subject-specific teaching assignment and must not become the scheduling source of truth. |
| `Class` | Implemented / partially historical | `Core/Domain/Entities/Class.cs:7-25` has optional `AcademicYearId`, `GradeId`, subject and teacher link collections, students, attendance, exams, and enrollments. `LegacyAcademicYear` remains mapped to the required `AcademicYear` column at `MainContext.cs:41-57`. |
| `ClassSubject` | Implemented | `Core/Domain/Entities/ClassSubject.cs:7-12` links a class and subject. `MainContext.cs:205-216` configures the composite key. This is the required membership check for planned lessons. |
| `Subject` | Implemented | `Core/Domain/Entities/Subject.cs:7-13` has subject name and qualification/membership collections. |
| `AcademicYear` | Implemented | `Core/Domain/Entities/AcademicYear.cs:3-12` has dates, `IsActive`, semesters, classes, and enrollments. `AcademicYearService.GetActiveAcademicYearAsync` queries active years, and activation/deactivation is manual (`Core/Service/AcademicYearService/AcademicYearService.cs:38-43`, `82-120`). |
| `Semester` | Implemented | `Core/Domain/Entities/Semester.cs:3-14` has required `AcademicYearId`, dates, `IsActive`, and exams. `SemesterService.ActivateSemesterAsync` requires the parent year to be active (`Core/Service/SemesterService/SemesterService.cs:97-116`). |
| `Student` | Implemented | `Core/Domain/Entities/Student.cs:8-29` belongs to one class and has attendance and grade-result collections. It does not have a planned-lesson or execution relationship. |
| `Attendance` | Implemented, not session-based | `Core/Domain/Entities/Attendance.cs:7-17` contains `StudentId`, `ClassId`, `Date`, and `Status`. It has no scheduled-lesson, execution, subject, teacher, or time-period reference. |
| `Exam` | Implemented | `Core/Domain/Entities/Exam.cs:7-26` references subject, class, grade, and optional semester. It is an assessment record, not a lesson occurrence. |
| `GradeResult` | Implemented | `Core/Domain/Entities/GradeResult.cs:7-18` references an exam, student, grade, and score. It must not be rewritten by schedule changes. |

### Persistence findings

`Infrastructure/Persistence/MainContext.cs:13-28` exposes DbSets for existing academic entities, but there are no scheduling DbSets. The context configures the `ClassSubject`, `TeacherSubject`, and `ClassTeacher` relationships at lines 192-229. It also configures `AcademicYear` to `Semester` and optional `Class`/`Enrollment` relationships at lines 46-64.

`AcademicYear.IsActive` and `Semester.IsActive` are represented in the entities and enforced operationally by service methods. No unique database index for one active academic year or one active semester is evident in the inspected `MainContext` configuration. Scheduling services should query the existing active-period services/specifications and validate that the selected semester belongs to the selected active academic year; they should not redesign academic-period management.

## 4. Existing Scheduling Components Inventory

| Component searched | Current status | Evidence |
|---|---|---|
| Recurring timetable/base timetable | Missing | No timetable entity, DbSet, DTO, service, specification, mapping, or controller appears in the project inventory or targeted source search. |
| Timetable entries | Missing | No lesson-slot entity or day/time scheduling model exists. |
| Weekly schedule | Missing | No week-start/week-end schedule model or generation service exists. |
| Planned lesson | Missing | No planned lesson entity is related to class, subject, teacher, academic year, or semester. |
| Holiday/calendar exception | Missing | No holiday, calendar exception, cancellation, or date-range scheduling component exists. |
| Teacher substitution | Missing | No substitution operation or historical replacement model exists. |
| Schedule change log | Missing | No scheduling audit/change-log entity or service exists. |
| Actual lesson execution | Missing | No lesson-session or delivery record exists. |
| Scheduling APIs | Missing | Presentation contains academic, attendance, exam, grade, enrollment, student, parent, admin, and authentication controllers, but no scheduling controller. |
| Automatic generation | Missing | No hosted service, timer, startup catch-up logic, or scheduling package was identified. |

The existing `AttendanceService` demonstrates the service pattern but cannot prove lesson delivery: it validates a student/class/date/status and prevents duplicate attendance for a student/date (`Core/Service/AttendanceService/AttendanceService.cs:57-101`, `136-143`).

## 5. Gap Analysis

The feature is currently a new bounded capability rather than an extension of an existing timetable implementation.

1. **Recurring planning:** missing base timetable and versioned entries.
2. **Academic scope:** existing classes can reference an academic year, and semesters reference an academic year, but no schedule records retain both academic context values.
3. **Weekly materialization:** missing independent weekly schedule and planned-lesson copies.
4. **Historical preservation:** changing a future base definition cannot currently be isolated from previously generated weeks because no copy/version model exists.
5. **Modification:** missing teacher replacement, time, class, subject, revision, and extra-lesson operations.
6. **Validation:** existing `ExamService` validates class-subject and class/semester-year consistency (`Core/Service/ExamService/ExamService.cs:113-145`), but no service validates teacher qualification or time overlap.
7. **Holiday behavior:** missing date-range exception, cancellation state, scheduling block, and explicit removal workflow.
8. **History:** missing old/new value capture, actor, reason, and operation history.
9. **Actual delivery:** attendance has no session link, and no separate execution record exists.
10. **Reliable automation:** no generation mechanism or idempotency constraint exists.

## 6. Proposed Domain Design

The following is the minimum conceptual model. Names are recommendations, not existing classes.

### `BaseTimetable`

Represents a recurring timetable definition scoped to an academic year and semester. Important properties should include an identifier, `AcademicYearId`, `SemesterId`, version number, effective-from metadata or generation applicability, creation time, and an immutable/current status if needed. It should relate to many base entries. Its purpose is planning configuration, not an actual event.

A base timetable must be immutable after it has generated a weekly schedule, or edits must create a new version. Avoid mutating an old version in place. If administrators edit the current future definition, create a new version and preserve the prior version.

### `BaseTimetableEntry`

Represents one recurring weekly slot: class, subject, qualified teacher, day-of-week, start time, and end time. It should reference its base timetable version. Optional display/order metadata may be useful, but it should not duplicate unrelated class-teacher assignment concepts. It is a recurring template, not proof that a lesson occurred.

### `WeeklySchedule`

Represents one generated Sunday-to-Saturday week within an academic year and semester. It should store the normalized Sunday week start, Saturday end or a derivable end, the source `BaseTimetable` version, generation timestamp, and a generation state if failure recovery requires it. It owns planned lessons and holiday/cancellation results for its dates.

There should be a database uniqueness rule for the chosen schedule scope and Sunday week start. If the approved scope is school-wide, this is likely `(AcademicYearId, SemesterId, WeekStartDate)`; if multiple schedule scopes are required, the scope key must be explicitly defined before implementation.

### `ScheduledLesson` / `PlannedLesson`

Represents an independent copy of a base entry for a particular date and time. Important properties should include weekly schedule ID, date, start/end time, class ID, subject ID, teacher ID, source base entry ID/version where applicable, lesson type (`Regular`, `Extra`, `Revision`, or an equivalent controlled value), status (`Planned`, `Cancelled`, or an equivalent controlled value), and modification metadata. It must retain the academic year and semester through the weekly schedule, with direct foreign keys only if query/index requirements justify them.

This entity is the authoritative plan for the generated week. It must permit modifications after the scheduled end time because the requirement explicitly allows historical planned-lesson edits. It must not be treated as delivery evidence.

Extra and revision lessons should use the same planned-lesson entity with a type/source distinction rather than separate redundant entities. They must pass the same relationship, conflict, academic-context, and holiday validations.

### `HolidayException`

Represents a calendar exception independently of the recurring timetable. Store academic context as appropriate, inclusive start and end dates, optional reason, creation metadata, and an active/removal state or explicit removal timestamp. The model must support a single date and consecutive date ranges.

Holiday date-range validation should require `StartDate <= EndDate`, reject invalid academic context, and define overlap behavior for existing holiday records. This entity is an exception, not a modified base timetable.

### `ScheduleChangeLog`

Represents an immutable change to a weekly planned lesson. It should reference the scheduled lesson, operation type, timestamp, optional reason, actor/user identifier when available, and old/new values. A structured JSON snapshot can capture the complete before/after lesson state if the project’s database/provider policy permits it; otherwise use explicit nullable old/new columns for class, subject, teacher, date, start time, end time, type, and status. The selected representation should be consistent and queryable.

### `ActualLessonExecution`

A separate record is recommended. It represents whether a planned lesson actually occurred, was partially delivered, or was cancelled/no-show for operational reasons, with actual start/end times, execution status, notes, and a link to the planned lesson. It is an actual event, not a template or plan.

## 7. Base Timetable Lifecycle and Versioning

1. Resolve the manually active `AcademicYear` and `Semester`, verifying that the semester belongs to the active year.
2. Create or edit a base timetable through a service. Editing a version that has already generated a week must produce a new version rather than mutate the source of historical schedules.
3. Validate every entry against class existence, class academic year, subject existence, class-subject membership, teacher existence, teacher-subject qualification, valid times, and teacher/class overlaps within the candidate version.
4. Mark one version as applicable for future generation according to an explicitly defined effective rule. The system should not silently infer applicability from dates beyond the approved Sunday-week rule.
5. When a week is generated, store the exact source base timetable version ID on `WeeklySchedule`. Copy entries into planned lessons, including the values needed to preserve the generated result even if the source version changes later.
6. Never use a live query from the current base entries to display historical weekly schedules. Historical weeks must read their own planned-lesson copies.

A version number or immutable version identifier should be unique within the academic scope. Concurrency protection should prevent two administrators from publishing conflicting current versions. If database temporal tables are not already part of project conventions, an explicit version relationship is preferable to introducing unrelated audit infrastructure.

## 8. Weekly Schedule Generation and Recovery

### Recommended mechanism

No background-job package is installed or was identified. The first implementation should use existing ASP.NET Core/.NET primitives rather than add a package: a hosted background service or an application-owned generation service invoked at startup and by a controlled administrative/manual endpoint. The generation logic must be in the service layer, not in a controller or timer callback.

A robust approach is a `WeeklyScheduleGenerationService` with an idempotent `EnsureWeeksGeneratedAsync` operation. At application startup and on the recurring weekly trigger, it should determine required Sunday weeks from the active academic context, identify missing weeks, and generate each missing week in order. This handles an application being offline at the beginning of a week. A manual administrative recovery endpoint can invoke the same service without duplicating logic; authorization remains outside this audit because it is postponed by the approved rules.

### Idempotency and transaction safety

- Normalize every requested week to its Sunday date using one documented school time zone.
- Query by the unique schedule scope and Sunday week start before creating a schedule.
- Add a database unique constraint for that key. The service should catch a duplicate-race failure and re-read the existing schedule rather than create a second one.
- Generate one week inside `IUnitOfWork.ExecuteInTransactionAsync`, inserting the schedule and all copied planned lessons atomically.
- If validation or persistence fails, roll back the schedule and copied lessons together; do not leave a half-generated week.
- Mark a generation state only if partial processing or retry diagnostics are required. A committed complete schedule is the success boundary.
- On retry, an existing completed week is returned/skipped; an incomplete state can be resumed or safely rebuilt according to the final persistence design.

### Week boundaries and holidays

The week is Sunday through Saturday. Generation should create planned lessons for each applicable day, then apply active holiday exceptions to those dates by cancelling planned lessons. It should not delete or alter base entries. The generation service should avoid creating new planned lessons for holiday dates.

## 9. Weekly Schedule Modification

All modifications belong in the weekly-schedule service and must operate on the selected planned lesson, not on the base version. Supported service operations should include changing a teacher, replacing an absent teacher, changing time, changing subject/class when relationships remain valid, adding an extra lesson, adding a revision lesson, and modifying a lesson after its scheduled end.

Every operation should:

1. Load the weekly schedule and lesson through a specification.
2. Confirm academic context and date membership.
3. Reject additions while an active holiday exception covers the date.
4. Validate references and all conflicts using the proposed complete state.
5. Capture the old values before mutation.
6. Apply the mutation and add one change-log record in the same transaction.
7. Return the updated planned lesson DTO.

A change to the teacher is not a teacher-assignment feature. The teacher on the planned lesson remains the actual class/subject assignment for that occurrence, while `TeacherSubject` only proves qualification. Do not use `ClassTeacher` as the source of actual teaching assignments.

A cancellation caused by a holiday should be represented as a planned-lesson status change associated with the holiday application, or by a separate cancellation result if the final design requires it. Removing a holiday must not restore lessons automatically. The administrator must explicitly review and add/modify lessons.

## 10. Holiday and Calendar Exception Management

Store holidays separately from `BaseTimetable` and do not delete, move, or mutate recurring entries. A date-range record should use inclusive dates and permit a missing reason. The service should reject reversed ranges, invalid academic contexts, and conflicting/overlapping ranges according to a final overlap policy.

When a holiday is created:

- Find all affected weekly schedules and planned lessons whose dates fall in the inclusive range.
- Mark all affected planned lessons cancelled in a transaction.
- Ensure cancelled lessons are excluded from actual-delivery counts.
- Record the cancellation reason/source if the change-log design supports system-generated entries.
- Prevent new extra, revision, or replacement lessons on covered dates.

If a holiday spans multiple weeks, process every existing affected weekly schedule. Future generation should apply the exception when it materializes those dates. If the holiday is removed, only the calendar block is removed; cancelled lessons remain cancelled and no lessons are automatically restored or moved.

The design must define whether an active holiday may be edited after cancellation and how its effects are logged. This is an implementation decision, but it must preserve the approved no-automatic-restoration rule.

## 11. Conflict Validation and Business Rules

The service layer should validate the proposed state before create or update. Specifications should provide efficient candidate queries; they should not replace the business decision in the service.

### Required checks

- **Class:** load the class or throw the existing class-specific not-found exception.
- **Subject:** load the subject or throw the existing subject-specific not-found exception.
- **Teacher:** load the teacher or throw the existing teacher-specific not-found exception.
- **Class-subject membership:** confirm a `ClassSubject` link for the selected class and subject.
- **Teacher qualification:** confirm a `TeacherSubject` link for the selected teacher and subject. This is qualification only, not assignment.
- **Class academic year:** confirm the class belongs to the schedule’s academic year, accounting for the existing optional `Class.AcademicYearId` and legacy field situation.
- **Schedule academic context:** confirm the weekly schedule’s academic year and semester are valid and that the semester belongs to the year.
- **Active context:** for new scheduling, require the existing active AcademicYear and active Semester rules. Historical weekly schedule reads and allowed historical corrections should not be confused with creating new active-period work.
- **Holiday:** reject new planned lessons and modifications that add/reschedule a lesson onto a covered holiday date.
- **Time values:** require `start < end`, a valid day/date relationship, and normalized time-zone handling.
- **Class conflict:** exclude the current lesson when updating, then reject another lesson for the same class/date when `startA < endB && startB < endA`.
- **Teacher conflict:** apply the same interval formula for the same teacher/date. Adjacent lessons are allowed because equality at an endpoint is not overlap.
- **Extra/revision lessons:** apply every check above; they are not validation exemptions.

The database should later receive foreign keys, composite indexes, uniqueness for weekly schedule scope/week, and indexes supporting date/class and date/teacher conflict queries. Ordinary relational unique constraints cannot generally express arbitrary interval non-overlap, so service validation plus transaction isolation/locking or a provider-specific constraint strategy may be required for concurrent writes.

## 12. Change Log and Historical Tracking

The change log should be append-only and reference the planned lesson and weekly schedule. Recommended fields include:

- `Id`.
- `ScheduledLessonId`.
- Operation enum/string such as `TeacherChanged`, `SubstituteTeacher`, `TimeChanged`, `SubjectChanged`, `ClassChanged`, `ExtraLessonAdded`, `RevisionLessonAdded`, `Cancelled`, or `RestoredByExplicitAction` if later approved.
- Timestamp in the chosen school/application time convention.
- Optional reason.
- Optional actor identity/user ID and display snapshot when identity is available.
- Old and new values, either as structured JSON snapshots or explicit typed columns.
- Optional source such as administrator action, generation, or holiday application.

Create the log in the same unit-of-work transaction as the planned-lesson mutation. This prevents a successful schedule change without its required history. Do not retrofit a generic audit framework for this feature. The log records schedule history only and must not rewrite Attendance, Exam, or GradeResult.

## 13. Planned Lessons vs Actual Lesson Execution

The existing `Attendance` entity cannot establish that a lesson was delivered: it has only student, class, date, and status (`Core/Domain/Entities/Attendance.cs:7-17`). It has no subject, teacher, start/end time, planned lesson, or execution identifier. Therefore a planned lesson should not be counted as delivered and should not directly overwrite existing attendance rows.

A separate `ActualLessonExecution` record is recommended as the bridge between planning and delivery. A possible relationship is one planned lesson to zero or one execution record, with execution status and actual times. Attendance integration should be decided separately:

- Preserve existing attendance semantics if current attendance is a daily student/class record.
- If attendance eventually becomes session-based, add a new explicit link or new session-attendance model rather than silently changing the meaning of historical rows.
- Cancelled planned lessons must be excluded from delivered-lesson calculations.
- Schedule edits after the fact must not rewrite historical Attendance, Exam, or GradeResult records.
- Exam and GradeResult remain assessment records; they should not be linked to a planned lesson merely because the subject/class matches.

The current `AttendanceService` validates and persists attendance independently (`Core/Service/AttendanceService/AttendanceService.cs:57-101`), which confirms that execution integration is currently absent.

## 14. API and DTO Design Proposal

The following endpoints are proposals only and are not implemented.

### Base timetable

- `GET /api/timetables` or a scope-specific equivalent: list versions for an academic year/semester.
- `GET /api/timetables/{id}`: return a version and entries.
- `POST /api/timetables`: create a base timetable version with entries.
- `PUT /api/timetables/{id}`: publish/replace a future version according to the final version contract.
- Optional explicit version/publish endpoint if editing and activation must be separate.

### Weekly schedules and lessons

- `GET /api/weekly-schedules?weekStart=...`: retrieve the independent generated week.
- `POST /api/weekly-schedules/generate`: invoke idempotent generation/recovery through the service.
- `GET /api/weekly-schedules/{id}/lessons`: list planned lessons, with class/teacher/date filters.
- `POST /api/weekly-schedules/{id}/lessons`: add an extra or revision lesson.
- `PUT /api/weekly-schedules/{id}/lessons`: update a planned lesson using an `UpdateScheduledLessonDto` containing the ID, consistent with the project’s update DTO convention.
- `DELETE` should be avoided for historical lesson removal unless the final business contract explicitly defines deletion; cancellation is generally safer for history.

### Holidays and history

- `POST /api/holidays`: create a single-date or inclusive range exception, with optional reason.
- `PUT /api/holidays/{id}` or an explicit removal operation: remove/deactivate the exception without restoring cancelled lessons.
- `GET /api/scheduled-lessons/{id}/history`: return change-log entries.

### DTO recommendations

Use `*Dto`, `Create*Dto`, and `Update*Dto` naming under `Shared/Shared`. Include IDs rather than navigation objects in request DTOs. Use `DayOfWeek` or a controlled representation, `TimeOnly` or the project’s chosen time representation, inclusive date fields for holidays, optional reason, and explicit lesson type/status. Response DTOs may include names and academic context for display. Request validation belongs in the service in addition to data annotations.

Controllers should be thin `Task<ActionResult<T>>` delegations through `IServiceManager`, with no conflict logic, manual ModelState handling, authentication changes, or intermediate business decisions. The project instruction file specifies its established response conventions; the implementation should follow the approved project contract rather than inventing alternative response behavior.

## 15. Service and Specification Design Proposal

### Service abstractions

Add scheduling contracts to `Core/ServiceAbstraction` and expose them through `IServiceManager`, following the existing `IAttendanceService`, `IAcademicYearService`, and `ISemesterService` shapes. Suggested responsibilities are:

- `IBaseTimetableService`: create/read/version/publish recurring definitions and validate entries.
- `IWeeklyScheduleService`: get a week, ensure/generate weeks, modify lessons, add extra/revision lessons, and retrieve history.
- `IHolidayService`: create/list/remove calendar exceptions and apply cancellation behavior.
- `ILessonExecutionService`: only if actual execution is included in the implementation phase; keep it separate from planned schedule mutation.

Concrete services should be constructed by `ServiceManger` and use the existing `IUnitOfWork` and AutoMapper conventions. No CQRS, MediatR, FluentValidation, new repository abstraction, or separate teacher-assignment service is recommended.

### Specifications

Suggested specifications under `Core/Service/ConcreteSpecifications` include:

- Base timetable by academic year/semester/version and current applicable version.
- Base entries by timetable ID.
- Weekly schedule by scope and Sunday week start, with source version and lessons included.
- Planned lessons by weekly schedule, date, class, teacher, and excluding a lesson ID for update conflict checks.
- Holiday exceptions overlapping a date range.
- Change history by planned lesson ordered by timestamp.
- Teacher qualification and class-subject membership lookups, either through existing link specifications or focused specifications.

Conflict specifications must express the actual interval predicate and date/class or date/teacher criteria. Avoid checking only equal start times. Specifications should include only the navigation properties required by DTO mapping and validation.

### Exceptions

Use entity-specific `NotFoundException` subclasses accepting only the ID, consistent with `Core/Domain/Exceptions/NotFoundExceptions/AttendanceNotFoundException.cs:5-7`. Potential types include base timetable, base entry, weekly schedule, planned lesson, holiday, and change log/execution where direct lookup is exposed. Use the existing `ValidationException` for invalid relationships, overlaps, holiday blocks, invalid ranges, and academic-context violations.

## 16. Persistence and Data Integrity Considerations

The implementation should add scheduling DbSets and EF relationships in `Infrastructure/Persistence/MainContext.cs` only after the domain design is approved. No migration or database update is part of this audit.

Recommended constraints and indexes:

- Foreign keys from entries, schedules, lessons, holidays, logs, and executions to their owners and referenced academic entities.
- Unique base timetable version per academic scope and version number.
- Unique weekly schedule per defined scope and Sunday week start.
- Index planned lessons by `(Date, ClassId)`, `(Date, TeacherId)`, and weekly schedule ID.
- Index holiday date ranges by academic context and dates.
- Index change logs by planned lesson and timestamp.
- Required date/time checks where supported, while retaining service validation.
- Restrict deletes for historical schedules, lessons, and logs; soft delete is not approved and must not be introduced.

Generation and modifications must use `ExecuteInTransactionAsync`. Conflict checks made concurrently need a defined isolation/locking approach; a transaction alone does not guarantee that two requests will not both pass a pre-insert overlap query. This risk should be tested against the actual SQL Server configuration.

The existing `GenericRepository.GetRepository<TEntity,TKey>()` implementation caches repositories by entity type name (`Infrastructure/Persistence/Contracts/UnitOfWork.cs:37-40`) and supports the required scheduling query shapes. No new repository abstraction is needed.

## 17. Risks and Dependencies

- **Schedule scope ambiguity:** the requirements specify weekly generation but do not explicitly define whether one weekly schedule is school-wide or one per academic year, semester, grade, or class.
- **Time zone:** Sunday boundaries and `DateTime`/`TimeOnly` storage need one documented school time zone.
- **Concurrent conflicts:** service overlap checks require a concurrency strategy in addition to normal validation.
- **Class academic-year migration state:** `Class.AcademicYearId` is nullable and `LegacyAcademicYear` remains mapped, so scheduling must define behavior for classes without a valid year link.
- **Actor identity:** `Admin` and `Teacher` have identity links, but the approved rules do not define how the current administrator is obtained for change logs. The audit recommends nullable actor identity until an approved identity contract exists.
- **Attendance semantics:** existing attendance is daily/class based and not session-based; execution integration requires an approved compatibility design.
- **Holiday overlap/edit policy:** the requirements define cancellation and no restoration, but not whether overlapping holiday records are merged, rejected, or allowed.
- **Generation hosting:** the application’s current `Program.cs` has no generation hosted service. A future implementation must avoid duplicate startup/recurring triggers.
- **Authorization:** authentication/authorization changes are explicitly postponed. Scheduling endpoints must not add `[Authorize]` or role policies in this feature phase.
- **Persistence rollout:** migrations and database changes require a separate approved phase; none were created or applied here.

## 18. Recommended Implementation Sequence

1. Approve schedule scope, time zone, actor identity behavior, holiday overlap policy, and execution/attendance integration boundary.
2. Add domain entities and relationships for base timetable versions, entries, weekly schedules, planned lessons, holidays, and change logs; add actual execution only if approved for the same phase.
3. Add persistence DbSets, indexes, foreign keys, and constraints in `MainContext` without changing unrelated entities.
4. Add DTOs and AutoMapper profiles under existing conventions.
5. Add specifications for scope/version retrieval, week retrieval, conflict checks, holiday overlap, and history.
6. Add domain exceptions and service abstractions; register them through `ServiceManger`.
7. Implement base version lifecycle and active academic-context validation.
8. Implement transactional, idempotent weekly generation with startup/missed-week recovery and duplicate protection.
9. Implement planned-lesson changes, extra/revision lessons, holiday cancellation/blocking, and append-only change logging.
10. Implement actual execution and the approved attendance bridge without rewriting historical records.
11. Add thin controllers and explicit response contracts through `IServiceManager`.
12. Add focused tests for interval boundaries, duplicate generation, downtime catch-up, rollback, holiday ranges/removal, qualification/membership, academic context, history, and post-end-time edits.
13. Build and test the solution, then create migrations only after schema approval. Do not inspect or modify seed data until the project’s final schema is approved.

## 19. Questions Requiring Approval

The core business rules in the request are treated as approved and are not reopened. The following implementation decisions are genuinely unspecified and should be resolved before coding:

1. What is the unique weekly schedule scope: school-wide, academic-year/semester, grade, class, or another scope?
2. Which time zone defines the Sunday boundary and how should daylight-saving transitions, if applicable, be handled?
3. Should base timetable versions be explicitly published with an effective week, or should the latest version at generation time always apply?
4. Should overlapping holiday date ranges be rejected, merged, or allowed as separate records?
5. What identity value should be recorded as the actor when a request is made by an authenticated administrator, and what is the fallback when identity is unavailable?
6. Should an actual execution record be implemented in the same phase as planned scheduling, and should existing daily Attendance remain unchanged or receive a new optional integration path?
7. What operational trigger is preferred for generation: hosted service, startup catch-up plus hosted service, or an externally invoked process using the same idempotent service?
8. What is the approved status vocabulary for planned, cancelled, executed, and non-delivered lessons?

These questions do not authorize changes to authentication, authorization, seed data, migrations, soft delete, or the existing AcademicYear/Semester design.

## 20. Final Audit Status

**Status: Audit complete; implementation missing.**

Verified current state:

- The layered architecture, generic repository, specification pattern, unit of work, AutoMapper, service manager, and centralized exception middleware are implemented and suitable for reuse.
- Teacher qualification is represented by `TeacherSubject`; class-subject membership is represented by `ClassSubject`.
- `ClassTeacher` exists as a general class/teacher link but is not a subject-specific assignment and is not suitable as the actual scheduled assignment source.
- Academic year and semester concepts, including manual active operations and semester-to-year relationship, are implemented.
- Attendance, exams, and grade results exist but are not lesson-session records.
- Timetable, weekly schedule, planned lesson, holiday exception, schedule change log, actual execution, generation service, scheduling DTOs, scheduling specifications, scheduling services, and scheduling controllers are missing.

Restriction verification:

- No separate Teacher Assignment feature is proposed.
- No `ClassSubjectTeacher` entity is proposed.
- No Teacher status field is proposed.
- No authentication or authorization change is proposed.
- No soft-delete change is proposed.
- Seed files and initializer contents were not inspected or modified.
- No migration was created, modified, or applied.
- No database-changing command was performed.
- No existing source file was modified.
- This task was audit-only; the only deliverable is this Markdown report.
