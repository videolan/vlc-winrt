using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Storage;
using VLC_WinRT.Utils;
using System.Xml.Serialization;
using System.IO;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Video;
using Windows.Data.Xml.Dom;

namespace VLC_WinRT.MediaMetaFetcher.Fetchers
{
    public class OpenSubtitleClient
    {
        private readonly string OpenSubtitlesAPIUrl = "http://api.opensubtitles.org/xml-rpc";
        private readonly string HTTP_USER_AGENT = "VLSub";
        private readonly string USER_AGENT = "VLSub 0.9";

        private bool configured = false;
        private string token;

        private async Task<bool> Initialize()
        {
            try
            {
                var client = new XmlHttpClient();
                var xml = await client.PostXmlAsync(OpenSubtitlesAPIUrl, "LogIn", new List<string> { "", "", "fre", USER_AGENT });
                var tokenNode = xml.GetElementsByTagName("name");
                token = System.Net.WebUtility.HtmlDecode(tokenNode[0].NextSibling.NextSibling.InnerText.ToString()).Trim();
                if (!string.IsNullOrEmpty(token))
                    configured = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("The app failed to get the OpenSubtitle token : " + e.ToString());
            }
            return configured;
        }

        public async Task<string> GetSubtitleUrl(VideoItem video)
        {
            if (!configured)
            {
                if (!await Initialize())
                {
                    return null;
                }
            }

            var file = await StorageFile.GetFileFromPathAsync(video.Path);
            var hash = await FileUtils.ComputeHash(file);
            var size = (await file.GetBasicPropertiesAsync()).Size;

            var client = new XmlHttpClient();

            var parameters = new List<object>();
            parameters.Add(token);
            parameters.Add(new SerializableList<SerializableDictionary<string, object>>()
            {
                new SerializableDictionary<string, object>
                {
                    { "sublanguageid", "eng" },
                    { "moviehash", hash },
                    { "moviebytesize", (double)size }
                }
            });
            var xml = await client.PostXmlAsync(OpenSubtitlesAPIUrl, "SearchSubtitles", parameters);
            var nodes = xml.GetElementsByTagName("name");
            for (uint i = 0; i < nodes.Count; i++)
            {
                var node = nodes.Item(i);
                if (node.InnerText == "ZipDownloadLink")
                {
                    return node.NextSibling.NextSibling.InnerText.Trim();
                }
            }
            return string.Empty;
        }

        public async Task<byte[]> DownloadSubtitle(VideoItem video, string uri)
        {
            var client = new HttpClient();
            var httpRequest = await client.GetAsync(uri);
            if (httpRequest.IsSuccessStatusCode)
            {
                return await httpRequest.Content.ReadAsByteArrayAsync();
            }
            return null;
        }
    }

    public class XmlHttpClient
    {
        HttpClient client = new HttpClient();

        public async Task<XmlDocument> PostXmlAsync(string uri, string rootNode, IEnumerable<object> parameters)
        {
            var xmlDoc = new XmlDocument();
            var methodCall = xmlDoc.CreateElement("methodCall");
            xmlDoc.AppendChild(methodCall);

            var methodName = xmlDoc.CreateElement("methodName");
            methodName.InnerText = rootNode;
            methodCall.AppendChild(methodName);

            var paramsElement = xmlDoc.CreateElement("params");
            foreach (var parameter in parameters)
            {
                var parameterNode = xmlDoc.CreateElement("param");
                var parameterValueNode = xmlDoc.CreateElement("value");

                parameterNode.AppendChild(parameterValueNode);
                paramsElement.AppendChild(parameterNode);
            }
            methodCall.AppendChild(paramsElement);

            var valueNodes = methodCall.GetElementsByTagName("value");
            int i = 0;
            foreach (var param in parameters)
            {
                var serializer = new XmlSerializer(param.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    using (var xmlWriter = System.Xml.XmlWriter.Create(textWriter))
                    {
                        serializer.Serialize(xmlWriter, param);
                    }
                    var paramDoc = new XmlDocument();
                    paramDoc.LoadXml(textWriter.ToString());

                    if (paramDoc.ChildNodes[1].ChildNodes.Count > 0)
                    {
                        valueNodes[i].InnerText = paramDoc.ChildNodes[1].ChildNodes[0].GetXml();
                    }
                    else
                    {
                        valueNodes[i].InnerText = paramDoc.ChildNodes[1].GetXml();
                    }
                }
                i++;
            }

            var strContent = System.Net.WebUtility.HtmlDecode(xmlDoc.GetXml());

            var httpContent = new StringContent(strContent, Encoding.UTF8, "text/xml");
            var httpResponse = await client.PostAsync(uri, httpContent);
            if (httpResponse.IsSuccessStatusCode)
            {
                var str = await httpResponse.Content.ReadAsStringAsync();
                var xmlResponse = new XmlDocument();
                xmlResponse.LoadXml(System.Net.WebUtility.HtmlDecode(str.Replace("\n", "")));
                return xmlResponse;
            }
            return null;
        }
    }
}