using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unisave.Examples.NewsStream.Backend;
using Unisave.Facades;
using UnityEngine;
using UnityEngine.UI;

namespace Unisave.Examples.NewsStream
{
    /// <summary>
    /// Script that pulls the news stream from the server
    /// and displays it to a given text UI element
    /// </summary>
    public class NewsStream : MonoBehaviour
    {
        public Text text;
        
        void Start()
        {
            PullLatestNews();
        }

        public void PullLatestNews()
        {
            text.text = "Pulling latest news...";
            
            OnFacet<NewsFacet>
                .Call<List<NewsItem>>(
                    nameof(NewsFacet.GetLatestNews)
                )
                .Then(news => {
                    DisplayNews(news);
                })
                .Catch(exception => {
                    text.text = "Pulling news failed with an exception:\n" +
                        exception.ToString();
                });
        }

        private void DisplayNews(List<NewsItem> news)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in news)
            {
                string title = item.Title;
                string meta = item.Timestamp.ToString("d") + " " + item.Link;
                
                sb.AppendLine(title);
                sb.AppendLine(meta);
                sb.AppendLine(
                    string.Concat(
                        Enumerable.Repeat(
                            "=",
                            Math.Max(title.Length, meta.Length)
                        )
                    )
                );
                sb.AppendLine(item.Description);
                sb.AppendLine();
                sb.AppendLine();
            }

            if (news.Count == 0)
                sb.AppendLine("There are no news :'(");

            text.text = sb.ToString();
        }
    }
}
