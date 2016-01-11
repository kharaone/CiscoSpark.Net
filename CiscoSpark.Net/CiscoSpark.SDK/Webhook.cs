using System;

namespace CiscoSpark.SDK
{
    public class Webhook
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Resource { get; set; }

        public string Event { get; set; }

        public string Filter { get; set; }

        public Uri TargetUrl { get; set; }

        public DateTime Created { get; set; }
    }
}