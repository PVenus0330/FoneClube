using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Domain
{
	public class Link
	{
		[JsonProperty("Href")]
		public string Href
		{
			get;
			set;
		}

		[JsonProperty("Method")]
		public string Method
		{
			get;
			set;
		}

		[JsonProperty("Rel")]
		public string Rel
		{
			get;
			set;
		}

		public Link()
		{
		}
	}
}