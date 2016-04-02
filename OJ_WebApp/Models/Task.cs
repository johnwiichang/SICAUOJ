using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OJ_WebApp.Models
{
    public class Task
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(6000000, ErrorMessage = "答案最长允许60000个字。")]
        [DisplayName("答案")]
        [AllowHtml]
        public String Answer { get; set; }

        public String Reply { get; set; }

        public double? Compiletime { get; set; }

        public double? Runtime { get; set; }

        public long? Mem { get; set; }

        [Required]
        public Boolean isPass { get; set; } = false;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        public virtual User Owner { get; set; }

        [Required]
        public virtual Compiler Compiler { get; set; }

        [Required]
        public virtual Issue Issue { get; set; }

        [Required]
        [ConcurrencyCheck]
        public String status { get; set; } = "WC";

        public String Handler { get; set; }

        [NotMapped]
        public String result
        {
            get
            {
                switch (status)
                {
                    case "WA":
                        return Resources.Language.WrongAnswer;
                    case "AC":
                        return Resources.Language.Accept;
                    case "PE":
                        return Resources.Language.FormatError;
                    case "TLE":
                        return Resources.Language.OutOfTimeLimitation;
                    case "MLE":
                        return Resources.Language.OutOfMemory;
                    case "CS":
                        return Resources.Language.Compiled;
                    case "CE":
                        return Resources.Language.CompileError;
                    case "WC":
                        return Resources.Language.InQueue;
                    case "CA":
                        return Resources.Language.CompilerAccept;
                    case "RE":
                        return Resources.Language.RuntimeError;
                    case "RA":
                        return Resources.Language.RunnerAccept;
                    default:
                        return Resources.Language.SystemError;
                }
            }
        }
    }
}