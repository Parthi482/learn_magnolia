using System.Net;
using System.Net.Mail;
using System.Text;

namespace Magnolia_cares.utils
{
    public class Email
    {

        private static readonly HttpClient client = new HttpClient();

        public static async Task<string> SimpleEmailHandlerAsync(string toEmail, string subject, string body, string? fileUrl = null)
        {
            try
            { 
                IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();
                string username = configuration["EmailSettings:Username"];
                string password = configuration["EmailSettings:Password"];
                string successResponse = configuration["EmailSettings:successResponse"];
                string smtpServer = configuration["EmailSettings:SmtpServer"];
                int smtpServerPort = int.Parse(configuration["EmailSettings:SmtpServerPort"]);

                using (MailMessage mail = new MailMessage())
                { 
                    mail.From = new MailAddress(username,"Thrive Connect");
                    mail.To.Add(toEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    if (fileUrl != null)    
                    {
                        string fileName = System.IO.Path.GetFileName(fileUrl);
                        MemoryStream fileStream = await DownloadFileToMemoryStreamAsync(fileUrl);
                        Attachment attachment = new Attachment(fileStream, fileName);
                        mail.Attachments.Add(attachment);
                    }

                    SmtpClient smtp = new SmtpClient(smtpServer, smtpServerPort);
                    smtp.EnableSsl = true;
                    smtp.Timeout = 10000;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    mail.BodyEncoding = UTF8Encoding.UTF8;
                    mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                    smtp.Credentials = new NetworkCredential(username, password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                }
                return successResponse;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }



        private static async Task<MemoryStream> DownloadFileToMemoryStreamAsync(string fileUrl)
        {
            HttpResponseMessage response = await client.GetAsync(fileUrl);
            response.EnsureSuccessStatusCode();

            MemoryStream memoryStream = new MemoryStream();
            await response.Content.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }

    }
}