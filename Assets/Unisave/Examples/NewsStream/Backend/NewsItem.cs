using System;

namespace Unisave.Examples.NewsStream.Backend
{
    /// <summary>
    /// Represents one message in the news stream
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// News item title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// When did this news item happen
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Short description of the item
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// URL link to more details about the item
        /// </summary>
        public string Link { get; set; }
    }
}