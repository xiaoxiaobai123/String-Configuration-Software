using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Burner_AdvanTech
{

    class GCanDrvier
    {
        //public static bool CanDriverStatusChanged
        //{
        //    set; get;
        //} = false;

        //public static bool CanDetected
        //{
        //    set; get;
        //} = false;
    }

    class TextHint : INotifyPropertyChanged
    {
        public static string hint;
        public string Hint
        {
            get { return hint; }
            set
            {
                hint = value;
                InvokePropertyChanged(new PropertyChangedEventArgs("Hint"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void InvokePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }
    }
}
