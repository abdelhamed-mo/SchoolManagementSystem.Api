using Domain.Entities;
using Domain.Exceptions;
using Domain.Exceptions.NotFoundExceptions;
using Service.ConcreteSpecifications;
using Shared;

namespace Service.BaseTimetableService
{
    public class BaseTimetableService(IUnitOfWork unitOfWork, IMapper mapper) : IBaseTimetableService
    {
        public Task<BaseTimetableDto> CreateAsync(CreateBaseTimetableDto dto)
            => SaveAsync(dto.ClassId, dto.SchoolDayStartTime, dto.PeriodDurationsMinutes, dto.BreakAfterPeriodNumber, dto.BreakDurationMinutes, dto.Entries, null);

        /*{
            var classId = dto.ClassId;
            var breakAfter = dto.BreakAfterPeriodNumber;
            var breakDuration = dto.BreakDurationMinutes;
            var durations = dto.PeriodDurationsMinutes;
            var entryDtos = dto.Entries;

            var classEntity = await GetClassAsync(classId);
            var durationList = durations?.ToList() ?? [];
            if (durationList.Count == 0 || durationList.Any(duration => duration <= 0))
                throw new ValidationException(new[] { "At least one positive period duration is required." });
            if (breakAfter.HasValue && (breakAfter.Value < 1 || breakAfter.Value >= durationList.Count))
                throw new ValidationException(new[] { "The break position must be between two periods." });
            if (breakAfter.HasValue && (!breakDuration.HasValue || breakDuration.Value <= 0))
                throw new ValidationException(new[] { "A configured break must have a positive duration." });

        }*/
        public Task<BaseTimetableDto> UpdateAsync(UpdateBaseTimetableDto dto)
            => SaveAsync(dto.ClassId, dto.SchoolDayStartTime, dto.PeriodDurationsMinutes, dto.BreakAfterPeriodNumber, dto.BreakDurationMinutes, dto.Entries, dto.Id);

        //public async Task<BaseTimetableDto> GetByClassAsync(int classId)
        //{
        //    await GetClassAsync(classId);
        //    var timetable = await unitOfWork.GetRepository<BaseTimetable, int>().GetAsync(new BaseTimetableSpecifications(classId));
        //    return timetable is null ? throw new BaseTimetableNotFoundException(classId) : mapper.Map<BaseTimetableDto>(timetable);
        //}
        public async Task<BaseTimetableDto> GetByClassAsync(int classId)
        {
            await GetClassAsync(classId);

            var timetableRepository =
                unitOfWork.GetRepository<BaseTimetable, int>();

            var timetable = await timetableRepository.GetAsync(
                new BaseTimetableSpecifications(classId));

            if (timetable is null)
                throw new BaseTimetableNotFoundException(classId);

            var dto = mapper.Map<BaseTimetableDto>(timetable);

            var entryIds = timetable.Entries
                .Select(entry => entry.Id)
                .ToList();

            if (entryIds.Count == 0)
                return dto;

            var linkRepository =
                unitOfWork.GetRepository<BaseTimetableEntryPeriod, int>();

            var links = await linkRepository.GetAllAsync(
                new BaseTimetableEntryPeriodSpecifications(entryIds));

            var periodNumbers = timetable.Periods
                .Where(period => !period.IsBreak)
                .ToDictionary(
                    period => period.Id,
                    period => period.PeriodNumber!.Value);

            var periodsByEntry = links
                .GroupBy(link => link.BaseTimetableEntryId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Where(link => periodNumbers.ContainsKey(link.BaseTimetablePeriodId))
                        .Select(link => periodNumbers[link.BaseTimetablePeriodId])
                        .OrderBy(number => number)
                        .ToList());

            foreach (var entryDto in dto.Entries)
            {
                entryDto.PeriodNumbers = periodsByEntry.TryGetValue(
                    entryDto.Id,
                    out var numbers)
                        ? numbers
                        : [];
            }

            dto.Periods = dto.Periods
                .OrderBy(period => period.SequenceNumber)
                .ToList();

            return dto;
        }
        private async Task<BaseTimetableDto> SaveAsync(int classId, TimeOnly start, IEnumerable<int> durations, int? breakAfter, int? breakDuration, IEnumerable<CreateBaseTimetableEntryDto> entryDtos, int? id)
        {
            var classEntity = await GetClassAsync(classId);
            var durationList = durations?.ToList() ?? [];
            if (durationList.Count == 0 || durationList.Any(duration => duration <= 0))
                throw new ValidationException(new[] { "At least one positive period duration is required." });
            if (breakAfter.HasValue && (breakAfter.Value < 1 || breakAfter.Value >= durationList.Count))
                throw new ValidationException(new[] { "The break position must be between two periods." });
            if (breakAfter.HasValue && (!breakDuration.HasValue || breakDuration.Value <= 0))
                throw new ValidationException(new[] { "A configured break must have a positive duration." });

            var repository = unitOfWork.GetRepository<BaseTimetable, int>();
            var timetable = id.HasValue
                ? await repository.GetAsync(new BaseTimetableSpecifications(id.Value, true))
                : await repository.GetAsync(new BaseTimetableSpecifications(classId, true));
            if (id.HasValue && timetable is null)
                throw new BaseTimetableNotFoundException(id.Value);
            if (!id.HasValue && timetable is not null)
                throw new ValidationException(new[] { "The class already has a base timetable. Use the update operation." });

            timetable ??= new BaseTimetable { ClassId = classId, Class = classEntity, CreatedAt = DateTime.UtcNow };
            timetable.SchoolDayStartTime = start;
            timetable.BreakAfterPeriodNumber = breakAfter;
            timetable.BreakDurationMinutes = breakDuration;
            timetable.ModifiedAt = DateTime.UtcNow;
            var entryPeriodRepository =
                unitOfWork.GetRepository<BaseTimetableEntryPeriod, int>();

            var entryRepository =
                unitOfWork.GetRepository<BaseTimetableEntry, int>();

            var periodRepository =
                unitOfWork.GetRepository<BaseTimetablePeriod, int>();

            var oldEntries = timetable.Entries.ToList();
            var oldPeriods = timetable.Periods.ToList();

            // Delete entry-period links first
            //foreach (var entry in oldEntries)
            //{
            //    foreach (var link in entry.Periods.ToList())
            //        entryPeriodRepository.Delete(link);
            //}
            var oldEntryIds = oldEntries.Select(entry => entry.Id).ToList();

            var oldLinks = await entryPeriodRepository.GetAllAsync(
                new BaseTimetableEntryPeriodSpecifications(oldEntryIds));

            foreach (var link in oldLinks)
                entryPeriodRepository.Delete(link);

            //await unitOfWork.SaveChangesAsync();

            // Delete old entries
            foreach (var entry in oldEntries)
            {

                entryRepository.Delete(entry);
                timetable.Entries.Remove(entry);
            }


            //await unitOfWork.SaveChangesAsync();

            // Delete old periods
            foreach (var period in oldPeriods)
            {
                periodRepository.Delete(period);
                timetable.Periods.Remove(period);
            }

            //await unitOfWork.SaveChangesAsync();

            var current = start;
            var sequenceNumber = 1;

            for (var index = 0; index < durationList.Count; index++)
            {
                var number = index + 1;

                timetable.Periods.Add(new BaseTimetablePeriod
                {
                    SequenceNumber = sequenceNumber++,
                    PeriodNumber = number,
                    StartTime = current,
                    EndTime = current.AddMinutes(durationList[index]),
                    IsBreak = false
                });

                current = current.AddMinutes(durationList[index]);

                if (breakAfter == number)
                {
                    timetable.Periods.Add(new BaseTimetablePeriod
                    {
                        SequenceNumber = sequenceNumber++,
                        PeriodNumber = null,
                        StartTime = current,
                        EndTime = current.AddMinutes(breakDuration!.Value),
                        IsBreak = true
                    });

                    current = current.AddMinutes(breakDuration.Value);
                }
            }


            var normalPeriods = timetable.Periods.Where(period => !period.IsBreak).ToDictionary(period => period.PeriodNumber!.Value);
            foreach (var dto in entryDtos ?? [])
            {
                if (dto.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday)
                    throw new ValidationException(new[] { "Base timetable lessons may only use Sunday through Thursday." });
                var numbers = dto.PeriodNumbers?.Distinct().OrderBy(number => number).ToList() ?? [];
                if (numbers.Count == 0 || numbers.Any(number => !normalPeriods.ContainsKey(number)))
                    throw new ValidationException(new[] { "Every lesson period must be a valid non-break period." });
                var entry = new BaseTimetableEntry { DayOfWeek = dto.DayOfWeek, SubjectId = dto.SubjectId, TeacherId = dto.TeacherId };
                foreach (var number in numbers)
                    entry.Periods.Add(new BaseTimetableEntryPeriod { BaseTimetableEntry = entry, BaseTimetablePeriod = normalPeriods[number] });
                timetable.Entries.Add(entry);
            }

            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                if (timetable.Id == 0) await repository.AddAsync(timetable); else repository.Update(timetable);
                await unitOfWork.SaveChangesAsync();
            });
            return mapper.Map<BaseTimetableDto>(timetable);
        }

        private async Task<Class> GetClassAsync(int id)
            => await unitOfWork.GetRepository<Class, int>().GetAsync(new ClassSpecifications(id))
                ?? throw new ClassNotFoundException(id);
    }
}
