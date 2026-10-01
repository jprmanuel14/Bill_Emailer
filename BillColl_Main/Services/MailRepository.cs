using BillColl_Main.Class;
using BillColl_Main.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class MailRepository : IMailService
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _environment;

        public MailRepository(IOptions<MailSettings> mailSettings, IWebHostEnvironment environment)
        {
            _mailSettings = mailSettings.Value;
            _environment = environment;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {



            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_mailSettings.Mail);

            if (mailRequest.ToEmail != "")
            {
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            }
            else
            {
                var list = InternetAddressList.Parse(mailRequest.ToEmails);

                email.To.AddRange(list);
            }

            if (mailRequest.ToCCEmail != "")
            {
                email.Cc.Add(MailboxAddress.Parse(mailRequest.ToCCEmail));
            }
            else if ((mailRequest.ToCCEmails != ""))
            {
                var list = InternetAddressList.Parse(mailRequest.ToCCEmails);

                email.Cc.AddRange(list);
            }
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();

            builder.HtmlBody = mailRequest.Body;
            if (mailRequest.WithImage && !string.IsNullOrEmpty(_environment.WebRootPath))
            {
                var imgFolder = Path.Combine(_environment.WebRootPath, "img");
                builder.Attachments.Add(Path.Combine(imgFolder, "BillingAndCollection.png"));
                builder.Attachments.Add(Path.Combine(imgFolder, "graph-pwc.png"));
                builder.Attachments.Add(Path.Combine(imgFolder, "meeting-pwc.png"));
                builder.Attachments.Add(Path.Combine(imgFolder, "message-pwc.png"));

                builder.Attachments[0].ContentId = "BillingAndCollection.png";
                builder.Attachments[1].ContentId = "graph-pwc.png";
                builder.Attachments[2].ContentId = "meeting-pwc.png";
                builder.Attachments[3].ContentId = "message-pwc.png";
            }
            email.Body = builder.ToMessageBody();
            //email.Headers["Disposition-Notification-To"] = "sanchojamesinocencio@yahoo.com";
            using var smtp = new SmtpClient();

            smtp.Connect(_mailSettings.Host, _mailSettings.Port);
            //smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            //smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);            
            await smtp.SendAsync(email);
            smtp.Disconnect(true);


        }
    }
}
