using Disco.Data.Repository;
using Disco.Models.Repository;
using Disco.Services.Plugins;
using Disco.Services.Tasks;
using LWT.DiscoPlugin.Configuration;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace LWT.DiscoPlugin.Internal
{
    public static class Helpers
    {
        // Regex for validating and parsing Acer SNID
        private static Regex AcerSnidParser =
            new Regex(@"^[A-Z]{2}[\w]{8}(\d{3})([A-Fa-f0-9]{5})(\d{2})[\w]{2}$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static void UninstallData(DiscoDataContext Database, PluginManifest Manifest, ScheduledTaskStatus Status)
        {
            Status.UpdateStatus("Removing Configuration");

            var config = new ConfigurationStore(Database);
            config.CustomerEntityId = null;
            config.CustomerUsername = null;
            Database.SaveChanges();

            // Storage Location will be cleared by the framework if requested by the user
        }

        public static bool HasAlternateSerialNumber(string SerialNumber, out string AlternateSerialNumber)
        {
            if (SerialNumber.Length == 22) // Is 22-length
            {
                var snidMatch = AcerSnidParser.Match(SerialNumber);
                if (snidMatch.Success) // Convert to 11-length SNID
                {
                    var snid = new StringBuilder(11);

                    // Take characters 15-17
                    snid.Append(snidMatch.Groups[1].Value);
                    // Take characters 18-22 (hexadecimal), convert to base10, 6 characters
                    snid.Append(int.Parse(snidMatch.Groups[2].Value, NumberStyles.AllowHexSpecifier).ToString("000000"));
                    // Take characters 23-24
                    snid.Append(snidMatch.Groups[3].Value);

                    AlternateSerialNumber = snid.ToString();
                    return true;
                }
            }

            AlternateSerialNumber = null;
            return false;
        }

        public static bool HasAlternateSerialNumber(this Device Device, out string AlternateSerialNumber)
        {
            // Only parse if DeviceModel is unknown or manufactured by ACER
            if (Device.DeviceModelId == 1 || Device.DeviceModel.Manufacturer.ToUpper().Contains("ACER"))
                return HasAlternateSerialNumber(Device.SerialNumber, out AlternateSerialNumber);
            else
            {
                AlternateSerialNumber = null;
                return false;
            }
        }

        public static string ParseSerialNumber(this Device Device)
        {
            string alternateSerialNumber;

            if (HasAlternateSerialNumber(Device, out alternateSerialNumber))
                return alternateSerialNumber;
            else
                return Device.SerialNumber;
        }

        public static string ParseSerialNumber(string SerialNumber)
        {
            string alternateSerialNumber;

            if (HasAlternateSerialNumber(SerialNumber, out alternateSerialNumber))
                return alternateSerialNumber;
            else
                return SerialNumber;
        }

    }
}