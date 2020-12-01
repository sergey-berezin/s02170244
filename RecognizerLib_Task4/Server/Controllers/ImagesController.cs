using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Database;
using ViewModelLib;
using Newtonsoft.Json;
using RecognizerLib;
using System.Net.Http;
using System.Net;
using System.Windows;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private IImagesDB dB;
        public ImagesController(IImagesDB imagesDB)
        {
            dB = imagesDB;
        }

        [HttpPut]
        public async Task<ImageInfoGet> Put([FromBody]object imageForServer)
        {
            Console.Out.WriteLine("Put");
            var des = JsonConvert.DeserializeObject<ImageInfoGet>(imageForServer.ToString());

            var tmp = new ImageInfoGet();
            var image = dB.GetImage(des.Path, des.JpegCover);
           
            if (image == null)
            {
                ImageRecognizer imageRecognizer = new ImageRecognizer();

                var info = imageRecognizer.RecognizeImage(Convert.FromBase64String(des.JpegCover), des.Path);
               
                tmp.Class = info.className[0];
                tmp.Confidence = info.confidence[0];

                tmp.Name = des.Name;
                tmp.Path = des.Path;
                tmp.JpegCover = des.JpegCover;

                dB.AddToDB(tmp);

                return tmp;
            }
            return image;
        }

        [HttpGet("{id}")]
        public void Clear(int id)
        {
            Console.Out.WriteLine("Clear");
            dB.ClearDB();
        }
        [HttpGet]
        public List<string> GetStats()
        {
            Console.Out.WriteLine("Statistics");
            return dB.GetStatistics();
        }

    }
}
