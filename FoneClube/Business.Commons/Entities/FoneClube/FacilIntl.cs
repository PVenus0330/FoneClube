using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class FacilActionRequest
    {
        public int Id { get; set; }
        public string Phone { get; set; }
        public string ICCID { get; set; }
        public string Comment { get; set; }
        public bool IsRefund { get; set; }
        public bool IsRevokeRefund { get; set; }
        public bool IsResellPoolAndBlock { get; set; }
        public bool IsResellPoolReset { get; set; }
        public bool IsRequestRefundFromContel { get; set; }
        public bool IsRefundGrantedByContel { get; set; }
        public bool IsRemoveFromPool { get; set; }
        public bool IsReset { get; set; }
    }

    public class FacilActionResponse
    {
        public int Id { get; set; }
        public bool IsRefundSuccess { get; set; }
        public bool IsRevokeRefundSuccess { get; set; }
        public bool IsResellPoolSuccess { get; set; }
        public bool IsResellPoolResetSuccess { get; set; }
        public bool IsRequestRefundFromContelSuccess { get; set; }
        public bool IsRefundGrantedByContelSuccess { get; set; }
        public bool IsResetSuccess { get; set; }
        public bool IsRemoveFromPoolSuccess { get; set; }
        public List<string> Info { get; set; }
    }
}
