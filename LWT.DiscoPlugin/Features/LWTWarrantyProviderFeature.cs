using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Plugins;
using Disco.Services.Plugins.Features.WarrantyProvider;
using LWT.DiscoPlugin.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Features
{
    [PluginFeature(Id = "LWTWarrantyProvider", Name = "Learning With Technology", PrimaryFeature = true)]
    public class LWTWarrantyProviderFeature : WarrantyProviderFeature
    {
        public override string WarrantyProviderId { get { return "LWT"; } }

        public override void Initialize(DiscoDataContext Database)
        {
            Internal.LWTJobDetails.JobDetailsCachePath = System.IO.Path.Combine(this.Manifest.PluginManifest.StorageLocation, "JobDetailCache");
        }

        #region Submit Job
        
        public override Type SubmitJobViewType { get { return null; } }

        public override dynamic SubmitJobViewModel(DiscoDataContext Database, Controller controller, Job Job, OrganisationAddress Address, User TechUser)
        {
            Internal.LWTWarrantyJobs.ValidateEnvironment(Database, controller, TechUser);

            return null;
        }

        public override Dictionary<string, string> SubmitJobParseProperties(DiscoDataContext Database, FormCollection form, Controller controller, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription)
        {
            Internal.LWTWarrantyJobs.ValidateEnvironment(Database, controller, TechUser);

            return null;
        }

        public override Dictionary<string, string> SubmitJobDiscloseInfo(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties)
        {
            return Internal.LWTWarrantyJobs.DiscloseInformation(Database, Job, Address, TechUser);
        }

        public override string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription, Dictionary<string, string> WarrantyProviderProperties)
        {
            return Internal.LWTWarrantyJobs.SubmitJob(Database, Job, Address, TechUser, FaultDescription);
        }
        #endregion

        #region Job Details
        
        public override Type JobDetailsViewType
        {
            get { return typeof(Views.WarrantyJobDetails); }
        }

        public override dynamic JobDetailsViewModel(DiscoDataContext Database, Controller controller, Job Job)
        {
            return Internal.LWTJobDetails.LoadJobDetails(Database, Job);
        }

        #endregion
    }
}