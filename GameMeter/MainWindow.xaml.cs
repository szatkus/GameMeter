using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Win32;
using System.IO;

namespace GameMeter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ProcessMeter meter = new ProcessMeter();
        
        public MainWindow()
        {
            InitializeComponent();
            meter.Changed += meter_Changed;
        }

        private void choose_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Executables|*.exe";
            if (dialog.ShowDialog() == true)
            {
                meter.FileName = dialog.FileName;
                System.Windows.Controls.Button runButton = (System.Windows.Controls.Button)FindName("runButton");
                runButton.IsEnabled = true;
                System.Windows.Controls.Image thumbnail = (System.Windows.Controls.Image) FindName("icon");
                thumbnail.Source = null;
                System.Drawing.Icon ico = System.Drawing.Icon.ExtractAssociatedIcon(meter.FileName);
                MemoryStream stream = new MemoryStream();
                ico.Save(stream);
                thumbnail.Source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        private void run_Click(object sender, RoutedEventArgs e)
        {
            meter.Run();
            ((System.Windows.Controls.Button)sender).IsEnabled = false;
            ((System.Windows.Controls.Button)FindName("chooseButton")).IsEnabled = false;
        }

        private void meter_Changed(object sender, MeterEventArgs e)
        {
            List<Polyline> lines = new List<Polyline>();
            
            this.Dispatcher.BeginInvoke((Action)delegate
            {
                if (!e.Finish)
                {
                    Canvas chart = ((System.Windows.Controls.Canvas)FindName("chart"));
                    chart.Children.Clear();
                    Rectangle bg = new Rectangle();
                    bg.Width = chart.Width;
                    bg.Height = chart.Height;
                    bg.Stroke =  System.Windows.Media.Brushes.Black;
                    bg.Fill = System.Windows.Media.Brushes.White;
                    chart.Children.Add(bg);
                    foreach (Plot plot in e.Plots.Values)
                    {
                        Polyline poly = new Polyline();
                        poly.StrokeThickness = 2;
                        poly.Stroke = new SolidColorBrush(plot.color);
                        int i = 0;
                        foreach (int value in plot.Values)
                        {
                            //Console.WriteLine(i);
                            int x = (int)((i * chart.ActualWidth) / plot.Values.Count);
                            int y = (int)((value * chart.ActualHeight) / meter.Scale);
                            //Console.WriteLine(x);
                            //Console.WriteLine(y);
                            Point point = new Point(x, y);
                            poly.Points.Add(point);
                            i++;
                        }
                        chart.Children.Add(poly);
                    }

                    Panel stats = ((System.Windows.Controls.Panel)FindName("stats"));
                    stats.Children.Clear();
                    for (int i = 0; i < meter.thresholds.Length; i++)
                    {
                        Label label = new Label();
                        label.Content = meter.thresholds[i] + "%: " + meter.winners[i];
                        label.Height = 28;
                        label.Width = stats.ActualWidth / meter.thresholds.Length;
                        stats.Children.Add(label);
                    }
                }
                else
                {
                    ((System.Windows.Controls.Button)FindName("chooseButton")).IsEnabled = true;
                    ((System.Windows.Controls.Button)FindName("runButton")).IsEnabled = true;
                }
                
            });
            
        }

    }
}
