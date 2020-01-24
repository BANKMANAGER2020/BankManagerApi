using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TheBankManagerAPI.Models
{
    public class Dashboard
    {
        public List<Transaction> AllCredits { get; set; }
        public List<Transaction> AllDebits { get; set; }
    }
}