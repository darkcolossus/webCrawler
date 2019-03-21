/*
 * Developed by: https://github.com/darkcolossus
 * Date: 20/03/2019
*/

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebCrawler
{
    //Summary: Main class that runs the app 
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Crawler webCrawler = new Crawler("https://es.wikipedia.org/wiki/");
            await webCrawler.Start();
            Console.ReadKey();
        }
    }

    //Summary: Class Crawler models
    class Crawler
    {
        private readonly object locking_webPagesVisited = new object();
        private int webPagesVisited = 0;
        private readonly object locking_webPages = new object();
        private List<string> webpages;
        private readonly object locking_wordCounter = new object();
        private Dictionary<string, int> wordCounter;

        public Crawler(string webpage) {
            webpages = new List<string> { webpage };
            wordCounter = new Dictionary<string, int>();
        }

        public async Task Start()
        {
            await Crawl(webpages[0]);
        }

        //Summary: crawl method into currentWebPage
        public async Task Crawl(string currentWebPage) {

            //when webPages max amount is reached, crawl ends
            if (webPagesVisited == 10)
            {
                return;
            }

            //webPagesVisited needs to be blocked beacuse it is a shared resource
            lock (locking_webPagesVisited)
            {
                webPagesVisited++;
            }

            //Request WebPage
            HttpClient client = new HttpClient();
            Console.WriteLine("Requesting the webpage {0}", currentWebPage);
            var html = await client.GetStringAsync(currentWebPage);

            //Parse Html: Only words from non-internal parts of the page must be processed (no inside of <script> etc).
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);
            htmlDocument.DocumentNode.Descendants().Where(n => n.Name == "script" || n.Name == "style" || n.Name == "noscript" || n.Name == "--").ToList().ForEach(n => n.Remove());

            //Get links
            var links = htmlDocument.DocumentNode.SelectNodes("//p//a[@href]").ToList();//.Attributes["href"].Value;
            string currentLink = "";
            bool flag = true;

            //Check link in webpages list
            lock (locking_webPages)
            {
                for (int i = 0; i < links.Count && flag; i++)
                {
                    currentLink = links[i].Attributes["href"].Value;

                    if (!webpages.Contains(currentLink))
                    {
                        webpages.Add("https://es.wikipedia.org" + currentLink);
                        flag = false;
                    }
                }
            }
             

            //Parallel tasks crawling webpages
            List<Task> tasks = new List<Task>
                {
                    Task.Run(() =>Crawl("https://es.wikipedia.org" + currentLink))
                };


            //Split words
            string myString = Regex.Replace(htmlDocument.DocumentNode.InnerText, @"\s+", " ").Replace("&nbsp;", "");
            String[] result = myString.Split();


            //Add words to wordCounter
            lock (locking_wordCounter)
            {
                foreach (var word in result)
                {
                    if (!wordCounter.ContainsKey(word))
                    {
                        wordCounter[word] = 1;
                    }
                    else
                    {
                        wordCounter[word] += 1;
                    }
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
    }



}
