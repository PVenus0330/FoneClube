using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit.Domain
{
	public class Customer
	{
		[JsonProperty("Email")]
		public string Email
		{
			get;
			set;
		}

		[JsonProperty("FirstName")]
		public string FirstName
		{
			get;
			set;
		}

		[JsonProperty("Id")]
		public string Id
		{
			get;
			set;
		}

		[JsonProperty("LastName")]
		public string LastName
		{
			get;
			set;
		}

		[JsonProperty("Name")]
		public string Name
		{
			get;
			set;
		}

		[JsonProperty("Phone")]
		public string Phone
		{
			get;
			set;
		}
        public string Status { get; set; }

        public Customer()
		{
		}
	}
}