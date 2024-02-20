using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.IO;

namespace DLSwtichBinTextEditor
{
    class TextEntity : INotifyPropertyChanged
    {
        public string Text
        {
            get => Texts[0].Text;
            set
            {
                foreach (var item in Texts)
                {
                    item.Text = value;
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
            }
        }
        public int Index => Texts[0].Index;
        public List<BinText> Texts { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    enum FileType
    {
        BG, SELECT, SPEAKERS
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private BinTextFile file;
        private List<TextEntity> Texts;
        private FileType FileType;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            SetFileType(FileType.SELECT);
        }
        private void MakeEntities(IEnumerable<BinText> texts,bool nooptimize = false)
        {
            texts = texts.Where(x => x.Text.Length > 0 && x.Text.Trim().Length > 0);
            var items = new List<TextEntity>();
            if (nooptimize)
            {
                foreach(var item in texts)
                {
                    items.Add(new TextEntity { Texts = new List<BinText> { item } });
                }
                Texts = items;
                return;
            }
            foreach (var item in texts)
            {
                var founded = false;
                foreach (var text in items)
                {
                    if (text.Text == item.Text)
                    {
                        text.Texts.Add(item);
                        founded = true;
                    }
                }
                if (!founded)
                {
                    items.Add(new TextEntity { Texts = new List<BinText> { item } });
                }
            }
            Texts = items;
        }
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (!(bool)ofd.ShowDialog())
            {
                return;
            }
            OpenFile(ofd.FileName);
        }
        private void OpenFile(string filename)
        {
            try
            {
                file = BinTextFile.ReadFromFile(filename);

                var stem = Path.GetFileNameWithoutExtension(filename);

                switch (stem)
                {
                    case "bg":
                        SetFileType(FileType.BG, false);
                        break;
                    case "select":
                        SetFileType(FileType.SELECT, false);
                        break;
                    case "speaker":
                        SetFileType(FileType.SPEAKERS, false);
                        break;
                }

                if (FileType == FileType.BG)
                {
                    MakeEntities(file.Texts.Skip(file.Header.SecondCount + 1));
                }
                else if (FileType == FileType.SELECT)
                {
                    MakeEntities(file.Texts/*.Skip(file.Header.FirstCount + 1)*/,true);
                }
                else
                {
                    MakeEntities(file.Texts.Skip(1).Take(file.Header.FirstCount));
                }
                TextsList.ItemsSource = Texts;
                TextsList.DataContext = Texts;
                Title = filename;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (file == null)
            {
                return;
            }
            file.Save(file.FileName);
        }

        private void SetFileType(FileType fileType, bool reopen = true)
        {
            FileType = fileType;
            if (fileType == FileType.SELECT)
            {
                FileTypeMenuItem1.IsChecked = true;
                FileTypeMenuItem2.IsChecked = false;
                FileTypeMenuItem3.IsChecked = false;
            }
            else if (fileType == FileType.BG)
            {
                FileTypeMenuItem1.IsChecked = false;
                FileTypeMenuItem2.IsChecked = true;
                FileTypeMenuItem3.IsChecked = false;
            }
            else if (fileType == FileType.SPEAKERS)
            {
                FileTypeMenuItem1.IsChecked = false;
                FileTypeMenuItem2.IsChecked = false;
                FileTypeMenuItem3.IsChecked = true;
            }
            if (file != null && reopen)
            {
                OpenFile(file.FileName);
            }
        }

        private void FileTypeSelect_Click(object sender, RoutedEventArgs e)
        {
            SetFileType(FileType.SELECT);
        }

        private void FileTypeBG_Click(object sender, RoutedEventArgs e)
        {
            SetFileType(FileType.BG);
        }

        private void FileTypeSpeakers_Click(object sender, RoutedEventArgs e)
        {
            SetFileType(FileType.SPEAKERS);
        }

        private void ImportExport_Click(object sender, RoutedEventArgs e)
        {
            var win = new ImportExportWindow(Texts);
            if (!(bool)win.ShowDialog())
            {
                return;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Texts = Texts.OrderBy(x => x.Text).ToList();
        }

        private void Window_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.All;
            }
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = e.Data.GetData(DataFormats.FileDrop) as string[];
                OpenFile(data[0]);
            }
        }
    }
}
