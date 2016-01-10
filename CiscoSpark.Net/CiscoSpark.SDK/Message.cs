using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoSpark.SDK
{
    public class Message
    {
        public string Id { get; set; }

        public string RoomId { get; set; }

        public string PersonId { get; set; }

        public string PersonEmail { get; set; }

        public string Text { get; set; }

        public string File { get; set; }

        public Uri[] Files { get; set; }
    }
}
