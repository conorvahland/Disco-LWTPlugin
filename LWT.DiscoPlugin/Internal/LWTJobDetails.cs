using Disco.Data.Repository;
using Disco.Models.Repository;
using HtmlAgilityPack;
using LWT.DiscoPlugin.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace LWT.DiscoPlugin.Internal
{
    internal static class LWTJobDetails
    {
        internal static string JobDetailsCachePath { get; set; }

        internal static WarrantyJobDetailsModel LoadJobDetails(DiscoDataContext dbContext, Job Job)
        {
            int onlineWarrantyId = default(int);
            string deviceSerialNumber = Job.DeviceSerialNumber;

            if (deviceSerialNumber != null && Job.JobMetaWarranty != null &&
                !string.IsNullOrWhiteSpace(Job.JobMetaWarranty.ExternalReference) &&
                int.TryParse(Job.JobMetaWarranty.ExternalReference, out onlineWarrantyId))
            {
                WarrantyJobDetailsModel model;

                // Try from Cache
                model = LoadJobDetailsFromCache(onlineWarrantyId, deviceSerialNumber);
                
                if (model == null)
                {
                    // Load from Web
                    model = LoadJobDetailsFromWeb(onlineWarrantyId, deviceSerialNumber);

                    // Cache Response
                    CacheJobDetails(model);
                }

                return model;
            }
            else
            {
                return EmptyJobDetails(onlineWarrantyId, deviceSerialNumber);
            }
        }

        private static WarrantyJobDetailsModel EmptyJobDetails(int OnlineWarrantyId, string DeviceSerialNumber)
        {
            return new WarrantyJobDetailsModel()
            {
                JobDetailsParsed = false,
                OnlineWarrantyId = OnlineWarrantyId,
                DeviceSerialNumber = DeviceSerialNumber
            };
        }

        #region Cache
        private static DateTime? NextCachePruneTime;
        private static WarrantyJobDetailsModel LoadJobDetailsFromCache(int OnlineWarrantyId, string DeviceSerialNumber)
        {
            if (!NextCachePruneTime.HasValue || NextCachePruneTime.Value < DateTime.Now)
                PruneCache();

            if (!Directory.Exists(JobDetailsCachePath))
                Directory.CreateDirectory(JobDetailsCachePath);

            string cacheFilename = CacheFileName(OnlineWarrantyId, DeviceSerialNumber);

            if (File.Exists(cacheFilename))
            {
                try
                {
                    DateTime cacheTime = File.GetLastWriteTime(cacheFilename);
                    if (cacheTime < DateTime.Now.AddHours(-1))
                        File.Delete(cacheFilename);
                    else
                    {
                        string cacheContent = File.ReadAllText(cacheFilename);
                        var cacheModel = JsonConvert.DeserializeObject<WarrantyJobDetailsModel>(cacheContent);
                        cacheModel.CacheAge = cacheTime;
                        return cacheModel;
                    }
                }
                catch (Exception) { } // Ignore Caching Errors
            }

            return null;
        }
        private static void PruneCache()
        {
            if (Directory.Exists(JobDetailsCachePath))
            {
                DateTime cacheExpiredTime = DateTime.Now.AddHours(-1);
                foreach (string cacheFile in Directory.EnumerateFiles(JobDetailsCachePath, "*.json"))
                {
                    try
                    {
                        DateTime cacheTime = File.GetCreationTime(cacheFile);
                        if (cacheTime < cacheExpiredTime)
                            File.Delete(cacheFile);
                    }
                    catch (Exception) { } // Ignore Errors
                }
            }
            NextCachePruneTime = DateTime.Now.AddHours(1);
        }
        private static void CacheJobDetails(WarrantyJobDetailsModel Details)
        {
            try
            {
                var jsonContent = JsonConvert.SerializeObject(Details);
                var cacheFilename = CacheFileName(Details.OnlineWarrantyId, Details.DeviceSerialNumber);
                File.WriteAllText(cacheFilename, jsonContent);
            }
            catch (Exception) { } // Ignore Cache Writing Errors
        }
        private static string CacheFileName(int OnlineWarrantyId, string DeviceSerialNumber)
        {
            return Path.Combine(JobDetailsCachePath, string.Format("{0}_{1}.json", OnlineWarrantyId, DeviceSerialNumber));
        }
        #endregion

        #region Web Retrieve
        private static WarrantyJobDetailsModel LoadJobDetailsFromWeb(int OnlineWarrantyId, string DeviceSerialNumber)
        {
            WarrantyJobDetailsModel model = new WarrantyJobDetailsModel()
            {
                JobDetailsParsed = false,
                OnlineWarrantyId = OnlineWarrantyId,
                DeviceSerialNumber = DeviceSerialNumber
            };

            string wreqUrl = string.Format("http://www.lwt.com.au/warrantydetails.aspx?SectorID=1&OnlineWarrantyID={0}&Serial={1}", OnlineWarrantyId, DeviceSerialNumber);

            HttpWebRequest wreq = (HttpWebRequest)HttpWebRequest.Create(wreqUrl);
            wreq.KeepAlive = false;
            wreq.Method = WebRequestMethods.Http.Get;

            HtmlDocument doc = new HtmlDocument();

            using (HttpWebResponse wres = (HttpWebResponse)wreq.GetResponse())
            {
                using (Stream s = wres.GetResponseStream())
                {
                    doc.Load(s);
                }
            }

            try
            {
                var formatProvider = CultureInfo.CurrentCulture;
                HtmlNode node;

                // Parse Details
                node = doc.GetElementbyId("ctl00_ContentPlaceHolder1_FormView1_WarrantyIDLabel");
                if (node == null)
                {
                    model.JobDetailsParsed = false;
                    return model;
                }
                model.LWTJobId = node.InnerText;
                node = doc.GetElementbyId("ctl00_ContentPlaceHolder1_FormView1_WarrantyDateTimeLoggedLabel");

                DateTime dateLogged;
                if (node != null && DateTime.TryParseExact(node.InnerText, "dd-MMM-yyyy", formatProvider, System.Globalization.DateTimeStyles.AssumeLocal, out dateLogged))
                    model.DateLogged = dateLogged;
                else
                    model.DateLogged = null;

                node = doc.GetElementbyId("ctl00_ContentPlaceHolder1_FormView1_DateCompletedLabel");

                DateTime dateCompleted;
                if (node != null && DateTime.TryParse(node.InnerText, out dateCompleted))
                    model.DateCompleted = dateCompleted;
                else
                    model.DateCompleted = null;

                node = doc.GetElementbyId("ctl00_ContentPlaceHolder1_FormView1_FaultDescriptionLabel");
                model.FaultDescription = node != null ? node.InnerHtml : "?";

                // Parse Actions
                node = doc.GetElementbyId("ctl00_ContentPlaceHolder1_GridView1");
                model.Actions = new List<WarrantyJobDetailsModel.JobDetailsActionViewModel>();
                if (node != null)
                {
                    foreach (var nodeActionRow in node.Descendants("tr").Skip(1))
                    {
                        var nodeActionDetails = nodeActionRow.Descendants("td").ToArray();
                        if (nodeActionDetails.Length == 4)
                        {
                            DateTime? actionDate = null;
                            DateTime actionDateParse;
                            if (node != null && DateTime.TryParseExact(nodeActionDetails[1].InnerText, "dd-MMM-yyyy", formatProvider, DateTimeStyles.AssumeLocal, out actionDateParse))
                                actionDate = actionDateParse;

                            model.Actions.Add(new WarrantyJobDetailsModel.JobDetailsActionViewModel()
                            {
                                Type = nodeActionDetails[0].InnerText,
                                Date = actionDate,
                                ActionDescription = nodeActionDetails[2].InnerHtml.Replace(Environment.NewLine, "<br />"),
                                TechnicianId = nodeActionDetails[3].InnerText
                            });
                        }
                    }
                }

                model.JobDetailsParsed = true;
                model.CacheAge = DateTime.Now;
            }
            catch (Exception)
            {
                // Ignore Parse Errors
            }

            return model;
        }
        #endregion
    }
}