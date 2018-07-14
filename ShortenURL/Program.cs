using Google.Apis.Services;
using Google.Apis.Urlshortener.v1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShortenURL
{
    class Program
    {
        public static string shortenIt(string url)
        {
            UrlshortenerService service = new UrlshortenerService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyBrqWxYLlP4Kx1x__ZngFoEyHC4m-vZg5c",
                ApplicationName = "Tung Xuan",
            });

            var m = new Google.Apis.Urlshortener.v1.Data.Url();
            m.LongUrl = url;
            return service.Url.Insert(m).Execute().Id;
        }
        [STAThread]
        static void Main(string[] args)
        {
            if (Clipboard.ContainsText(TextDataFormat.Text))
            {
                string originalURL = Clipboard.GetText(TextDataFormat.Text);
                string shortURL = shortenIt(originalURL);
                Clipboard.SetText(shortURL, TextDataFormat.Text);
            }
        }
    }
}
