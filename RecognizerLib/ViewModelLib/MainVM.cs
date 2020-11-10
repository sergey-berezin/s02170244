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

namespace ViewModelLib
{
    public interface IFolderDialog
    {
        string OpenFolder();
    }
    
    public class MainVM: INotifyPropertyChanged
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
        
        private readonly ICommand openCommand;
        public ICommand Open { get { return openCommand; } }

        private readonly ICommand recognizeCommand;
        public ICommand Recognize { get { return recognizeCommand; } }

        private readonly ICommand stopCommand;
        public ICommand Stop { get { return stopCommand; } }

        private readonly ICommand selectCommand;
        public ICommand Select { get { return selectCommand; } }

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

    
        public void SelectDirectory()
        {
            string stream = folderDialog.OpenFolder();
            if (!stream.Equals(String.Empty))
            {
                Progress = 0;
                var dir = Directory.GetFiles(stream);
                Images = new ObservableCollection<ImageVM>();
                List<string> fileNames = new List<string>();
                foreach (var file in dir)
                {
                    var fileInfo = new FileInfo(file);
                    Images.Add(new ImageVM(fileInfo.FullName, fileInfo.Name));
                    fileNames.Add(fileInfo.FullName);
                }
               
                imageRecognizer = new ImageRecognizer(fileNames, new ForResults(this));
                ClassVMs = new ObservableCollection<ImageClassVM>();
            }

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
