using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModelLib
{
    public class ImageInfoGet
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public float Confidence { get; set; }
        public string JpegCover { get; set; }
        public ImageInfoGet()
        {

        }
    }
}
