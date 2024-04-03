using System;
using System.Runtime.Serialization;

namespace CieloLib.Eft.Domain
{
	public enum ItemCreditType
	{
		[EnumMember(Value="D")]
		Debit,
		[EnumMember(Value="C")]
		Credit
	}
}