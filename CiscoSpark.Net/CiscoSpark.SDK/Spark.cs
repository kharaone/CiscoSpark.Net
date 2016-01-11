using System;
using System.Linq;
using System.Threading.Tasks;
using CiscoSpark.SDK;
using NLog;

namespace CiscoSpark.SDK
{
    public abstract class Spark
    {
        public abstract IRequestBuilder<Room> Rooms();
        public abstract IRequestBuilder<Membership> Memberships();
        public abstract IRequestBuilder<Message> Messages();
        public abstract IRequestBuilder<Person> People();
        public abstract IRequestBuilder<Webhook> Webhooks();

        public class Builder
        {
            private Uri _redirectUri;
            private string _authCode;
            private string _accessToken;
            private string _refreshToken;
            private string _clientId;
            private string _clientSecret;
            private Logger _logger;
            private Uri _baseUrl = new Uri("https://api.ciscospark.com/v1");

            public Builder BaseUrl(Uri uri)
            {
                this._baseUrl = uri;
                return this;
            }

            public Builder RedirectUri(Uri uri)
            {
                this._redirectUri = uri;
                return this;
            }

            public Builder AuthCode(string code)
            {
                this._authCode = code;
                return this;
            }

            public Builder AccessToken(string accessToken)
            {
                this._accessToken = accessToken;
                return this;
            }

            public Builder RefreshToken(string refreshToken)
            {
                this._refreshToken = refreshToken;
                return this;
            }

            public Builder ClientId(string clientId)
            {
                this._clientId = clientId;
                return this;
            }

            public Builder ClientSecret(string clientSecret)
            {
                this._clientSecret = clientSecret;
                return this;
            }

            public Builder Logger(Logger logger)
            {
                this._logger = logger;
                return this;
            }

            public Spark Build()
            {
                return new SparkConcrete(new Client(_baseUrl, _authCode, _redirectUri, _accessToken, _refreshToken, _clientId, _clientSecret, _logger));
            }
        }

        public static Builder GetBuilder()
        {
            return new Builder();
        }
    }
}
