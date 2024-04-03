using System;
using FoneClube.Business.Commons.Entities.FoneClube.email;

namespace FoneClube.Business.Commons.Entities.FoneClube
{
    public class Flag
    {
        public enum Type
        {
            UpgradeLinha = 1,
            DowngradeLinha = 2
        }

        public int Id { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool PendingInteraction { get; set; }
        public string InteractionDescription { get; set; }
        public bool PhoneFlag { get; set; }

        public int IdType { get; set; }
        public string TypeDescription { get; set; }
        public FullEmail FullEmail { get; set; }
    }
}