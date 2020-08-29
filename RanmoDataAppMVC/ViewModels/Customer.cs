using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RanmoDB; 

namespace RanmoDataAppMVC.ViewModels
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string ContactPerson { get; set; }
        public string Address { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> TimeStamp { get; set; }

        [RegularExpression(@"^\d+.\d{0,2}", ErrorMessage = "Amount Should be a valid number with 2 decimals. ex: 2345.50 ")]
        public Nullable<decimal> Balance { get; set; }

        public R_Customer ConvertVwModelToDB(Customer customer)
        {
            var c = new R_Customer();
            c.Id = customer.Id;
            c.CustomerName = customer.CustomerName;
            c.ContactPerson = customer.ContactPerson;
            c.Address = customer.Address;
            c.Tel = customer.Tel;
            c.Email = customer.Email;

            return c;
        }

        public Customer ConvertDBToVwModel(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var customerVM = dbEF.R_Customer
            .Where(p => p.Id == Id)
            .Select(q => new Customer
            {
                Id = q.Id,
                CustomerName = q.CustomerName,
                ContactPerson = q.ContactPerson,
                Address = q.Address,
                Tel = q.Tel,
                Email = q.Email
            })
            .FirstOrDefault();

            return customerVM;
        }

        public R_Customer ConvertVwModelToDB(int? Id)
        {
            var dbEF = new RanSanDBEntities();
            var customerEF = dbEF.R_Customer
                                .Where(p => p.Id == Id)
                                .FirstOrDefault();

            return customerEF;
        }


    }










}