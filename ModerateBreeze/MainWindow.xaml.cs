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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Xml;
using System.IO;
using System.Text;
using System.Net;
using System.Timers;
using System.Windows.Threading;
using System.Drawing;
using System.Windows.Interop;
using System.Globalization;

namespace ModerateBreeze
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class WeatherDataObject
    {
        public DateTime moment;
        public string timeFrom  = "";
        public int numberEx = 0;
        public int temperature = 0;
        public WeatherDataObject(string TimeFrom, string NumberEx, string Temperature)
        {
            // 2015-05-06T03:00:00 parse
            // protip: HH for 24hr clock, hh for 12hr 
            DateTime.TryParseExact(TimeFrom, "yyyy-MM-ddTHH:mm:ss", null, 
                DateTimeStyles.None, out moment);
            timeFrom = TimeFrom;
            numberEx = Convert.ToInt32(NumberEx);
            temperature = Convert.ToInt32(Temperature);
        }
    }
    public partial class MainWindow : Window
    {
        System.Drawing.Image rain;
        System.Drawing.Image cloud;
        System.Drawing.Image partlyCloudy;
        System.Drawing.Image lightning;
        System.Drawing.Image sun;

        public static string currentLocationUrl = "";
        public static string currentPlace = "";
        Timer aTimer;
        int timerCounter = 0;
        List<WeatherDataObject> forcastList = new List<WeatherDataObject>();
        bool[] monitorEnabled= {false,false};


        public MainWindow()
        {
            InitializeComponent();
            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += aTimer_Elapsed;
            aTimer.Enabled = true;

            rain = Properties.Resources.rain;
            cloud = Properties.Resources.cloud;
            partlyCloudy = Properties.Resources.partly_cloudy;
            lightning = Properties.Resources.lightning;
            sun = Properties.Resources.sun;

            Label lblStartText = new Label();
            lblStartText.Content = "ModerateBreeze weather app still in beta\nChoose your location, start the monitor and have a nice day!\n\ngithub.com/studiefredfredrik";
            stackPanelV.Children.Add(lblStartText);
            this.Icon = pictureFromNumber(1);
        }

        void aTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Dispatch does work on ui thread
            Dispatcher.BeginInvoke(new Action(delegate
            {
                if (lblStatus.Content.ToString() != currentPlace && currentPlace != "")
                {
                    lblStatus.Content = currentPlace;
                    lblStatus.Background = System.Windows.Media.Brushes.LightGreen;
                }
            }));

            if (monitorEnabled[1] && !monitorEnabled[0]) //enabled now but not at last iteration
            {

            }

            if (monitorEnabled[1] && monitorEnabled[0]) //enabled now and was last iteration
            {
                timerCounter++;
                if (timerCounter > 15 * 60 * 1000) // Update every 15 minutes
                {

                }
            }

        }

        private void btnSetLocation_Click(object sender, RoutedEventArgs e)
        {
            CitySelect s = new CitySelect();
            s.ShowDialog();
        }

        void parseXmlAndGenerateObjects(string xmlString)
        {
            forcastList.Clear();
            StringBuilder output = new StringBuilder();
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            {
                while (true)
                {
                    try
                    {
                        reader.ReadToFollowing("tabular");
                        while (true)
                        {
                            reader.ReadToFollowing("time");
                            reader.MoveToFirstAttribute();
                            string fromTime = reader.Value;
                            reader.ReadToFollowing("symbol");
                            reader.MoveToFirstAttribute();
                            reader.MoveToNextAttribute();
                            string numberEx = reader.Value;
                            reader.ReadToFollowing("temperature");
                            reader.MoveToFirstAttribute();
                            reader.MoveToNextAttribute();
                            string temperature = reader.Value;
                            forcastList.Add(new WeatherDataObject(fromTime, numberEx, temperature));
                        }
                    }
                    catch(Exception ex)
                    {
                        break; // Stop looping if we hit exceptions (end of loop, no data etc expected)
                    }
                }
            }
        }
        private BitmapSource pictureFromNumber(int number)
        {
            if (number < 3) return Bitmap2BitmapImage((Bitmap)sun);
            if (number < 4) return Bitmap2BitmapImage((Bitmap)partlyCloudy);
            if (number < 50) return Bitmap2BitmapImage((Bitmap)rain);
            else return Bitmap2BitmapImage((Bitmap)lightning);
        }

        private BitmapSource Bitmap2BitmapImage(Bitmap bitmap)
        {
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                           bitmap.GetHbitmap(),
                           IntPtr.Zero,
                           Int32Rect.Empty,
                           BitmapSizeOptions.FromEmptyOptions());
            return i;
        }



        private void displayWeatherObjects()
        {
            // Clear old entries before filling in new ones
            stackPanelV.Children.Clear();

            foreach (WeatherDataObject w in forcastList)
            {
                // Add lines horisontally for attributes in the same time-segment
                StackPanel stackPanelH = new StackPanel();
                if (w.moment.Date == DateTime.Now.Date && w.moment.Hour == DateTime.Now.Hour + 1) stackPanelH.Background = System.Windows.Media.Brushes.LightGray;
                stackPanelH.Orientation = Orientation.Horizontal;
                Label lblTime = new Label();
                lblTime.Content = w.timeFrom.Replace("T","  ") + " ";
                stackPanelH.Children.Add(lblTime);
                //Label lblType = new Label();
                //lblType.Content = w.numberEx;
                //stackPanelH.Children.Add(lblType);
                System.Windows.Controls.Image wType = new System.Windows.Controls.Image();
                wType.Source = pictureFromNumber(w.numberEx);
                wType.Height = 40;
                wType.Width = 40;
                stackPanelH.Children.Add(wType);
                Label lblTemperature = new Label();
                lblTemperature.Content = " " + w.temperature+"°C\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t\t"; // easy now
                stackPanelH.Children.Add(lblTemperature);

                // Then add each the time-segment to the vertical list
                stackPanelV.Children.Add(stackPanelH);
            }
            // Set content to a scrollable box protip: CanContentScroll="True" :D
            scrollView.Content = stackPanelV;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (btnStart.Content.ToString() == "Start monitor")
            {
                if (currentLocationUrl != "")
                {
                    try
                    {
                        lblMonitorStatus.Content = "Fetching data...";
                        lblMonitorStatus.Background = System.Windows.Media.Brushes.BlanchedAlmond;
                        // Create webclient to download string
                        WebClient client = new WebClient();
                        string value = client.DownloadString(currentLocationUrl);
                        parseXmlAndGenerateObjects(value);

                        lblMonitorStatus.Content = "Monitor: On ";
                        lblMonitorStatus.Background = System.Windows.Media.Brushes.LightGreen;

                        displayWeatherObjects();
                        monitorEnabled[1] = true;
                        btnStart.Content = "Stop monitor";

                    }
                    catch (Exception ex)
                    {
                        lblMonitorStatus.Content = "Error: " + ex.Message;
                        lblMonitorStatus.Background = System.Windows.Media.Brushes.Red;
                        monitorEnabled[1] = false;
                        btnStart.Content = "Start monitor";
                    }
                }
                else
                {
                    lblMonitorStatus.Content = "Error: no location set?";
                    lblMonitorStatus.Background = System.Windows.Media.Brushes.Red;
                    monitorEnabled[1] = false;
                    btnStart.Content = "Start monitor";
                }
            }
            else
            {
                monitorEnabled[1] = false;
                lblMonitorStatus.Content = "Monitor: Off ";
                lblMonitorStatus.Background = System.Windows.Media.Brushes.Orange;
                btnStart.Content = "Start monitor";
            }
        }        
    }
}
