using System;
using System.Collections.Generic;
using Unisave.Entities;
using Unisave.Facades;

namespace Unisave.Examples.NewsStream.Backend
{
    /// <summary>
    /// Caches the news in the database so that we don't need to
    /// work so hard with each request.
    /// </summary>
    public class NewsCacheEntity : Entity
    {
        /// <summary>
        /// How old cache is considered to be expired
        /// </summary>
        public const double CacheExpiryMinutes = 10;
        
        /// <summary>
        /// Holds the cached news
        /// </summary>
        public List<NewsItem> News { get; set; }
            = new List<NewsItem>();

        /// <summary>
        /// Returns the cached news, or null if there's nothing cached
        /// </summary>
        public static List<NewsItem> GetCachedNews()
        {
            NewsCacheEntity entity = GetOrCreateEntity();

            // nothing cached
            if (entity.News == null || entity.News.Count == 0)
                return null;
            
            // cached, but expired
            double age = (DateTime.UtcNow - entity.UpdatedAt).TotalMinutes;
            if (age > CacheExpiryMinutes)
                return null;

            return entity.News;
        }

        /// <summary>
        /// Stores news items in the cache
        /// </summary>
        public static void SetCachedNews(List<NewsItem> news)
        {
            NewsCacheEntity entity = GetOrCreateEntity();
            entity.News = news;
            entity.Save(); // sets the UpdatedAt value to "now" automatically
        }
        
        /// <summary>
        /// Finds the cache entity in the database or creates and saves new one
        /// </summary>
        private static NewsCacheEntity GetOrCreateEntity()
        {
            var entity = DB.First<NewsCacheEntity>();
            
            if (entity == null)
            {
                entity = new NewsCacheEntity();
                entity.Save();
            }

            return entity;
        }
    }
}