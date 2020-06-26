using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ExpenseManager.Models
{
    public class Card
    {
        public int Id { get; set; }

        [DataType(DataType.Currency)]
        public double Balance { get; set; }
        public virtual ApplicationUser Owner { get; set; }
        [Required]
        public string OwnerId { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
    }
}