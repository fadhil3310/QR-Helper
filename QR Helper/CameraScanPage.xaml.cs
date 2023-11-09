using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.Media;
using Windows.Media.Capture;
using Windows.System.Display;
using Windows.Graphics.Display;
using ZXing;
using Windows.ApplicationModel.DataTransfer;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Windows.Media.MediaProperties;
using Windows.System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QR_Helper
{
		/// <summary>
		/// An empty page that can be used on its own or navigated to within a Frame.
		/// </summary>
		public sealed partial class CameraScanPage : Page
		{
				MediaCapture mediaCapture = null;
				DisplayRequest displayRequest = null;
				Boolean isCameraOpened = false;
				Boolean isDisplayRequested = false;

				ThreadPoolTimer poolTimer = null;

				public CameraScanPage()
				{
						this.InitializeComponent();

						OpenCamera();
				}

				private async void OpenCamera()
				{
						try
						{
								mediaCapture = new MediaCapture();

								await mediaCapture.InitializeAsync();
								isCameraOpened = true;

								displayRequest = new DisplayRequest();
								displayRequest.RequestActive();
								isDisplayRequested = true;

								CameraPreview.Source = mediaCapture;
								await mediaCapture.StartPreviewAsync();

								VideoEncodingProperties encodingProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

								TimeSpan timeSpan = TimeSpan.FromMilliseconds(500);
								poolTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
								{
										VideoFrame videoFrame = await mediaCapture.GetPreviewFrameAsync(new VideoFrame(BitmapPixelFormat.Bgra8, (int)encodingProperties.Width, (int)encodingProperties.Height));
										
										var barcodeReader = new BarcodeReader();
										var result = barcodeReader.Decode(videoFrame.SoftwareBitmap);

										if (result != null)
										{
												source.Cancel();
												await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
												{
														Frame.GoBack();
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
												});
										}
								}, timeSpan);

						}
						catch (UnauthorizedAccessException)
						{
								ContentDialog dialog = new ContentDialog
								{
										Title = "Permission Denied",
										Content = "QR Helper can't access your camera. Please check your camera and microphone settings in Windows Settings.",
										PrimaryButtonText = "Close"
								};
								await dialog.ShowAsync();
								Frame.GoBack();
						}
						catch (Exception ex)
						{
								ContentDialog dialog = new ContentDialog
								{
										Title = "Error",
										Content = "QR Helper has encountered an error.\nError code: " + ex.Message + "\n" + ex.StackTrace,
										PrimaryButtonText = "Close"
								};
								await dialog.ShowAsync();
								Frame.GoBack();
						}


						/*CameraCaptureUI captureUI = new CameraCaptureUI();
						captureUI.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
						var photo = await captureUI.CaptureFileAsync(CameraCaptureUIMode.Photo);

						if (photo != null)
						{
								IRandomAccessStream stream = await photo.OpenAsync(FileAccessMode.Read);
								BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
								SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
								/*SoftwareBitmapSource softwareBitmapSource = new SoftwareBitmapSource();
								await softwareBitmapSource.SetBitmapAsync(SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied));
								photoCapture.Source = softwareBitmapSource;

								WriteableBitmap writeableBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
								writeableBitmap.SetSource(stream);
								photoCapture.Source = writeableBitmap;

								var barcodeReader = new BarcodeReader();
								var scanResult = barcodeReader.Decode(writeableBitmap);

								if (scanResult != null)
								{
										ContentDialog dialog = new ContentDialog
										{
												Title = "Text",
												Content = scanResult.Text,
												PrimaryButtonText = "Copy to clipboard",
												CloseButtonText = "Close",
										};
										ContentDialogResult dialogResult = await dialog.ShowAsync();

										if (dialogResult == ContentDialogResult.Primary)
										{
												DataPackage dataPackage = new DataPackage();
												dataPackage.RequestedOperation = DataPackageOperation.Copy;
												dataPackage.SetText(scanResult.Text);
												Clipboard.SetContent(dataPackage);
										}
								}
								else
								{
										ContentDialog dialog = new ContentDialog
										{
												Title = "Failed",
												Content = "There is no Barcode",
												CloseButtonText = "Close"
										};
										await dialog.ShowAsync();
								}*/
				}

				/*private async Task<Result> AnalyzeCamera(VideoEncodingProperties encodingProperties)
				{
						

						/*int count = 0;
						Timer timer = new Timer();
						timer.Interval = 200;
						timer.AutoReset = false;
						timer.Elapsed += timerElapsed;
						timer.Enabled = true;

						async Task ScanCode()
						{
								VideoFrame videoFrame = await mediaCapture.GetPreviewFrameAsync(new VideoFrame(BitmapPixelFormat.Bgra8, (int)encodingProperties.Width, (int)encodingProperties.Height));

								var barcodeReader = new BarcodeReader();
								var result = barcodeReader.Decode(videoFrame.SoftwareBitmap);
								if (result == null)
								{
										returnValue = AnalyzeCamera(encodingProperties);
								}
								else
								{
										returnValue = result;
								}
						}
						
						async void timerElapsed(Object source, ElapsedEventArgs e)
						{
								count++;
								if(count == 200)
								{
										await ScanCode();
								}
						}
				}*/

				private async void CloseCamera()
				{
						if (poolTimer != null)
						{
								poolTimer.Cancel();
						}
						if (mediaCapture != null && isCameraOpened)
						{
								await mediaCapture.StopPreviewAsync();

								CameraPreview.Source = null;

								mediaCapture.Dispose();
								mediaCapture = null;
						}
						if (displayRequest != null && isDisplayRequested)
						{
								displayRequest.RequestRelease();
						}
				}

				private void CloseButton_Click(object sender, RoutedEventArgs e)
				{
						Frame.GoBack();
				}

				protected override void OnNavigatedFrom(NavigationEventArgs e)
				{
						CloseCamera();
				}
		}
}
