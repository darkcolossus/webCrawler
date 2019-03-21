/*
 * Developed by: https://github.com/darkcolossus
 * Date: 20/03/2019
*/

using System;
using System.Collections.Generic;
using System.Net.Http;
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
        object locking_webPagesVisited = new object();
        private int webPagesVisited = 0;
        private List<string> webpages;
        private Dictionary<string, int> wordCounter;

        public Crawler(string webpage) {
            webpages = new List<string> { webpage };
            wordCounter = new Dictionary<string, int>();
        }

        public async Task Start()
        {
            await crawl(webpages[0]);
        }

        //Summary: crawl method into currentWebPage
        public async Task crawl(string currentWebPage) {

            //webPagesVisited needs to be blocked beacuse it is a shared resource
            lock (locking_webPagesVisited)
            {
                webPagesVisited++;
            }

            //Request WebPage
            HttpClient client = new HttpClient();
            Console.WriteLine("Requesting the webpage {0}", currentWebPage);


        }
    }



}
