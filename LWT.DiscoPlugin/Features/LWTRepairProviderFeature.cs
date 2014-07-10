using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.RepairProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Features
{
    [PluginFeature(Id = "LWTRepairProvider", Name = "Learning With Technology", PrimaryFeature = false)]
    public class LWTRepairProviderFeature : RepairProviderFeature
    {
        public override string ProviderId { get { return "LWT"; } }

        #region Submit Job

        public override Tuple<Type, dynamic> SubmitJobBegin(DiscoDataContext Database, Controller controller, Job Job, OrganisationAddress Address, User TechUser)
        {
            Internal.LWTJobs.ValidateEnvironment(Database, controller, TechUser);

            return null;
        }

        public override Dictionary<string, string> SubmitJobDiscloseInfo(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription, Dictionary<string, string> ProviderProperties)
        {
            return Internal.LWTJobs.DiscloseInformation(Database, Job, Address, TechUser);
        }

        public override string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription, Dictionary<string, string> ProviderProperties)
        {
            return Internal.LWTJobs.SubmitRepairJob(Database, Job, Address, TechUser, RepairDescription);
        }

        #endregion


        #region Job Details

        public override bool JobDetailsSupported { get { return true; } }

        public override Tuple<Type, dynamic> JobDetails(DiscoDataContext Database, Controller controller, Job Job)
        {
            var model = Internal.LWTJobDetails.LoadRepairJobDetails(Database, Job);

            return Tuple.Create(typeof(Views.JobDetails), (object)model);
        }

        #endregion

    }
}