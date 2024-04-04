using Tellma.Model.Common;

namespace Tellma.Client
{
    public abstract class ApplicationSettingsClientBase<TSettingsForSave, TSettings> : ClientBase 
        where TSettings : Entity 
        where TSettingsForSave : Entity
    {
        #region Lifecycle

        internal ApplicationSettingsClientBase(IClientBehavior behavior) : base(behavior)
        {
        }

        #endregion

        #region API

        // TODO
        //public virtual async Task<TSettings> GetSettings(Request<SelectExpandArguments> request = null, CancellationToken cancellation = default)
        //{
        //    // Prepare the URL
        //    var urlBldr = GetActionUrlBuilder();

        //    // Add query parameters
        //    var args = request?.Arguments ?? new GetByIdArguments();

        //    urlBldr.AddQueryParameter(nameof(args.Select), args.Select);
        //    urlBldr.AddQueryParameter(nameof(args.Expand), args.Expand);

        //    // Prepare the message
        //    var method = HttpMethod.Get;
        //    using var msg = new HttpRequestMessage(method, urlBldr.Uri);

        //    // Send the message
        //    using var httpResponse = await SendAsync(msg, request, cancellation).ConfigureAwait(false);
        //    await httpResponse.EnsureSuccess(cancellation).ConfigureAwait(false);

        //    // Extract the response
        //    var response = await httpResponse.Content
        //        .ReadAsAsync<GetEntityResponse<TSettings>>(cancellation)
        //        .ConfigureAwait(false);

        //    var settings = response.Result;
        //    var relatedEntities = response.RelatedEntities;

        //    var singleton = new List<TSettings> { settings };
        //    ClientUtil.Unflatten(singleton, relatedEntities, cancellation);

        //    return settings;
        //}

        #endregion
    }
}
