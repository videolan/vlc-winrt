using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace VLC.Model
{
    [XmlRoot("data")]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {
        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }
        public void ReadXml(System.Xml.XmlReader reader)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();
            if (wasEmpty)
                return;
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement("item");
                reader.ReadStartElement("key");
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();
                reader.ReadStartElement("value");
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();
                this.Add(key, value);
                reader.ReadEndElement();
                reader.MoveToContent();
            }
            reader.ReadEndElement();
        }
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));
            writer.WriteStartElement("value");
            writer.WriteStartElement("struct");
            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement("member");

                writer.WriteStartElement("name");
                writer.WriteString(key.ToString());
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                TValue value = this[key];
                if (value is string)
                    writer.WriteStartElement("string");
                else if (value is double)
                    writer.WriteStartElement("double");
                writer.WriteString(value.ToString());

                writer.WriteEndElement();
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
        #endregion
    }
}