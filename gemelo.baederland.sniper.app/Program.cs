using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using Gemelo.Components.Common.Text;

namespace gemelo.baederland.sniper.app
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Baederland Sniper");

            SendEmail("Baederland Sniper", "", "Start");

            while (true)
            {
                ScanSite("https://www.baederland-shop.de/schwimmschule/bronze-23242.html");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                ScanSite("https://www.baederland-shop.de/schwimmschule/bronze-23243.html");

                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
        private static void ScanSite(string url)
        {
            Console.WriteLine("{0}: Anfrage bei: {1}",DateTime.Now, url);
            try
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
                            int length = Math.Min(1000, posEnd - posStart);
                            string content = text.SecureSubstring(posStart, length).Trim();
                            string numberString = content.SecureSubstring(startIndex: 0, length: 5).Trim();
                            if (numberString[0] != '0')
                            {
                                Console.WriteLine("Platz gefunden: {0}, {1}", content, url);
                                SendEmail("Bäderland Platz gefunden!", url, content);
                            }
                        }
                        response.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine("Fehler: {0}", exp.Message);
            }
        }

        public static void SendEmail(string subject, string url, string content)
        {
            string to = "baederland-sniper@tr.gemelo.de";
            string from = "baederland-sniper@tr.gemelo.de";
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = $"{url}\n{content}";
            SmtpClient client = new SmtpClient("PETE-MAIL2019.pete.local");
            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            client.UseDefaultCredentials = true;
            client.EnableSsl = true;

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
