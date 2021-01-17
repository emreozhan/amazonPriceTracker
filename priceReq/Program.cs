using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace priceReq
{
    class Program
    {
        static void Main(string[] args)
        { 
            TrackList trackDetails = JsonConvert.DeserializeObject<TrackList>(File.ReadAllText(@"..//..//..//trackList.Json"));

            while (true)
            {
                ColoredLogE("Turn Start", ConsoleColor.Blue);

                foreach (var trck in trackDetails.ProductList)
                {
                    Uri url = new Uri(trck.Url);
                    WebClient client = new WebClient();

                    string html = client.DownloadString(url); // html kodları indiriyoruz.

                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(html); // html kodlarını bir HtmlDocment nesnesine yüklüyoruz.
 
                    try
                    {
                        String productPriceWithCurrency = String.Empty;
                        try
                        {
                            productPriceWithCurrency = document.DocumentNode.SelectNodes("//*[@id=\"priceblock_ourprice\"]")[0].InnerHtml;  // a etiketlerinin içerisinden class haberbas olanları seçiyoruz.
                        }
                        catch
                        {
                            throw new Exception("Out of Stock: " + trck.Name);
                        }

                        try
                        {
                            if (!String.IsNullOrWhiteSpace(productPriceWithCurrency))
                            {
                                productPriceWithCurrency = productPriceWithCurrency.Remove(productPriceWithCurrency.IndexOf('.'), 1);
                                int purePrice = Convert.ToInt32(productPriceWithCurrency.Substring(1, productPriceWithCurrency.IndexOf(',') - 1));

                                if (purePrice <= trck.AlertPrice)
                                {
                                    ColoredLogE(purePrice + " * " + trck.Name, ConsoleColor.Yellow);

                                    if (trackDetails.BrowserOpenWhenAlert)
                                    {
                                        var psi = new ProcessStartInfo(trackDetails.BrowserExePath);
                                        psi.Arguments = trck.Url;
                                        Process.Start(psi);
                                    }
                                }
                                else
                                {
                                    ColoredLogE(purePrice + " * " +trck.Name , ConsoleColor.DarkMagenta);

                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("Price Cannot Calculated: " + trck.Name);

                        }

                    }
                    catch (Exception ex)
                    {
                         ColoredLogE(ex.Message, ConsoleColor.DarkRed);
                    }



                }
                Thread.Sleep(3000);
            }
 
        }

        public static void ColoredLogE(string msg, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            Console.WriteLine(msg.Substring(0, msg.Length > 70 ? 70 : msg.Length));
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }

 


    [Serializable]
    public class TrackList
    {
        public string MainAlertPrice { get; set; }
        public string BrowserExePath { get; set; }
        public bool BrowserOpenWhenAlert { get; set; }
        
        public List<Product> ProductList { get; set; }
    }


    [Serializable]
    public class Product
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Site { get; set; }
        public int AlertPrice { get; set; }

    }
}
