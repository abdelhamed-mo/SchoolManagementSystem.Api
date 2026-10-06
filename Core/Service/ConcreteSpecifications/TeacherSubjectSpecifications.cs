using System;
using System.Collections.Generic;
using System.Text;

namespace Service.ConcreteSpecifications
{
	public class TeacherSubjectSpecifications : Specifications<TeacherSubject>
	{
		public TeacherSubjectSpecifications(int tId)
			: base(ts => ts.TeacherId == tId)
		{
			AddInclude(s => s.Subject);
		}
	}
}
