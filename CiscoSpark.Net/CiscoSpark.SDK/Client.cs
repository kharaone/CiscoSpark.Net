using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;

namespace CiscoSpark.SDK
{
    public class Client
    {
        private readonly string _accessToken;
        private readonly Uri _baseUri;

        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly Logger _logger;
        private readonly Uri _redirectUri;


        private readonly string _trackingId = "TrackingID";
        public readonly string Iso8601Format = "yyyy-MM-dd'T'HH:mm:ss.SSS'Z'";
        private string _authCode;
        private string _refreshToken;

        public Client(Uri baseUri, string authCode, Uri redirectUri, string accessToken, string refreshToken,
            string clientId, string clientSecret, Logger logger)
        {
            _baseUri = baseUri;
            _authCode = authCode;
            _redirectUri = redirectUri;
            _accessToken = accessToken;
            _refreshToken = refreshToken;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _logger = logger;
        }

        public T Post<T>(string path, T body)
        {
            return JsonConvert.DeserializeObject<T>(Request("POST", path, null, body));
        }

        public T Post<T>(Uri url, T body)
        {
            return JsonConvert.DeserializeObject<T>(Request(url, "POST", body));
        }

        public T Put<T>(string path, T body)
        {
            return JsonConvert.DeserializeObject<T>(Request("PUT", path, null, body));
        }

        public T Put<T>(Uri url, T body)
        {
            return JsonConvert.DeserializeObject<T>(Request(url, "PUT", body));
        }

        public T Get<T>(string path, List<string[]> parameters) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(Request("GET", path, parameters, new T()));
        }

        public T Get<T>(Uri url) where T : new()
        {
            return JsonConvert.DeserializeObject<T>(Request(url, "GET", new T()));
        }

        public class DataJsonAttributeContainer<T>
        {
            public List<T> Items { get; set; }
        }

        public T DeserializeFromJson<T>(string json)
        {
            T deserializedProduct = JsonConvert.DeserializeObject<T>(json);
            return deserializedProduct;
        }

        public IEnumerable<T> List<T>(string path, List<string[]> parameters) where T : new()
        {
            return List<T>(GetUri(path, parameters));
        }

        public IEnumerable<T> List<T>(Uri url) where T : new()
        {
            try
            {
                var container = DeserializeFromJson<DataJsonAttributeContainer<T>>(Request(url, "GET", new T()));
                return container.Items;
            }
            catch (IOException ex)
            {
                throw new SparkException(ex);
            }

        }

        public void Delete(string path)
        {
            Delete(GetUri(path, null));
        }

        private Uri GetUri(string path, List<string[]> parameters)
        {
            var urlStringBuilder = new StringBuilder(string.Concat(_baseUri, path));
            if (parameters != null)
            {
                urlStringBuilder.Append("?");
                foreach (var parameter in parameters)
                {
                    urlStringBuilder
                        .Append(Encode(parameter[0]))
                        .Append("=")
                        .Append(Encode(parameter[1]))
                        .Append("&");
                }
            }

            Uri url;
            try
            {
                url = new Uri(urlStringBuilder.ToString());
            }
            catch (MalformedUrlException e)
            {
                throw new SparkException("bad url: " + urlStringBuilder, e);
            }
            return url;
        }

        private string Encode(string value)
        {
            try
            {
                return HttpUtility.UrlEncode(value, Encoding.UTF8);
            }
            catch (UnsupportedEncodingException ex)
            {
                throw new SparkException(ex);
            }
        }


        public void Delete(Uri url)
        {
            try
            {
                HttpClient connection = GetConnection(url);
                var response = connection.DeleteAsync("DELETE").Result;
                if(!response.IsSuccessStatusCode) CheckForErrorResponse(response);
            }
            catch (IOException ex)
            {
                throw new SparkException(ex);
            }
        }

        private void CheckForErrorResponse(HttpResponseMessage response)
        {
            var statusCode = (int) response.StatusCode;
            if (statusCode == 401)
            {
                throw new NotAuthenticatedException();
            }

            if (statusCode < 200 || statusCode >= 400)
            {
                var errorMessageBuilder = new StringBuilder("bad response code ");
                errorMessageBuilder.Append(statusCode);
                try
                {
                    string responseMessage = response.ReasonPhrase;
                    if (responseMessage != null)
                    {
                        errorMessageBuilder.Append(" ");
                        errorMessageBuilder.Append(responseMessage);
                    }
                }
                catch (IOException)
                {
                    // ignore
                }

                if (_logger != null)
                {
                    _logger.Error("Response Body {0}: {1}", new[] { _trackingId, response.Content.ReadAsStringAsync().Result });
                }

                throw new SparkException(errorMessageBuilder.ToString());
            }
        }

        private HttpClient GetConnection(Uri url)
        {

            var client = new HttpClient(new HttpClientHandler());
            //client.DefaultRequestHeaders.Add("Content-type", "application/json");
            if (_accessToken != null) {
                string authorization = _accessToken;
                if (!authorization.StartsWith("Bearer "))
                {
                    authorization = string.Format("Bearer {0}",_accessToken);
                }
                client.DefaultRequestHeaders.Add("Authorization", authorization);
            }
            client.DefaultRequestHeaders.Add(_trackingId, Guid.NewGuid().ToString());

            return client;
        }


        private string Request<T>(string method, string path, List<string[]> parameters, T body)
        {
            var url = GetUri(path, parameters);
            return Request(url, method, body);
        }

        public string Request<T>(Uri uri, string method, T body)
        {
            if (_accessToken == null)
            {
                if (!Authenticate())
                {
                    throw new NotAuthenticatedException();
                }
            }

            try
            {
                return DoRequest(uri, method, body);
            }
            catch (NotAuthenticatedException ex)
            {
                if (Authenticate())
                {
                    return DoRequest(uri, method, body);
                }
                throw ex;
            }
        }

        private string DoRequest(Uri url, string method, object body)
        {
            try
            {
                HttpClient connection = GetConnection(url);
                HttpResponseMessage response;
                var trackingId = connection.DefaultRequestHeaders.GetValues(_trackingId).Single();

                if (_logger != null)
                {
                    _logger.Info("Request {0}: {1} {2}", trackingId, method, connection.BaseAddress);
                }
                var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var bodyJSON = JsonConvert.SerializeObject(body, settings);
                if (_logger != null)
                {
                    _logger.Info("Request Body {0}: {1}", trackingId, bodyJSON);

                }
                response = Query(url, bodyJSON, connection, method);


                if (response.IsSuccessStatusCode)
                {
                    int responseCode = (int)response.StatusCode;
                    if (_logger != null)
                    {
                        _logger.Info("Response {0}: {1} {2}", trackingId, responseCode,
                            response.Content.ReadAsStringAsync().Result);
                    }

                }
                return response.Content.ReadAsStringAsync().Result;

            }
            catch (IOException io)
            {
                throw new SparkException("io error", io);
            }
            catch(Exception ex)
            {
                throw new SparkException("An error occurred during the request", ex);
            }
        }

        private HttpResponseMessage Query(Uri url, string bodyJson, HttpClient connection, string method)
        {
            switch (method)
            {
                case "GET":
                    return connection.GetAsync(url).Result;
                case "POST":
                    return connection.PostAsync(url, new StringContent(bodyJson, Encoding.UTF8, "application/json")).Result;
                case "PUT":
                    return connection.PostAsync(url, new StringContent(bodyJson, Encoding.UTF8, "application/json")).Result;
            }
            return null;
        }

        private bool Authenticate()
        {
            throw new NotImplementedException();
        }


    }
}