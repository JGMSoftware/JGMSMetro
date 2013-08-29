using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace JGMSoftware
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        Windows.Storage.StorageFolder localFolder = Windows.Storage.ApplicationData.Current.LocalFolder;


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                //If data is cached, this will later be set to true and used when loading the app in offline mode.
                Boolean cacheAvailable;
                if (localSettings.Values.ContainsKey("cacheAvailable"))
                {
                    cacheAvailable = (bool)localSettings.Values["cacheAvailable"];
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("No key was found.");
                    cacheAvailable = false;
                }
                IStorageFile cacheFile = await localFolder.CreateFileAsync("cache.xml",CreationCollisionOption.ReplaceExisting);

                // Add this code after "rootFrame = new Frame();"
                var connectionProfile = Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile();
                //There is an internet connection
                if (connectionProfile != null)
                {
                    FeedDataSource feedDataSource = (FeedDataSource)App.Current.Resources["feedDataSource"];
                    if (feedDataSource != null)
                    {
                        if (feedDataSource.Feeds.Count == 0)
                        {
                            await feedDataSource.GetFeedsAsync();

                            System.Diagnostics.Debug.WriteLine("Caching feeds...");
                            await Windows.Storage.FileIO.WriteTextAsync(cacheFile, prepCacheFeed(feedDataSource));
                            localSettings.Values["cacheAvailable"] = true;

                        }
                    }
                }
                else
                //There is no internet connection
                {


                    if (cacheAvailable == true)
                    {

                        //If there is cached data available, use that
                        var cacheMessageDialog = new Windows.UI.Popups.MessageDialog("An internet connection is needed to download the feed articles. The feeds will be loaded from an offline cache - there may be more up to date articles available. Check again when you have an internet connection!", "Offline Mode");
                        cacheMessageDialog.Commands.Add(new UICommand("Okay"));
                        var cacheMsgShow = cacheMessageDialog.ShowAsync();

                        //Load the data from the cache
                        FeedDataSource feedDataSource = (FeedDataSource)App.Current.Resources["feedDataSource"];
                        IStorageFile cacheIn = await localFolder.GetFileAsync("cache.xml");
                        String cacheData = await Windows.Storage.FileIO.ReadTextAsync(cacheIn);
                        feedDataSource = loadCache(cacheData);

                    }
                    else
                    {

                        //If there is no cached data, display an error then close the app.
                        var errorMessageDialog = new Windows.UI.Popups.MessageDialog("An internet connection is needed to download the feed articles. No cached data was detected, so the app will now close.", "Internet Connection Error");
                        errorMessageDialog.Commands.Add(new UICommand("Okay", msgClose, 0));
                        var result = errorMessageDialog.ShowAsync();
                    }
                }



                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }


            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        //Close the app when the message box ok button is clicked.
        void msgClose(IUICommand command)
        {
            App.Current.Exit();
        }

        public static String prepCacheFeed(FeedDataSource feedDataSource)
        {
            try
            {
                //Serialize the feed data
                XmlSerializer xmlIzer = new XmlSerializer(typeof(FeedDataSource));
                var writer = new StringWriter();
                xmlIzer.Serialize(writer, feedDataSource);
                System.Diagnostics.Debug.WriteLine(writer.ToString());
                return writer.ToString();
            }

            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                return String.Empty;
            }

        }

        public static FeedDataSource loadCache(String cache)
        {
            try
            {
                //Deserialize the feed data
                XmlSerializer xmlIzer = new XmlSerializer(typeof(FeedDataSource));
                XmlReader xmlRead = XmlReader.Create(cache);
                FeedDataSource cachedFeed = new FeedDataSource();
                cachedFeed = (xmlIzer.Deserialize(xmlRead)) as FeedDataSource;
                return cachedFeed;
            }

            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                FeedDataSource cacheFail = new FeedDataSource();
                return cacheFail;
            }
        }
    }
}
