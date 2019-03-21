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
            Crawler webCrawler = new Crawler("https://es.wikipedia.org/wiki/", 20);
            await webCrawler.Start();
            Console.ReadKey();
        }
    }

    //Summary: Class Crawler models
    class Crawler
    {
        private string seed;
        private int webPagesAmountToCrawl = 0;
        private readonly object locking_webPagesVisited = new object();
        private int webPagesVisited = 0;
        private readonly object locking_webPages = new object();
        private List<string> webpages;
        private readonly object locking_wordCounter = new object();
        private Dictionary<string, int> wordCounter;

        public Crawler(string webpage, int amountToCrawl) {
            seed = webpage;
            webpages = new List<string> { webpage };
            webPagesAmountToCrawl = amountToCrawl;
            wordCounter = new Dictionary<string, int>();
        }

        public async Task Start()
        {
            await Crawl(webpages[0]);
            try
            {
                Console.WriteLine();
                Console.WriteLine("En total se visitaron: {0} pagina(s)", webPagesVisited);
                Console.WriteLine();

                //Distict keywords by key and count, and then order by count.
                wordCounter = wordCounter.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                //Save wordcounter in a file
                using (System.IO.StreamWriter file =
                 new System.IO.StreamWriter(@"WebCrawler.txt"))
                {
                    file.WriteLine("Word  \t\t\t   | Occurrences");
                    foreach (var line in wordCounter)
                    {
                        file.WriteLine(line.Key + " \t\t\t\t " + wordCounter[line.Key]);
                    }
                }

                //print Top 50 keyword to console.
                Console.WriteLine("================================================================================");
                Console.WriteLine("|| Top 50 words. If you want to see the full wordcounter, open WebCrawler.txt ||");
                Console.WriteLine("================================================================================");

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("================================================================================");
                Console.WriteLine("|| Word  \t\t\t   | No. of Occurrences  \t\t\t||");
                Console.WriteLine("================================================================================");

                foreach (var aux in wordCounter.Take(50))
                {
                    Console.WriteLine("{0}  \t\t\t\t    {1}", aux.Key, wordCounter[aux.Key]);
                }

               
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

        //Summary: crawl method into currentWebPage
        public async Task Crawl(string currentWebPage) {

            //when webPages max amount is reached, crawl ends
            if (webPagesVisited == webPagesAmountToCrawl)
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
            var links = htmlDocument.DocumentNode.SelectNodes("//p//a[@href]").ToList();
            string currentLink = "";
            bool flag = true;

            //Check link in webpages list
            lock (locking_webPages)
            {
                for (int i = 0; i < links.Count && flag; i++)
                {
                    currentLink = links[i].Attributes["href"].Value;

                    if (!webpages.Contains(seed + currentLink))
                    {
                        webpages.Add(seed + currentLink);
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
