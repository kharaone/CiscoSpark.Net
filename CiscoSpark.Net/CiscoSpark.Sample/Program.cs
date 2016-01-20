using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoSpark.SDK;

namespace CiscoSpark.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            // To obtain a developer access token, visit http://developer.ciscospark.com
            String accessToken = "";

            // Initialize the client
            Spark spark = Spark.GetBuilder()
                .BaseUrl(new Uri("https://api.ciscospark.com/v1"))
                .AccessToken(accessToken)
                .Build();

            // List the rooms that I'm in
            spark.Rooms()
                .Iterate()
                .ToList().ForEach(r => {
                                           Console.WriteLine("{0}, created {1}: {2}", r.Title, r.Created, r.Id);
                });

            // Create a new room
            Room room = new Room {Title = "Hello World"};
            room = spark.Rooms().Post(room);


            // Add a coworker to the room
            Membership membership = new Membership
            {
                RoomId = room.Id,
                PersonEmail = "gfontaine@blizzard.com"
            };
            spark.Memberships().Post(membership);


            // List the members of the room
            spark.Memberships()
                .QueryParam("roomId", room.Id)
                .Iterate().ToList()
                .ForEach(member => {
                                       Console.WriteLine(member.PersonEmail);
                });


            // Post a text message to the room
            Message message = new Message
            {
                RoomId = room.Id,
                Text = "Hello World!"
            };
            spark.Messages().Post(message);


            // Share a file with the room
            message = new Message
            {
                RoomId = room.Id,
                Files = new[] {new Uri("http://www.webex.co.uk/ciscospark/includes/images/media_2.png")}
            };
            spark.Messages().Post(message);
        }

    }
}
