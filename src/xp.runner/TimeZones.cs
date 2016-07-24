using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Xp.Runners
{
    static class TimeZones
    {

        // Mapping table generated from
        //
        // $ curl http://unicode.org/repos/cldr/trunk/common/supplemental/windowsZones.xml \
        //   | grep 'territory="001"'
        //   | sed -E 's/.+other=("[^"]+").+type=("[^"]+")..+/        { \1, \2 },/g'
        //
        // Inspired by https://github.com/lmcnearney/timezoneinfo-olson-mapper
        private static Dictionary<string, string> mapping = new Dictionary<string, string>()
        {
            { "Dateline Standard Time", "Etc/GMT+12" },
            { "UTC-11", "Etc/GMT+11" },
            { "Hawaiian Standard Time", "Pacific/Honolulu" },
            { "Alaskan Standard Time", "America/Anchorage" },
            { "Pacific Standard Time (Mexico)", "America/Santa_Isabel" },
            { "Pacific Standard Time", "America/Los_Angeles" },
            { "US Mountain Standard Time", "America/Phoenix" },
            { "Mountain Standard Time (Mexico)", "America/Chihuahua" },
            { "Mountain Standard Time", "America/Denver" },
            { "Central America Standard Time", "America/Guatemala" },
            { "Central Standard Time", "America/Chicago" },
            { "Central Standard Time (Mexico)", "America/Mexico_City" },
            { "Canada Central Standard Time", "America/Regina" },
            { "SA Pacific Standard Time", "America/Bogota" },
            { "Eastern Standard Time (Mexico)", "America/Cancun" },
            { "Eastern Standard Time", "America/New_York" },
            { "US Eastern Standard Time", "America/Indianapolis" },
            { "Venezuela Standard Time", "America/Caracas" },
            { "Paraguay Standard Time", "America/Asuncion" },
            { "Atlantic Standard Time", "America/Halifax" },
            { "Central Brazilian Standard Time", "America/Cuiaba" },
            { "SA Western Standard Time", "America/La_Paz" },
            { "Newfoundland Standard Time", "America/St_Johns" },
            { "E. South America Standard Time", "America/Sao_Paulo" },
            { "SA Eastern Standard Time", "America/Cayenne" },
            { "Argentina Standard Time", "America/Buenos_Aires" },
            { "Greenland Standard Time", "America/Godthab" },
            { "Montevideo Standard Time", "America/Montevideo" },
            { "Bahia Standard Time", "America/Bahia" },
            { "Pacific SA Standard Time", "America/Santiago" },
            { "UTC-02", "Etc/GMT+2" },
            { "Azores Standard Time", "Atlantic/Azores" },
            { "Cape Verde Standard Time", "Atlantic/Cape_Verde" },
            { "Morocco Standard Time", "Africa/Casablanca" },
            { "UTC", "Etc/GMT" },
            { "GMT Standard Time", "Europe/London" },
            { "Greenwich Standard Time", "Atlantic/Reykjavik" },
            { "W. Europe Standard Time", "Europe/Berlin" },
            { "Central Europe Standard Time", "Europe/Budapest" },
            { "Romance Standard Time", "Europe/Paris" },
            { "Central European Standard Time", "Europe/Warsaw" },
            { "W. Central Africa Standard Time", "Africa/Lagos" },
            { "Namibia Standard Time", "Africa/Windhoek" },
            { "Jordan Standard Time", "Asia/Amman" },
            { "GTB Standard Time", "Europe/Bucharest" },
            { "Middle East Standard Time", "Asia/Beirut" },
            { "Egypt Standard Time", "Africa/Cairo" },
            { "Syria Standard Time", "Asia/Damascus" },
            { "South Africa Standard Time", "Africa/Johannesburg" },
            { "FLE Standard Time", "Europe/Kiev" },
            { "Turkey Standard Time", "Europe/Istanbul" },
            { "Israel Standard Time", "Asia/Jerusalem" },
            { "Kaliningrad Standard Time", "Europe/Kaliningrad" },
            { "Libya Standard Time", "Africa/Tripoli" },
            { "Arabic Standard Time", "Asia/Baghdad" },
            { "Arab Standard Time", "Asia/Riyadh" },
            { "Belarus Standard Time", "Europe/Minsk" },
            { "Russian Standard Time", "Europe/Moscow" },
            { "E. Africa Standard Time", "Africa/Nairobi" },
            { "Iran Standard Time", "Asia/Tehran" },
            { "Arabian Standard Time", "Asia/Dubai" },
            { "Azerbaijan Standard Time", "Asia/Baku" },
            { "Russia Time Zone 3", "Europe/Samara" },
            { "Mauritius Standard Time", "Indian/Mauritius" },
            { "Georgian Standard Time", "Asia/Tbilisi" },
            { "Caucasus Standard Time", "Asia/Yerevan" },
            { "Afghanistan Standard Time", "Asia/Kabul" },
            { "West Asia Standard Time", "Asia/Tashkent" },
            { "Ekaterinburg Standard Time", "Asia/Yekaterinburg" },
            { "Pakistan Standard Time", "Asia/Karachi" },
            { "India Standard Time", "Asia/Calcutta" },
            { "Sri Lanka Standard Time", "Asia/Colombo" },
            { "Nepal Standard Time", "Asia/Katmandu" },
            { "Central Asia Standard Time", "Asia/Almaty" },
            { "Bangladesh Standard Time", "Asia/Dhaka" },
            { "N. Central Asia Standard Time", "Asia/Novosibirsk" },
            { "Myanmar Standard Time", "Asia/Rangoon" },
            { "SE Asia Standard Time", "Asia/Bangkok" },
            { "North Asia Standard Time", "Asia/Krasnoyarsk" },
            { "China Standard Time", "Asia/Shanghai" },
            { "North Asia East Standard Time", "Asia/Irkutsk" },
            { "Singapore Standard Time", "Asia/Singapore" },
            { "W. Australia Standard Time", "Australia/Perth" },
            { "Taipei Standard Time", "Asia/Taipei" },
            { "Ulaanbaatar Standard Time", "Asia/Ulaanbaatar" },
            { "Tokyo Standard Time", "Asia/Tokyo" },
            { "Korea Standard Time", "Asia/Seoul" },
            { "Yakutsk Standard Time", "Asia/Yakutsk" },
            { "Cen. Australia Standard Time", "Australia/Adelaide" },
            { "AUS Central Standard Time", "Australia/Darwin" },
            { "E. Australia Standard Time", "Australia/Brisbane" },
            { "AUS Eastern Standard Time", "Australia/Sydney" },
            { "West Pacific Standard Time", "Pacific/Port_Moresby" },
            { "Tasmania Standard Time", "Australia/Hobart" },
            { "Magadan Standard Time", "Asia/Magadan" },
            { "Vladivostok Standard Time", "Asia/Vladivostok" },
            { "Russia Time Zone 10", "Asia/Srednekolymsk" },
            { "Central Pacific Standard Time", "Pacific/Guadalcanal" },
            { "Russia Time Zone 11", "Asia/Kamchatka" },
            { "New Zealand Standard Time", "Pacific/Auckland" },
            { "UTC+12", "Etc/GMT-12" },
            { "Fiji Standard Time", "Pacific/Fiji" },
            { "Tonga Standard Time", "Pacific/Tongatapu" },
            { "Samoa Standard Time", "Pacific/Apia" },
            { "Line Islands Standard Time", "Pacific/Kiritimati" }
        };

        private static Regex olson = new Regex("^[A-Za-z]+/[A-Za-z0-0_-]+$");

        /// <summary>Maps a Windows timezone to the Olson equivalent. Returns null failure</summary>
        public static string Olson(this TimeZoneInfo self)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string value = null;
                mapping.TryGetValue(self.Id, out value);
                return value;
            }
            else if (olson.IsMatch(self.DisplayName))
            {
                return self.DisplayName;
            }
            else
            {
                return null;
            }
        }
    }
}