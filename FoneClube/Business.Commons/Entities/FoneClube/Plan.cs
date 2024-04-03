using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Plan
    {
        public int Id { get; set; }
        public int IdOperator { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public bool? Active { get; set; }
        public int Cost { get; set; }
    }

    public class EditParam
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
