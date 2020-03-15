using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Rest.Net;
using Rest.Net.Interfaces;

namespace GoogleAnalytics
{
    public enum SessionControl { None, Start, End }

    public class Tracker
    {
        public string TrackingId { get; private set; }

        public string DataSource { get; set; }

        public string ApplicationName { get; set; }

        public string ApplicationId { get; set; }

        public string ApplicationVersion { get; set; }

        public string ApplicationInstallerId { get; set; }

        public bool AnonymizeIp { get; set; }
        
        private RestClient _restClient = new RestClient("https://www.google-analytics.com");

        public Tracker(string trackingId)
        {
            TrackingId = trackingId;
        }

        public async Task TrackPageview(string path, string title, string customerId, string userId = null,
            string screenResolution = null, string viewportSize = null, string documentEncoding = null,
            string screenColors = null, string userLanguage = null,
            SessionControl session = SessionControl.None)
        {
            List<KeyValuePair<string, string>> parameters = GetCommonParameters("pageview", customerId, userId);
            parameters.Add(new KeyValuePair<string, string>("dp", path));

            AddParameterIfNotEmpty(parameters, "dt", title);
            if (session != SessionControl.None)
            {
                parameters.Add(new KeyValuePair<string, string>("sc", session == SessionControl.Start ? "start" : "end"));
            }

            await SendCollectRequest(parameters);
        }

        public async Task StartTrackingPageview(string path, string title, string customerId, string userId = null,
            string screenResolution = null, string viewportSize = null, string documentEncoding = null,
            string screenColors = null, string userLanguage = null)
        {
            await TrackPageview(path, title, customerId, userId, screenResolution, viewportSize, documentEncoding, screenColors, userLanguage, SessionControl.Start);
        }

        public async Task StopTrackingPageview(string path, string title, string customerId, string userId = null,
            string screenResolution = null, string viewportSize = null, string documentEncoding = null,
            string screenColors = null, string userLanguage = null)
        {
            await TrackPageview(path, title, customerId, userId, screenResolution, viewportSize, documentEncoding, screenColors, userLanguage, SessionControl.End);
        }

        public async Task TrackEvent(string category, string action, string label, int? value, string customerId, string userId)
        {
            List<KeyValuePair<string, string>> parameters = GetCommonParameters("event", customerId, userId);
            parameters.Add(new KeyValuePair<string, string>("ec", category));
            parameters.Add(new KeyValuePair<string, string>("ea", action));

            AddParameterIfNotEmpty(parameters, "el", label);
            if (value != null)
            {
                AddParameterIfNotEmpty(parameters, "ev", value.ToString());
            }

            await SendCollectRequest(parameters);
        }

        public async Task TrackException(string description, bool fatal, string customerId, string userId)
        {
            List<KeyValuePair<string, string>> parameters = GetCommonParameters("exception", customerId, userId);
            parameters.Add(new KeyValuePair<string, string>("exd", description));
            parameters.Add(new KeyValuePair<string, string>("exf", fatal ? "1" : "0"));
            await SendCollectRequest(parameters);
        }

        public async Task TrackTime(string category, string variableName, string label, long timeInMs, string customerId, string userId)
        {
            List<KeyValuePair<string, string>> parameters = GetCommonParameters("timing", customerId, userId);
            parameters.Add(new KeyValuePair<string, string>("utc", category));
            parameters.Add(new KeyValuePair<string, string>("utv", variableName));
            parameters.Add(new KeyValuePair<string, string>("utt", timeInMs.ToString()));

            AddParameterIfNotEmpty(parameters, "utl", label);
            
            await SendCollectRequest(parameters);
        }
        
        private async Task SendCollectRequest(List<KeyValuePair<string, string>> parameters)
        {
            IRestRequest request = new RestRequest("/collect", Http.Method.POST);
            FormUrlEncodedContent content = new FormUrlEncodedContent(parameters);
            request.SetContent(content);
            var trackResult = await _restClient.ExecuteAsync(request);
        }

        private List<KeyValuePair<string, string>> GetCommonParameters(string hitType, string customerId, string userId)
        {
            List <KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("v", "1"),
                new KeyValuePair<string, string>("tid", TrackingId),
                new KeyValuePair<string, string>("cid", customerId),
                new KeyValuePair<string, string>("t", hitType)
            };

            AddParameterIfNotEmpty(parameters, "uid", userId);
            AddParameterIfNotEmpty(parameters, "ds", DataSource);
            AddParameterIfNotEmpty(parameters, "an", ApplicationName);
            AddParameterIfNotEmpty(parameters, "aid", ApplicationId);
            AddParameterIfNotEmpty(parameters, "av", ApplicationVersion);
            AddParameterIfNotEmpty(parameters, "aiid", ApplicationInstallerId);

            return parameters;
        }

        private void AddParameterIfNotEmpty(List<KeyValuePair<string, string>> parameters, string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                parameters.Add(new KeyValuePair<string, string>(name, value));
            }
        }
    }
}
