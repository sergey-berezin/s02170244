using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Server.Database;
using ViewModelLib;
namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private IImagesDB dB;
        public DataController(IImagesDB imagesDB)
        {
            dB = imagesDB;
        }
        [HttpGet]
        public List<ClassNames> GetClasses()
        {
            Console.Out.WriteLine("GetClasses");
            return dB.GetClasses();
        }
        [HttpGet ("{id}")]
        public List<ImgNames> GetImages(string id)
        {
            Console.Out.WriteLine("GetImages");
            return dB.GetImages(id);
        }
    }
}
