using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Tasks;
using LWT.DiscoPlugin.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LWT.DiscoPlugin.Internal
{
    public static class Helpers
    {

        public static void UninstallData(DiscoDataContext Database, PluginManifest Manifest, ScheduledTaskStatus Status)
        {
            Status.UpdateStatus("Removing Configuration");

            var config = new ConfigurationStore(Database);
            config.CustomerEntityId = null;
            config.CustomerUsername = null;
            Database.SaveChanges();

            // Storage Location will be cleared by the framework if requested by the user
        }

    }
}