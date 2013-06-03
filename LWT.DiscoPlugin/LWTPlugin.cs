using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LWT.DiscoPlugin
{
    [Plugin(Id = "LWTPlugin", Name = "Learning With Technologies", Author = "Disco Development Team",
         Url = "http://www.lwt.com.au/", HostVersionMin = "1.2.0521.0")]
    public class LWTPlugin : Plugin
    {

        public override void Uninstall(DiscoDataContext dbContext, bool UninstallData, ScheduledTaskStatus Status)
        {
            if (UninstallData)
                Internal.Helpers.UninstallData(dbContext, this.Manifest, Status);
        }
    }
}