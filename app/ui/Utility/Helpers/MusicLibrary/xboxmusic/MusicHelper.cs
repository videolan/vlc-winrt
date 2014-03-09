/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using XboxMusicLibrary.Models;
using XboxMusicLibrary.Settings;

namespace XboxMusicLibrary
{
    public class MusicHelper
    {
        public event ErrorEventHandler Failed;

        protected void OnFailed(ErrorEventArgs e)
        {
            if (Failed != null) Failed(this, e);
        }

        /// <summary>
        /// Get a developer authentication Access Token obtained from Azure Data Market.
        /// Used to identify the third-party application using the Xbox Music RESTful API.
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns></returns>
        public async Task<string> GetAccessToken(string clientId, string clientSecret)
        {
            string token = string.Empty;

            const string service = "https://datamarket.accesscontrol.windows.net/v2/OAuth2-13";
            const string scope = "http://music.xboxlive.com";
            const string grantType = "client_credentials";

            using (HttpClient proxy = new HttpClient())
            {
                var postData = new Dictionary<string, string>();
                postData["client_id"] = clientId;
                postData["client_secret"] = clientSecret;
                postData["scope"] = scope;
                postData["grant_type"] = grantType;

                HttpContent content = new FormUrlEncodedContent(postData);

                // Authentication Request
                HttpResponseMessage response = await proxy.PostAsync(new Uri(service), content);
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parsing content to extract the token
                    token = ExtractTokenFromJson(responseContent);
                }
                else
                {
                    // Triggers Failed Event
                    Music error = Music.PopulateObject(responseContent);
                    OnFailed(new ErrorEventArgs(error.Error));
                }
            }

            return token;
        }

        private string ExtractTokenFromJson(string json)
        {
            string token = string.Empty;

            Match match = Regex.Match(json, ".*\"access_token\":\"(?<token>.*?)\".*", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                token = match.Groups["token"].Value;
            }

            return token;
        }

        /// <summary>
        /// Access a small number of items from a media catalog.
        /// </summary>
        /// <param name="token">Required. A valid developer authentication Access Token obtained from Azure Data Market, used to identify the third-party application using the Xbox Music RESTful API.</param>
        /// <param name="namespaceids">Required. The ID or IDs to be looked up. Each ID is prefixed by a namespace and ".". You can specify from 1 to 10 IDs in your request.</param>
        /// <param name="extras">Optional. List of extra fields that can be optionally requested (at the cost of performance).</param>
        /// <param name="culture">Optional. The standard two-letter code that identifies the country/region of the user. If not specified, the value defaults to the geolocated country/region of the client's IP address. Responses will be filtered to provide only those that match the user's country/region.</param>
        /// <returns></returns>
        public async Task<Music> LookupMediaCatalog(string token, string[] namespaceids, Extras[] extras = null, Culture culture = null)
        {
            const string scope = "https://music.xboxlive.com/1/content/{0}/lookup";
            Music lookup = null;
            
            // Formatting namespace ids argument
            StringBuilder idsbuilder = new StringBuilder();
            for(int cpt = 0; cpt < namespaceids.Length; cpt++)
            {
                if (cpt != 0) idsbuilder.Append("+"); idsbuilder.Append(namespaceids[0]);
            }

            // Formatting extras argument
            StringBuilder extrasbuilder = new StringBuilder();
            if (extras != null)
            {
                for (int cpt = 0; cpt < extras.Length; cpt++)
                {
                    if (cpt != 0) extrasbuilder.Append("+"); extrasbuilder.Append(extras[cpt].ToString());
                }
            }

            // Formatting lookup request Uri
            StringBuilder service = new StringBuilder();
            service.Append(string.Format(scope, idsbuilder.ToString()));

            service.Append("?accessToken=Bearer+");
            service.Append(WebUtility.UrlEncode(token));

            if (extras != null) 
            {
                service.Append(string.Format("&extras={0}", extrasbuilder.ToString()));
            }

            if (culture != null)
            {
                service.Append(string.Format("&language={0}", culture.Language));
                service.Append(string.Format("&country={0}", culture.Country));
            }

            using (HttpClient proxy = new HttpClient())
            {
                // Lookup Request - Look up a small number of items from a media catalog.
                HttpResponseMessage response = await proxy.GetAsync(new Uri(service.ToString()));
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parsing Content to populate Music object
                    lookup = Music.PopulateObject(responseContent);
                }
                else
                {
                    // Triggers Failed Event
                    Music error = Music.PopulateObject(responseContent);
                    OnFailed(new ErrorEventArgs(error.Error));
                }
            }

            return lookup;
        }

        /// <summary>
        /// Search for a potentially large number of items from a media catalog.
        /// </summary>
        /// <param name="token">Required. A valid developer authentication Access Token obtained from Azure Data Market, used to identify the third-party application using the Xbox Music RESTful API.</param>
        /// <param name="query">Required. The search query.</param>
        /// <param name="extras">Optional. The extras query</param>
        /// <param name="maxitems">Optional. Positive integer from 1 to 25, inclusive. The maximum number of results that should be returned in the response. If this parameter is not set, the response will be limited to a maximum of 25 results. There is no guarantee that all search results will be returned in a single response; the response may contain a truncated list of responses and a continuation token.</param>
        /// <param name="filters">A subcategory of item types, in case the client is interested in only one or more specific types of items. If this parameter is not provided, the search will be performed in all categories.</param>
        /// <param name="culture">Optional. The standard two-letter code that identifies the country/region of the user. If not specified, the value defaults to the geolocated country/region of the client's IP address. Responses will be filtered to provide only those that match the user's country/region.</param>
        /// <returns></returns>
        public async Task<Music> SearchMediaCatalog(string token, string query, Extras[] extras = null, uint maxitems = 25, Filters[] filters = null, Culture culture = null)
        {
            const string scope = "https://music.xboxlive.com/1/content/music/search";
            Music search = null;

            // Formatting filters argument
            StringBuilder filtersbuilder = new StringBuilder();
            if (filters != null)
            {
                for (int cpt = 0; cpt < filters.Length; cpt++)
                {
                    if (cpt != 0) filtersbuilder.Append("+"); filtersbuilder.Append(filters[cpt].ToString().ToLower());
                }
            }
            else
            {
                filtersbuilder.Append("music");
            }
            // Formatting extras argument
            StringBuilder extrasbuilder = new StringBuilder();
            if (extras != null)
            {
                for (int cpt = 0; cpt < extras.Length; cpt++)
                {
                    if (cpt != 0) extrasbuilder.Append("+"); extrasbuilder.Append(extras[cpt].ToString());
                }
            }

            using (HttpClient proxy = new HttpClient())
            {
                StringBuilder service = new StringBuilder();
                service.Append(scope);
                service.Append("?q=");
                service.Append(WebUtility.UrlEncode(query));
                service.Append("&maxitems=");
                service.Append(maxitems.ToString());
                service.Append("&filter=");
                service.Append(filtersbuilder.ToString());
                service.Append("&accessToken=Bearer+");
                service.Append(WebUtility.UrlEncode(token));
                if (extras != null)
                {
                    service.Append(string.Format("&extras={0}", extrasbuilder.ToString()));
                }

                if (culture != null)
                {
                    service.Append(string.Format("&language={0}", culture.Language));
                    service.Append(string.Format("&country={0}", culture.Country));
                }

                // Authentication Request
                HttpResponseMessage response = await proxy.GetAsync(new Uri(service.ToString()));
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parsing Content to populate Music object
                    search = Music.PopulateObject(responseContent);
                }
                else
                {
                    // Triggers Failed Event
                    Music error = Music.PopulateObject(responseContent);
                    OnFailed(new ErrorEventArgs(error.Error));
                }
            }

            return search;
        }

        /// <summary>
        /// Request the continuation of an incomplete list of content.
        /// </summary>
        /// <param name="token">Required. A valid developer authentication Access Token obtained from Azure Data Market, used to identify the third-party application using the Xbox Music RESTful API.</param>
        /// <param name="continuationToken">Required. A Continuation Token provided in an earlier service response and optionally passed back to the service to request the continuation of an incomplete list of content.</param>
        /// <param name="namespaceids">Optional. The ID or IDs to be looked up. Each ID is prefixed by a namespace and ".". You can specify from 1 to 10 IDs in your request.</param>
        /// <returns></returns>
        public async Task<Music> LoadNext(string token, string continuationToken, string[] namespaceids = null)
        {
            const string scopesearch = "https://music.xboxlive.com/1/content/music/search";
            const string scopelookup = "https://music.xboxlive.com/1/content/{0}/lookup";

            string scope = string.Empty;
            string jsonContent = string.Empty;

            Music next = null;

            // Formatting namespace ids
            if (namespaceids != null)
            {
                StringBuilder ids = new StringBuilder();
                for (int cpt = 0; cpt < namespaceids.Length; cpt++)
                {
                    if (cpt != 0) ids.Append("+"); ids.Append(namespaceids[0]);
                }

                scope = string.Format(scopelookup, ids.ToString());
            }
            else
            {
                scope = scopesearch;
            }

            using (HttpClient proxy = new HttpClient())
            {
                StringBuilder service = new StringBuilder();
                service.Append(scope);
                service.Append("?continuationToken=");
                service.Append(continuationToken);
                service.Append("&accessToken=Bearer+");
                service.Append(WebUtility.UrlEncode(token));

                // Authentication Request
                HttpResponseMessage response = await proxy.GetAsync(new Uri(service.ToString()));
                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Parsing Content to populate Music object
                    next = Music.PopulateObject(responseContent);
                }
                else
                {
                    // Triggers Failed Event
                    Music error = Music.PopulateObject(responseContent);
                    OnFailed(new ErrorEventArgs(error.Error));
                }
            }

            return next;
        }
    }
}
