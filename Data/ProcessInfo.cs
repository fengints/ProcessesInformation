using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class ProcessInfo : INotifyPropertyChanged
    {
        private int id;
        private Icon icon1;
        private string name;
        private string fullName;
        private string cMDString;
        private string userName;
        private string bitDepth;
        private bool? isElevated;
        private bool? sign;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        public Icon icon
        {
            get => icon1;
            set
            {
                icon1 = value;
                OnPropertyChanged(nameof(icon));
            }
        }
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                OnPropertyChanged(nameof(FullName));
            }
        }
        public string CMDString
        {
            get => cMDString; set
            {
                cMDString = value;
                OnPropertyChanged(nameof(CMDString));
            }
        }
        public string UserName
        {
            get => userName; set
            {
                userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }
        public string BitDepth
        {
            get => bitDepth;
            set
            {
                bitDepth = value;
                OnPropertyChanged(nameof(BitDepth));
            }
        }
        public bool? IsElevated
        {
            get => isElevated; set
            {
                isElevated = value;
                OnPropertyChanged(nameof(IsElevated));
            }
        }
        public bool? Sign
        {
            get => sign; set
            {
                sign = value;
                OnPropertyChanged(nameof(Sign));
            }
        }

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
