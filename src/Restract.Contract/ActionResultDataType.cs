namespace Restract.Contract
{
    using System;

    public class ActionResultDataType
    {
        public Type MethodReturnType { get; set; }
        public Type ResponseDataType { get; set; }
        public bool IsAsync { get; set; }
        public bool ReturnResponseMessage { get; set; }
    }
}