using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using OpenBabel;

namespace MoleculeDataBase
{
    [Serializable]
    public class OBMolecule : OBMol
    {
        public OBMolecule() : base()
        {

        }

        public OBMolecule(OBMol mol) : base(mol)
        {

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
        /// Десериализация из битового формата
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static OBMolecule FromBin(Stream ms)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            // получаем поток, куда будем записывать сериализованный объект
            ms.Position = 0;
            return (OBMolecule)formatter.Deserialize(ms);
        }
    }
}
