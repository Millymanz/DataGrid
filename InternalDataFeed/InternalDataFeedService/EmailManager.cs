using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Configuration;

namespace InternalDataFeedService
{
    public class EmailManager
    {
        private readonly int _port;
        private readonly string _host;
        private readonly string _toDefault;
        private readonly string _from;
        private readonly string _username;
        private readonly string _password;


        public EmailManager()
        {
            _host = ConfigurationManager.AppSettings["htHost"];
            Int32.TryParse(ConfigurationManager.AppSettings["htPort"], out _port);
            _username = ConfigurationManager.AppSettings["htUsername"];
            _password = ConfigurationManager.AppSettings["htPassword"];
            _toDefault = ConfigurationManager.AppSettings["htEmailTo"];
            _from = ConfigurationManager.AppSettings["htEmailFrom"];
        }

        public void Send(string[] recipientList, string emailSubject, string msg)
        {
            try
            {
                if (string.IsNullOrEmpty(msg)) { return; }

                var smtpClient = new SmtpClient(_host)
                    {
                        EnableSsl = false,
                        Timeout = 10000,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false
                    };

                if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(_password)) smtpClient.Credentials = new NetworkCredential(_username, _password);

                var mailMessage = new MailMessage {From = new MailAddress(_from), Subject = emailSubject, Body = msg};

                var addresses = recipientList;

                if (addresses == null || !addresses.Any())
                {
                    addresses = _toDefault.Split(';');
                }

                foreach (var item in addresses)
                {
                    mailMessage.To.Add(item);
                }

                mailMessage.IsBodyHtml = true; //remove

                mailMessage.BodyEncoding = Encoding.UTF8;
                mailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                smtpClient.Send(mailMessage);
            }
            catch (Exception ex)
            {
                var msga = "Send :: Email message sending error";
                Console.WriteLine("Email Report Exception: {0}", ex.StackTrace);
            }
        }

    }


}
