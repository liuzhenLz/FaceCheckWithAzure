using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FaceCheckWithAzure
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
           InitializeComponent();
            TapGestureRecognizer tapGesture = new TapGestureRecognizer() { };
            tapGesture.Tapped += TapGesture_Tapped;
            this.addressStackL.GestureRecognizers.Add(tapGesture);
        }

        private void TapGesture_Tapped(object sender, EventArgs e)
        {
            Navigation.PushAsync(new MapViewPage());
        }
    }
}
