using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace CrossBorders.MVC.Models
{
    public class HistoryStat
    {
        [HiddenInput]
        public string Guid { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        
        public int Count { get; set; }
    }
}