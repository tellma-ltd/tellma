using System.Text.Json;

namespace Tellma.Integration.Zatca
{
    /// <summary>
    /// Wrapper for all responses from ZATCA API.
    /// </summary>
    /// <typeparam name="T">The type of the Dto that the response body is mapped to.</typeparam>
    public class Response<T> : Response where T : class
    {
        private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

        /// <summary>
        /// Create a new instance of <see cref="Response{T}"/>.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="result"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public Response(ResponseStatus status, T? result) : base(status)
        {
            Result = result;

            if (IsSuccess && Result == null)
            {
                throw new InvalidOperationException("If the status code is successful, the result cannot be null.");
            }
        }

        /// <summary>
        /// The parsed response body from the ZATCA API.
        /// <para/>
        /// This property is: <br/>
        /// - Not NULL when the status is <see cref="ResponseStatus.Success"/> or <see cref="ResponseStatus.SuccessWithWarnings"/>. <br/>
        /// - Maybe NULL when the status is <see cref="ResponseStatus.InvalidRequest"/>. <br/>
        /// - NULL for all other statuses.
        /// </summary>
        public T? Result { get; }

        /// <summary>
        /// Returns <see cref="Result"/> or throws an error if it is NULL.
        /// </summary>
        public T ResultOrThrow() => Result ?? throw new InvalidOperationException("Result is null.");

        /// <summary>
        /// Converts <see cref="Result"/> to a JSON string.
        /// </summary>
        /// <returns></returns>
        public string ToJson() => Result == null ? "" : JsonSerializer.Serialize(Result, _jsonOptions);
    }

    public abstract class Response(ResponseStatus status)
    {

        /// <summary>
        /// The response status from the ZATCA API.
        /// </summary>
        public ResponseStatus Status { get; } = status;

        /// <summary>
        /// Returns true if the <see cref="Status"/> is <see cref="ResponseStatus.Success"/> or <see cref="ResponseStatus.SuccessWithWarnings"/>.
        /// </summary>
        public bool IsSuccess => ((int)Status >= 200) && ((int)Status <= 299);
    }
}