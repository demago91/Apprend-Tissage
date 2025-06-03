using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apprend_Tissage.AppClasses
{
    public class Preferences
    {
        public List<Clavier> claviers { get; set; } = [];

        public TimeSpan Temps_C1 { get; set; } 

        public TimeSpan Temps_C2 { get; set; }

        public TimeSpan Temps_C3 { get; set; }


        public double TailleX { get; set; }

        public double TailleY { get; set; }

        public double PositionX { get; set; }

        public double PositionY { get; set; }

        public void SetPosition(double x, double y)
        {
            PositionX = x;
            PositionY = y;
        }

        public void SetTaille(double x, double y)
        {
            TailleX = x;
            TailleY = y;
        }

        public void SetScore(Clavier c, TimeSpan ts)
        {
            claviers.First(x => x.Id == c.Id).Score = ts;
        }
    }
}
