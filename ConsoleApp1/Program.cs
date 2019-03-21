/*
 * Developed by: https://github.com/darkcolossus
 * Date: 20/03/2019
*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebCrawler
{
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
        private int webPagesVisited = 0;
        private List<string> webpages;
        private Dictionary<string, int> wordCounter;

        public Crawler(string webpage) {
            webpages = new List<string> { webpage };
            wordCounter = new Dictionary<string, int>();
        }

        public async Task Start() {

        }
    }



}
