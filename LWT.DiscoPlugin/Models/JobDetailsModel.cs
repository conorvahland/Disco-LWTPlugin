using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LWT.DiscoPlugin.Models
{
    public class JobDetailsModel
    {
        public int OnlineWarrantyId { get; set; }
        public string DeviceSerialNumber { get; set; }

        public bool JobDetailsParsed { get; set; }
        public DateTime CacheAge { get; set; }
        public string LWTJobId { get; set; }
        public DateTime? DateLogged { get; set; }
        public DateTime? DateCompleted { get; set; }
        public string FaultDescription { get; set; }
        public List<JobDetailsActionViewModel> Actions { get; set; }

        public class JobDetailsActionViewModel
        {
            public string Type { get; set; }
            public DateTime? Date { get; set; }
            public string ActionDescription { get; set; }
            public string TechnicianId { get; set; }
        }
    }
}