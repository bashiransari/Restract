namespace Restract.Tests.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Contract.Attributes;
    using Restract.Contract.Parameter;
    using Restract.Core;
    using Restract.Descriptors;
    using Restract.Execution;

    [TestFixture]
    public class ResourceDescriptorResolverTests
    {
        private ResourceDescriptorResolver _resourceDescriptorResolver;
        private Mock<IAttributeFinder> _attributeFinder;
        private string _typeName;
        private ResourceAttribute _resourceAttribute;
        private Mock<ITypeActivator> _typeActivator;
        private Mock<IResourceActionDescriptorResolver> _resourceActionDescriptorResolver;

        [SetUp]
        public void Setup()
        {
            _attributeFinder = new Mock<IAttributeFinder>();
            _typeActivator = new Mock<ITypeActivator>();
            _resourceActionDescriptorResolver = new Mock<IResourceActionDescriptorResolver>();

            _resourceDescriptorResolver = new ResourceDescriptorResolver(_attributeFinder.Object, _typeActivator.Object, _resourceActionDescriptorResolver.Object);

            _typeName = Guid.NewGuid().ToString();
            _resourceAttribute = new ResourceAttribute(_typeName);

            _attributeFinder
                .Setup(p => p.GetAttributes<ResourceAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(() => new List<ResourceAttribute>() { _resourceAttribute });
        }

        [Test]
        public void GivenResourceTypeIsNull_WhenResolvedInvoked_ThenArgumentNullExceptionShouldBeThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => _resourceDescriptorResolver.Resolve(null));
        }

        [Test]
        public void GivenThereIsNoResourceAttribute_WhenResolvedInvoked_ThenTypeNameShouldBeUsedAsResourseUrl()
        {
            _attributeFinder
                .Setup(p => p.GetAttributes<ResourceAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(new List<ResourceAttribute>());

            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));

            Assert.AreEqual(typeof(IDummyContract).Name, resourceDescriptor.ResourceUrl.Template);
        }

        [Test]
        public void GivenThereIsAResourceAttribute_WhenResolvedInvoked_ThenPathShouldBeUsedAsResourseUrl()
        {
            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));

            Assert.AreEqual(_typeName, resourceDescriptor.ResourceUrl.Template);
        }


        [Test]
        public void GivenThereAreParamtersInTheResourceUri_WhenResolvedInvoked_ThenParametersShouldBeAddedToResource()
        {
            _resourceAttribute = new ResourceAttribute("/items/{id}/subItems");

            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));


            Assert.AreEqual("id", resourceDescriptor.Parameters.First().Name);
            Assert.IsNull(resourceDescriptor.Parameters.First().ValueResolver);
        }

        [Test]
        public void GivenThereAreParamtersInTheResourceUriAndParamterBindings_WhenResolvedInvoked_ThenParametersShouldBeAddedToResourceWithCorrectValueResolvers()
        {
            _resourceAttribute = new ResourceAttribute("/items/{id}/subItems");

            var bindingAttribute = new ParameterBindingAttribute(HttpParameterType.Uri, "id")
            {
                Value = "value"
            };

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(new List<ParameterBindingAttribute>() { bindingAttribute });

            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));

            Assert.AreEqual("id", resourceDescriptor.Parameters.First().Name);
            Assert.AreEqual(typeof(FixValueResolver), resourceDescriptor.Parameters.First().ValueResolver.GetType());
        }

        [Test]
        public void GivenThereAreParameterBindingAttribute_WhenResolvedInvoked_ThenParametersShouldBeAddedToResource()
        {
            var bindingAttribute = new ParameterBindingAttribute(HttpParameterType.Header, "auth")
            {
                Value = "password"
            };

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(new List<ParameterBindingAttribute>() { bindingAttribute });

            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));

            Assert.AreEqual("auth", resourceDescriptor.Parameters.First().Name);
            var parameter = resourceDescriptor.Parameters.First();
            Assert.AreEqual(HttpParameterType.Header, parameter.Type);
            Assert.AreEqual(typeof(FixValueResolver), parameter.ValueResolver.GetType());
            Assert.AreEqual("password", parameter.ValueResolver.GetValue(null, null));
        }

        [Test]
        public void GivenThereIsParameterBindingAttributeWithValueResolver_WhenResolvedInvoked_ThenParametersShouldBeAddedToResourceWithCorrectResolverInstance()
        {
            var valueResolver = new Mock<ParameterValueResolver>();
            valueResolver.Setup(p => p.GetValue(It.IsAny<string>(), It.IsAny<MethodCallInfo>())).Returns("ParamValue");

            var bindingAttribute = new ParameterBindingAttribute(HttpParameterType.Header, "auth")
            {
                ValueResolver = typeof(string)
            };

            _typeActivator.Setup(p => p.Activate(typeof(string))).Returns(valueResolver.Object);

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(new List<ParameterBindingAttribute>() { bindingAttribute });

            var resourceDescriptor = _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract));

            var parameter = resourceDescriptor.Parameters.First();
            Assert.AreEqual(valueResolver.Object.GetType(), parameter.ValueResolver.GetType());
            Assert.AreEqual("ParamValue", parameter.ValueResolver.GetValue(null, null));
        }

        [Test]
        public void GivenTypeActivatorCannotActivateResourceParameterValueResolverType_WhenResolvedInvoked_ThenInvalidOperationExceptionShouldBrThrown()
        {
            var valueResolver = new Mock<ParameterValueResolver>();
            valueResolver.Setup(p => p.GetValue(It.IsAny<string>(), It.IsAny<MethodCallInfo>())).Returns("ParamValue");

            var bindingAttribute = new ParameterBindingAttribute(HttpParameterType.Header, "auth")
            {
                ValueResolver = typeof(string)
            };

            _typeActivator.Setup(p => p.Activate(typeof(string))).Returns(null);

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(It.IsAny<TypeInfo>(), false))
                .Returns(new List<ParameterBindingAttribute>() { bindingAttribute });

            Assert.Throws<InvalidOperationException>(() => _resourceDescriptorResolver
                .Resolve(typeof(IDummyContract)));

        }
    }
}
