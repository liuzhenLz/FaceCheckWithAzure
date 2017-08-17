using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace FaceCheckWithAzure
{
    public class MapViewPage : ContentPage
    {
        WebView _showAddressWV;
        public MapViewPage()
        {
            BackgroundColor = Color.White;
            Title = "地图";
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _showAddressWV = new WebView()
            {
                Source = "https://www.baidu.com"
            };
            this.Content = _showAddressWV;
        }
    }
}