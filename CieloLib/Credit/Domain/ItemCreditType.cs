using System;
using System.Runtime.Serialization;

namespace CieloLib.Credit.Domain
{
	public enum ItemCreditType
	{
		[EnumMember(Value="D")]
		Debit,
		[EnumMember(Value="C")]
		Credit
	}
}