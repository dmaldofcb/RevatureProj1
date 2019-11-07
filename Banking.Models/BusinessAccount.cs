using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Banking.Models
{
    public class BusinessAccount
    {
        public int Id { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]

        [Range(0, Double.PositiveInfinity)]
        public decimal Balance { get; set; }

        [DisplayName("Overdraft Facility")]
        [DisplayFormat(DataFormatString = "{0:c}")]

        public decimal Overdraft { get; set; }
        [DisplayName("Account Status")]
        public bool IsOpen { get; set; }

        [DisplayFormat(DataFormatString = "{0:P2}")]
        [DisplayName("Interest Rate Overdraft")]
        public decimal InterestRate { get; set; }

        [DisplayName("Account Number")]
        public string AccountNumber { get; set; }

        // public virtual UserBanking User { get; set; }

        public string UserID { get; set; }
    }
}
