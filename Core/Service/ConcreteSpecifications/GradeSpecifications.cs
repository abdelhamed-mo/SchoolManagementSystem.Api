using System;
using System.Collections.Generic;
using System.Text;

namespace Service.ConcreteSpecifications
{
	public class GradeSpecifications : Specifications<Grade>
	{
		public GradeSpecifications(string gradeName)
			: base(g => g.Name.ToUpper() == gradeName.ToUpper())
		{

		}

		public GradeSpecifications(Shared.Params.GradesParams _params, bool applyPagination = true)
			: base(g => string.IsNullOrWhiteSpace(_params.Search) || g.Name.ToUpper().Contains(_params.Search!.ToUpper().Trim()))
		{
			// GradeDto requires only Id and Name; avoid unnecessary includes
			if (applyPagination)
				ApplyPagination(_params.PageSize, _params.PageIndex);
		}
	}
}
