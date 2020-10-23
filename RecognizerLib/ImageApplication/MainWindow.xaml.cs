using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModelLib;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Numerics.Tensors;
using Microsoft.ML.OnnxRuntime;

namespace ImageApplication
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainVM mainVM { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            mainVM = new MainVM(new MyFileDialogServices(), this.Dispatcher);
            this.DataContext = mainVM;
            
        }
    }
    public class MyFileDialogServices : IFolderDialog
    {
        public string OpenFolder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
                return fbd.SelectedPath;
            else return String.Empty;

        }
    }
    
    
}
