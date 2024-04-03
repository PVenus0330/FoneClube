using FoneClube.Business.Commons.Entities.FoneClube.email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.flag
{
    public class GenericFlag
    {
        public int Id { get; set; }
        public Nullable<int> IdPhone { get; set; }
        public Nullable<System.DateTime> Registro { get; set; }
        public Nullable<System.DateTime> Update { get; set; }
        public Nullable<bool> PendingInteraction { get; set; }
        public string Description { get; set; }
        public Nullable<int> IdFlagType { get; set; }
        public Nullable<int> IdPerson { get; set; }
        public int? PlanId { get; set; }

        public FullEmail FullEmail { get; set; }
    }

    public class FlagResponse
    {
        public bool FlagSuccess { get; set; }
        public bool EmailSuccess { get; set; }
    }
}
