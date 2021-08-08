using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace gemelo.baederland.sniper.app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Baederland Sniper");

            while (true)
            {
                ScanSite("https://www.baederland-shop.de/schwimmschule/bronze-23242.html");
                ScanSite("https://www.baederland-shop.de/schwimmschule/bronze-23243.html");

                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
        private static void ScanSite(string url)
        {
            using (var webClient = new WebClient())
            {
                using (Stream response = webClient.OpenRead(url))
                {
                    using (StreamReader reader = new StreamReader(response))
                    {
                        string text = reader.ReadToEnd();

                        //         < li class="freie-plaetze">
                        //0														freie Plätze
                        //         </li>

                        string searchText = "<li class=\"freie-plaetze\">";
                        int posStart = text.IndexOf(searchText) + searchText.Length;
                        int posEnd = text.IndexOf("</li>", posStart);
                        string content = text.Substring(posStart, posEnd - posStart).Trim();

                        if (content[0] != '0')
                        {
                            SendEmail("Bäderland Platz gefunden!", url, content);
                        }
                    }
                    response.Close();
                }
            }
        }

        public static void SendEmail(string subject, string url, string content)
        {
            string to = "baederland-sniper@tr.gemelo.de";
            string from = "baederland-sniper@tr.gemelo.de";
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = $"{url}\n{content}";
            SmtpClient client = new SmtpClient("mail.pete.de");
            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            client.UseDefaultCredentials = true;

            try
            {
                client.Send(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
            }
        }
    }
}
