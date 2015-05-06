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
using System.Windows.Shapes;
using System.Web;
using System.Net;
using System.IO;

namespace ModerateBreeze
{
    /// <summary>
    /// Interaction logic for CitySelect.xaml
    /// </summary>
    public partial class CitySelect : Window
    {
        private string currentLocationURL = "";
        public CitySelect()
        {
            InitializeComponent();
            this.Closing += CitySelect_Closing;
            btnSaveAndClose.IsEnabled = false;
        }
        private void CitySelect_Closing(object sender, EventArgs e)
        {
            if (this.currentLocationURL != "")
            {
                MainWindow.currentLocationUrl = this.currentLocationURL;
                MainWindow.currentPlace = txtLocation.Text;
            }
            
        }
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Søket er her:
            //http://www.yr.no/soek/soek.aspx?sted=
            //
            //Eksempel på korrekt stedstring:
            //http://www.yr.no/place/Norge/Telemark/Skien/Skien/forecast_hour_by_hour.xml

            // Create webclient to download string
            WebClient client = new WebClient();
            string value = client.DownloadString("http://www.yr.no/soek/soek.aspx?sted="+ txtLocation.Text);

            // Split to get relevant parts
            string[] results = value.Split(new string[]{"href=\"/sted/Norge/"}, StringSplitOptions.None);
            results = results[1].Split(new string[] { "\" title=" }, StringSplitOptions.None);

            // Clear last value if any
            lstBoxResults.Items.Clear();

            // Add the constant bits
            string place = "http://www.yr.no/place/Norge/" + results[0] + "forecast_hour_by_hour.xml";
            lstBoxResults.Items.Add(place);
        }

        private void btnSaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void lstBoxResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentLocationURL = lstBoxResults.SelectedItem.ToString();
            btnSaveAndClose.IsEnabled = true;
        }
        
    }
}
