using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
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
        
        double progress;


        public MainPage()
        {
            this.InitializeComponent();
        }

      async private void getYoutubeFileProps(Stream stream) {
            var url = UrlText.Text;

            //var url = "https://www.youtube.com/watch?v=bnsUkE8i0tU";
            var id = YoutubeClient.ParseVideoId(url); // "bnsUkE8i0tU"
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(id);

            var title = video.Title; // "Infected Mushroom - Spitfire [Monstercat Release]"
                                     // StatusText.Text = String.Format("{0} ", title);
            var author = video.Author; // "Monstercat"
            var duration = video.Duration; // 00:07:14
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            var ext = streamInfo.Container.GetFileExtension();
            string displayName = Regex.Replace(title.ToString(), @"[^0-9a-zA-Z]+", "-");

            var progressHandler = new Progress<double>(p =>
            {
                Progress = p;

                StatusText1.Text = String.Format("{0}% of {1}% complete.", Convert.ToInt32(Math.Floor(p * 100)), 100);
                Debug.WriteLine(progress);
                if (p == 1)
                {
                    MediaSource mediaSource = MediaSource.CreateFromStream(stream.AsRandomAccessStream(), "video/MPEG-4");
                    mediaPlayerElement.Source = mediaSource;
                    mediaPlayerElement.AutoPlay = true;
                }
            });

            try
            {
                await client.DownloadMediaStreamAsync(streamInfo, stream, progressHandler);
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.ToString());
                StatusText1.Text = "Cannot download";
            }

        }

        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            var url = UrlText.Text; 

            //var url = "https://www.youtube.com/watch?v=bnsUkE8i0tU";
            var id = YoutubeClient.ParseVideoId(url); // "bnsUkE8i0tU"
            var client = new YoutubeClient();

            var video = await client.GetVideoAsync(id);

            var title = video.Title; // "Infected Mushroom - Spitfire [Monstercat Release]"
           // StatusText.Text = String.Format("{0} ", title);
            var author = video.Author; // "Monstercat"
            var duration = video.Duration; // 00:07:14

            StatusText.Text = $"   title: {title}  |    author: {author}   |   duration: {duration} ";

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
            // Removes All special characters from a String(Title). With special characters in string 
            // you cannot use that string as filename while writing to stream. It throws File Not found exception.
            string displayName=Regex.Replace(title.ToString(), @"[^0-9a-zA-Z]+", "");
            Debug.WriteLine($"{displayName}");
           
            //Windows.Storage.StorageFolder folder = await ApplicationData.Current.LocalFolder;

            // StorageFile file = await DownloadsFolder.CreateFileAsync($"file.{ext}");

            var file = await KnownFolders.VideosLibrary.OpenStreamForWriteAsync($"{displayName}.{ext}", CreationCollisionOption.GenerateUniqueName);
           
            
            var progressHandler = new Progress<double>(p => 
            { 
                Progress = p;
                
                StatusText1.Text = String.Format("{0}% of {1}% complete.", Convert.ToInt32(Math.Floor(p * 100)), 100);
                Debug.WriteLine(progress);
                if (p == 1) {
                    MediaSource mediaSource = MediaSource.CreateFromStream(file.AsRandomAccessStream(), "video/MPEG-4");
                    mediaPlayerElement.Source = mediaSource;
                    mediaPlayerElement.AutoPlay = true;
                }
            });

            
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

        


       
       async private void SaveWithFilepickerAndDownload(object sender, RoutedEventArgs e)
        {

          
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Video", new List<string>() { ".mp4",".mkv" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "New File";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);

                var stream = await file.OpenStreamForWriteAsync();
                getYoutubeFileProps(stream);

                // write to file
                //       await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.



                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    this.StatusText.Text = "File " + file.Name + " was saved.";
                    Debug.WriteLine("File created Successfully. Download started");
                }
                else
                {
                    this.StatusText.Text = "File " + file.Name + " couldn't be saved.";
                }
            }
            else
            {
                this.StatusText.Text = "Operation cancelled.";
            }


        }
        public double Progress
        {
            get
            {
                return this.progress;
            }
            set
            {
                this.progress = value;
            }
        }

    }
}
