using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
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
        DownloadOperation downloadOperation;
        CancellationTokenSource cancellationToken;
        double progress;


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
            StatusText.Text = String.Format("{0} ", title);
            var author = video.Author; // "Monstercat"
            var duration = video.Duration; // 00:07:14

            StatusText.Text = $"        id: {id}    client: {client}   title: {title}      author: {author}      duration: {duration} ";

            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            // ...or highest bitrate audio stream
            // var streamInfo = streamInfoSet.Audio.WithHighestBitrate();

            // ...or highest quality & highest framerate MP4 video stream
            // var streamInfo = streamInfoSet.Video
            //    .Where(s => s.Container == Container.Mp4)
            //    .OrderByDescending(s => s.VideoQuality)
            //    .ThenByDescending(s => s.Framerate)
            //    .First();


            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            var ext = streamInfo.Container.GetFileExtension();

            Debug.WriteLine("Trying to download");

            //Windows.Storage.StorageFolder folder = await ApplicationData.Current.LocalFolder;

            // StorageFile file = await DownloadsFolder.CreateFileAsync($"file.{ext}");

            var file = await KnownFolders.VideosLibrary.OpenStreamForWriteAsync($"file.{ext}", CreationCollisionOption.GenerateUniqueName);
           
            Progress = 0;

            var progressHandler = new Progress<double>(p => 
            { 
                Progress = p;
                
                StatusText1.Text = String.Format("{0}% of {1}% complete.", Convert.ToInt32(Math.Floor(p * 100)), 100);
                Debug.WriteLine(progress);
            });

             
            StatusText.Text = this.progress.ToString();

            
            //IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            try
            {
                await client.DownloadMediaStreamAsync(streamInfo, file, progressHandler);
            }
            catch (Exception ex) {
                
                Debug.WriteLine(ex.ToString());
                StatusText1.Text = "Cannot download";
            }
            //  IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
           
        
        }

        
        public double Progress
        {
            get
            {   return  this.progress;
         }
            set
            {
                this.progress = value;
            }
        }

    }
}
