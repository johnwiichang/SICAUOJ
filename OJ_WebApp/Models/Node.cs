using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class Node
    {
        [Key]
        public String Name { get; set; }

        public DateTime CompilerLastReport { get; set; } = DateTime.Now.AddYears(-100);

        public DateTime RunnerLastReport { get; set; } = DateTime.Now.AddYears(-100);

        [Required]
        public int MaxTask { get; set; } = 2;

        [Required]
        public String WorkDir { get; set; } = @"D:\";

        [Required]
        public Boolean inUse { get; set; } = false;

        [Required]
        public int Heartbeat { get; set; } = 5000;

        public int Compiling { get; set; } = 0;

        public int Running { get; set; } = 0;
    }
}