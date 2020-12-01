using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<ImageData> Images { get; set; }
        //public DbSet<ImageClassData> Classes { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseLazyLoadingProxies(). UseSqlite(@"Data Source = C:\Users\Маша\Documents\Visual Studio 2017\Projects\RecognizerLib — копия\Server\mylib.db");
    }
}
