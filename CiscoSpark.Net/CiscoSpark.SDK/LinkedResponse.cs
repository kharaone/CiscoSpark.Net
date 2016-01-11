using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CiscoSpark.SDK
{
    public class LinkedResponse<T> where T : new()
    {
        private readonly  Client _client;
        private Client.Response _response;
        private readonly IBodyCreator<T> _bodyCreator;
        private readonly Dictionary<string, Uri> _urls = new Dictionary<string, Uri>();

        private static readonly Regex LinkPattern = new Regex(@"\s*<(\S+)>\s*;\s*rel=""(\S+)"",?");

        public LinkedResponse(Client client, Uri uri, IBodyCreator<T> bodyCreator)
        {
            _client = client;
            _bodyCreator = bodyCreator;
            FollowUrl(uri);
        }

        private void FollowUrl(Uri uri)
        {
            try
            {
                _response = _client.Request(uri, "GET", new T());

                int responseCode = (int) _response.StatusCode;
                if (!IsOk(responseCode))
                {
                    throw new IOException("bad response code: " + responseCode);
                }
                ParseLinks(_response);
            }
            catch (IOException e)
            {
                throw new SparkException(e);
            }
        }

        private bool IsOk(int responseCode)
        {
            return (responseCode >= 200 && responseCode < 400);
        }

        private void ParseLinks(Client.Response response)
        {
            _urls.Clear();
            var link = response.GetHeaderField("Link");
            if (string.IsNullOrEmpty(link)) return;
            
            var matcher = LinkPattern.Match(link);
            while (matcher.Success)
            {
                var url = matcher.Groups[1].Value;
                var foundRel = matcher.Groups[2].Value;
                _urls.Add(foundRel, new Uri(url));
            }
        }

        public interface IBodyCreator<T>
        {
            T Create(HttpContent content);
        }

        public ICollection<string> GetLinks()
        {
            return _urls.Keys.ToList();
        }

        public T ConsumeBody()
        {
            return _bodyCreator.Create(_response.Content);
        }

        public bool HasLink(string rel)
        {
            var hasLink = _urls.ContainsKey(rel);
            return hasLink;
        }

        public Uri GetLink(string rel)
        {
            return HasLink(rel)?_urls[rel]:null;
        }

        public void FollowLink(string rel)
        {
            if (HasLink(rel))
            {
                FollowUrl(GetLink(rel));
            }
            else {
                throw new KeyNotFoundException($"{rel} doesn't exist in our link dictionary");
            }
        }
    }
}
