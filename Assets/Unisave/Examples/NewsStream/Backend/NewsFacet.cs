using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Xml;
using Unisave.Facets;

namespace Unisave.Examples.NewsStream.Backend
{
    public class NewsFacet : Facet
    {
        /// <summary>
        /// Returns the latest news, prepared for display
        /// </summary>
        public List<NewsItem> GetLatestNews()
        {
            // === try to get news from the cache first ===
            
            var cachedNews = NewsCacheEntity.GetCachedNews();

            if (cachedNews != null)
                return cachedNews;
            
            // === only then query all the news sources to build the news ===

            var news = BuildTheNews();
            
            NewsCacheEntity.SetCachedNews(news);
            
            return news;
        }

        /// <summary>
        /// Does all the painful work of building the news
        /// from individual sources
        /// </summary>
        private List<NewsItem> BuildTheNews()
        {
            var rawFromSources = DummySource()
                .Concat(UnisaveCommunitySource());
            
            return rawFromSources
                .OrderBy(n => n.Timestamp)
                .Reverse()
                .ToList();
        }

        /// <summary>
        /// Dummy news source that returns the introduction message
        /// </summary>
        private IEnumerable<NewsItem> DummySource()
        {
            yield return new NewsItem() {
                Title = "Welcome to an example news stream",
                Timestamp = DateTime.Now,
                Link = "https://unisave.cloud/",
                Description = "Check out the 'Backend/NewsFacet.cs' file, " +
                              "that's where the news are gathered and " +
                              "prepared."
            };
        }

        /// <summary>
        /// News source that pulls data from the Unisave community RSS feed
        /// </summary>
        private IEnumerable<NewsItem> UnisaveCommunitySource()
        {
            string xml;
            using (var http = new HttpClient())
            {
                // TODO: pull URL from .env file
                xml = http
                    .GetStringAsync("https://unisave.cloud/community/feed/")
                    .GetAwaiter()
                    .GetResult();
            }

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNodeList xmlItems = doc.SelectNodes("//channel/item");

            if (xmlItems == null)
                yield break;
            
            foreach (XmlNode node in xmlItems)
            {
                var titleNode = node.SelectSingleNode("title");
                var timestampNode = node.SelectSingleNode("pubDate");
                var linkNode = node.SelectSingleNode("link");
                var descriptionNode = node.SelectSingleNode("description");
                
                yield return new NewsItem {
                    Title = titleNode?.InnerText,
                    Timestamp = DateTime.Parse(timestampNode?.InnerText),
                    Link = linkNode?.InnerText,
                    Description = descriptionNode?.InnerText
                };
            }
        }
    }
}