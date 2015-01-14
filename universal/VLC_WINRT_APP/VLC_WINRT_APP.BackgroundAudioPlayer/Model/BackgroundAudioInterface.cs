using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

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

        public static BackgroundTrackItem DeserializeObjectAudioTrack(this string s)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(BackgroundTrackItem));
            StringReader textReader = new StringReader(s);

            return xmlSerializer.Deserialize(textReader) as BackgroundTrackItem;
        }

        public static string SerializeAudioTracks(object tracks)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<BackgroundTrackItem>));
            StringWriter textWriter = new StringWriter();

            xmlSerializer.Serialize(textWriter, tracks);
            return textWriter.ToString();
        }

        public static object DeserializeAudioTracks(string tracksString)
        {
            XmlSerializer xmlDeSerializer = new XmlSerializer(typeof(List<BackgroundTrackItem>));
            StringReader textReader = new StringReader(tracksString);

            return xmlDeSerializer.Deserialize(textReader) as List<BackgroundTrackItem>;
        } 
    }
}
