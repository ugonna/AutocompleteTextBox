using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ComponentTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            List<string> countries = new List<string>();
            // Some countries selected randomly

            countries.Add("Nigeria"); // :)
            countries.Add("Afghanistan");
            countries.Add("Algeria");
            countries.Add("American Samoa");
            countries.Add("Bahamas");
            countries.Add("Brazil");
            countries.Add("Cameroon");
            countries.Add("Denmark");
            countries.Add("Egypt");
            countries.Add("Finland");
            countries.Add("Ivory Coast");
            countries.Add("Jamaica");
            countries.Add("Malaysia");
            countries.Add("Netherlands");
            countries.Add("Samoa");
            countries.Add("Senegal");
            countries.Add("Tunisia");
            countries.Add("United Kingdom");
            countries.Add("United States");
            countries.Add("Vietnam");
            countries.Add("Yemen");
            countries.Add("Zambia");

            countryTextBox.DataContext = countries;
        }
    }
}
