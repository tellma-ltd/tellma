namespace Tellma.Controllers.Dto
{
    /// <summary>
    /// This DTO carries data with a version (etag) associated with it,
    /// clients are expected to cache this data permanently, until the user signs out
    /// (e.g. in web storage), clients are also expected to pass all versions in the 
    /// headers of EVERY API request, and the API returns in the headers whether each 
    /// version is fresh or stale. 
    /// This pattern is used when a piece of data is required before client startup
    /// (because it affects the UI for example), and we want to achieve instantaneous
    /// startup for clients that rely on backend data to manipulate their UI
    /// 
    /// Examples of such data are:
    ///  - User permissions
    ///  - Some settings, e.g. language settings
    ///  - Views and configurable documents specs
    /// </summary>
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
