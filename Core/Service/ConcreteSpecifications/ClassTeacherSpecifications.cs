using System;
using System.Collections.Generic;
using System.Text;

namespace Service.ConcreteSpecifications
{
	public class ClassTeacherSpecifications : Specifications<ClassTeacher>
	{
		public ClassTeacherSpecifications(int tId)
			: base(ct => ct.TeacherId == tId)
		{
		}
	}
}
