using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiscoSpark.SDK
{
    public interface IRequestBuilder<T>
    {
        IRequestBuilder<T> QueryParam(string key, string value);
        IRequestBuilder<T> Path(params object[] paths);
        IRequestBuilder<TNewType> Path<TNewType>(string path) where TNewType : new();
        IRequestBuilder<T> Url(Uri url);
        T Post(T body);
        T Put(T body);
        T Get();
        void Delete();
        IEnumerable<T> iterate();
    }
}
