using System;

namespace CieloLib.Credit.Domain
{
	public enum CieloCreditRequestType
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