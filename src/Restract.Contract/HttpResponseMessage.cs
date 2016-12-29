namespace Restract.Contract
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;

    public class HttpResponseMessage<T> : HttpResponseMessage
    {
        private readonly HttpResponseMessage _originalMessage;
        public T ContentObject { get; set; }
        public HttpResponseMessage(HttpResponseMessage originalMessage, T contentObject)
        {
            ContentObject = contentObject;
            _originalMessage = originalMessage;
        }

        public new HttpResponseHeaders Headers => _originalMessage.Headers;
        public new bool IsSuccessStatusCode => _originalMessage.IsSuccessStatusCode;

        public new HttpContent Content
        {
            get
            {
                return _originalMessage.Content;
            }
            set
            {
                _originalMessage.Content = value;
            }
        }


        public new string ReasonPhrase
        {
            get
            {
                return _originalMessage.ReasonPhrase;
            }
            set
            {
                _originalMessage.ReasonPhrase = value;
            }
        }

        public new HttpRequestMessage RequestMessage
        {
            get
            {
                return _originalMessage.RequestMessage;
            }
            set
            {
                _originalMessage.RequestMessage = value;
            }
        }

        public new HttpStatusCode StatusCode
        {
            get
            {
                return _originalMessage.StatusCode;
            }
            set
            {
                _originalMessage.StatusCode = value;
            }
        }

        public new System.Version Version
        {
            get
            {
                return _originalMessage.Version;
            }
            set
            {
                _originalMessage.Version = value;
            }
        }

        public new HttpResponseMessage EnsureSuccessStatusCode()
        {
            return _originalMessage.EnsureSuccessStatusCode();
        }

        public new string ToString()
        {
            return _originalMessage.ToString();
        }

        public new void Dispose()
        {
            _originalMessage.Dispose();
            base.Dispose();
        }

    }
}