using System;
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
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Win32;
using System.Xml.Linq;

namespace DLSwtichBinTextEditor
{
    /// <summary>
    /// Логика взаимодействия для ImportExportWindow.xaml
    /// </summary>
    public partial class ImportExportWindow : Window
    {
        private Dictionary<TextEntity, BinText> Texts { get; set; } = new Dictionary<TextEntity, BinText>();
        internal ImportExportWindow(List<TextEntity> texts)
        {
            InitializeComponent();
            foreach (var text in texts)
            {
                Texts.Add(text, new BinText());
            }
        }

        private void ImportXML_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ofd = new OpenFileDialog
                {
                    Filter = "XML Files (*.xml)|*.xml"
                };
                if ((bool)!ofd.ShowDialog())
                {
                    return;
                }
                var doc = XDocument.Load(ofd.FileName);
                var root = doc.Root;
                int i = 0;
                var elems = root.Elements();
                var count = elems.Count();
                if (count > Texts.Count)
                {
                    var res = MessageBox.Show("The number of lines in the file exceeds the original! Cut and continue?", "Attention!", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.No)
                    {
                        return;
                    }
                    elems = elems.Take(Texts.Count);
                }
                else if (count < Texts.Count)
                {
                    var res = MessageBox.Show("The number of elements is less than the original! Сontinue?", "Attention!", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                foreach (var elem in elems)
                {
                    Texts[Texts.ElementAt(i).Key].Text = elem.Value;
                    i++;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }

        private void ImportText_Click(object sender, RoutedEventArgs e)
        {
            var win = new ImportTextWindow();
            win.Texts = Texts.Select(x => x.Value.Text);
            if (!(bool)win.ShowDialog())
            {
                return;
            }
            var elems = win.Texts;
            var count = elems.Count();
            if (count > Texts.Count)
            {
                var res = MessageBox.Show("The number of lines in the file exceeds the original! Cut and continue?", "Attention!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    return;
                }
                elems = elems.Take(Texts.Count);
            }
            else if (count < Texts.Count)
            {
                var res = MessageBox.Show("The number of elements is less than the original! Сontinue?", "Attention!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    return;
                }
            }
            var i = 0;
            if (elems.Any(x => x.Length > 40))
            {
                var res = MessageBox.Show("Some elements length is more then 40! Cut and continue?", "Attention!", MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    return;
                }
                elems = elems.Select(x => x.Length > 40 ? x.Substring(0, 40) : x);
            }
            foreach (var elem in elems)
            {
                Texts.ElementAt(i).Key.Text = elem;
                Texts[Texts.ElementAt(i).Key].Text = elem;
                i++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Texts)
            {
                item.Key.Text = item.Value.Text;
            }
        }

        private void ExportXML_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "XML Files (*.xml)|*.xml";
            if (!(bool)sfd.ShowDialog())
            {
                return;
            }
            var document = new XDocument();
            foreach (var item in Texts)
            {
                var el = new XElement("Row", item.Value.Text);
                document.Root.Add(el);
            }
            document.Save(sfd.FileName);
        }

        private void ExportText_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Text Files (*.txt)|*.txt";
            if (!(bool)sfd.ShowDialog())
            {
                return;
            }
            using (var writer = new StreamWriter(File.OpenWrite(sfd.FileName), Encoding.UTF8))
            {
                foreach (var (index,item) in Texts.Select((x,index)=>(index,x)))
                {
                    writer.Write($"{item.Key.Text}");
                    if (index != Texts.Count - 1)
                    {
                        writer.WriteLine();
                    }
                }
            }
        }
    }
}
