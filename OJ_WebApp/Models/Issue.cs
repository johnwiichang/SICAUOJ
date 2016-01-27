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
    public class Issue
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "标题最长允许50个字。")]
        [DisplayName("标题")]
        public String Title { get; set; }

        [Required]
        [DisplayName("题目")]
        public String Content { get; set; }

        [Required]
        [DisplayName("示例输入")]
        [AllowHtml]
        public String Input { get; set; }

        [Required]
        [DisplayName("示例输出")]
        [AllowHtml]
        public String Output { get; set; }

        [Required]
        [DisplayName("编译时间")]
        public Int32 ComplieTime { get; set; } = 3000;

        [Required]
        [DisplayName("运行时间")]
        public Int32 RunTime { get; set; } = 3000;

        [Required]
        [DisplayName("内存限制")]
        public Int32 PrivateMemorySize { get; set; } = 10485760;

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public virtual ICollection<Lib> Libs { get; set; }
        public virtual ICollection<Task> Tasks { get; set; }
    }
}