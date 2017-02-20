using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZXing.Mobile;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace qr_generator
{

    static class StringExtensions
    {

        public static IEnumerable<String> SplitInParts(this String s, Int32 partLength)
        {
            if (s == null)
                throw new ArgumentNullException("s");
            if (partLength <= 0)
                throw new ArgumentException("Part length has to be positive.", "partLength");

            for (var i = 0; i < s.Length; i += partLength)
                yield return s.Substring(i, Math.Min(partLength, s.Length - i));
        }

    }

    public sealed partial class QRDisplayPage : Page
    {

        public string dataSizeText { get; private set; }
        public string dataContentsText { get; private set; }
        public object fileNameText { get; private set; }
        public object currentFrameNumberText { get; private set; }
        public object totalFramesText { get; private set; }

        private System.Threading.Timer _timer;

        private StorageFile midiFile;
        private string midiFileContents;
        private List<string> splitMidiFileContents;

        private int inputDataIndex = 0;

        private List<Windows.UI.Xaml.Media.Imaging.WriteableBitmap> imageQueue = new List<Windows.UI.Xaml.Media.Imaging.WriteableBitmap>();
        private List<int> dataSizeInfoQueue = new List<int>();
        private List<string> fakeInputDataList = new List<string>();

        private BarcodeWriter barcodeWriter = new BarcodeWriter

        {
            Format = ZXing.BarcodeFormat.QR_CODE,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = 750,
                Height = 750,
                Margin = 30
            }
        };

        public QRDisplayPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is StorageFile)
            {
                midiFile = e.Parameter as StorageFile;
                fileNameText = midiFile.Name;
                Bindings.Update();
                var rawFileContents = await ReadFile(midiFile);
                midiFileContents = System.Text.Encoding.ASCII.GetString(rawFileContents);

                splitMidiFileContents = midiFileContents.SplitInParts(208).ToList();

                byte[] header = new byte[7];
                for (var i = 0; i < splitMidiFileContents.Count; i++)
                {
                    header[0] = Convert.ToByte(i);
                    splitMidiFileContents[i] = System.Convert.ToBase64String(header) + "\n" + splitMidiFileContents[i];
                }
                
                GenerateBitmap();
            }

            base.OnNavigatedTo(e);
            _timer = new System.Threading.Timer(new System.Threading.TimerCallback((obj) => Refresh()), null, 0, 2000);
        }

        public async Task<byte[]> ReadFile(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }


        private void GenerateBitmap()
        {
            foreach (var inputData in splitMidiFileContents)
            {
                var image = barcodeWriter.Write(inputData);
                imageQueue.Add(image);
                var size = inputData.Length * sizeof(char);
                dataSizeInfoQueue.Add(size);
            }
        }

        private async void Refresh()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (inputDataIndex >= imageQueue.Count)
                {
                    inputDataIndex = 0;
                }
                currentFrameNumberText = inputDataIndex;
                totalFramesText = imageQueue.Count;
                var image = imageQueue[inputDataIndex];
                var size = dataSizeInfoQueue[inputDataIndex];
                var contents = splitMidiFileContents[inputDataIndex];
                if(image != null)
                {
                    imageBarcode.Source = image;
                    dataSizeText = size.ToString();
                    dataContentsText = contents;
                }
                inputDataIndex++;
                Bindings.Update();
            });

        }
    }
}
