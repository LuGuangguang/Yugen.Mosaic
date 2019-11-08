﻿using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Yugen.Mosaic.Uwp.Enums;
using Yugen.Mosaic.Uwp.Extensions;
using Yugen.Mosaic.Uwp.Helpers;
using Yugen.Mosaic.Uwp.Models;
using Yugen.Mosaic.Uwp.Services;
using Yugen.Toolkit.Uwp.Helpers;
using Yugen.Toolkit.Uwp.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Yugen.Mosaic.Uwp
{
    public class MainViewModel : BaseViewModel
    {
        private WriteableBitmap masterBmpSource;
        public WriteableBitmap MasterBpmSource
        {
            get { return masterBmpSource; }
            set { Set(ref masterBmpSource, value); }
        }


        private int tileWidth = 50;
        public int TileWidth
        {
            get { return tileWidth; }
            set
            {
                Set(ref tileWidth, value);
                tileSize.Width = tileWidth;
            }
        }

        private int tileHeight = 50;
        public int TileHeight
        {
            get { return tileHeight; }
            set
            {
                Set(ref tileHeight, value);
                tileSize.Height = tileHeight;
            }
        }

        private Size tileSize = new Size(50, 50);

        private ObservableCollection<WriteableBitmap> tileBmpList = new ObservableCollection<WriteableBitmap>();
        public ObservableCollection<WriteableBitmap> TileBmpList
        {
            get { return tileBmpList; }
            set { Set(ref tileBmpList, value); }
        }


        private WriteableBitmap outputBmpSource;
        public WriteableBitmap OutputBmpSource
        {
            get { return outputBmpSource; }
            set { Set(ref outputBmpSource, value); }
        }

        private int outputWidth = 100;
        public int OutputWidth
        {
            get { return outputWidth; }
            set
            {
                Set(ref outputWidth, value);
                outputSize.Width = outputWidth;
            }
        }

        private int outputHeight = 100;
        public int OutputHeight
        {
            get { return outputHeight; }
            set
            {
                Set(ref outputHeight, value);
                outputSize.Height = outputHeight;
            }
        }

        private Size outputSize = new Size(100, 100);

        private bool isLoading;
        public bool IsLoading
        {
            get { return isLoading; }
            set { Set(ref isLoading, value); }
        }

        private bool isAdjustHue;
        public bool IsAdjustHue
        {
            get { return isAdjustHue; }
            set { Set(ref isAdjustHue, value); }
        }

        private Image masterImage;
        private List<Image> tileImageList = new List<Image>();


        private async Task<WriteableBitmap> ImageToWriteableBitmap(Image masterImage)
        {
            InMemoryRandomAccessStream outputStream = new InMemoryRandomAccessStream();
            masterImage.SaveAsBmp(outputStream.AsStreamForWrite());
            return await BitmapFactory.FromStream(outputStream);
        }


        public async void AddMasterButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var masterFile = await FilePickerHelper.OpenFile(new List<string> { ".jpg", ".png" });
            if (masterFile == null)
                return;
            
            using (var inputStream = await masterFile.OpenReadAsync())
            using (var stream = inputStream.AsStreamForRead())
            {
                masterImage = Image.Load(stream);
                //MasterBpmSource = await BitmapFactory.FromStream(stream);
            }

            MasterBpmSource = await ImageToWriteableBitmap(masterImage);
        }

        public async void AddTilesButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var files = await FilePickerHelper.OpenFiles(new List<string> { ".jpg", ".png" });
            if (files == null)
                return;

            foreach (var file in files)
            {
                Image image;
                using (var inputStream = await file.OpenReadAsync())
                using (var stream = inputStream.AsStreamForRead())
                {
                    image = Image.Load(stream);
                    tileImageList.Add(image);
                    //var bmp = await BitmapFactory.FromStream(stream);
                    //tileBmpList.Add(bmp);
                }

                var bmp = await ImageToWriteableBitmap(image);
                tileBmpList.Add(bmp);
            }
        }

        public void GenerateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            IsLoading = true;
            var resizedMaster = MasterBpmSource.Resize(outputWidth, outputHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

            MosaicService mosaicClass = new MosaicService();
            LockBitmap mosaicBmp = mosaicClass.GenerateMosaic(resizedMaster, outputSize, tileBmpList.ToList(), tileSize, isAdjustHue);

            OutputBmpSource = mosaicBmp.Output;
            IsLoading = false;
        }

        public async void SaveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var fileFormat = FileFormat.Jpg;
            var file = await FilePickerHelper.SaveFile("Mosaic", "Image", fileFormat.FileFormatToString());
            if (file == null)
                return;

            await WriteableBitmapHelper.WriteableBitmapToStorageFile(file, outputBmpSource, fileFormat);
        }


        //Parallel.For(0, 10, i =>
        //{
        //    System.Diagnostics.Debug.WriteLine($"{i} {clone.PixelHeight}");
        //});

        //public async Task RunTasks(WriteableBitmap clone)
        //{
        //    var tasks = new List<Task>();

        //    tasks.Add(Task.Run(() => DoWork(400, 1, clone)));
        //    tasks.Add(Task.Run(() => DoWork(200, 2, clone)));
        //    tasks.Add(Task.Run(() => DoWork(300, 3, clone)));

        //    await Task.WhenAll(tasks);
        //}

        //public async Task DoWork(int delay, int n, WriteableBitmap masterImageSource)
        //{
        //    await Task.Delay(delay);
        //    System.Diagnostics.Debug.WriteLine($"{n} {masterImageSource.PixelHeight}");
        //}
    }
}
