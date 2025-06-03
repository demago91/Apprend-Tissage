using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprend_Tissage.AppClasses
{
    internal class CboxItem
    {
        public int Value { get; set; }
        public string DisplayValue { get; set; }

        public CboxItem()
        {
            
        }

        public CboxItem(string txt, int val)
        {
             Value = val;
            DisplayValue = txt;
        }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
