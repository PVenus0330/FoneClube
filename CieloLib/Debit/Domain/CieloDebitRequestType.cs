using System;

namespace CieloLib.Debit.Domain
{
	public enum CieloDebitRequestType
	{
		Authorization,
		Verify,
		Capture,
		Sale,
		Void,
		Refund,
		Credit,
		Force,
		Tokenization,
		BatchClose
	}
}