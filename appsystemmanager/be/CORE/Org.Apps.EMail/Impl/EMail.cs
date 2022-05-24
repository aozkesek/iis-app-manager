using Org.Apps.Aspect;
using Org.Apps.EMail.Intf;
using System;
using System.Net;
using System.Net.Mail;

namespace Org.Apps.EMail.Impl
{
    public class EMailLogic : IEMailLogic
    {
        private string mEmailFrom;
        private string mMessageTemplate;
        private string mEmailUser;
        private string mEmailPassword;
        private string mEmailHost;
        private int mEmailPort;

        public string EmailFrom { get { return mEmailFrom; } set { mEmailFrom = value; } }

        public string MessageTemplate { get { return mMessageTemplate; } set { mMessageTemplate = value; } }

        public string EmailUser { get { return mEmailUser; } set { mEmailUser = value; } }

        public string EmailPassword { get { return mEmailPassword; } set { mEmailPassword = value; } }

        public string EmailHost { get { return mEmailHost; } set { mEmailHost = value; } }

        public int EmailPort { get { return mEmailPort; } set { mEmailPort = value; } }

        [PerformanceLoggingAdvice]
        public void SendNewPasswordNotification(String receiver, String messagePart)
        {
            SendMessage(
                PrepareMessage(
                    receiver,
                    "IP Telefon Servisleri kullanımız için hazırdır.",
                    messagePart));
        }

        #region helpers

        private MailMessage PrepareMessage(string receiver, string subject, string messagePart)
        {
            MailMessage message = new MailMessage(EmailFrom, receiver);
            message.Subject = subject;
            message.Body = string.Format(MessageTemplate, messagePart);
            return message;
        }

        private void SendMessage(MailMessage mailMessage)
        {
            using (SmtpClient googleClient = new SmtpClient(EmailHost, EmailPort))
            {
                googleClient.Credentials = new NetworkCredential(EmailUser, EmailPassword);
                googleClient.EnableSsl = true;
                googleClient.Send(mailMessage);
            }
        }

        #endregion helpers
    }
}