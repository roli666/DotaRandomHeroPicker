using AngleSharp;
using AngleSharp.Html.Dom;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotaRandomHeroPicker
{
    class Program
    {
        static Program()
        {
            Console.SetWindowSize(50, 3);
            Console.SetBufferSize(50, 3);
            Console.Title = "Dota Random Hero Picker";
        }
        static void Main()
        {
            var context = BrowsingContext.New(Configuration.Default);
            string urlAddress = "https://dota2.gamepedia.com/Heroes";
            var rand = new Random();
            string picked_hero = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);

            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;
            var loading = Task.Run(() =>
            {
                int counter = 0;
                Console.Write("Loading:");
                int cursorAfterLoading = Console.CursorLeft;
                while (!cancelToken.IsCancellationRequested)
                {
                    counter++;
                    switch (counter % 4)
                    {
                        case 0: Console.Write("/"); break;
                        case 1: Console.Write("-"); break;
                        case 2: Console.Write("\\"); break;
                        case 3: Console.Write("|"); break;
                    }
                    Thread.Sleep(100);
                    Console.SetCursorPosition(cursorAfterLoading, Console.CursorTop);
                }
                Console.Clear();
                Console.SetCursorPosition(0, 0);
            }, cancelToken);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (string.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string data = readStream.ReadToEnd();

                response.Close();
                readStream.Close();

                var document = context.OpenAsync(req => req.Content(data));
                Task.WaitAll(document);
                cancelTokenSource.Cancel();
                var res = document.Result;
                var heroes = res.Images.Where(img => img.GetAttribute("width") == "150" && img.GetAttribute("height") == "84").Select(img => img.ParentElement as IHtmlAnchorElement).Select(elem => elem.Title);
                loading.Wait();
                if (!heroes.Any())
                {
                    Console.WriteLine($"Heroes could not be retrieved from the server.");
                    Thread.Sleep(5);
                    return;
                }
                int randomHeroIndex = rand.Next(0, heroes.Count());
                picked_hero = heroes.ElementAt(randomHeroIndex);
                Console.WriteLine($"Your random hero is: {picked_hero}");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine($"Something went wrong. Check your internet connection.");
            }
        }
    }
}
