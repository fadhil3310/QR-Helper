using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ZXing;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace QR_Helper
{
		/// <summary>
		/// An empty page that can be used on its own or navigated to within a Frame.
		/// </summary>
		public sealed partial class CreatePage : Page
		{
				WriteableBitmap codeBitmap = null;
				string codeType = null;
				string codeContent = null;

				Boolean isMinimal = false;

				public CreatePage()
				{
						this.InitializeComponent();

						//GridLayout.RowDefinitions[0].Height = new GridLength(100);

						/*ContentDialog dialog = new ContentDialog
						{
								Title = "Scan result",
								Content = rowDefinitions[1].,
								PrimaryButtonText = "Copy to clipboard",
								CloseButtonText = "Close",
						};
						dialog.ShowAsync();*/

						//Frame.SizeChanged += Frame_SizeChanged;

						//Window.Current.SizeChanged += Current_SizeChanged;
						if (parentGrid.Width < 592)
						{
								setToMinimalView(true);
						}
				}

				private void Frame_SizeChanged(object sender, SizeChangedEventArgs e)
				{
						windowSizeTextBlock.Text = e.NewSize.Height.ToString();
						parentGrid.RowDefinitions[0].Height = new GridLength(e.NewSize.Height);
				}

				private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
				{
						
				}

				private async void CreateCodeButton_Click(object sender, RoutedEventArgs e)
				{
						int codeWidth = 0;
						int codeHeight = 0;
						try
						{
								codeWidth = int.Parse(codeWidthTextBox.Text);
								codeHeight = int.Parse(codeHeightTextBox.Text);
						}
						catch
						{
								ContentDialog dialog = new ContentDialog
								{
										Title = "Wrong input",
										Content = "Width and height must be a number",
										CloseButtonText = "Close",
								};
								await dialog.ShowAsync();
								return;
						}

						var barcode = new BarcodeWriter();
						switch (codeTypeComboBox.SelectedValue)
						{
								case "QR Code":
										barcode.Format = BarcodeFormat.QR_CODE;
										break;
								case "Data matrix":
										barcode.Format = BarcodeFormat.DATA_MATRIX;
										break;
								case "Aztec":
										barcode.Format = BarcodeFormat.AZTEC;
										break;
								case "UPC-A":
										barcode.Format = BarcodeFormat.UPC_A;
										break;
								case "UPC-E":
										barcode.Format = BarcodeFormat.UPC_E;
										break;
								case "Code 39":
										barcode.Format = BarcodeFormat.CODE_39;
										break;
								case "Code 93":
										barcode.Format = BarcodeFormat.CODE_93;
										break;
								case "Code 128":
										barcode.Format = BarcodeFormat.CODE_128;
										break;
								case "EAN-8":
										barcode.Format = BarcodeFormat.EAN_8;
										break;
								case "EAN-13":
										barcode.Format = BarcodeFormat.EAN_13;
										break;
								case "Codabar":
										barcode.Format = BarcodeFormat.CODABAR;
										break;
								case "MSI":
										barcode.Format = BarcodeFormat.MSI;
										break;
								case "PDF417":
										barcode.Format = BarcodeFormat.PDF_417;
										break;
								case "IMB":
										barcode.Format = BarcodeFormat.IMB;
										break;
								case "ITF":
										barcode.Format = BarcodeFormat.ITF;
										break;
						}

						barcode.Options.Width = codeWidth;
						barcode.Options.Height = codeHeight;
						barcode.Options.GS1Format = (bool)GS1CheckBox.IsChecked;
						barcode.Options.PureBarcode = (bool)pureBarcodeCheckBox.IsChecked;
						try
						{
								codeBitmap = barcode.Write(contentTextBox.Text);
						}
						catch (Exception ex)
						{
								ContentDialog dialog = new ContentDialog
								{
										Title = "Wrong input",
										Content = ex.Message,
										CloseButtonText = "Close",
								};
								await dialog.ShowAsync();
								return;
						}
						codeContent = contentTextBox.Text;
						codeType = codeTypeComboBox.SelectedValue.ToString();
						codeImage.Source = codeBitmap;

						if (!saveCodeButton.IsEnabled)
						{
								saveCodeButton.IsEnabled = true;
								// Workaround to fix SaveCodeButton icon not using the correct color
								saveCodeButton.Foreground = (Brush) Application.Current.Resources["ButtonForegroundThemeBrush"];
						}
				}

				private async void SaveCodeButton_Click(object sender, RoutedEventArgs e)
				{
						if (codeBitmap != null)
						{
								int lastIndex = 20;
								string dot = "..";
								if (codeContent.Length < 20)
								{
										lastIndex = codeContent.Length;
										dot = "";
								}

								FileSavePicker fileSavePicker = new FileSavePicker();
								fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
								fileSavePicker.SuggestedFileName = codeType + " - " + codeContent.Substring(0, lastIndex) + dot;
								fileSavePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpeg", ".jpg", ".png" });

								StorageFile file = await fileSavePicker.PickSaveFileAsync();
								if (file != null)
								{
										Guid fileType;
										Boolean isThumbnailGenerated = true;
										switch (file.FileType)
										{
												case ".jpeg":
														fileType = BitmapEncoder.JpegEncoderId;
														break;
												case ".jpg":
														fileType = BitmapEncoder.JpegEncoderId;
														break;
												case ".png":
														fileType = BitmapEncoder.PngEncoderId;
														isThumbnailGenerated = false;
														break;
										}

										BitmapEncoder bitmapEncoder = await BitmapEncoder.CreateAsync(fileType, await file.OpenAsync(FileAccessMode.ReadWrite));
										SoftwareBitmap softwareBitmap = SoftwareBitmap.CreateCopyFromBuffer(codeBitmap.PixelBuffer, BitmapPixelFormat.Bgra8, codeBitmap.PixelWidth, codeBitmap.PixelHeight);
										bitmapEncoder.SetSoftwareBitmap(softwareBitmap);

										bitmapEncoder.IsThumbnailGenerated = isThumbnailGenerated;

										try
										{
												await bitmapEncoder.FlushAsync();
										}
										catch (Exception ex)
										{
												ContentDialog dialog = new ContentDialog
												{
														Title = "Something went wrong",
														Content = "Code couldn't be saved\nError code: " + ex.Message + "\n" + ex.StackTrace,
														CloseButtonText = "Close",
												};
												await dialog.ShowAsync();
												/*try
												{
														bitmapEncoder.IsThumbnailGenerated = false;
														await bitmapEncoder.FlushAsync();
												}
												catch (Exception ex)
												{
														ContentDialog dialog = new ContentDialog
														{
																Title = "Something went wrong",
																Content = "Code couldn't be saved\nError code: " + ex.Message + "\n" + ex.StackTrace,
																CloseButtonText = "Close",
														};
														await dialog.ShowAsync();
												}*/
										}


										/*BitmapImage bitmapImage = new BitmapImage();
										bitmapImage.DecodePixelWidth = codeImage.PixelWidth;
										bitmapImage.DecodePixelHeight = codeImage.PixelHeight;
										await bitmapImage.SetSourceAsync(codeImage.PixelBuffer.AsStream().AsRandomAccessStream());
										//var convertedBitmap = SoftwareBitmap.Convert(await decoder.GetSoftwareBitmapAsync(), BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
										//var buffer = codeImage.PixelBuffer;
										//convertedBitmap.CopyToBuffer(buffer);
										await FileIO.WriteBufferAsync(file, bitmapImage.);

										FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

										if (status == FileUpdateStatus.Failed)
										{

										}*/
								}
						}
				}

				private void ContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
				{
						if (contentTextBox.Text == "")
						{
								createCodeButton.IsEnabled = false;
								// Workaround to fix CreateCodeButton icon not using the correct color
								//CreateCodeButton.Foreground = (Brush)Application.Current.Resources["ButtonDisabledForegroundThemeBrush"];
						}
						else if (!createCodeButton.IsEnabled)
						{
								createCodeButton.IsEnabled = true;
								// Workaround to fix CreateCodeButton icon not using the correct foreground color
								//CreateCodeButton.Foreground = (Brush)Application.Current.Resources["ButtonForegroundThemeBrush"];
						}
				}

				private void parentGrid_SizeChanged(object sender, SizeChangedEventArgs e)
				{
						//parentGrid.ColumnDefinitions[0].Width = new GridLength(e.NewSize.Width / 2);
						//parentGrid.ColumnDefinitions[1].Width = new GridLength(e.NewSize.Width / 2);
						if (!isMinimal)
						{
								if (e.NewSize.Width < 592)
								{
										setToMinimalView(true);
								}
								else
								{
										contentTextBox.Width = e.NewSize.Width - 378;
								}
						}
						else if (e.NewSize.Width > 592)
						{
								setToMinimalView(false);
						}
						//windowSizeTextBlock.Text = e.NewSize.Width.ToString();
				}

				private void setToMinimalView(Boolean mode)
				{
						if (mode)
						{
								contentStackPanel.Orientation = Orientation.Vertical;
								contentTextBox.Width = double.NaN;
								createCodeButton.Margin = new Thickness(0, 4, 0, 0);
								optionSizeStackPanel.Orientation = Orientation.Vertical;
								codeWidthTextBox.Width = double.NaN;
								codeHeightTextBox.Width = double.NaN;
								codeHeightTextBox.Margin = new Thickness(0, 4, 0, 0);
						}
						else
						{
								contentStackPanel.Orientation = Orientation.Horizontal;
								contentTextBox.Width = 270;
								createCodeButton.Margin = new Thickness(4, 0, 0, 0);
								optionSizeStackPanel.Orientation = Orientation.Horizontal;
								codeWidthTextBox.Width = 140;
								codeHeightTextBox.Width = 140;
								codeHeightTextBox.Margin = new Thickness(4, 0, 0, 0);
						}
						isMinimal = mode;
				}
		}
}
