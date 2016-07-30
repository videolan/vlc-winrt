using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VLC.Model
{
    public class SerializableList<T> : List<T>, IXmlSerializable
    {
        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            XmlSerializer objSerializer = new XmlSerializer(typeof(T));

            writer.WriteStartElement("array");
            foreach (var item in this)
            {
                objSerializer.Serialize(writer, item);
            }
            writer.WriteEndElement();
        }
    }
}
