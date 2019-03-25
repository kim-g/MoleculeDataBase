using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoleculeDataBase
{
    [Serializable]
    public class UserTransport : Serializable
    {
        public int id;
        public string Surname;
        public string Name;
        public string SecondName;
        public int LaboratoryID;
        public string LaboratoruName;
        public string LaboratoryAbbr;
        public string Login;
        public string Job;
        public int Permissions;
    }
}
