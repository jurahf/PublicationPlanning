using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PublicationPlanning
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            VersionTracking.Track();
            lblData.Text = $"Publication Planning{Environment.NewLine}Version: {VersionTracking.CurrentVersion}, build: {VersionTracking.CurrentBuild}{Environment.NewLine} by Jurahf.";
        }
    }
}