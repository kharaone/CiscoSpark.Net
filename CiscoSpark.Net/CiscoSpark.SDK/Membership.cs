namespace CiscoSpark.SDK
{
    public class Membership
    {
        public string Id { get; set; }

        public string RoomId { get; set; }

        public string PersonId { get; set; }

        public string PersonEmail { get; set; }

        public bool IsModerator { get; set; }

        public bool IsMonitor { get; set; }
    }
}