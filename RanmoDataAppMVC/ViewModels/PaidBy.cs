using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RanmoDB;

namespace RanmoDataAppMVC.ViewModels
{
    public class PaidBy
    {
        public int Id { get; set; }
        public string Paid_By { get; set; }

        public R_PaidBy ConvertVwModelToDB(PaidBy paidBy)
        {
            var ps = new R_PaidBy();
            ps.Id = paidBy.Id;
            ps.PaidBy = paidBy.Paid_By;

            return ps;
        }

        public PaidBy ConvertDBToVwModel(int? Id)
        {
            RanSanDBEntities dbEF = new RanSanDBEntities();
            var PaidByVM = dbEF.R_PaidBy
            .Where(p => p.Id == Id)
            .Select(q => new PaidBy { Id = q.Id, Paid_By = q.PaidBy })
            .FirstOrDefault();

            return PaidByVM;
        }

        public R_PaidBy ConvertVwModelToDB(int? Id)
        {
            RanSanDBEntities dbEF = new RanSanDBEntities();
            var PaidByEF = dbEF.R_PaidBy
                                .Where(p => p.Id == Id)
                                .FirstOrDefault();

            return PaidByEF;
        }
    }



}