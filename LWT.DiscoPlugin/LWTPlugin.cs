using Disco.Data.Repository;
using Disco.Services.Plugins;
using Disco.Services.Tasks;

namespace LWT.DiscoPlugin
{
    [Plugin(Id = "LWTPlugin", Name = "Learning With Technologies", Author = "Disco Development Team",
         Url = "http://www.lwt.com.au/", HostVersionMin = "2.0.0918.1700")]
    public class LWTPlugin : Plugin
    {

        public override void Uninstall(DiscoDataContext Database, bool UninstallData, ScheduledTaskStatus Status)
        {
            if (UninstallData)
                Internal.Helpers.UninstallData(Database, this.Manifest, Status);
        }
    }
}