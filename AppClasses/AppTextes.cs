using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprend_Tissage.AppClasses
{
    public class AppTextes
    {
        public Dictionary<string, string> Textes = [];

        public void Ajouter(string cle, string texte)
        {
            Textes.Add(cle, texte);
        }

        public string Get(string cle)
        {
            if(Textes.TryGetValue(cle, out string? value))
                return value;

            return string.Empty;
        }
    }
}
