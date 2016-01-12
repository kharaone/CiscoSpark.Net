namespace CiscoSpark.SDK
{
    public class SparkConcrete : Spark
    {
        private Client _client;
        public SparkConcrete(Client client)
        {
            this._client = client;
        }

        public override IRequestBuilder<Room> Rooms()
        {
            return new RequestBuilder<Room>(_client, "/rooms");
        }

        public override IRequestBuilder<Membership> Memberships()
        {
            return new RequestBuilder<Membership>(_client, "/memberships");
        }

        public override IRequestBuilder<Message> Messages()
        {
            return new RequestBuilder<Message>(_client, "/messages");
        }

        public override IRequestBuilder<Person> People()
        {
            return new RequestBuilder<Person>(_client, "/people");
        }

        public override IRequestBuilder<Webhook> Webhooks()
        {
            return new RequestBuilder<Webhook>(_client, "/webhooks");
        }
    }
}