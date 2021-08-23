namespace Tellma.Api.Dto
{
    /// <summary>
    /// Carries data with a version (etag) associated with it, clients can cache this 
    /// data permanently (e.g. in web storage) until the user signs out, clients can
    /// also pass all versions in the headers of EVERY API request, and the API returns
    /// in the response headers whether each version is fresh or stale. 
    /// </summary>
    /// <remarks>
    /// This pattern is used when a piece of data is required before client startup
    /// (because it affects the UI for example), and we want to achieve instantaneous
    /// startup for clients that rely on backend data to manipulate their UI
    /// Examples of such data are:
    ///  - User permissions <br/>
    ///  - Settings, e.g. language settings <br/>
    ///  - Views and configurable documents specs <br/>
    /// </remarks>
    public class Versioned<TData>
    {
        public Versioned(TData data, string version)
        {
            Version = version;
            Data = data;
        }

        public string Version { get; }

        public TData Data { get; }
    }
}
