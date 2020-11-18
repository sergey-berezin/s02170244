using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RecognizerLib;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Windows.Threading;
using System.Threading;
using System.Windows;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace ViewModelLib
{
    public interface IFolderDialog
    {
        string OpenFolder();
    }

    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Database.ApplicationContext db;

        ObservableCollection<ImageVM> images = new ObservableCollection<ImageVM>();
        public ObservableCollection<ImageVM> Images
        {
            get
            {
                return images;
            }
            set
            {
                images = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Images)));

            }
        }

        ObservableCollection<ImageClassVM> classVMs = new ObservableCollection<ImageClassVM>();
        public ObservableCollection<ImageClassVM> ClassVMs
        {
            get
            {
                return classVMs;
            }
            set
            {
                classVMs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassVMs)));

            }
        }
        public ImageClassVM selectedImgType { get; set; }
        ObservableCollection<ImageVM> selectedImages = new ObservableCollection<ImageVM>();

        public ObservableCollection<ImageVM> SelectedImages
        {
            get
            {
                return selectedImages;
            }
            set
            {
                selectedImages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImages)));

            }

        }

        int doneImgCount = 0;

        public int Progress
        {
            get
            {
                if (Images.Count() > 0)
                    return doneImgCount;
                else return 0;

            }
            set
            {
                doneImgCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Progress)));

            }
        }
        string stats;
        public string Statistics
        {
            get
            {
                return stats;
            }
            set
            {
                stats = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Statistics)));
            }
        }
        public void GetStatistics()
        {
            string res = "";
            var q = from img in db.Images select img;
            foreach(var img in q)
            {
                res += img.Path + ";    Number of requests: " + img.count + "\n";
            }
            Statistics = res;
        }
        private readonly ICommand openCommand;
        public ICommand Open { get { return openCommand; } }

        private readonly ICommand recognizeCommand;
        public ICommand Recognize { get { return recognizeCommand; } }

        private readonly ICommand stopCommand;
        public ICommand Stop { get { return stopCommand; } }

        private readonly ICommand selectCommand;
        public ICommand Select { get { return selectCommand; } }

        private readonly ICommand clearCommand;
        public ICommand Clear { get { return clearCommand; } }

        private readonly ICommand showCommand;
        public ICommand Show { get { return showCommand; } }

        bool clearFlag = false;

        IFolderDialog folderDialog;

        public Dispatcher dispatcher;
        public ImageRecognizer imageRecognizer;
        
        public MainVM(IFolderDialog dialog, Dispatcher disp = null)
        {
            dispatcher = disp;
            folderDialog = dialog;
            openCommand = new MyCommand(_ => { return true; }, _ => SelectDirectory());
            recognizeCommand = new MyCommand(_ => { return Images.Count() > 0; }, _ => RecognizeFunck());
            stopCommand = new MyCommand(_ => { return Images.Count() > 0; }, _ => imageRecognizer.Cancel());
            selectCommand = new MyCommand(_ => { return ClassVMs.Count() > 0; }, _ => SelectImgs());
            clearCommand = new MyCommand(_ => { return db != null; }, _ => ClearDB());
            showCommand = new MyCommand(_ => { return true; }, _ => GetStatistics());

        }
        public void ClearDB()
        {
            //db.Database.ExecuteSqlRaw("DELETE from Images");
            db.Images.RemoveRange(db.Images);
            
            db.SaveChanges();
            clearFlag = true;
           
        }
        public void SelectImgs()
        {
            ObservableCollection<ImageVM> tmp = new ObservableCollection<ImageVM>();
            //MessageBox.Show(selectedImgType.Type);
            foreach (var img in Images)
            {
                if (img.ClassName == selectedImgType.Type)
                {
                    tmp.Add(img);
                }
            }
            
            SelectedImages = tmp;
            
        }

        public void RecognizeFunck()
        {

            ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
            {
   
                imageRecognizer.GetResults();
   
            }));
            
        }

        private byte[] ConvertImageToByteArray(string fileName)
        {
            Bitmap bitMap = new Bitmap(fileName);
            ImageFormat bmpFormat = bitMap.RawFormat;
            var imageToConvert = System.Drawing.Image.FromFile(fileName);
            using (MemoryStream ms = new MemoryStream())
            {
                imageToConvert.Save(ms, bmpFormat);
                return ms.ToArray();
            }
        }
        public void SelectDirectory()
        {
            string stream = folderDialog.OpenFolder();
            if (!stream.Equals(String.Empty))
            {
                Progress = 0;
                ClassVMs = new ObservableCollection<ImageClassVM>();

                db = new Database.ApplicationContext();
                var dir = Directory.GetFiles(stream);
                Images = new ObservableCollection<ImageVM>();
                List<string> fileNames = new List<string>();
                
                foreach (var file in dir)
                {
                    bool flag = false;
                    var fileInfo = new FileInfo(file);
                    
                    foreach (var img in db.Images)
                    {
                        if (fileInfo.FullName == img.Path)
                        {
                            var code1 = ConvertImageToByteArray(fileInfo.FullName);
                            IStructuralEquatable equ = code1;
                            var code2 = img.Details.Image;
                            if (equ.Equals(code2, EqualityComparer<object>.Default))
                            {
                                img.count++;

                                db.SaveChanges();
                                Images.Add(new ImageVM(fileInfo.FullName, fileInfo.Name, img.Confidence, img.ClassName));

                                flag = true;
                                Progress++;

                                dispatcher.BeginInvoke(new Action(() =>
                                {
                                    if (ClassVMs.Count() > 0)
                                    {
                                        bool flag1 = false;
                                        foreach (var imgClass in ClassVMs)
                                        {
                                            if (imgClass.Type == img.ClassName)
                                            {
                                                imgClass.Count++;
                                                flag1 = true;
                                                break;
                                            }
                                        }
                                        if (!flag1)
                                        {
                                            ClassVMs.Add(new ImageClassVM(img.ClassName, 1));

                                        }

                                    }
                                    else
                                    {
                                        ClassVMs.Add(new ImageClassVM(img.ClassName, 1));

                                    }
                                }));
                                break;
                            }

                        }
                    }
                    
                    if (!flag)
                    {
                        Images.Add(new ImageVM(fileInfo.FullName, fileInfo.Name));
                        fileNames.Add(fileInfo.FullName);
                    }
                    
                }
               
                imageRecognizer = new ImageRecognizer(fileNames, new ForResults(this));
                
            }

        }
    }
    public class ForResults : IResults
    {
        MainVM mainVM;
        private byte[] ConvertImageToByteArray(string fileName)
        {
            Bitmap bitMap = new Bitmap(fileName);
            ImageFormat bmpFormat = bitMap.RawFormat;
            var imageToConvert = System.Drawing.Image.FromFile(fileName);
            using (MemoryStream ms = new MemoryStream())
            {
                imageToConvert.Save(ms, bmpFormat);
                return ms.ToArray();
            }
        }
        public ForResults(MainVM main)
        {
            mainVM = main;
        }
        public void ReturnRes(ImageInfo info)
        {
            foreach (var img in mainVM.Images)
            {
                if (info.path == img.Path)
                {
                    img.Confidence = info.confidence[0];
                    img.ClassName = info.className[0];
                    Database.ImageDetails imageDetails = new Database.ImageDetails { Image = ConvertImageToByteArray(info.path) };
                    mainVM.db.Images.Add(new Database.ImageData { Path = info.path, Name = img.Name, ClassName = info.className[0], Confidence = info.confidence[0], Details = imageDetails });
                    mainVM.db.SaveChanges();
                }
            }
            mainVM.Progress++;
            mainVM.dispatcher.BeginInvoke(new Action(() =>
            {
                if (mainVM.ClassVMs.Count() > 0)
                {
                    bool flag = false;
                    foreach (var imgClass in mainVM.ClassVMs)
                    {
                        if (imgClass.Type == info.className[0])
                        {
                            imgClass.Count++;
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        mainVM.ClassVMs.Add(new ImageClassVM(info.className[0], 1));
                        
                    }

                }
                else
                {
                    mainVM.ClassVMs.Add(new ImageClassVM(info.className[0], 1));
                   
                }
                mainVM.db.SaveChanges();
            }));
        }
    }
}
