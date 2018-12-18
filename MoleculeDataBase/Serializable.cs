using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MoleculeDataBase
{
    [Serializable]
    public class Serializable
    {
        public void SaveToFile(string FileName)
        {
            using (FileStream fs = File.OpenWrite(FileName))
            {
                MemoryStream ms = ToBin();
                ms.CopyTo(fs);
            }
        }

        /// <summary>
        /// Сериализация в битовый формат
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToBin()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Сериализация в SOAP формат
        /// </summary>
        /// <returns></returns>
        public string ToSOAP()
        {
            SoapFormatter formatter = new SoapFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            MemoryStream ms = new MemoryStream();
            formatter.Serialize(ms, this);
            ms.Position = 0;
            byte[] ResByte = new byte[ms.Length];
            ms.Read(ResByte, 0, ResByte.Length);
            return Encoding.UTF8.GetString(ResByte);
        }

        /// <summary>
        /// Сериализация в XML поток
        /// </summary>
        /// <returns></returns>
        public MemoryStream ToXMLStream()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());
            MemoryStream ms = new MemoryStream();
            serializer.Serialize(ms, this);
            ms.Position = 0;
            return ms;
        }

        /// <summary>
        /// Сериализация в XML
        /// </summary>
        /// <returns></returns>
        public string ToXML()
        {
            MemoryStream ms = ToXMLStream();
            byte[] buff = new byte[ms.Length];
            ms.Read(buff, 0, buff.Length);
            return Encoding.UTF8.GetString(buff);
        }

        /// <summary>
        /// Десериализация из битового формата
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static object FromBin(Stream ms)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            ms.Position = 0;            
            return formatter.Deserialize(ms);
        }

        /// <summary>
        /// Десериализация из SOAP формата
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static Serializable FromSOAP(Stream ms)
        {
            SoapFormatter formatter = new SoapFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            ms.Position = 0;
            return (Serializable)formatter.Deserialize(ms);
        }

        /// <summary>
        /// Десериализация из SOAP формата
        /// </summary>
        /// <param name="SOAP"></param>
        /// <returns></returns>
        public static Serializable FromSOAP(string SOAP)
        {
            return FromSOAP(new MemoryStream(Encoding.UTF8.GetBytes(SOAP)));
        }

        /// <summary>
        /// Десериализация из XML
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static T From_XML<T>(Stream ms)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            ms.Position = 0;
            return (T)serializer.Deserialize(ms);
        }

        public static T From_XML<T>(string Data)
        {
            return From_XML<T>(new MemoryStream(Encoding.UTF8.GetBytes(Data)));
        }

        static public Serializable LoadFromFile(string FileName)
        {
            using (FileStream fs = File.OpenRead(FileName))
            {
                return (Serializable)FromBin(fs);
            }
        }

        static public T LoadFromXMLFile<T>(string FileName)
        {
            using (FileStream fs = File.OpenRead(FileName))
            {
                return From_XML<T>(fs);
            }
        }
    }
}
