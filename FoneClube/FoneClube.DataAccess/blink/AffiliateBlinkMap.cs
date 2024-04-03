using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.DataAccess.blink
{
    public partial class AffiliateBlinkMap : BaseEntity
    {
        public int AffiliateId { get; set; }

        public int CustomerId { get; set; }

        public string BLinkUrl { get; set; }

        public int BLinkId { get; set; }

    }
}
