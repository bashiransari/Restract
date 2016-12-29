namespace Restract.Execution
{
    using System;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Restract.Contract;
    using Restract.Serialization;

    public class ActionResultBuilder: IActionResultBuilder
    {
        private readonly ISerializer _serializer;
        public ActionResultBuilder(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public object BuildActionResult(
            ActionResultDataType resultDataType,
            Task<HttpResponseMessage> responseMessage)
        {
            var result = BuildActionResultAsync(resultDataType, responseMessage);

            if (!resultDataType.IsAsync)
            {
                result.Wait();
                return result.Result;
            }
            else
            {
                return WrapInTask(result, resultDataType);
            }
        }

        public async Task<object> BuildActionResultAsync(ActionResultDataType resultDataType, Task<HttpResponseMessage> responseMessage)
        {
            object result = null;
            var response = await responseMessage.ConfigureAwait(false);

            if (resultDataType.ResponseDataType != typeof(void))
            {
                result = await GetResponseData(response, resultDataType).ConfigureAwait(false);
            }

            if (resultDataType.ReturnResponseMessage)
            {
                result = WrapInHttpResponseMessage(result, response, resultDataType);
            }
            return result;
        }

        private async Task<object> GetResponseData(HttpResponseMessage response, ActionResultDataType resultDataType)
        {
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var result = _serializer.Deserialize(
                responseContent,
                resultDataType.ResponseDataType);

            return result;
        }

        private static object WrapInHttpResponseMessage(object result, HttpResponseMessage originalResponse, ActionResultDataType resultDataType)
        {
            if (resultDataType.ResponseDataType == typeof(void)) return originalResponse;

            var genericType = typeof(HttpResponseMessage<>).MakeGenericType(resultDataType.ResponseDataType);
            var httpResponse = Activator.CreateInstance(genericType, originalResponse, result);
            return httpResponse;
        }

        private object WrapInTask(object result, ActionResultDataType resultDataType)
        {
            if (resultDataType.ResponseDataType == typeof(void))
            {
                if (!resultDataType.ReturnResponseMessage)
                {
                    return result;
                }
            }

            var returnDataType = resultDataType.MethodReturnType.GetTypeInfo().GetGenericArguments()[0];

            var castAsyncResultMethodGeneric = CastAsyncResultMethod.MakeGenericMethod(returnDataType);

            var asyncresult = castAsyncResultMethodGeneric.Invoke(this, new[] { result });
            return asyncresult;
        }

        private static readonly MethodInfo CastAsyncResultMethod =
            typeof(ActionResultBuilder).GetTypeInfo().GetMethod("CastAsyncResult", BindingFlags.NonPublic | BindingFlags.Static);


        // ReSharper disable once UnusedMember.Local
        private static async Task<T> CastAsyncResult<T>(Task<object> task)
        {
            var result = await task.ConfigureAwait(false);

            return (T)result;
        }
    }
}