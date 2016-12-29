namespace Restract.Tests.Descriptors
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Descriptors;

    [TestFixture]
    public class ActionResultDataTypeResolverTests
    {
        private ActionResultDataTypeResolver _actionResultDataTypeResolver;
        private Mock<MethodInfo> _methodInfo;

        [SetUp]
        public void SetUp()
        {
            _actionResultDataTypeResolver = new ActionResultDataTypeResolver();
            _methodInfo = new Mock<MethodInfo>();

        }

        [TestCase(typeof(int), typeof(int), false, false)]
        [TestCase(typeof(Task<string>), typeof(string), true, false)]
        [TestCase(typeof(HttpResponseMessage<string>), typeof(string), false, true)]
        [TestCase(typeof(Task<HttpResponseMessage<string>>), typeof(string), true, true)]
        public void WhenResolveIsCalledWithMethodInfoThenResultShouldBeBasedOnMethodInfoReturnType(Type returnType, Type responseType, bool isAsync, bool returnResponseMessage)
        {
            _methodInfo.Setup(p => p.ReturnType).Returns(returnType);

            var res = _actionResultDataTypeResolver.Resolve(_methodInfo.Object);

            Assert.AreEqual(returnType, res.MethodReturnType);
            Assert.AreEqual(responseType, res.ResponseDataType);
            Assert.AreEqual(isAsync, res.IsAsync);
            Assert.AreEqual(returnResponseMessage, res.ReturnResponseMessage);
        }
    }
}