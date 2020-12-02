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
using System.Net.Http;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace ViewModelLib
{
    public interface IFolderDialog
    {
        string OpenFolder();
    }

    public class MainVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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
        List<string> stats;
        public List<string> Statistics
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
            clearCommand = new MyCommand(_ => { return true; }, _ => ClearDataBase());
            showCommand = new MyCommand(_ => { return true; }, _ => GetStatistics());

        }
        public async void GetStatistics()
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync("http://localhost:5000/api/images/");
            
            Statistics = JsonConvert.DeserializeObject<List<string>>(result);
            //Statistics = 

        }
        public async void ClearDataBase()
        {
            var client = new HttpClient();
            await client.DeleteAsync("http://localhost:5000/api/images/");
           
            Statistics = new List<string>();
        }
        public void SelectImgs()
        {
            if (selectedImgType != null)
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
        }

        public async void  RecognizeFunck()
        {

            //ThreadPool.QueueUserWorkItem(new WaitCallback(_ =>
            //{
            foreach (var img in Images)
            {
                var tmpImage = new ImageInfoGet();
                var client = new HttpClient();

                tmpImage.Path = img.Path;
                tmpImage.Path = img.Name;
                tmpImage.JpegCover = Convert.ToBase64String(ConvertImageToByteArray(img.Path));

                var s = JsonConvert.SerializeObject(tmpImage);
                var c = new StringContent(s);

                c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var putResult = await client.PutAsync("http://localhost:5000/api/images/", c);
                string body = await putResult.Content.ReadAsStringAsync();
                ImageInfoGet info = JsonConvert.DeserializeObject<ImageInfoGet>(body);
                //MessageBox.Show(body);

                //ImageVM imageVM;

                //imageVM = new ImageVM(img.Path, img.Name, info.Confidence, info.Class);


                //Images.Add(imageVM);
                img.Confidence = info.Confidence;
                img.ClassName = info.Class;
                Progress++;

                this.dispatcher.BeginInvoke(new Action(() =>
                {
                    if (ClassVMs.Count() > 0)
                    {
                        bool flag = false;
                        foreach (var imgClass in ClassVMs)
                        {
                            if (imgClass.Type == info.Class)
                            {
                                imgClass.Count++;
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            ClassVMs.Add(new ImageClassVM(info.Class, 1));

                        }

                    }
                    else
                    {
                        ClassVMs.Add(new ImageClassVM(info.Class, 1));

                    }
                }));

            }
            //imageRecognizer.GetResults();
            


            //}));

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
        public async void SelectDirectory()
        {
            string stream = folderDialog.OpenFolder();

            if (!stream.Equals(String.Empty))
            {
                var dir = Directory.GetFiles(stream);
                Images = new ObservableCollection<ImageVM>();
                ClassVMs = new ObservableCollection<ImageClassVM>();
                Progress = 0;
                foreach (var file in dir)
                {
                    var fileInfo = new FileInfo(file);
                    var imageVM = new ImageVM(fileInfo.FullName, fileInfo.Name);
                    Images.Add(imageVM);
                }

                //ClassVMs = new ObservableCollection<ImageClassVM>();

                //Progress = 0;
                //var dir = Directory.GetFiles(stream);
                //Images = new ObservableCollection<ImageVM>();
                ////List<string> fileNames = new List<string>();
                //HttpClient client = new HttpClient();
                //foreach (var file in dir)
                //{


                //    var tmpImage = new ImageInfoGet();

                //    var fileInfo = new FileInfo(file);
                //    tmpImage.Path = fileInfo.FullName;
                //    tmpImage.Path = fileInfo.Name;
                //    tmpImage.JpegCover = Convert.ToBase64String(ConvertImageToByteArray(fileInfo.FullName));

                //    var s = JsonConvert.SerializeObject(tmpImage);
                //    var c = new StringContent(s);

                //    c.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                //    var putResult = await client.PutAsync("http://localhost:5000/api/images/", c);
                //    string body = await putResult.Content.ReadAsStringAsync();
                //    ImageInfoGet info = JsonConvert.DeserializeObject<ImageInfoGet>(body);
                //    //MessageBox.Show(body);

                //    ImageVM imageVM;

                //    imageVM = new ImageVM(fileInfo.FullName, fileInfo.Name, info.Confidence, info.Class);


                //    Images.Add(imageVM);
                //    Progress++;

                //    await this.dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        if (ClassVMs.Count() > 0)
                //        {
                //            bool flag = false;
                //            foreach (var imgClass in ClassVMs)
                //            {
                //                if (imgClass.Type == info.Class)
                //                {
                //                    imgClass.Count++;
                //                    flag = true;
                //                    break;
                //                }
                //            }
                //            if (!flag)
                //            {
                //                ClassVMs.Add(new ImageClassVM(info.Class, 1));

                //            }

                //        }
                //        else
                //        {
                //            ClassVMs.Add(new ImageClassVM(info.Class, 1));

                //        }
                //    }));
                //}
            }
            //imageRecognizer = new ImageRecognizer(fileNames, new ForResults(this));

        }

        
    }
    public class ForResults : IResults
    {
        MainVM mainVM;
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
            }));
        }
    }
}
