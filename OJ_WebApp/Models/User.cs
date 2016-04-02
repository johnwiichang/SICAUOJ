using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class User
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "名字最长允许50个字。")]
        [DisplayName("名字")]
        public String Name { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "E-mail地址过长，请检查。")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4})$", ErrorMessage = "请输入正确的Email.")]
        [DisplayName("电子邮箱")]
        public String Email { get; set; }

        [Required]
        [DisplayName("密码")]
        public String Password { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        [Required]
        public Boolean isLost { get; set; } = false;

        public String Verification { get; set; }

        public virtual ICollection<Group> Groups { get; set; }
        public virtual Compiler Compiler { get; set; }

        [NotMapped]
        public int Solved { get; set; }

        [NotMapped]
        public double SpanOfSolved { get; set; }

        [NotMapped]
        public int Err { get; set; }

        [NotMapped]
        public double Total { get; set; }
    }
}