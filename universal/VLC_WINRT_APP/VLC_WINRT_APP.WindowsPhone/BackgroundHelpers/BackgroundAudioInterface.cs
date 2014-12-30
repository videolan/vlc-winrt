using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.BackgroundHelpers
{
    public static class AudioBackgroundInterface
    {
        public static string SerializeObjectAudioTrack(this TrackItem toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static object DeserializeObjectAudioTrack(this string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrackItem));
            StringReader textReader = new StringReader(s);

            return xmlSerializer.Deserialize(textReader);
        }

        public static string SerializeObjectListTrack(this object toSerialize)
        {
            List<TrackItem> local = toSerialize as List<TrackItem>;
            XmlSerializer xmlSerializer = new XmlSerializer(local.GetType());
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }

        public static object DeserializeObjectListTrack(this string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TrackItem>));
            StringReader textReader = new StringReader(s);

            return xmlSerializer.Deserialize(textReader);
        }
    }
}
