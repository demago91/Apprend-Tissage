using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprend_Tissage
{
    public class Phrase
    {
        public string Texte { get; set; }

        public int Reussie { get; set; } = 0;

        public string ContexteEcriture { get; set; } = "";

        public string EstReussi
        {
            get
            {
                string str = Reussie == 0 ? "non" : "OUI";
                return str;
            }
        }

        public bool IsFill
        {
            get
            {
                return !string.IsNullOrEmpty(Texte);
            }
        }

        public override string ToString()
        {
            return Texte;
        }
    }
}
