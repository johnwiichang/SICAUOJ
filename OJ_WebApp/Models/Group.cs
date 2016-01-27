using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class Group
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "名字最长允许50个字。")]
        [DisplayName("组名")]
        public String Name { get; set; }

        public Boolean isAdmin { get; set; } = false;

        public virtual ICollection<User> Users { get; set; }
        public virtual ICollection<Lib> Libs { get; set; }
    }
}