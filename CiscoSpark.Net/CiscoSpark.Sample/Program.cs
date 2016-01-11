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
                .iterate()
                .ToList().ForEach(r => {
                                           Console.WriteLine("{0}, created {1}: {2}", r.Title, r.Created, r.Id);
                });

            // Create a new room
            Room room = new Room();
            room.Title = "Hello World";
            room = spark.Rooms().Post(room);


            // Add a coworker to the room
            Membership membership = new Membership();
            membership.RoomId = room.Id;
            membership.PersonEmail = "wile_e_coyote@acme.com";
            spark.Memberships().Post(membership);


            // List the members of the room
            spark.Memberships()
                .QueryParam("roomId", room.Id)
                .iterate().ToList()
                .ForEach(member => {
                                       Console.WriteLine(member.PersonEmail);
                });


            // Post a text message to the room
            Message message = new Message();
            message.RoomId = room.Id;
            message.Text = "Hello World!";
            spark.Messages().Post(message);


            // Share a file with the room
            message = new Message();
            message.RoomId = room.Id;
            message.Files = new[] {new Uri("http://example.com/hello_world.jpg")};
            spark.Messages().Post(message);
        }

    }
}
