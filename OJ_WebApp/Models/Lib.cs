using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class Lib
    {
        public Int32 Id { get; set; }

        [Required]
        [MaxLength(50, ErrorMessage = "名字最长允许50个字。")]
        [DisplayName("组名")]
        public String Name { get; set; }

        [Required]
        [MaxLength(200, ErrorMessage = "名字最长允许200个字。")]
        [DisplayName("介绍")]
        public String Intro { get; set; }

        [Required]
        public Boolean isPrivate { get; set; } = false;

        public virtual ICollection<Issue> Issues { get; set; }
        public virtual ICollection<Group> Groups { get; set; }
        public virtual ICollection<Game> Games { get; set; }
    }
}