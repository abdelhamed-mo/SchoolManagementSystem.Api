using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
	public class Class : BaseEntity<int>
	{
		public string Name { get; set; } 
		public int? AcademicYearId { get; set; }
		public AcademicYear? AcademicYear { get; set; }

		// Retained until the legacy seed data can be safely backfilled to AcademicYearId.
		[System.Text.Json.Serialization.JsonPropertyName("AcademicYear")]
		public string LegacyAcademicYear { get; set; } = string.Empty;

		public int GradeId { get; set; }
		public Grade Grade { get; set; }

		public ICollection<Student> Students { get; set; }
		public ICollection<ClassSubject> ClassSubjects { get; set; }
		public ICollection<ClassTeacher> ClassTeachers { get; set; }
		public ICollection<Attendance> Attendances { get; set; }
		public ICollection<Exam> Exams { get; set; }
		public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
		public ICollection<BaseTimetable> BaseTimetables { get; set; } = new List<BaseTimetable>();
		public ICollection<WeeklySchedule> WeeklySchedules { get; set; } = new List<WeeklySchedule>();
	}
}
