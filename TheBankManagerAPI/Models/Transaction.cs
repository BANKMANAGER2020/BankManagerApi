using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBankManagerAPI.Models
{
    public class Transaction
    {
        public double amounts { get; set; }

        public double balance { get; set; }

        public string transactionDate { get; set; }

        public string descrption { get; set; }

        public string transactionType { get; set; }

        public string BankCode { get; set; }

        public string TransChannelDescription { get; set; }


    }
}
