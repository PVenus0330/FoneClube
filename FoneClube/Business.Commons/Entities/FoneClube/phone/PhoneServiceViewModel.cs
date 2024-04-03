using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.phone
{
	public class PhoneServiceViewModel
	{
		public int ServiceId { get; set; }
		public string ServiceName { get; set; }
		public int? PersonPhoneId { get; set; }
		public DateTime? ActiveDate { get; set; }
		public DateTime? DeActiveDate { get; set; }
	}
}
