using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Html;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace JGMSoftware
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class Article2 : JGMSoftware.Common.LayoutAwarePage
    {
        public Article2()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            LoadArticle((string)navigationParameter);          
        }
         
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private void LoadArticle(String articleTitle)
        {
            //Set the page title to the article title passed into the page, which is also the navigation parameter.
            pageTitle.Text = articleTitle;
            //Retrieve the article from the downloaded list of feed items (articles)
            var article = FeedDataSource.GetItem(articleTitle);
            //Set the article text to the content from this item
            String articlecontent = article.Content;
            //Get the screen size to set the correct sizes for the WebView
            var bounds = Window.Current.Bounds;
            //Minus a margin
            Double size = bounds.Height - 200;
            //Format the content string with some CSS and markup using the WebContentHelper class
            string content = WebContentHelper.WrapHtml(articlecontent, 1.0, size);
            //Display the article content
            webView.NavigateToString(content);
        }
    }
}
