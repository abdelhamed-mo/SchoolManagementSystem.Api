using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
	public class Subject : BaseEntity<int>
	{
		public string Name { get; set; } 

		public ICollection<TeacherSubject> TeacherSubjects { get; set; } 
		public ICollection<ClassSubject> ClassSubjects { get; set; } 
		public ICollection<Exam> Exams { get; set; }
		public ICollection<BaseTimetableEntry> BaseTimetableEntries { get; set; } = new List<BaseTimetableEntry>();
		public ICollection<ScheduledLesson> ScheduledLessons { get; set; } = new List<ScheduledLesson>();
	}
}
