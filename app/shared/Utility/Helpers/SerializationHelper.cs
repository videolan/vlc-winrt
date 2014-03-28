/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Newtonsoft.Json;

namespace VLC_WINRT.Utility.Helpers
{
    public static class SerializationHelper
    {
        public static async Task<string> ReadFromFile(string fileName, StorageFolder folder = null)
        {
            folder = folder ?? ApplicationData.Current.LocalFolder;
            try
            {
                var file = await folder.GetFileAsync(fileName);

                using (var fs = await file.OpenAsync(FileAccessMode.Read))
                {
                    using (var inStream = fs.GetInputStreamAt(0))
                    {
                        using (var reader = new DataReader(inStream))
                        {
                            await reader.LoadAsync((uint)fs.Size);
                            string data = reader.ReadString((uint)fs.Size);
                            reader.DetachStream();
                            return data;
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Debug.WriteLine(String.Format("{0}: File not found", fileName));
                return null;
            }
        }
        #region XML
        public static async Task SerializeAsXml<T>(this T objectGraph, string fileName, StorageFolder folder = null, CreationCollisionOption options = CreationCollisionOption.FailIfExists)
        {
            folder = folder ?? ApplicationData.Current.LocalFolder;
            try
            {
                var file = await folder.CreateFileAsync(fileName, options);

                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    var ser = new XmlSerializer(typeof(T));
                    ser.Serialize(stream, objectGraph);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }
        }
        public static async Task<T> LoadFromXmlFile<T>(
            string fileName,
            StorageFolder folder = null)
        {
            var xmlString = await ReadFromFile(fileName, folder);
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xmlString));
            var ser = new XmlSerializer(typeof(T));
            T result = (T)ser.Deserialize(ms);
            return result;
        }

        #endregion

        #region JSON
        public async static Task SerializeAsJson<T>(
            this T objectGraph,
            string fileName,
            StorageFolder folder = null,
            CreationCollisionOption options = CreationCollisionOption.FailIfExists)
        {
            folder = folder ?? ApplicationData.Current.LocalFolder;

            try
            {
                var file = await folder.CreateFileAsync(fileName, options);
                string json = JsonConvert.SerializeObject(objectGraph);
                await FileIO.WriteTextAsync(file, json);
                //using (var stream = await file.OpenStreamForWriteAsync())
                //{
                //var ser = new DataContractJsonSerializer(typeof(T));
                //ser.WriteObject(stream, objectGraph);
                //}
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                if (Debugger.IsAttached)
                    Debugger.Break();

                throw;
            }
        }

        public static string SerializeAsJson<T>(this T graph)
        {
            if (graph == null)
                return null;

            var ser = new DataContractJsonSerializer(typeof(T));
            var ms = new MemoryStream();
            ser.WriteObject(ms, graph);
            var bytes = ms.ToArray();

            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public static async Task<T> LoadFromJsonFile<T>(string fileName, StorageFolder folder = null) where T : new()
        {
            var json = await ReadFromFile(fileName, folder);
            if (json == null)
                return new T();
            return LoadFromJsonString<T>(json);
        }

        public static T LoadFromJsonString<T>(string json)
        {
            //var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            //var ser = new DataContractJsonSerializer(typeof(T));
            //T result = (T)ser.ReadObject(ms);

            return JsonConvert.DeserializeObject<T>(json);
        }
        #endregion
    }
}
