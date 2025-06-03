using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprend_Tissage.AppClasses
{
    public class Clavier
    {
        public string Id { get; set; }

        public string Nom { get; set; }

        public List<char> alphabet { get; set; } = [];

        public TimeSpan Score { get; set; }

        public int[] Separateurs { get; set; } = [];

    }
}
