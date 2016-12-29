namespace Restract.Descriptors
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;
    using Restract.Contract;

    public class ActionResultDataTypeResolver : IActionResultDataTypeResolver
    {
        public ActionResultDataType Resolve(MethodInfo methodInfo)
        {
            var returnDataType = new ActionResultDataType
            {
                MethodReturnType = methodInfo.ReturnType,
            };

            var responseDataType = methodInfo.ReturnType;

            returnDataType.IsAsync = CanStripType<Task>(responseDataType, out responseDataType);

            returnDataType.ReturnResponseMessage = CanStripType<HttpResponseMessage>(responseDataType, out responseDataType);

            returnDataType.ResponseDataType = responseDataType;
            return returnDataType;
        }

        private static bool CanStripType<TWrapper>(Type type, out Type innerType)
        {
            if (!typeof(TWrapper).GetTypeInfo().IsAssignableFrom(type))
            {
                innerType = type;
                return false;
            }

            if (type.GetTypeInfo().IsGenericType)
            {
                innerType = type.GetTypeInfo().GetGenericArguments().First();
            }
            else
            {
                innerType = typeof(void);
            }
            return true;
        }
    }
}