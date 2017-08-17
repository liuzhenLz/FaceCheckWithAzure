using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;
using Plugin.Media;
using System.Threading.Tasks;
using Plugin.Media.Abstractions;
using Newtonsoft.Json;
using Org.Apache.Http.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using FFImageLoading.Forms;
using FFImageLoading.Transformations;

namespace FaceCheckWithAzure
{
    public class LoginPage : ContentPage
    {

        private CachedImage CameraImage { get; set; }
        private CachedImage TwitterImage { get; set; }
        private Entry HandleAccount;

        private Button TakePhoneBtn;
        private Button VerifyBtn;

        private StackLayout RootStacLayout;


        private const string subscriptionKey = "0eabd93a3f424013bb69cec4e6ca534a";
        private const string uriBase = "https://southeastasia.api.cognitive.microsoft.com/face/v1.0/";
        private DetectResponse[] tempDetectResponse;
        private string faceId1;
        private string faceId2;
        private VerifyResponse results;
        private Stream _imageStream;

        private Plugin.Media.Abstractions.MediaFile mediaFile { get; set; }

        private async Task<byte[]> GetTakeImageStream()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                if (CrossMedia.Current.IsTakePhotoSupported)
                {
                    await Task.Yield();
                    var pickMediaOptions = new PickMediaOptions
                    {
                        PhotoSize = PhotoSize.Large,
                    };

                    mediaFile = await CrossMedia.Current.PickPhotoAsync(pickMediaOptions);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        mediaFile.GetStream().CopyTo(ms);
                        byte[] bytes = ms.ToArray();
                        return bytes;
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("提示", "您尚未开启相册权限,您可以去 设置->隐私->相册 开启访问相册权限", "知道了");
                }
            }
            return null;
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            string url = "https://twitter.com/" + HandleAccount.Text + "/profile_image?size=original";
            TwitterImage.Source = url;
            GetFaceIdUrl(url);
        }
        public void Face()
        {
            var client = new Baidu.Aip.Face.Face("uGU69Iudv3GFAePr1ZOZWGzp", "U7oZ4EatcctoEKXOFCbpSzDyIHEZUW3p");           


            byte[] bytes = new byte[_imageStream.Length];
            _imageStream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            _imageStream.Seek(0, SeekOrigin.Begin);
            var images = new byte[][] { bytes, bytes };

            // 人脸对比
            var result = client.FaceMatch(images);
        }
        private void ChooseAccoutBtn_Clicked(object sender, EventArgs e)
        {
            string url = "https://twitter.com/" + HandleAccount.Text + "/profile_image?size=original";
            TwitterImage.Source = url;
            GetFaceIdUrl(url);
        }

        public async void GetFaceIdUrl(string url)
        {
            // TwitterLabel.Text = "Detecting Face..."; //显示进度loading

            HttpClient client = new HttpClient();

            // 请求头
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // 请求参数。第三个可选参数是“details”。
            string requestParameters = "returnFaceId=true";

            // 拼接验证 API
            string uri = uriBase + "detect?" + requestParameters;

            HttpResponseMessage response;

            // Request body. Posts a JSON.
            byte[] byteData = Encoding.UTF8.GetBytes("{ \"url\":\"" + url + "\"}");

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (contentString != "[]")
                    {
                        this.faceId1 = DeserialiseFaceId(contentString);
                        // TwitterLabel.Text = "faceId1 : " + faceId1;
                    }
                    else
                    {
                        //CameraImageLabel.Text = "No face detected, please try again.";
                        await DisplayAlert("提示", "没有识别到人脸，请重试", "确定");
                    }
                }
                else
                {
                    // CameraImageLabel.Text = "Am error occured, please try again.";
                    await DisplayAlert("提示", "发现一个异常，请再试一次", "确定");
                }
            }
        }
        public string DeserialiseFaceId(string json)
        {
            tempDetectResponse = JsonConvert.DeserializeObject<DetectResponse[]>(json);
            return tempDetectResponse[0].faceId;
        }

        private byte[] ResizeImage(Plugin.Media.Abstractions.MediaFile imageFile)
        {
            if (imageFile != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    imageFile.GetStream().CopyTo(ms);
                    byte[] bytes = ms.ToArray();
                    return bytes;
                }
            }
            else
            {
                return null;
            }
        }

        public async void GetFaceIdImage(string imageFilePath)
        {
            //  CameraImageLabel.Text = "Detecting Face...";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            string requestParameters = "returnFaceId=true";
            string uri = uriBase + "detect?" + requestParameters;
            HttpResponseMessage response;
            byte[] byteData = ResizeImage(this.mediaFile);
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);

                if (response.IsSuccessStatusCode)
                {
                    string contentString = await response.Content.ReadAsStringAsync();

                    if (contentString != "[]")
                    {
                        faceId2 = DeserialiseFaceId(contentString);
                        //  CameraImageLabel.Text = "faceId2 : " + faceId2;
                    }
                    else
                    {
                        await DisplayAlert("提示", "没有识别到人脸，请重试", "确定");
                    }
                }
                else
                {
                    await DisplayAlert("提示", "发现一个异常，请再试一次", "确定");
                }
            }
        }

        //**************** SetUI 部分 ***************

        private void SetUIsForThisView()
        {
            this.CameraImage = new CachedImage()
            {
                HeightRequest = 200,
                WidthRequest = 200,
                Aspect = Aspect.AspectFit,
                //BackgroundColor = Color.Aqua,
            };
            this.TwitterImage = new CachedImage()
            {
                HeightRequest = 100,
                WidthRequest = 100,
                DownsampleWidth = 100,
                DownsampleHeight = 100,
                HorizontalOptions = LayoutOptions.Center,
                LoadingPlaceholder = "icon.png",
                Aspect = Aspect.AspectFit,

            };
            TwitterImage.Transformations.Add(new CircleTransformation() { BorderSize = 4, BorderHexColor = "#333333" });

            this.HandleAccount = new Entry()
            {
                Text = "@ZhenLiu2017",
                HeightRequest = 44,
                //WidthRequest = 200,
                //HorizontalOptions = LayoutOptions.StartAndExpand,
                Keyboard = Keyboard.Email,
            };
            var chooseAccoutBtn = new Button()
            {
                BackgroundColor = Color.Orange,
                Text = "切换账户",
                TextColor = Color.White,
                HeightRequest = 44,
                WidthRequest = 80,
            };
            chooseAccoutBtn.Clicked += ChooseAccoutBtn_Clicked;
            //var accoutStackL = new StackLayout()
            //{
            //    HeightRequest = 44,
            //    Orientation = StackOrientation.Horizontal,
            //    Children = { HandleAccount, chooseAccoutBtn },
            //};

            this.TakePhoneBtn = new Button()
            {
                Text = "拍照",
                HeightRequest = 44,
                WidthRequest = 80,
                BackgroundColor = Color.Orange,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.StartAndExpand,
            };
            TakePhoneBtn.Clicked += TakePhoneBtn_Clicked;

            this.VerifyBtn = new Button()
            {
                Text = "验证登录",
                HeightRequest = 44,
                WidthRequest = 80,
                BackgroundColor = Color.Orange,
                TextColor = Color.White,
                HorizontalOptions = LayoutOptions.EndAndExpand,
            };
            VerifyBtn.Clicked += VerifyBtn_Clicked;

            var chooseStack = new StackLayout()
            {
                VerticalOptions = LayoutOptions.End,
                HeightRequest = 44,
                Orientation = StackOrientation.Horizontal,
                Children = { TakePhoneBtn, VerifyBtn },
            };

            RootStacLayout = new StackLayout()
            {
                Padding = new Thickness(10, 10, 10, 10),
                BackgroundColor = Color.Transparent,
                Children =
                {
                    TwitterImage,
                    HandleAccount,
                    chooseAccoutBtn,
                    chooseStack,
                    CameraImage,
                }
            };
            Grid pageGrid = new Grid { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            pageGrid.Children.Add(new Image { Source = "bg.png" }, 0, 0);
            pageGrid.Children.Add(RootStacLayout, 0, 0);
            Content = pageGrid;

        }

        private async void TakePhoneBtn_Clicked(object sender, EventArgs e)
        {
            Plugin.Media.Abstractions.StoreCameraMediaOptions options = new Plugin.Media.Abstractions.StoreCameraMediaOptions()
            {
                SaveToAlbum = false,
                DefaultCamera = CameraDevice.Front,
            };
            var mediaFile = await CrossMedia.Current.TakePhotoAsync(options);
            CameraImage.Source = ImageSource.FromStream(() =>
            {
                return mediaFile.GetStream();
            });
            _imageStream = mediaFile.GetStream();
        }

        private async void VerifyBtn_Clicked(object sender, EventArgs e)
        {
            Face();
            //HttpClient client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            //string requestParameters = "returnFaceId=true";
            //string uri = uriBase + "verify?" + requestParameters;
            //HttpResponseMessage response;

            //byte[] byteData = Encoding.UTF8.GetBytes("{ \"faceId1\":\"" + faceId1 + "\",\"faceId2\":\"" + faceId2 + "\"}");
            //using (ByteArrayContent content = new ByteArrayContent(byteData))
            //{
            //    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            //    response = await client.PostAsync(uri, content);
            //    string contentString = await response.Content.ReadAsStringAsync();
            //    results = JsonConvert.DeserializeObject<VerifyResponse>(contentString);
            //    if (results.isIdentical == true)
            //    {
            //        //  ResulLabel.Text = "Faces belong to the same person with a confidence score of " + results.confidence.ToString();
            //        //验证成功_允许登录
            //        await this.Navigation.PushAsync(new MainPage());
            //    }
            //    else
            //    {
            //        // await this.Navigation.PushAsync(new MainPage());
            //        await DisplayAlert("登录失败", "经验证，与注册脸不是同一张脸", "确定");
            //    }
            //}
        }

        public LoginPage()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                this.Padding = new Thickness(0, 20, 0, 0);
            }
            NavigationPage.SetHasNavigationBar(this, false);

            SetUIsForThisView();

        }
    }
}