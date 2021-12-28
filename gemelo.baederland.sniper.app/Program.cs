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

            SendEmail("Hamburg Impft Sniper wurde gestartet", "https://www.hamburg-impft.de/", "[Start]");
            //SendEmail("Baederland Sniper wurde gestartet", "http://www.gemelo.de", "[Start]");

            while (true)
            {
                ScanSiteForImpfTermine("https://www.hamburg-impft.de/");
                //ScanSiteForFreePlace("https://www.baederland-shop.de/schwimmschule/bronze-23323.html");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                //ScanSiteForFreePlace("https://www.baederland-shop.de/schwimmschule/bronze-23324.html");

                //ScanSiteForExistingCourse("https://www.baederland.de/kurse/kursfinder/?course%5Blocation%5D=&course%5Blatlng%5D=&course%5Bpool%5D%5B%5D=15&course%5Bcategory%5D%5B%5D=85&course%5Bdate%5D=");

                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }
        private static void ScanSiteForFreePlace(string url)
        {
            Console.WriteLine("{0}:ScanSiteForFreePlace Anfrage bei: {1}", DateTime.Now, url);
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
                            if (numberString[0] != '0' && numberString != "-1")
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

        private static void ScanSiteForExistingCourse(string url)
        {
            Console.WriteLine("{0}: ScanSiteForExistingCourse bei: {1}", DateTime.Now, url);
            try
            {
                using (var webClient = new WebClient())
                {
                    using (Stream response = webClient.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(response))
                        {
                            string text = reader.ReadToEnd();
                            string searchText = "Zu Ihrer Suche gibt es leider keine passenden Kurse";
                            bool noCourse = text.Contains(searchText);
                            if (!noCourse)
                            {
                                Console.WriteLine("Kurs gefunden: {0}", url);
                                SendEmail("Bäderland Kurs gefunden!", url, "");
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

        private static void ScanSiteForImpfTermine(string url)
        {
            Console.WriteLine("{0}: ScanSiteForExistingCourse bei: {1}", DateTime.Now, url);
            try
            {
                using (var webClient = new WebClient())
                {
                    using (Stream response = webClient.OpenRead(url))
                    {
                        using (StreamReader reader = new StreamReader(response))
                        {
                            string text = reader.ReadToEnd();
                            string searchText = "Keine Impfstelle mit freien Terminen gefunden";
                            bool noCourse = text.Contains(searchText);
                            if (!noCourse)
                            {
                                Console.WriteLine("Impfestelle gefunden, aber gibt es auch einen Termin?: {0}", url);
                                string searchTextTerminStart = "<select name=\"pat[0][DatumUhrzeit]\" class=\"form-control select-alle-termine select-termin auswahl-termin\">";
                                int indexOfTerminStart = text.IndexOf(searchTextTerminStart);
                                if (indexOfTerminStart > 0)
                                {
                                    indexOfTerminStart += searchTextTerminStart.Length;
                                    string searchTextTerminEnd = "</select>";
                                    int indexOfTerminEnd = text.IndexOf(searchTextTerminEnd, indexOfTerminStart);

                                    if ((indexOfTerminEnd - indexOfTerminStart) > 5)
                                    {
                                        string terminContent = text.SecureSubstring(indexOfTerminStart, indexOfTerminEnd - indexOfTerminStart);
                                        SendEmail("Impftermin vorhanden!", url, terminContent, websiteContent: text);
                                    }
                                }
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


        public static void SendEmail(string subject, string url, string content, string websiteContent = "")
        {
            //string to = "reinhold@gemelo.de";
            //string to = "thies@ampelsprinter.de";

            string to = "hamburg.impft.tr@gemelo.de";
            //string to = "baederland-sniper@tr.gemelo.de";
            string from = "reinhold@gemelo.de";
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body =
                $"Lieber Hamburg Impft Sniper Empfänger.\n\n" +
                $"wir haben folgendes neues Ereignis für dich festgestellt:\n\n" +
                $"{subject}\n\n" +
                $"mit folgendem Inhalt: {content}\n\n" +
                $"auf der URL: {url}\n\n\n" +
                $"Herzliche Grüße\n\n" +
                $"dein automatischer HamburgImpftFinder\n\n" +
                $"{websiteContent}";
            //$"dein automatischer Bäderlandkursefinder";

            SmtpClient client = new SmtpClient("PETE-MAIL2019.pete.local");
            // Credentials are necessary if the server requires the client
            // to authenticate before it will send email on the client's behalf.
            //client.UseDefaultCredentials = true;

            //client.UseDefaultCredentials = false;
            //client.Credentials = new NetworkCredential(userName: "Thies", password: "Dicker1$1$1",domain: "pete.local");

            client.Credentials = CredentialCache.DefaultNetworkCredentials;

            client.EnableSsl = false;

            try
            {
                client.Send(message);
                Console.WriteLine("Email gesendet");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
                    ex.ToString());
            }
        }
    }
}
