using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViewModelLib;
namespace Server.Database
{
    public interface IImagesDB
    {
        public ImageInfoGet GetImage(string fileName, string mas);
        public void ClearDB();
        public void AddToDB(ImageInfoGet imageInfo);

        public List<string> GetStatistics();
       
        //public ImageInfoGet GetImage(string fileName);
    }
    public class InMemoryDataBase : IImagesDB
    {
        public Database.ApplicationContext db;

        public void AddToDB(ImageInfoGet imageInfo)
        {
            db = new Database.ApplicationContext();
            Database.ImageDetails imageDetails = new Database.ImageDetails { Image = Convert.FromBase64String(imageInfo.JpegCover) };
            db.Images.Add(new Database.ImageData { Path = imageInfo.Path, Name = imageInfo.Name, ClassName = imageInfo.Class, Confidence = imageInfo.Confidence, Details = imageDetails });
            db.SaveChanges();
        }

        public void ClearDB()
        {
            db = new Database.ApplicationContext();
           
            db.Images.RemoveRange(db.Images);
      
            db.SaveChanges();
            
        }

        public ImageInfoGet GetImage(string fileName, string mas)
        {
            ImageInfoGet tmp = new ImageInfoGet();
            tmp.Path = fileName;
            tmp.JpegCover = mas;
          
            db = new Database.ApplicationContext();
            foreach(var img in db.Images)
            {
                if (fileName == img.Path)
                {
                    var mas1 = Convert.FromBase64String(mas);
                    IStructuralEquatable equ = mas1;
                    var masFromDB = img.Details.Image;
                    if (equ.Equals(masFromDB, EqualityComparer<object>.Default))
                    {
                        img.count++;
                        db.SaveChanges();
                        tmp.Name = img.Name;
                        tmp.Class = img.ClassName;
                        tmp.Confidence = img.Confidence;
                        return tmp;

                    }
                }
            }
            return null;
        }

        public List<string> GetStatistics()
        {
            List<string> result = new List<string>();
            db = new Database.ApplicationContext();
            var q = from img in db.Images select img;
            foreach (var img in q)
            {
                string res = "";
                res += img.Path + ";    Number of requests: " + img.count;
                result.Add(res);
            }
            return result;
        }
    }
}
