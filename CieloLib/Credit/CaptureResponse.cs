using Newtonsoft.Json;
using System;
using System.Runtime.CompilerServices;

namespace CieloLib.Credit
{
	public class CaptureResponse : CieloResponse
    {

		public CaptureResponse()
		{
		}

        public int Status { get; set; }
        public string Tid { get; set; }
        public string ProofOfSale { get; set; }
        public int ReturnCode { get; set; }
        public string ReturnMessage { get; set; }
        public CieloLib.Domain.Link[] Links { get; set; }
    }
}