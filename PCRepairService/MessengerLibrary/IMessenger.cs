using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLibrary
{
    public interface IMessenger
    {
        Task SendMessageAsync(Message messageobj);
        void SendMessage(Message messageobj);
        void HandleMessages(string exchange = "_");
    }
}
