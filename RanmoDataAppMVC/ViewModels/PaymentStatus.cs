using RanmoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RanmoDataAppMVC.ViewModels
{
    public class PaymentStatus
    {
        public int Id { get; set; }
        public string Payment_Status { get; set; }

        public R_PaymentStatus ConvertVwModelToDB(PaymentStatus paymentStatus)
        {
            var ps = new R_PaymentStatus();
            ps.Id = paymentStatus.Id;
            ps.PaymentStatus = paymentStatus.Payment_Status;

            return ps;
        }

        public PaymentStatus ConvertDBToVwModel(int? Id)
        {
            RanSanDBEntities dbEF = new RanSanDBEntities();
            var paymentStatusVM = dbEF.R_PaymentStatus
            .Where(p => p.Id == Id)
            .Select(q => new PaymentStatus { Id = q.Id, Payment_Status = q.PaymentStatus })
            .FirstOrDefault();

            return paymentStatusVM;
        }

        public R_PaymentStatus ConvertVwModelToDB(int? Id)
        {
            RanSanDBEntities dbEF = new RanSanDBEntities();
            var paymentStatusEF = dbEF.R_PaymentStatus
                                .Where(p => p.Id == Id)
                                .FirstOrDefault();

            return paymentStatusEF;
        }

    }
}