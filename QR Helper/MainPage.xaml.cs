using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace QR_Helper
{
		/// <summary>
		/// An empty page that can be used on its own or navigated to within a Frame.
		/// </summary>
		public sealed partial class MainPage : Page
		{
				string selectedItem = "Scan";

				public MainPage()
				{
						this.InitializeComponent();

						var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
						coreTitleBar.ExtendViewIntoTitleBar = true;

						/*ContentDialog dialog = new ContentDialog
						{
								Title = "Scan result",
								Content = titlebar.Height,
								PrimaryButtonText = "Copy to clipboard",
								CloseButtonText = "Close",
						};
						dialog.ShowAsync();*/

						TitleBar.Padding = new Thickness(coreTitleBar.SystemOverlayLeftInset, 0, coreTitleBar.SystemOverlayRightInset, 0);

						Window.Current.SetTitleBar(TitleBar);

						//coreTitleBar.LayoutMetricsChanged += titleBar_LayoutMetricsChanged;
						NavView.DisplayModeChanged += NavView_DisplayModeChanged;

						var titleBar = ApplicationView.GetForCurrentView().TitleBar;
						titleBar.ButtonBackgroundColor = Colors.Transparent;
						titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
				}

				private void NavView_DisplayModeChanged(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs args)
				{
						switch (args.DisplayMode)
						{
								case Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal:
										TitleBar.Height = 48;
										break;
								case Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Compact:
										TitleBar.Height = 32;
										break;
								case Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Expanded:
										TitleBar.Height = 32;
										break;
						}
				}

				private void titleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar coreTitleBar, object args)
				{
						TitleBar.Height = coreTitleBar.Height;
						//NavView.ContentOverl .Margin = new Thickness(0, coreTitleBar.Height, 0, 0);
						/*ContentDialog dialog = new ContentDialog
						{
								Title = "Scan result",
								Content = coreTitleBar.Height,
								PrimaryButtonText = "Copy to clipboard",
								CloseButtonText = "Close",
						};
						dialog.ShowAsync();*/
				}

				private void NavigationView_Loaded(object sender, RoutedEventArgs e)
				{
						NavView.SelectedItem = itemScan;
						NavView.Header = "Scan";
						ContentFrame.Navigate(typeof(ScanPage));
				}

				private void NavigationView_ItemInvoked(Microsoft.UI.Xaml.Controls.NavigationView sender, Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs args)
				{
						string invokedItem = null;
						if (args.IsSettingsInvoked) { invokedItem = "Settings"; }
						else { invokedItem = args.InvokedItem.ToString(); }

						if(invokedItem != selectedItem)
						{
								Type destination = null;
								switch (invokedItem)
								{
										case "Scan":
												destination = typeof(ScanPage);
												NavView.Header = "Scan";
												break;
										case "Create":
												destination = typeof(CreatePage);
												NavView.Header = "Create";
												break;
										case "Settings":
												destination = typeof(SettingsPage);
												NavView.Header = "Settings";
												break;
								}

								selectedItem = invokedItem;
								ContentFrame.Navigate(destination, args.RecommendedNavigationTransitionInfo);
						}
				}
		}
}
