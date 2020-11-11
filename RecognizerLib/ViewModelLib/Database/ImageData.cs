using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModelLib.Database
{
    public class ImageDetails
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
    }
    public class ImageData
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string ClassName { get; set; }
        public float Confidence { get; set; }
        virtual public ImageDetails Details { get; set; }
        public int count { get; set; }
    }
}
