using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RecognizerLib;
namespace ViewModelLib
{
    public class ImageVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        ImageInfo imageInfo;
        public string Name
        {
            get
            {
                return imageInfo.name;
            }
            set
            {
                imageInfo.name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        public string Path
        {
            get
            {
                return imageInfo.path;
            }
            set
            {
                imageInfo.path = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Path)));
            }
        }
        public string ClassName
        {
            get
            {
                    return imageInfo.className[0];
                
            }
            set
            {
                imageInfo.className[0] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassName)));
            }
        }
        public float Confidence
        {
            get
            {
                
                    return imageInfo.confidence[0];
              
            }
            set
            {
                imageInfo.confidence[0] = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Confidence)));
            }
        }
        public ImageVM(string p, string n)
        {
            imageInfo = new ImageInfo();
            imageInfo.className.Add("none");
            imageInfo.confidence.Add(0);
            Path = p;
            Name = n;
        }
    }
}
