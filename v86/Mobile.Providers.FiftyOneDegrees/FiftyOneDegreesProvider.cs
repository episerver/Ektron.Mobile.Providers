using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Hosting;
using Ektron.Cms;
using Ektron.Cms.Common;
using Ektron.Cms.Mobile;
using FiftyOne.Foundation.Mobile.Detection;
using FiftyOne.Foundation.Mobile.Detection.Entities;
using FiftyOne.Foundation.Mobile.Detection.Factories;

namespace Mobile.Providers.FiftyOneDegrees
{
    public class FiftyOneDegreesProvider : IWURFLProvider
    {
        /// <summary>
        /// The default file name of the 51 degrees library
        /// </summary>
        private const string _defaultFilename = "~/App_Data/51Degrees-LiteV3.2.dat";

        /// <summary>
        /// Getter to return the location configured in the app settings section of the web.config
        /// </summary>
        private string configurationLocation
        {
            get
            {
                string configVal = null;
                try
                {
                    configVal = ConfigurationManager.AppSettings["51DegreesLocation"];
                }
                catch (Exception e)
                {
                    MobileDeviceException myex = new MobileDeviceException("Could not read App settings key '51DegreesLocation' from the web.config", e);
                }
                return configVal;
            }
        }

        /// <summary>
        /// Mapped final location of 51 Degrees Dat file
        /// </summary>
        private string customDatPath
        {
            get
            {
                return (configurationLocation != null)
                    ? HostingEnvironment.MapPath(configurationLocation)
                    : HostingEnvironment.MapPath(_defaultFilename);
            }
        }

        /// <summary>
        /// shared final calculated location of 51 Degrees Dat file
        /// </summary>
        private static string _calculatedFilename = null;

        /// <summary>
        /// Accessor to retrieve the current location of the 51 Degrees Dat file
        /// </summary>
        private string filename
        {
            get
            {
                return _calculatedFilename = _calculatedFilename ?? customDatPath;
            }
        }

        /// <summary>
        /// Get list of all operating systems known to 51 Degrees
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvaliableOs()
        {
            using (DataSet ds = StreamFactory.Create(filename, false))
            {
                return ds.Properties["PlatformName"].Values.Select(t => t.Name).ToList();
            }
        }

        /// <summary>
        /// Internal cache to store lookups by useragent
        /// </summary>
        private static ConcurrentDictionary<string, EkDeviceInfo> privateCache = new ConcurrentDictionary<string, EkDeviceInfo>();

        /// <summary>
        /// Look up device information by request user agent
        /// </summary>
        /// <param name="userAgent">Browser user string</param>
        /// <returns>Device information</returns>
        public EkDeviceInfo GetDeviceInfo(string userAgent)
        {
            EkDeviceInfo retval = null;
            if (string.IsNullOrEmpty(userAgent)) return ConvertDeviceInfo((Profile)null);
            if (!privateCache.TryGetValue(userAgent, out retval))
            {
                using (DataSet ds = StreamFactory.Create(filename, false))
                {
                    Provider provider = new Provider(ds);
                    Match match = provider.Match(userAgent);
                    retval = ConvertDeviceInfo(match);
                }
                if (retval != null)
                {
                    privateCache.TryAdd(userAgent, retval);
                }
            }
            return retval;
        }

        /// <summary>
        /// Look up device by model name
        /// </summary>
        /// <param name="modelName">string to find devices with matching model name</param>
        /// <returns>device information</returns>
        public EkDeviceInfo GetDeviceInfoByModel(string modelName)
        {
            using (DataSet ds = StreamFactory.Create(filename, false))
            {
                try
                {
                    var profiles = ds.FindProfiles("HardwareModel", modelName);
                    return profiles.Select(t => ConvertDeviceInfo(t)).FirstOrDefault();
                }
                catch (Exception e)
                {
                    MobileDeviceException myex = new MobileDeviceException("Could not look up profiles by HardwareModel, does your license and Dat file support this?", e);
                    EkException.LogException(myex);
                }
            }
            return ConvertDeviceInfo((Profile)null);
        }

        /// <summary>
        /// Search across all available properties for a searchterm
        /// </summary>
        /// <param name="searchTerm">Term to search for in all properties</param>
        /// <returns>List of matching devices</returns>
        public List<EkDeviceInfo> SearchForDevices(string searchTerm)
        {
            List<EkDeviceInfo> retval = new List<EkDeviceInfo>();
            searchTerm = searchTerm.ToUpper();

            using (DataSet ds = StreamFactory.Create(filename, false))
            {
                Provider provider = new Provider(ds);
                var mobileProfiles = ds.FindProfiles("IsMobile", "True");
                IEnumerable<Profile> profiles = new List<Profile>();

                //This looks wacky, but the various properties are only available to certain levels of license. 
                //We check each one to determine if it is available, and if so then add the matching profiles 
                //to the returned list.
                if (ds.Properties.Any(t => t.Name == "HardwareModel")) {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["HardwareModel"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "HardwareFamily"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["HardwareFamily"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "HardwareName"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["HardwareName"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "HardwareVendor"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["HardwareVendor"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "PlatformName"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["PlatformName"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "PlatformVendor"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["PlatformVendor"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "PlatformVersion"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["PlatformVersion"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "BrowserName"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["BrowserName"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "BrowserVendor"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["BrowserVendor"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }
                if (ds.Properties.Any(t => t.Name == "BrowserVersion"))
                {
                    profiles = profiles.Concat(mobileProfiles
                        .Where(t => t["BrowserVersion"].ToString().ToUpper().Contains(searchTerm))).ToList();
                }

                profiles = profiles
                    .GroupBy(t => t.ProfileId)
                    .Select(group => group.First());
                return profiles.Select(t => ConvertDeviceInfo(t)).ToList();
            }
        }

        /// <summary>
        /// Convert from 51 Degrees Match to Ektron EkDeviceInfo
        /// </summary>
        /// <param name="device">51 Degrees Match</param>
        /// <returns>EkDeviceInfo</returns>
        private EkDeviceInfo ConvertDeviceInfo(Match device)
        {
            EkDeviceInfo ret = new EkDeviceInfo();

            // Start with generic values by default
            ret.DeviceModel = EkEnumeration.DefaultDeviceModels.Generic.ToString();
            ret.DeviceOs = EkEnumeration.DefaultDeviceModels.Generic.ToString();
            ret.DeviceType = ((int)EkEnumeration.DefaultDeviceModels.Generic).ToString();
            ret.DeviceResWidth = "-1";
            ret.DeviceResHeight = "-1";
            ret.DeviceImageWidth = "-1";
            ret.DeviceImageHeight = "-1";
            ret.DeviceBrandName = EkEnumeration.DefaultDeviceModels.Generic.ToString();

            if (device == null) return ret;

            // Copy property values over directly
            ret.DeviceUserAgent = device.UserAgent;
            ret.DeviceID = device.DeviceId;

            // Overwrite properties with any returned capabilities
            ret.DeviceModel = device["HardwareModel"] != null ? device["HardwareModel"].ToString() : ret.DeviceModel;
            ret.DeviceModel = (ret.DeviceModel == "Unknown") ? EkEnumeration.DefaultDeviceModels.Generic.ToString() : ret.DeviceModel;
            if (ret.DeviceModel == EkEnumeration.DefaultDeviceModels.Generic.ToString() && device["IsMobile"].ToBool())
            {
                ret.DeviceModel = EkEnumeration.DefaultDeviceModels.GenericMobile.ToString();
            }
            ret.DeviceOs = device["PlatformName"] != null ? device["PlatformName"].ToString() : ret.DeviceOs;

            ret.DeviceType = device["IsMobile"] != null ? (device["IsMobile"].ToBool() ? "1" : "0") : ret.DeviceType;
            ret.DeviceType = device["IsTablet"] != null ? (device["IsTablet"].ToBool() ? "2" : ret.DeviceType) : ret.DeviceType;

            ret.DeviceResWidth = device["ScreenPixelsWidth"] != null ? device["ScreenPixelsWidth"].ToInt().ToString() : ret.DeviceResWidth;
            ret.DeviceResHeight = device["ScreenPixelsHeight"] != null ? device["ScreenPixelsHeight"].ToInt().ToString() : ret.DeviceResHeight;
            ret.DeviceImageWidth = device["ScreenPixelsWidth"] != null ? device["ScreenPixelsWidth"].ToInt().ToString() : ret.DeviceImageWidth;
            ret.DeviceImageHeight = device["ScreenPixelsHeight"] != null ? device["ScreenPixelsHeight"].ToInt().ToString() : ret.DeviceImageHeight;
            ret.DeviceBrandName = device["HardwareVendor"] != null ? device["HardwareVendor"].ToString() : ret.DeviceBrandName;
            ret.DeviceOs = device["PlatformName"] != null ? device["PlatformName"].ToString() : ret.DeviceOs;

            return ret;
        }

        /// <summary>
        /// Convert from 51 Degrees Profile to Ektron EkDeviceInfo
        /// </summary>
        /// <param name="device">51 Degrees Profile</param>
        /// <returns>EkDeviceInfo</returns>
        private EkDeviceInfo ConvertDeviceInfo(Profile device)
        {
            EkDeviceInfo ret = new EkDeviceInfo();

            // Start with generic values by default
            ret.DeviceModel = EkEnumeration.DefaultDeviceModels.Generic.ToString();
            ret.DeviceOs = EkEnumeration.DefaultDeviceModels.Generic.ToString();
            ret.DeviceType = ((int)EkEnumeration.DefaultDeviceModels.Generic).ToString();
            ret.DeviceResWidth = "-1";
            ret.DeviceResHeight = "-1";
            ret.DeviceImageWidth = "-1";
            ret.DeviceImageHeight = "-1";
            ret.DeviceBrandName = EkEnumeration.DefaultDeviceModels.Generic.ToString();

            if (device == null) return ret;

            // Copy property values over directly
            ret.DeviceID = "Profile:" + device.ProfileId.ToString();

            // Overwrite properties with any returned capabilities
            ret.DeviceModel = device["HardwareModel"] != null ? device["HardwareModel"].ToString() : ret.DeviceModel;
            ret.DeviceModel = (ret.DeviceModel == "Unknown") ? EkEnumeration.DefaultDeviceModels.Generic.ToString() : ret.DeviceModel;
            if (ret.DeviceModel == EkEnumeration.DefaultDeviceModels.Generic.ToString() && device["IsMobile"].ToBool())
            {
                ret.DeviceModel = EkEnumeration.DefaultDeviceModels.GenericMobile.ToString();
            }
            ret.DeviceOs = device["PlatformName"] != null ? device["PlatformName"].ToString() : ret.DeviceOs;

            ret.DeviceType = device["IsMobile"] != null ? (device["IsMobile"].ToBool() ? "1" : "0") : ret.DeviceType;
            ret.DeviceType = device["IsTablet"] != null ? (device["IsTablet"].ToBool() ? "2" : ret.DeviceType) : ret.DeviceType;

            ret.DeviceResWidth = device["ScreenPixelsWidth"] != null ? device["ScreenPixelsWidth"].ToInt().ToString() : ret.DeviceResWidth;
            ret.DeviceResHeight = device["ScreenPixelsHeight"] != null ? device["ScreenPixelsHeight"].ToInt().ToString() : ret.DeviceResHeight;
            ret.DeviceImageWidth = device["ScreenPixelsWidth"] != null ? device["ScreenPixelsWidth"].ToInt().ToString() : ret.DeviceImageWidth;
            ret.DeviceImageHeight = device["ScreenPixelsHeight"] != null ? device["ScreenPixelsHeight"].ToInt().ToString() : ret.DeviceImageHeight;
            ret.DeviceBrandName = device["HardwareVendor"] != null ? device["HardwareVendor"].ToString() : ret.DeviceBrandName;
            ret.DeviceOs = device["PlatformName"] != null ? device["PlatformName"].ToString() : ret.DeviceOs;

            return ret;
        }
    }
}
