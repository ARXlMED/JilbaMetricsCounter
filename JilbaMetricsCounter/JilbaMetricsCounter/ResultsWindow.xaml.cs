using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace JilbaMetricsCounter
{
    /// <summary>
    /// Логика взаимодействия для ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        public ResultsWindow(GilbAnalyzer.Result r)
        {
            InitializeComponent();

            var rows = new List<GilbRow>
            {
                new GilbRow { Metric = "Абсолютная сложность CL",       Value = r.CL.ToString() },
                new GilbRow { Metric = "Относительная сложность cl",    Value = r.cl.ToString("F3", CultureInfo.InvariantCulture) },
                new GilbRow { Metric = "Максимальный уровень вложенности CLI", Value = r.CLI.ToString() },
                //new GilbRow { Metric = "Всего операторов N",            Value = r.N.ToString() },
                //new GilbRow { Metric = "Метрика Маккейба Z(G) = CL + 1", Value = r.McCabe.ToString(), Bold = true },
            };

            Grid.ItemsSource = rows;

            Closing += (s, e) =>
            {
                if (Owner is MainWindow mw && !mw.IsVisible) mw.ComeBack();
            };
        }

        void Back_Click(object sender, RoutedEventArgs e)
        {
            if (Owner is MainWindow mw) mw.ComeBack();
            Close();
        }
    }
}
