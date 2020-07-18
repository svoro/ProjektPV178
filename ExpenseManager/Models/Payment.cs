using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExpenseManager.Models
{
    public class Payment
    {

        public int Id { get; set; }
        private string _date;
        [Required]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"\d{2}\.\d{2}\.\d{4}", ErrorMessage = "Date must be in DD.MM.YYYY format")]
        public string Date
        {
            get { return _date; }
            set
            {
                if (value == null)
                {
                    _date = DateTime.Now.ToString("dd.MM.yyyy");
                }
                try
                {
                    DateTime.ParseExact(value, "dd.MM.yyyy", null);
                }
                catch
                {
                    _date = DateTime.Now.ToString("dd.MM.yyyy");
                    return;
                }
                _date = value;
            }
        }
        [Required]
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        public int CardId { get; set; }
        public virtual Card Card { get; set; }



        override
        public string ToString()
        {
            return this.Name + "," + this.Date + "," + this.Price.ToString().Replace(',','.');
        }
    }
}