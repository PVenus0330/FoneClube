using System;

namespace CieloLib.Eft.Domain
{
	public enum EftRequestType
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