//using Newtonsoft.Json;
//using System;
//using System.Net.Http;
//using System.Net.Http.Formatting;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace BSharp.IntegrationTests.Utilities
//{
//    public static class CustomHttpClientExtensions
//    {
//        //public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data)
//        //    => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) });

//        //public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, string requestUri, T data, CancellationToken cancellationToken)
//        //    => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) }, cancellationToken);

//        //public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data)
//        //    => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) });

//        //public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient httpClient, Uri requestUri, T data, CancellationToken cancellationToken)
//        //    => httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri) { Content = Serialize(data) }, cancellationToken);

//        //private static HttpContent Serialize(object data)
//        //    => new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");


//        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, string requestUri, T value)
//        {
//            return client.DeleteAsJsonAsync(requestUri, value, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, new JsonMediaTypeFormatter(), "application/json", cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value)
//        {
//            return client.DeleteAsJsonAsync(requestUri, value, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsJsonAsync<T>(this HttpClient client, Uri requestUri, T value, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, new JsonMediaTypeFormatter(), "application/json", cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, mediaType: (MediaTypeHeaderValue)null, cancellationToken: cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, BuildHeaderValue(mediaType), cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, string requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
//        {
//            if (client is null)
//            {
//                throw new ArgumentNullException(nameof(client));
//            }

//            var content = new ObjectContent<T>(value, formatter, mediaType);
//            return client.SendAsync(MakeDeleteMessage(new Uri(requestUri, UriKind.Relative), content, cancellationToken));
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, mediaType: (MediaTypeHeaderValue)null, cancellationToken: cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, mediaType, CancellationToken.None);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, string mediaType, CancellationToken cancellationToken)
//        {
//            return client.DeleteAsync(requestUri, value, formatter, BuildHeaderValue(mediaType), cancellationToken);
//        }

//        public static Task<HttpResponseMessage> DeleteAsync<T>(this HttpClient client, Uri requestUri, T value, MediaTypeFormatter formatter, MediaTypeHeaderValue mediaType, CancellationToken cancellationToken)
//        {
//            if (client == null)
//            {
//                throw new ArgumentNullException(nameof(client));
//            }

//            var content = new ObjectContent<T>(value, formatter, mediaType);
//            return client.SendAsync(MakeDeleteMessage(requestUri, content, cancellationToken));
//        }

//        private static MediaTypeHeaderValue BuildHeaderValue(string mediaType)
//        {
//            return mediaType != null ? new MediaTypeHeaderValue(mediaType) : null;
//        }

//        private static HttpRequestMessage MakeDeleteMessage(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
//        {
//            var message = new HttpRequestMessage
//            {
//                Content = content,
//                Method = HttpMethod.Delete,
//                RequestUri = requestUri,
//            };

//            return message;
//        }
//    }
//}
