using AnnotationTool.Commands;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;

namespace AnnotationTool.ViewModel
{
    class MyViewModel : INotifyPropertyChanged
    {
        private string[] _images;
        private string _selectedImagepath;
        public string[] Images
        {
            get { return _images; }
            set
            {
                _images = value;
                OnPropertyChanged();
            }
        }
        public string SelectedImagePath
        {
            get { return _selectedImagepath; }
            set
            {
                _selectedImagepath = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        private StrokeCollection _lines;
        public StrokeCollection Lines
        {
            get { return _lines; }
            set
            {
                _lines = value;
                OnPropertyChanged();
            }
        }

        public ICommand SelectImageCommand { private set; get; }
        public ICommand MouseDownCommand { private set; get; }

        public List<InkCanvasEditingMode> EditingModes { get; set; }


        public MyViewModel()
        {
            _lines = new StrokeCollection();
            _images = GetFolderFiles();
            _selectedImagepath = Images[0];
            SelectImageCommand = new RelayCommand<object>(ChangeSelectedImage);
            EditingModes = new List<InkCanvasEditingMode>() { InkCanvasEditingMode.Ink, InkCanvasEditingMode.EraseByStroke, InkCanvasEditingMode.Select };
        }


        private void ChangeSelectedImage(object newPath)
        {
            SelectedImagePath = (string)newPath;
        }

        private string[] GetFolderFiles()
        {
            string[] files = null;
            //using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            //{
            //    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

            //    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            //    {
            //        files = Directory.GetFiles(fbd.SelectedPath);
            files = Directory.GetFiles("../../Images");
            //    }
            //}

            return files;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string info = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }
}
