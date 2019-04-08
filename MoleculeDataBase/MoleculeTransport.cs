using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MoleculeDataBase
{
    /// <summary>
    /// Класс для передачи информации о молекуле от сервера клиенту
    /// </summary>
    [Serializable]
    public class MoleculeTransport : Serializable
    {
        public int ID;
        public string Name;
        public laboratory Laboratory;
        public person Person;
        public string Structure;
        public string State;
        public string Melting_Point;
        public string Conditions;
        public string Other_Properties;
        public string Mass;
        public string Solution;
        public int Status;
        public List<string> Analysis;
        public List<file> Files;

        public void Save(string FileName)
        {
            // передаем в конструктор тип класса
            XmlSerializer formatter = new XmlSerializer(typeof(MoleculeTransport));

            // получаем поток, куда будем записывать сериализованный объект
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                formatter.Serialize(fs, this);
            }
        }
    }

    public class laboratory
    {
        public int ID;
        public string Name;
        public string Abb;
    }

    public class person
    {
        public int ID;
        public string Name;
        public string FathersName;
        public string Surname;
        public string Job;
    }

    public class file
    {
        public int ID;
        public string Name;
    }
}
