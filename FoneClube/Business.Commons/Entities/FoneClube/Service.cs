using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Service
    {
        public int Id { get; set; }
        public Nullable<int> IdPhone { get; set; }
        public Nullable<int> IdService { get; set; }
        public Nullable<bool> bitAtivo { get; set; }
        public string Descricao { get; set; }
        public Nullable<System.DateTime> dteUpdate { get; set; }
        public Nullable<System.DateTime> dteAtivacao { get; set; }
        public Nullable<System.DateTime> dteDesativacao { get; set; }
    }
}
