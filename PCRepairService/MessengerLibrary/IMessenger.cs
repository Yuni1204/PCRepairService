using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLibrary
{
    public interface IMessenger
    {
        Task SendOutboxMessageAsync(Message messageobj);
        void SendOutboxMessage(Message messageobj);
        void HandleMessages(string exchange = "ServiceOrderReply");
    }
}
