using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LWT.DiscoPlugin.Models
{
    public class ConfigurationModel
    {
        [Required(ErrorMessage = "Customer Entity Id is Required")]
        public string CustomerEntityId { get; set; }

        [Required(ErrorMessage = "Customer Username is Required")]
        public string CustomerUsername { get; set; }
    }
}