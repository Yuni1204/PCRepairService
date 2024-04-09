using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessengerLibrary
{
    public class Message
    {
        public int Id { get; set; }
        public string? exchange { get; set; }
        public string? messageType { get; set; }
        public string? content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
