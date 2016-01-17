using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CiscoSpark.SDK;


namespace CiscoSpark.Net.Controllers
{
    public class WebhookController : ApiController
    {
        // POST api/values
        public WebhookRequest Post(WebhookRequest webhook)
        {
            HttpContent requestContent = Request.Content;
            string jsonContent = requestContent.ReadAsStringAsync().Result;

            String accessToken = "MzRhZWIwYzAtYzQ3YS00ZmYzLTk0YWYtZTQ4NzU4MDRlZDVlNDZiODg5ZmYtZDI";

            // Initialize the client
            Spark spark = Spark.GetBuilder()
                .BaseUrl(new Uri("https://api.ciscospark.com/v1"))
                .AccessToken(accessToken)
                .Build();
            
            var messageIncoming = webhook.Data;
            if(!messageIncoming.PersonId.StartsWith("mmbot"))  return webhook;

            var messageId = messageIncoming.Id;
            var message = spark.Messages().Path("/" + messageId).Get();

            // Post a text message to the room
            //Message message = new Message();
            //message.RoomId = room.Id;
            message.RoomId = "Y2lzY29zcGFyazovL3VzL1JPT00vNzk5NTY3MTAtYjllMi0xMWU1LTk2ZjEtNDlhZDgxNDk5NTQ0";
            message.Text = string.Format("Vous avez écrit : {0}", message.Text);
            spark.Messages().Post(message);

            return webhook;
        }

    }

    public class WebhookRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Resource { get; set; }
        public string Event { get; set; }
        public string Filter { get; set; }
        public WebhookRequestData Data { get; set; }
    }

    public class WebhookRequestData
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string PersonId { get; set; }
        public string PersonEmail { get; set; }

        public DateTime Created { get; set; }
    }

    public class WebhookResponseData
    {
        public string Id { get; set; }
        public string RoomId { get; set; }
        public string PersonId { get; set; }
        public string PersonEmail { get; set; }

        public DateTime Created { get; set; }
        public string Text { get; set; }
    }
}
