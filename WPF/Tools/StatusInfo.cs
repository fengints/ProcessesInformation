using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Tools
{
    public class StatusInfo : INotifyPropertyChanged
    {
        public StatusInfo()
        {

        }
        public StatusInfo(int statusMin, int statusMax)
        {
            this.StatusMin = statusMin;
            this.StatusMax = statusMax;
        }
        private int status;
        private int statusMax;
        private int statusMin;

        public int CurrentStatus { get => status; set { status = value; OnPropertyChanged(nameof(CurrentStatus)); } }
        public int StatusMin { get => statusMin; set { statusMin = value; OnPropertyChanged(nameof(StatusMin)); } }
        public int StatusMax { get => statusMax; set { statusMax = value; OnPropertyChanged(nameof(StatusMax)); } }


        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
