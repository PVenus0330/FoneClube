using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Debit.Domain
{
	public class Track
	{
		[JsonProperty("duration")]
		public long Duration
		{
			get;
			set;
		}

		[JsonProperty("name")]
		public string Name
		{
			get;
			set;
		}

		public Track()
		{
		}
	}
}