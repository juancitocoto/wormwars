using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace WormWars.Core
{
    // Thin wrapper over ipinfo.io's IP-to-location lookup. Pass an API token for
    // production use (https://ipinfo.io/signup) - the unauthenticated tier is
    // rate-limited and shared across every caller on the same egress IP.
    public class IpGeolocationClient
    {
        const string DefaultBaseUrl = "https://ipinfo.io";
        const int DefaultTimeoutSeconds = 10;

        readonly string _baseUrl;
        readonly string _apiToken;
        readonly int _timeoutSeconds;

        public IpGeolocationClient(string apiToken = null, string baseUrl = DefaultBaseUrl, int timeoutSeconds = DefaultTimeoutSeconds)
        {
            _apiToken = apiToken;
            _baseUrl = baseUrl.TrimEnd('/');
            _timeoutSeconds = timeoutSeconds;
        }

        public async Task<IpGeolocationResult> LookupAsync(string ipAddress, CancellationToken cancellationToken = default)
        {
            // Require a parseable IP (not a hostname) so the value can't be used to redirect
            // the request somewhere other than ipinfo.io's per-IP endpoint.
            if (!IPAddress.TryParse(ipAddress ?? string.Empty, out _))
            {
                return IpGeolocationResult.Failure(ipAddress, "Value is not a valid IP address.");
            }

            var url = $"{_baseUrl}/{UnityWebRequest.EscapeURL(ipAddress)}/json";
            if (!string.IsNullOrEmpty(_apiToken))
            {
                url += $"?token={UnityWebRequest.EscapeURL(_apiToken)}";
            }

            using var request = UnityWebRequest.Get(url);
            request.timeout = _timeoutSeconds;
            using var registration = cancellationToken.Register(request.Abort);

            await request.SendWebRequest();
            cancellationToken.ThrowIfCancellationRequested();

            if (request.result != UnityWebRequest.Result.Success)
            {
                return IpGeolocationResult.Failure(ipAddress, $"{request.error} (HTTP {request.responseCode})");
            }

            try
            {
                var dto = JsonUtility.FromJson<IpInfoResponseDto>(request.downloadHandler.text);
                return ToResult(dto, ipAddress);
            }
            catch (Exception ex)
            {
                return IpGeolocationResult.Failure(ipAddress, $"Failed to parse response: {ex.Message}");
            }
        }

        static IpGeolocationResult ToResult(IpInfoResponseDto dto, string requestedIp)
        {
            var result = new IpGeolocationResult
            {
                Success = true,
                Ip = string.IsNullOrEmpty(dto.ip) ? requestedIp : dto.ip,
                CountryCode = dto.country,
                Region = dto.region,
                City = dto.city,
                PostalCode = dto.postal,
                Timezone = dto.timezone,
                Organization = dto.org,
            };

            var locParts = dto.loc?.Split(',');
            if (locParts?.Length == 2
                && double.TryParse(locParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                && double.TryParse(locParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var lng))
            {
                result.Latitude = lat;
                result.Longitude = lng;
            }

            return result;
        }

        [Serializable]
        class IpInfoResponseDto
        {
            public string ip;
            public string city;
            public string region;
            public string country;
            public string loc;
            public string org;
            public string postal;
            public string timezone;
        }
    }
}
