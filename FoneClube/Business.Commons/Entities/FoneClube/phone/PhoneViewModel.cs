using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoneClube.Business.Commons.Entities.FoneClube.phone
{
	public class PhoneViewModel
	{
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PersonPhoneId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PersonId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int OperatorId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PlanId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int ?StatusId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int PhoneNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int DDNumber { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int UsoLinha { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string PersonName { get; set; }
		public string NickName { get; set; }
		public string Email { get; set; }
		public string DocumentNumber { get; set; }
		public string Register { get; set; }
		public string Born { get; set; }
		public string Gender { get; set; }
		public string Phone { get; set; }
		public string CompletePhone { get; set; }
		public string DisplayPhone { get; set; }
		public string OperatorName { get; set; }
		public string MasterOperatorName { get; set; }
		public string PlanDescription { get; set; }

		public string EntradaLinha { get; set; }
		public string SaidaLinha { get; set; }
		public string StatusName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal PlanCost { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int AmoutPrecoVip { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal AdditionServiceCost { get; set; }


        public string CodigoCliente { get; set; }

        public string RazaoSocial { get; set; }

        public string CCID { get; set; }


        public bool? IsPortability { get; set; }
		public bool IsActive { get; set; }
		public bool IsPhoneClube { get; set; }
		public bool Dono { get; set; }
		public bool IsPrecoVip { get; set; }
		public bool IsDelete { get; set; }
		public bool BonusConceded { get; set; }



		public int PlanOptionId { get; set; }
		public List<PhoneServiceViewModel>  PhoneServices { get; set; }

        //deprecated we do not use any kind of business rule or client side properties here, it was Jas development
		#region Calcualte Properties

		public string DisplayPlanoOperador
		{
			get
			{
				return string.Format("{0} {1}", OperatorName, MasterOperatorName);
			}
		}

		public string DisplayOperator 
		{
			get
			{
				if (OperatorId == 1)
					return "A";
				return "E";
			}
		}

		public string DisplayOperatorCss
		{
			get
			{
				if (OperatorId == 1)
					return "row-green";
				return "row-red";
			}
		}

		public decimal CalculatePlanCost
		{
			get
			{
				decimal value = 0;
				if (PlanCost > 0)
				{
					value = ((PlanCost + AdditionServiceCost) / 100);
				}
				return value;
			}
		}

		public string DisplayPlanCost
		{
			get
			{
				var value = "";
				if (CalculatePlanCost > 0)
				{
					value = CalculatePlanCost.ToString("R$ 0.00");
				}
				return value;
			}
		}

		public decimal CalculateAmoutPrecoVip
		{
			get
			{
				decimal value = 0;
				if (AmoutPrecoVip > 0)
				{
					value = ((AmoutPrecoVip + AdditionServiceCost) / 100);
				}
				return value;
			}
		}

		public string DisplayAmoutPrecoVip
		{
			get
			{
				var value = "";
				if (AmoutPrecoVip > 0)
				{
					value = ((AmoutPrecoVip + AdditionServiceCost) / 100).ToString("R$ 0.00");
				}
				return value;
			}
		}

		public string DisplayUsoLinha
		{

			get
			{
                if (UsoLinha == -1)
                    return "Indefinido";

                if (UsoLinha == 1)
                    return "sim";
                else
					return "não";
			}
		}

        



        #endregion

    }
}
