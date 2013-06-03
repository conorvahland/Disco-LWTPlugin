using Disco.Data.Configuration;
using Disco.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LWT.DiscoPlugin.Configuration
{
    public class ConfigurationStore : ConfigurationBase
    {
        public ConfigurationStore(DiscoDataContext context) : base(context) { }

        public override string Scope { get { return "Warranty_LWT"; } }

        public string CustomerEntityId
        {
            get { return Get<string>(null); }
            set { Set(value); }
        }

        public string CustomerUsername
        {
            get { return Get<string>(null); }
            set { Set(value); }
        }
    }
}