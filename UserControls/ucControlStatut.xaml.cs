using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apprend_Tissage.UserControls
{
    /// <summary>
    /// Logique d'interaction pour ucControlStatut.xaml
    /// </summary>
    public partial class ucControlStatut : UserControl
    {
        public ucControlStatut()
        {
            InitializeComponent();
        }

        public void Set(Brush col)
        {
            bdResult.Background = col;
        }

        public void Set(Brush col, string txt)
        {
            bdResult.Background = col;
            tbResult.Text = txt;
        }

        public void SetOk()
        {
            Set(Brushes.YellowGreen, " ✓ - CORRECT");
        }

        public void SetNotOk()
        {
            Set(Brushes.Red, " ✗ - INCORRECT");
        }

        internal void SetDefault(string txt = "")
        {
            Set(Brushes.DarkGray, txt);
        }
    }
}
