using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace JilbaMetricsCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string? _path = null;

        public MainWindow() 
        { 
            InitializeComponent(); 
        }

        void Choose_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Rust files (*.rs)|*.rs|Все файлы (*.*)|*.*",
                CheckFileExists = true
            };
            if (dlg.ShowDialog() == true)
            {
                _path = dlg.FileName;
                FilePathBox.Text = _path;
                AnalyzeButton.IsEnabled = true;
                StatusText.Text = "Файл выбран.";
            }
        }

        void Analyze_Click(object sender, RoutedEventArgs e)
        {
            if (_path == null || !File.Exists(_path)) return;
            try
            {
                string code = File.ReadAllText(_path);
                var result = GilbAnalyzer.Analyze(code);

                var win = new ResultsWindow(result);
                win.Owner = this;
                win.Show();
                Hide();
            }
            catch (Exception ex) { StatusText.Text = "Ошибка: " + ex.Message; }
        }

        public void ComeBack() { Show(); Activate(); }
    }
}