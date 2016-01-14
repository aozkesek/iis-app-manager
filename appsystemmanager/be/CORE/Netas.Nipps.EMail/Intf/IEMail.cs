using System;

namespace Netas.Nipps.EMail.Intf
{
    public interface IEMailLogic
    {
        void SendNewPasswordNotification(String receiver, String messagePart);

        string EmailFrom { get; set; }

        string MessageTemplate { get; set; }

        string EmailUser { get; set; }

        string EmailPassword { get; set; }

        string EmailHost { get; set; }

        int EmailPort { get; set; }
    }
}