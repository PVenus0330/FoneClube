using FoneClube.Business.Commons.Entities;
using FoneClube.Business.Commons.Entities.FoneClube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace FoneClube.DataAccess
{
    public class AccountAccess
    {
        public List<Plan> GetPlans()
        {
            var plans = new List<Plan>();

            using (var ctx = new FoneClubeContext())
            {
                plans.Add(new Plan
                {
                    Id = 0,
                    Description = "Select",
                    IdOperator = 1000
                });
                foreach (var plan in ctx.tblPlansOptions.Where(p => p.intBitActive == true).ToList())
                {
                    //se ficar lento colocar Enum e não colher de tabela
                    var operatorName = ctx.tblOperadoras.FirstOrDefault(o => o.intIdOperator == plan.intIdOperator).txtName;
                    
                    plans.Add(new Plan
                    {
                        Id = plan.intIdPlan,
                        IdOperator = plan.intIdOperator,
                        Value = plan.intCost,
                        Description = string.Format("{0} - {1}", plan.txtDescription, operatorName)
                    });
                }
                plans = plans.OrderByDescending(p => p.IdOperator).ToList();
            }
            return plans;
        }

        public List<Plan> GetPlansById(int id)
        {
            var plans = new List<Plan>();

            using (var ctx = new FoneClubeContext())
            {
                foreach (var plan in ctx.tblPlansOptions.Where(p => p.intBitActive == true && p.intIdOperator == id).ToList())
                {
                    //se ficar lento colocar Enum e não colher de tabela
                    var operatorName = ctx.tblOperadoras.FirstOrDefault(o => o.intIdOperator == plan.intIdOperator).txtName;

                    plans.Add(new Plan
                    {
                        Id = plan.intIdPlan,
                        IdOperator = plan.intIdOperator,
                        Value = plan.intCost,
                        Description = string.Format("{0} - {1}", plan.txtDescription, operatorName)
                    });

                    plans.OrderBy(p => p.IdOperator);
                }
            }
            return plans;
        }

        public List<Operator> GetOperators()
        {
            var operators = new List<Operator>();
            using (var ctx = new FoneClubeContext())
            {
                foreach (var item in ctx.tblOperadoras.ToList())
                    operators.Add(new Operator { Id = item.intIdOperator, Name = item.txtName });

            }
            return operators;
        }
    }
}
