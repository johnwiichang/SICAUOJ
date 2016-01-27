using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace OJ_WebApp.Models
{
    public class Compiler
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(20, ErrorMessage = "名字最长允许20个字。")]
        [DisplayName("名称")]
        public String Name { get; set; }

        [Required]
        public Boolean isForbidden { get; set; } = false;

        [Required]
        public Boolean isScript { get; set; } = false;

        public String CodeFormat { get; set; }

        public string ExecutionFormat { get; set; }

        public string CompilerPath { get; set; }

        public string CompilerArgs { get; set; }

        public string RunnerPath { get; set; }

        public string RunnerArgs { get; set; }
    }
}