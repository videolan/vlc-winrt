using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using Windows.Data.Json;
using Newtonsoft.Json;

namespace VLC_WINRT_APP.BackgroundAudioPlayer
{
    public static class AudioBackgroundInterface
    {
        public static string SerializeObjectAudioTrack(this BackgroundTrackItem toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static object DeserializeObjectAudioTrack(this string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BackgroundTrackItem));
            StringReader textReader = new StringReader(s);

            return xmlSerializer.Deserialize(textReader);
        }

        public static string SerializeObjectListTrack(this object toSerialize)
        {
            List<BackgroundTrackItem> local = toSerialize as List<BackgroundTrackItem>;
            XmlSerializer xmlSerializer = new XmlSerializer(local.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static object DeserializeObjectListTrack(this string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<BackgroundTrackItem>));
            StringReader textReader = new StringReader(s);
            return xmlSerializer.Deserialize(textReader);
        }
    }
}
