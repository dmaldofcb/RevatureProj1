using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Banking.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        [DisplayName("Transaction Type")]
        public string Type { get; set; }
        [DisplayFormat(DataFormatString = "{0:c}")]
        [DisplayName("Transaction Amount")]
        public decimal Amount { get; set; }
        [Required]
        [DisplayName("Account Number")]
        public string AccountNum { get; set; }

        public string UserID { get; set; }

        [DisplayName("Date/Time Transaction")]
        [DisplayFormat( DataFormatString = "{0:MM/dd/yyyy hh:mm:ss}")]

        public DateTime TimeTransaction { get; set; }

    }
}
