using System;
using System.Collections.Generic;
using System.Text;

namespace CiscoSpark.SDK
{
    public class RequestBuilder<T>:IRequestBuilder<T> where T : new()
    {
        private Client _client;
        private StringBuilder _pathBuilder;
        private List<string[]> _parameters;
        private Uri _url;

        public RequestBuilder(Client client, string path): this(client, new StringBuilder(path), new List<string[]>())
        {}

        private RequestBuilder(Client client, StringBuilder pathBuilder, List<String[]> parameters)
        {
            this._client = client;
            this._pathBuilder = pathBuilder;
            this._parameters = parameters;
        }

        public IRequestBuilder<T> QueryParam(string key, string value)
        {
            _parameters.Add(new[] { key, value });
            return this;
        }

        public IRequestBuilder<T> Path(params object[] paths)
        {
            foreach (var path in paths)
            {
                _pathBuilder.Append(path);
            }
            return this;
        }

        public IRequestBuilder<TNewType> Path<TNewType>(string path, TNewType clazz) where TNewType : new()
        {
            _pathBuilder.Append(path);
            return new RequestBuilder<TNewType>(_client, _pathBuilder, _parameters);
        }

        public IRequestBuilder<T> Url(Uri url)
        {
            _url = url;
            return this;

        }

        public T Post(T body)
        {
            return _url == null ? _client.Post(_pathBuilder.ToString(), body) : _client.Post(_url, body);
        }

        public T Put(T body)
        {
            return _url == null ? _client.Put(_pathBuilder.ToString(), body) : _client.Put(_url, body);
        }

        public T Get()
        {
            return _url == null ? _client.Get<T>(_pathBuilder.ToString(), _parameters) : _client.Get<T>(_url);
        }

        public void Delete()
        {
            if (_url != null)
            {
                _client.Delete(_url);
            }
            else {
                _client.Delete(_pathBuilder.ToString());
            }
        }

        public IEnumerable<T> iterate()
        {
            return _url == null ? _client.List<T>(_pathBuilder.ToString(), _parameters) : _client.List<T>(_url);
        }
    }
}