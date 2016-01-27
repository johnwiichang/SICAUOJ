using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class Game
    {
        public Int32 Id { get; set; }

        [Required]
        [DisplayName("比赛名称")]
        [MaxLength(50, ErrorMessage = "比赛名称最长允许50个字")]
        public String Name { get; set; }

        [Required]
        [DisplayName("开始时间")]
        public DateTime BeginTime { get; set; }

        [Required]
        [DisplayName("结束时间")]
        public DateTime EndTime { get; set; }

        [Required]
        [DisplayName("比赛题库")]
        public virtual Lib GameLib { get; set; }
    }
}