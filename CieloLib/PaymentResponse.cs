using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CieloLib
{
    public class PaymentResponse<T>
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public Exception Exception { get; set; }
        public T Response { get; set; }
    }
}
