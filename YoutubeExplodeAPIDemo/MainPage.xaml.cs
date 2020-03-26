using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace YoutubeExplodeAPIDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlText.Text; 

            //var url = "https://www.youtube.com/watch?v=bnsUkE8i0tU";
            var id = YoutubeClient.ParseVideoId(url); // "bnsUkE8i0tU"
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(id);

            var title = video.Title; // "Infected Mushroom - Spitfire [Monstercat Release]"
            var author = video.Author; // "Monstercat"
            var duration = video.Duration; // 00:07:14

            propText.Text = $"        id: {id}    client: {client}   title: {title}      author: {author}      duration: {duration} ";

            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            var ext = streamInfo.Container.GetFileExtension();

            Debug.WriteLine("Trying to download");

            //Windows.Storage.StorageFolder folder = await ApplicationData.Current.LocalFolder;
           
           // StorageFile file = await DownloadsFolder.CreateFileAsync($"file.{ext}");

             var file = await KnownFolders.VideosLibrary.OpenStreamForWriteAsync($"file.{ext}", CreationCollisionOption.GenerateUniqueName);
            //IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            await client.DownloadMediaStreamAsync(streamInfo, file);
          //  IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);

        }



    }
}
