using Disco.Data.Repository;
using Disco.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Configuration
{
    public class ConfigurationHandler : PluginConfigurationHandler
    {
        public override PluginConfigurationHandler.PluginConfigurationHandlerGetResponse Get(DiscoDataContext dbContext, Controller controller)
        {
            var store = new ConfigurationStore(dbContext);
            var model = store.ToViewModel();

            return GetResponse(typeof(Views.Configuration), model);
        }

        public override bool Post(DiscoDataContext dbContext, FormCollection form, Controller controller)
        {
            var store = new ConfigurationStore(dbContext);
            var model = controller.ToConfigurationModel();

            if (model == null)
                return false;

            model.UpdateStore(store);
            return true;
        }
    }
}