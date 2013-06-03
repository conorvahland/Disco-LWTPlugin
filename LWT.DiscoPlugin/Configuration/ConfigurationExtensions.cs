using LWT.DiscoPlugin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Disco.Services.Plugins;

namespace LWT.DiscoPlugin.Configuration
{
    internal static class ConfigurationExtensions
    {

        public static ConfigurationModel ToViewModel(this ConfigurationStore config)
        {
            return new ConfigurationModel()
            {
                CustomerEntityId = config.CustomerEntityId,
                CustomerUsername = config.CustomerUsername
            };
        }

        public static ConfigurationModel ToConfigurationModel(this Controller controller)
        {
            ConfigurationModel model = new ConfigurationModel();

            if (controller.TryUpdateModel(model))
                return model;
            else
                return null;
        }

        public static void UpdateStore(this ConfigurationModel model, ConfigurationStore config)
        {
            config.CustomerEntityId = model.CustomerEntityId;
            config.CustomerUsername = model.CustomerUsername;
        }

    }
}