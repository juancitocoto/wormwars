using System;

namespace WormWars.Core
{
    [Serializable]
    public class IpGeolocationResult
    {
        public bool Success;
        public string ErrorMessage;
        public string Ip;
        public string CountryCode;
        public string Region;
        public string City;
        public string PostalCode;
        public string Timezone;
        public string Organization;
        public double? Latitude;
        public double? Longitude;

        public static IpGeolocationResult Failure(string ip, string errorMessage) => new IpGeolocationResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            Ip = ip,
        };
    }
}
