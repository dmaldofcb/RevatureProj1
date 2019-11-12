using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Banking.Models;

namespace RevatureProj1.ViewModels
{
    public class CheckingTransferViewModel
    {
        public List<BusinessAccount> BusinessList { get; set; }
        public IEnumerable<CheckingAccount> CheckingList { get; set; }
        //public List<CheckingAccount> CheckingList { get; set; }

        public List<LoanAccount> LoanList { get; set; }

        [Range(.01, Double.PositiveInfinity)]
        [DisplayName("Tranfer Amount")]
        public decimal Amount { get; set; }

    }
}
