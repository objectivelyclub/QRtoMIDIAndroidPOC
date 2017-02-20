using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ZXing.Mobile;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace qr_generator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public StorageFile midiFile;
        public String midiFileName;
        public byte[] midiFileContents;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void qrGenerateButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(QRDisplayPage), midiFile);           
        }

        private async void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add(".mid");
            filePicker.FileTypeFilter.Add(".midi");
            midiFile = await filePicker.PickSingleFileAsync();
            if (midiFile != null)
            {
                fileNameTextBlock.Text = midiFile.Name;
            }
        }
    }
}
