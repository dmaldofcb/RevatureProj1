using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Banking.Models
{
    public class LoanAccount
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]

        [Range(.01, Double.PositiveInfinity)]
        [DisplayName("Loan Amount")]
        public decimal Balance { get; set; }
        [DisplayName("Account Status")]
        public bool IsOpen { get; set; }

        [DisplayFormat(DataFormatString = "{0:P2}")]
        [DisplayName("Interest Rate")]
        public decimal InterestRate { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }

        public string UserID { get; set; }

    }
}
