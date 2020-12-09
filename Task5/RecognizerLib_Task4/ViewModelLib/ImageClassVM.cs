using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModelLib
{
    public class ImageClassVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string type;
        int count = 0;
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }
        public ImageClassVM(string name, int c)
        {
            Type = name;
            Count = c;
        }
    }
}
