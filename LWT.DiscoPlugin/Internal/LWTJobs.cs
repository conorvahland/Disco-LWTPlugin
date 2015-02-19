using Disco.Data.Repository;
using Disco.Models.BI.Config;
using Disco.Models.Repository;
using Disco.Services.Plugins.Features.WarrantyProvider;
using LWT.DiscoPlugin.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LWT.DiscoPlugin.Internal
{
    internal static class LWTJobs
    {
        public static bool ValidateEnvironment(DiscoDataContext Database, Controller controller, User TechUser)
        {
            // Validate Configuration
            var config = new ConfigurationStore(Database);
            if (string.IsNullOrWhiteSpace(config.CustomerEntityId))
                controller.ModelState.AddModelError(string.Empty, "LWT Customer Entity Id is Required (See LWT Plugin Configuration)");
            if (string.IsNullOrWhiteSpace(config.CustomerUsername))
                controller.ModelState.AddModelError(string.Empty, "LWT Customer Username is Required (See LWT Plugin Configuration)");

            // Validate TechUser Email Address
            if (string.IsNullOrEmpty(TechUser.EmailAddress))
                controller.ModelState.AddModelError(string.Empty, "LWT Requires a Technician Email Address (Update your Email Address in Active Directory)");
            if (string.IsNullOrEmpty(TechUser.PhoneNumber))
                controller.ModelState.AddModelError(string.Empty, "LWT Requires a Technician Phone Number (Update your Telephone Number in Active Directory)");

            return controller.ModelState.IsValid;
        }

        public static Dictionary<string, string> DiscloseInformation(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser)
        {
            var config = new ConfigurationStore(Database);

            string serialNumberMessage;
            if (Job.Device.HasAlternateSerialNumber(out serialNumberMessage))
                serialNumberMessage += " [Acer SNID]";
            else
                serialNumberMessage = Job.DeviceSerialNumber;

            return new Dictionary<string, string>()
            {
                {"LWT Customer Entity Id", config.CustomerEntityId},
                {"LWT Customer Username", config.CustomerUsername},
                {"Contact Name", TechUser.DisplayName},
                {"Contact Company", Address.Name},
                {"Contact Address", Address.Address},
                {"Contact Suburb", Address.Suburb},
                {"Contact Postcode", Address.Postcode},
                {"Contact Phone", TechUser.PhoneNumber},
                {"Contact Email", TechUser.EmailAddress},
                {"Device Serial Number", serialNumberMessage},
                {"Device Product Description", String.Format("{0} {1}", Job.Device.DeviceModel.Manufacturer, Job.Device.DeviceModel.Model)},
                {"Device Room Location", String.Format("Customer Job Id: {0}", Job.Id)}
            };
        }

        public static string SubmitRepairJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string RepairDescription)
        {
            if (string.IsNullOrEmpty(RepairDescription))
                RepairDescription = "REPAIR REQUEST";
            else
                RepairDescription = "REPAIR REQUEST" + Environment.NewLine + RepairDescription;

            return SubmitJob(Database, Job, Address, TechUser, RepairDescription);
        }

        public static string SubmitWarrantyJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription)
        {
            return SubmitJob(Database, Job, Address, TechUser, FaultDescription);
        }

        private static string SubmitJob(DiscoDataContext Database, Job Job, OrganisationAddress Address, User TechUser, string FaultDescription)
        {
            // Send Job to LWT
            var config = new ConfigurationStore(Database);

            // Build Http Post Body
            var httpBody = new StringBuilder("Automated=1&");
            httpBody.Append("EntityID=").Append(HttpUtility.UrlEncode(config.CustomerEntityId));
            httpBody.Append("&txtUserName=").Append(HttpUtility.UrlEncode(config.CustomerUsername));
            httpBody.Append("&txtContactName=").Append(HttpUtility.UrlEncode(TechUser.DisplayName));
            httpBody.Append("&txtContactCompany=").Append(HttpUtility.UrlEncode(Address.Name));
            httpBody.Append("&txtContactAddress=").Append(HttpUtility.UrlEncode(Address.Address));
            httpBody.Append("&txtContactSuburb=").Append(HttpUtility.UrlEncode(Address.Suburb));
            httpBody.Append("&txtContactPostcode=").Append(HttpUtility.UrlEncode(Address.Postcode));
            httpBody.Append("&txtContactPhone=").Append(HttpUtility.UrlEncode(TechUser.PhoneNumber));
            httpBody.Append("&txtContactEmail=").Append(HttpUtility.UrlEncode(TechUser.EmailAddress));
            httpBody.Append("&txtSerialNumber=").Append(HttpUtility.UrlEncode(Job.Device.ParseSerialNumber()));
            httpBody.Append("&txtProductDescription=").Append(HttpUtility.UrlEncode(String.Format("{0} {1}", Job.Device.DeviceModel.Manufacturer, Job.Device.DeviceModel.Model)));
            httpBody.Append("&txtRoomLocation=").Append(HttpUtility.UrlEncode("Customer Job Id: ")).Append(Job.Id);
            httpBody.Append("&txtFaultDescription=").Append(HttpUtility.UrlEncode(FaultDescription));

            HttpWebRequest wreq = (HttpWebRequest)HttpWebRequest.Create("http://www.lwt.com.au/Warranty.asp");
            wreq.KeepAlive = false;
            wreq.Method = WebRequestMethods.Http.Post;
            wreq.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter sw = new StreamWriter(wreq.GetRequestStream()))
                sw.Write(httpBody.ToString());

            using (HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse())
            {
                string stringResponse;

                using (Stream s = wres.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(s))
                    {
                        stringResponse = sr.ReadToEnd();
                    }
                }

                // If response is an Integer assume Job Reference Number, otherwise error.
                int jobReference;
                if (int.TryParse(stringResponse, out jobReference))
                    return jobReference.ToString();
                else
                    throw new WarrantyProviderSubmitJobException(stringResponse);
            }
        }

    }
}