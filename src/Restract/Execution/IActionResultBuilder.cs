namespace Restract.Execution
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using Restract.Contract;

    public interface IActionResultBuilder
    {
        object BuildActionResult(ActionResultDataType resultDataType, Task<HttpResponseMessage> responseMessage);
    }
}