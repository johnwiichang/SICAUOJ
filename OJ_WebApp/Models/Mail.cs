using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class Mail
    {
        public String Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "E-mail地址过长，请检查。")]
        [RegularExpression(@"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4})$", ErrorMessage = "请输入正确的Email.")]
        [DisplayName("电子邮箱")]
        public String Email { get; set; }

        [Required]
        public String IPAdd { get; set; }

        [Required]
        public DateTime CreateTime { get; set; } = DateTime.Now;
    }
}