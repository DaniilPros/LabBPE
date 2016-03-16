using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Imaging;
using ComparisonLab.Models;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ComparisonLab
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Picture> _imageItems;
        private ObservableCollection<String> _textCollection; 
        public MainPage()
        {
            this.InitializeComponent();
            _imageItems = new ObservableCollection<Picture>();
            _textCollection = new ObservableCollection<string>();
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assets = await appInstalledFolder.GetFolderAsync("Assets");
            var imagesFolder =await assets.GetFolderAsync("Images");
            var allImages = (await imagesFolder.GetFilesAsync(CommonFileQuery.OrderByName)).ToList();
                    
            var images = new List<StorageFile>();
            for (var i = 0; i < 10; i++)
            {
                var rand= new Random();
                var image = allImages[rand.Next(allImages.Count)];
                if (!Enumerable.Contains(images, image))
                {
                    images.Add(image);
                }
                else i--;
            }

            //Image = new BitmapImage(new Uri(image.Path, UriKind.RelativeOrAbsolute))
            var imageItemsL = images.Select(image => new Picture {
                Image = image.Path,
                Tag = image.Name.Substring(0, image.Name.Length - 4)}
               ).ToList();

            imageItemsL.ForEach(i=>_imageItems.Add(i));
            //ImagesView.ItemsSource = imageItems;



            var textItemsL = images.Select(i => new TextBlock
            {
                Text = i.Name.Substring(0,i.Name.Length - 4),
                FontSize = 24,
                FontFamily = new FontFamily("Comic Sans MS")
            }).ToList();
            textItemsL = Stir<TextBlock>(textItemsL);

            textItemsL.ForEach(t=>_textCollection.Add(t.Text));
            TextView.ItemsSource = _textCollection;
        }

        private void Image_Drop(object sender, DragEventArgs e)
        {
            object textBlockObject;
            if (!e.DataView.Properties.TryGetValue("text", out textBlockObject)) return;
            var textBlock = textBlockObject as TextBlock;

            var tag = ((Image) sender).Tag.ToString();

            if (tag != textBlock?.Text) return;
            _imageItems.Remove(Search(_imageItems,tag));
            _textCollection.Remove(textBlock?.Text);
        }

        private static Picture Search(IEnumerable<Picture> list, string tag)
        {
            var l = list.Where(i => i.Tag == tag).ToList();
            return l.Count == 1 ? l[0] : null;
        }

        private static List<T> Stir<T>(IReadOnlyList<T> list)
        {
            var list2 = new List<T>();
            var listI = new List<int>();
            var rand  =  new Random();
            while (list.Count > listI.Count)
            {
                var i = rand.Next(list.Count);
                if (Enumerable.Contains(listI, i)) continue;
                listI.Add(i);
                list2.Add(list[i]);
            }
            return list2;
        }


        private void TextView_OnDragStarting(UIElement sender, DragStartingEventArgs e)
        {
            e.Data.Properties.Add("text",sender as TextBlock);
        }

        private void ImagesView_OnDragEnter(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Link;
        }

        private void ImagesView_OnDragLeave(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.None;
        }

    }
}
