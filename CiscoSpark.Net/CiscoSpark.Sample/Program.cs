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
            String accessToken = "<<secret>>";

            // Initialize the client
            Spark spark = Spark.GetBuilder()
                    .BaseUrl(new Uri("https://api.ciscospark.com/v1"))
                    .AccessToken(accessToken)
                    .Build();

            // List the rooms that I'm in
            spark.Rooms()
                    .iterate()
                    .forEachRemaining(room-> {
                System.out.println(room.getTitle() + ", created " + room.getCreated() + ": " + room.getId());
            });

            // Create a new room
            Room room = new Room();
            room.setTitle("Hello World");
            room = spark.Rooms().Post(room);


            // Add a coworker to the room
            Membership membership = new Membership();
            membership.setRoomId(room.getId());
            membership.setPersonEmail("wile_e_coyote@acme.com");
            spark.Memberships().post(membership);


            // List the members of the room
            spark.Memberships()
                    .queryParam("roomId", room.getId())
                    .iterate()
                    .forEachRemaining(member-> {
                System.out.println(member.getPersonEmail());
            });


            // Post a text message to the room
            Message message = new Message();
            message.setRoomId(room.getId());
            message.setText("Hello World!");
            spark.Messages().post(message);


            // Share a file with the room
            message = new Message();
            message.setRoomId(room.getId());
            message.setFiles(URI.create("http://example.com/hello_world.jpg"));
            spark.Messages().post(message);
        }

    }
}
