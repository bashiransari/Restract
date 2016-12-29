namespace Restract.Tests.Descriptors
{
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Descriptors;

    [TestFixture]
    public class ResourceDescriptorTests
    {
        private ResourceDescriptor _resourceDescriptor;
        private Mock<IResourceActionDescriptorResolver> _resourceActionDescriptorResolver;

        [SetUp]
        public void Setup()
        {
            _resourceActionDescriptorResolver = new Mock<IResourceActionDescriptorResolver>();

            _resourceDescriptor = new ResourceDescriptor(_resourceActionDescriptorResolver.Object);
        }

        [Test]
        public void GivenResourceActionDescriptorHasBeenResolvedOnce_WhenResolvedInvoked_ThenResourceActionDescriptorResolverShouldNotBeCalled()
        {
            var methodInfo = GetType().GetMethods()[0];

            _resourceDescriptor.GetActionDescriptor(methodInfo);
            _resourceActionDescriptorResolver.Verify(p => p.Resolve(It.IsAny<MethodInfo>(), It.IsAny<IResourceDescriptor>()), Times.Once());

            _resourceActionDescriptorResolver.Reset();

            _resourceDescriptor.GetActionDescriptor(methodInfo);
            _resourceActionDescriptorResolver.Verify(p => p.Resolve(It.IsAny<MethodInfo>(), It.IsAny<IResourceDescriptor>()), Times.Never());
        }

    }
}
