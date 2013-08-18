using JGMSoftware;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Syndication;

namespace JGMSoftware
{
    // FeedData
    // Holds info for a single blog feed, including a list of blog posts (FeedItem).
    public class FeedData
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PubDate { get; set; }

        private List<FeedItem> _Items = new List<FeedItem>();
        public List<FeedItem> Items
        {
            get
            {
                return this._Items;
            }
        }
    }

    // FeedItem
    // Holds info for a single blog post.
    public class FeedItem
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Content { get; set; }
        public DateTime PubDate { get; set; }
        public Uri Link { get; set; }
        //Thumbnail is the image shown on the tile on the main page
        public BitmapImage Thumbnail { get; set; }
    }

    // FeedDataSource
    // Holds a collection of blog feeds (FeedData), and contains methods needed to
    // retreive the feeds.
    public class FeedDataSource
    {
        private ObservableCollection<FeedData> _Feeds = new ObservableCollection<FeedData>();
        public ObservableCollection<FeedData> Feeds
        {
            get
            {
                return this._Feeds;
            }
        }

        public async Task GetFeedsAsync()
        {
            Task<FeedData> feed1 = GetFeedAsync("http://www.jgmsoftware.co.uk/feed");
            this.Feeds.Add(await feed1);
        }

        private async Task<FeedData> GetFeedAsync(string feedUriString)
        {
            Windows.Web.Syndication.SyndicationClient client = new SyndicationClient();
            Uri feedUri = new Uri(feedUriString);

            try
            {
                SyndicationFeed feed = await client.RetrieveFeedAsync(feedUri);

                // This code is executed after RetrieveFeedAsync returns the SyndicationFeed.
                // Process the feed and copy the data you want into the FeedData and FeedItem classes.
                FeedData feedData = new FeedData();

                if (feed.Title != null && feed.Title.Text != null)
                {
                    feedData.Title = feed.Title.Text;
                }
                if (feed.Subtitle != null && feed.Subtitle.Text != null)
                {
                    feedData.Description = feed.Subtitle.Text;
                }
                if (feed.Items != null && feed.Items.Count > 0)
                {
                    // Use the date of the latest post as the last updated date.
                    feedData.PubDate = feed.Items[0].PublishedDate.DateTime;

                    //For every article in the feed
                    foreach (SyndicationItem item in feed.Items)
                    {
                        FeedItem feedItem = new FeedItem();
                        if (item.Title != null && item.Title.Text != null)
                        {
                            feedItem.Title = item.Title.Text;
                            //System.Diagnostics.Debug.WriteLine("FOUND ARTICLE " + item.Title.Text);
                        }
                        if (item.PublishedDate != null)
                        {
                            feedItem.PubDate = item.PublishedDate.DateTime;
                        }
                        if (item.Authors != null && item.Authors.Count > 0)
                        {
                            feedItem.Author = item.Authors[0].Name.ToString();
                        }
                        // Handle the differences between RSS and Atom feeds.
                        if (feed.SourceFormat == SyndicationFormat.Atom10)
                        {
                            if (item.Content != null && item.Content.Text != null)
                            {
                                feedItem.Content = item.Content.Text;
                            }
                            if (item.Id != null)
                            {
                                feedItem.Link = new Uri("http://windowsteamblog.com" + item.Id);
                            }
                        }
                        else if (feed.SourceFormat == SyndicationFormat.Rss20)
                        {
                            if (item.Summary != null && item.Summary.Text != null)
                            {
                                feedItem.Content = item.Summary.Text;
                            }
                            if (item.Links != null && item.Links.Count > 0)
                            {
                                feedItem.Link = item.Links[0].Uri;
                            }
                        }

                        //For each article, generate a list of all the image URLs in the content.
                        List<Uri> links = new List<Uri>();
                        string regexImgSrc = @"<img[^>]*?src\s*=\s*[""']?([^'"" >]+?)[ '""][^>]*?>";
                        MatchCollection matchesImgSrc = Regex.Matches(feedItem.Content, regexImgSrc, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                        foreach (Match m in matchesImgSrc)
                        {
                            string href = m.Groups[1].Value;
                            links.Add(new Uri(href));

                        }

                        //Set the tile image to a default image
                        Uri img = new Uri("http://jgmsoftware.azurewebsites.net/wp-content/uploads/2013/05/cropped-cropped-JGMSoftware125.png");

                        if (links.Count != 0)
                        {
                            //If the article has any images, set the tile image to the first one.
                            img = links.ElementAt(0);
                        }

                        //System.Diagnostics.Debug.WriteLine(feedItem.Title + ": " + img);
                        feedItem.Thumbnail = new BitmapImage(img);
                        //System.Diagnostics.Debug.WriteLine("--Successfully set image " + img + " as image for " + feedItem.Title);
                        feedData.Items.Add(feedItem);
                        //System.Diagnostics.Debug.WriteLine("--Successfully added this article to the collection\n");




                    }
                }
                return feedData;
            }
            catch (Exception)
            {
                return null;
            }
        }


        // Returns the feed that has the specified title.
        public static FeedData GetFeed(string title)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;

            var matches = _feedDataSource.Feeds.Where((feed) => feed.Title.Equals("JGM Software"));

            if (matches.Count() == 1) return matches.First();
            return null;

            // Simple linear search is acceptable for small data sets
            //var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            //var matches = _feedDataSource.Feeds.Where((feed) => feed.Title.Equals("JGM Software"));
            //if (matches.Count() == 1) return matches.First();
            //return null;
        }

        // Returns the post that has the specified title.
        public static FeedItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var _feedDataSource = App.Current.Resources["feedDataSource"] as FeedDataSource;
            var _feeds = _feedDataSource.Feeds;

            var matches = _feedDataSource.Feeds.SelectMany(group => group.Items).Where((item) => item.Title.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }
    }
}