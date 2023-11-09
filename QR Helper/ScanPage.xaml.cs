using Windows.UI.Xaml.Controls;
using Windows.Media.Capture;
using Windows.Storage;
using System;
using Windows.Storage.Pickers;
using ZXing;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QR_Helper
{
		/// <summary>
		/// An empty page that can be used on its own or navigated to within a Frame.
		/// </summary>
		public sealed partial class ScanPage : Page
		{
				public ScanPage()
				{
						this.InitializeComponent();

				}

				private void CaptureImageButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
				{
						Frame.Navigate(typeof(CameraScanPage));
				}

				private async void OpenImageButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
				{
						FileOpenPicker filePicker = new FileOpenPicker();
						filePicker.ViewMode = PickerViewMode.Thumbnail;
						filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
						filePicker.FileTypeFilter.Add(".png");
						filePicker.FileTypeFilter.Add(".jpg");
						filePicker.FileTypeFilter.Add(".jpeg");

						Windows.Storage.StorageFile file = await filePicker.PickSingleFileAsync();
						if(file != null)
						{
								changeProgressBarVisibility(true);

								IRandomAccessStreamWithContentType stream = await file.OpenReadAsync();
								BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
								scanForBarcode(await decoder.GetSoftwareBitmapAsync());

								changeProgressBarVisibility(false);
						}
				}

				private async void scanForBarcode(SoftwareBitmap bitmap)
				{
						var barcodeReader = new BarcodeReader();
						var result = barcodeReader.Decode(bitmap);
						if (result != null)
						{
								ContentDialog dialog = new ContentDialog
								{
										Title = "Scan result",
										Content = result.Text,
										PrimaryButtonText = "Copy to clipboard",
										CloseButtonText = "Close",
								};
								ContentDialogResult dialogResult = await dialog.ShowAsync();

								if (dialogResult == ContentDialogResult.Primary)
								{
										DataPackage dataPackage = new DataPackage();
										dataPackage.RequestedOperation = DataPackageOperation.Copy;
										dataPackage.SetText(result.Text);
										Clipboard.SetContent(dataPackage);
								}
						}
						else
						{
								/*CoreApplicationView appView = CoreApplication.CreateNewView();
								int appViewId = 0;
								await appView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
								{
										Frame appViewFrame = new Frame();
										appViewFrame.Navigate(typeof(SettingsPage), null);
										Window.Current.Content = appViewFrame;
										Window.Current.Activate();

										appViewId = ApplicationView.GetForCurrentView().Id;
								});
								await ApplicationViewSwitcher.TryShowAsStandaloneAsync(appViewId);*/

								ContentDialog dialog = new ContentDialog
								{
										Title = "No code",
										Content = "We can't find any code in the picture",
										CloseButtonText = "Close"
								};
								await dialog.ShowAsync();
						}
				}

				private void changeProgressBarVisibility(Boolean visibility)
				{
						if (visibility)
						{
								progressBar.Visibility = Visibility.Visible;
								OpenImageButton.IsEnabled = false;
								CaptureImageButton.IsEnabled = false;
						}
						else
						{
								progressBar.Visibility = Visibility.Collapsed;
								OpenImageButton.IsEnabled = true;
								CaptureImageButton.IsEnabled = true;
						}
				}

				private void dropGrid_DragOver(object sender, DragEventArgs e)
				{
						e.AcceptedOperation = DataPackageOperation.Copy;
				}

				private async void dropGrid_Drop(object sender, DragEventArgs e)
				{
						if (e.DataView.Contains(StandardDataFormats.StorageItems))
						{
								var items = await e.DataView.GetStorageItemsAsync();
								if (items.Count == 1)
								{
										StorageFile file = (StorageFile) items[0];
										if (file.FileType == ".jpg" || file.FileType == ".jpeg" || file.FileType == ".png")
										{
												changeProgressBarVisibility(true);

												BitmapDecoder bitmapDecoder = await BitmapDecoder.CreateAsync(await file.OpenAsync(FileAccessMode.Read));
												scanForBarcode(await bitmapDecoder.GetSoftwareBitmapAsync());

												changeProgressBarVisibility(false);
										}
								}
						}
				}

				private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
				{
						ContentDialog contentDialog = new SupportedCodesDialog();
						await contentDialog.ShowAsync();
				}
		}
}
