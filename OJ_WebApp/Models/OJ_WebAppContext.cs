using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace OJ_WebApp.Models
{
    public class OJ_WebAppContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public OJ_WebAppContext() : base("name=OJ_WebAppContext")
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Issue> Issues { get; set; }
        public virtual DbSet<Task> Tasks { get; set; }
        public virtual DbSet<Lib> Libs { get; set; }
        public virtual DbSet<Compiler> Compilers { get; set; }
        public virtual DbSet<Game> Games { get; set; }
        public virtual DbSet<Node> Nodes { get; set; }
        public virtual DbSet<Mail> Mails { get; set; }
    }
}
