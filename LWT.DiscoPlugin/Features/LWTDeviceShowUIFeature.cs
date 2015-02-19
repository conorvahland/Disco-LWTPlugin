using Disco.Data.Repository;
using Disco.Models.UI.Device;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.UIExtension;
using LWT.DiscoPlugin.Internal;
using System.Text;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Features
{
    [PluginFeature(Id = "LWTDeviceShowUI", Name = "Learning With Technology - Device Show", PrimaryFeature = false)]
    public class LWTDeviceShowUIFeature : UIExtensionFeature<DeviceShowModel>
    {
        public override void Initialize(DiscoDataContext Database)
        {
            // Register UI Extension
            this.Register();
        }

        public override UIExtensionResult ExecuteAction(ControllerContext context, DeviceShowModel model)
        {
            // Add Alternate SNID to UI
            string snid;
            if (model.Device.HasAlternateSerialNumber(out snid))
            {
                var script = new StringBuilder();
                script.AppendLine("$(function() {");
                script.Append(@"   $('#Device_Show_Details_Asset tbody').prepend('<tr><td><span>Acer SNID:</span></td><td><h4 class=""alert""><strong>").Append(snid).AppendLine(@"</strong></h4></td></tr>');");
                script.Append(@"   $('#Device_Show_Status').before('<span title=""Acer SNID"" style=""font-size: .8em; font-weight: 100""> (").Append(snid).AppendLine(@")</span>');");
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