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
        /// Сериализация в MOL формат
        /// </summary>
        /// <returns></returns>
        public string ToMol()
        {
            OBConversion conv = new OBConversion();
            conv.SetOutFormat("mol2");
            return conv.WriteString(this);
        }

        /// <summary>
        /// Десериализация из MOL формата
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        public static OBMolecule FromMol(string Molecule)
        {
            OBMolecule mol = new OBMolecule();

            OBConversion conv = new OBConversion();
            conv.SetInFormat("mol2");
            conv.ReadString(mol, Molecule);

            return mol;
        }
    }
}
