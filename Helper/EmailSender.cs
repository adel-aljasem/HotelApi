using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_Api.Helper
{
    public class EmailSender 
    {
        private readonly MailJetSettings mailJetSettings;

        public EmailSender(IOptions<MailJetSettings> mailJetSettings)
        {
            this.mailJetSettings = mailJetSettings.Value;
        }


        public async Task<MailjetResponse> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailjetClient client = new MailjetClient(mailJetSettings.PublicKey,mailJetSettings.PrivateKey );
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
               .Property(Send.FromEmail, mailJetSettings.Email)
               .Property(Send.FromName, "SpaceStudio")
               .Property(Send.Subject, subject)
               .Property(Send.TextPart, "Dear passenger, welcome to Mailjet! May the delivery force be with you!")
               .Property(Send.HtmlPart, htmlMessage)
               .Property(Send.Recipients, new JArray {
                new JObject {
                 {"Email", email}
                 }
                   });
            MailjetResponse response = await client.PostAsync(request);
            return response;
        }
    }
}
