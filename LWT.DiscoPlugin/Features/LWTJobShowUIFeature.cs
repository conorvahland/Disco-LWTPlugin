using Disco.Data.Repository;
using Disco.Models.UI.Job;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.UIExtension;
using LWT.DiscoPlugin.Internal;
using System.Text;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Features
{
    [PluginFeature(Id = "LWTJobShowUI", Name = "Learning With Technology - Job Show", PrimaryFeature = false)]
    public class LWTJobShowUIFeature : UIExtensionFeature<JobShowModel>
    {
        public override void Initialize(DiscoDataContext Database)
        {
            // Register UI Extension
            this.Register();
        }

        public override UIExtensionResult ExecuteAction(ControllerContext context, JobShowModel model)
        {
            // Add Alternate SNID to UI
            string snid;
            if (model.Job.DeviceSerialNumber != null && model.Job.Device.HasAlternateSerialNumber(out snid))
            {
                var script = new StringBuilder();
                script.AppendLine("$(function() {");
                script.Append(@"   $('#Job_Show_Device_Details').prepend('<div title=""Acer SNID"">SNID: ").Append(snid).AppendLine(@"</div>');");
                script.Append("});");

                return ScriptInline(script.ToString());
            }
            else
            {
                return Nothing();
            }
        }
    }
}