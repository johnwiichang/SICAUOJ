using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace OJ_WebApp.Service
{
    public class EmailConfigurationProvider : ConfigurationSection
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("account", IsRequired = true)]
        public string Account
        {
            get { return this["account"].ToString(); }
            set { this["account"] = value; }
        }

        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return this["password"].ToString(); }
            set { this["password"] = value; }
        }

        [ConfigurationProperty("server", IsRequired = true)]
        public string Server
        {
            get { return this["server"].ToString(); }
            set { this["server"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return Int32.Parse(this["port"].ToString()); }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("isSSL", IsRequired = true)]
        public bool IsSSL
        {
            get { return Boolean.Parse(this["isSSL"].ToString()); }
            set { this["isSSL"] = value; }
        }

        [ConfigurationProperty("emailhtmluri", IsRequired = false)]
        public string EmailHtmlUri
        {
            get { return this["emailhtmluri"].ToString(); }
            set { this["emailhtmluri"] = value; }
        }
    }
}