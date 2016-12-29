namespace Restract.Tests.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Moq;
    using NUnit.Framework;
    using Restract.Contract;
    using Restract.Contract.Attributes;
    using Restract.Contract.Parameter;
    using Restract.Core;
    using Restract.Core.DependencyResolver;
    using Restract.Descriptors;
    using Restract.Execution;

    [TestFixture]
    public class ResourceActionDescriptorResolverTests
    {
        private ResourceActionDescriptorResolver _resourceActionDescriptorResolver;
        private Mock<IResourceDescriptor> _resourceDescriptor;
        private Mock<MethodInfo> _methodInfo;
        private Mock<IAttributeFinder> _attributeFinder;
        private Mock<IDependencyResolver> _dependencyResolver;
        private List<ParameterBindingAttribute> _parameterBindings;
        private List<ResourceActionAttribute> _resourceActionAttributes;
        private Mock<IActionResultDataTypeResolver> _actionResultDataTypeResolver;
        private Mock<ITypeActivator> _typeActivator;

        private HttpParameterCollection _resourceDescriptorParameters;
        private readonly TemplateUri _resourceUri = new TemplateUri("resource/");

        [SetUp]
        public void Setup()
        {
            _attributeFinder = new Mock<IAttributeFinder>();
            _methodInfo = new Mock<MethodInfo>();
            _resourceDescriptor = new Mock<IResourceDescriptor>();
            _dependencyResolver = new Mock<IDependencyResolver>();
            _actionResultDataTypeResolver = new Mock<IActionResultDataTypeResolver>();
            _typeActivator = new Mock<ITypeActivator>();

            _resourceDescriptorParameters = new HttpParameterCollection()
            {
                new HttpParameter("Param", HttpParameterType.Uri, null)
            };

            _methodInfo.Setup(p => p.Name).Returns("MethodName");

            _resourceDescriptor.Setup(p => p.Parameters).Returns(_resourceDescriptorParameters);

            _resourceDescriptor.Setup(p => p.ResourceUrl).Returns(_resourceUri);

            _resourceActionDescriptorResolver = new ResourceActionDescriptorResolver(_attributeFinder.Object, _actionResultDataTypeResolver.Object, _typeActivator.Object);

            _parameterBindings = new List<ParameterBindingAttribute>();
            _resourceActionAttributes = new List<ResourceActionAttribute>();

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(It.IsAny<MethodInfo>(), false))
                .Returns(() => _parameterBindings);

            _attributeFinder
                .Setup(p => p.GetAttributes<ResourceActionAttribute>(It.IsAny<MethodInfo>(), false))
                .Returns(() => _resourceActionAttributes);
        }

        [Test]
        public void GivenMethodInfoIsNull_WhenResolveInvoked_ThenArgumentNullExceptionShouldBeThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => _resourceActionDescriptorResolver.Resolve(null, _resourceDescriptor.Object));
        }

        [Test]
        public void GivenResourceDescriptorIsNull_WhenResolveInvoked_ThenArgumentNullExceptionShouldBeThrown()
        {
            Assert.Throws<ArgumentNullException>(
                () => _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, null));
        }

        [Test]
        public void GivenResourceDescriptorIsValid_WhenResolveInvoked_ThenResourceActionDescriptorShouldBeCreatedUsingResourceDescriptor()
        {
            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);
            Assert.AreEqual(_resourceDescriptor.Object, resourceActionDescriptor.ResourceDescriptor);

            foreach (var param in _resourceDescriptorParameters)
            {
                var exists = resourceActionDescriptor
                    .Parameters
                    .Any(p => p.Name == param.Name &&
                                p.Type == param.Type &&
                                p.ValueResolver == param.ValueResolver);

                Assert.IsTrue(exists);
            }
        }

        [TestCase(Method.Head)]
        [TestCase(Method.Get)]
        [TestCase(Method.Post)]
        [TestCase(Method.Delete)]
        public void GivenThereIsAResoureActionAttribute_WhenResolveInvoked_ThenActionMethodShouldBeAccordingToTheAttribute(Method method)
        {
            _resourceActionAttributes.Add(new ResourceActionAttribute(method));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            Assert.AreEqual(method.ToString(), resourceActionDescriptor.Method.Method);
        }

        [TestCase("Get", "GET")]
        [TestCase("Post", "POST")]
        [TestCase("Test", "TEST")]
        public void GivenThereIsNoResoureActionAttribute_WhenResolveInvoked_ThenActionMethodShouldBeAccordingToTheMethodName(string methodName, string httpMethod)
        {
            _methodInfo.Setup(p => p.Name).Returns(methodName);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            Assert.AreEqual(httpMethod, resourceActionDescriptor.Method.Method);
        }


        [TestCase("Get", "GET")]
        [TestCase("GetAsync", "GET")]
        [TestCase("Post", "POST")]
        [TestCase("PostAsync", "POST")]
        public void GivenThereIsNoResoureActionAttributeAndTaskIsAsync_WhenResolveInvoked_ThenActionMethodShouldBeAccordingToTheMethodName(string methodName, string httpMethod)
        {
            _methodInfo.Setup(p => p.Name).Returns(methodName);
            _methodInfo.Setup(p => p.ReturnType).Returns(typeof(Task<List<int>>));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            Assert.AreEqual(httpMethod, resourceActionDescriptor.Method.Method);
        }

        [TestCase("MyAction")]
        [TestCase("Path/action")]
        [TestCase("Path/action/")]
        public void GivenThereIsAResoureActionAttribute_WhenResolveInvoked_ThenActionPathShouldBeAccordingToTheAttribute(string path)
        {
            _resourceActionAttributes.Add(new ResourceActionAttribute(path));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            var actionPath = TemplateUri.Append(_resourceUri, new TemplateUri(path));
            Assert.AreEqual(actionPath.Template, resourceActionDescriptor.ActionPath.Template);
        }

        [Test]
        public void GivenThereIsAResoureActionAttributeWithParametricPath_WhenResolveInvoked_ThenPthParametersShouldBeAddedToParametersLits()
        {
            _resourceActionAttributes.Add(new ResourceActionAttribute("users/{id}/products/{name}"));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            var idExists = resourceActionDescriptor.Parameters.Any(p => p.Name == "id" && p.Type == HttpParameterType.Uri);
            var nameExists = resourceActionDescriptor.Parameters.Any(p => p.Name == "name" && p.Type == HttpParameterType.Uri);

            Assert.IsTrue(idExists);
            Assert.IsTrue(nameExists);
        }

        [Test]
        public void GivenThereAreParameterBindingAttributes_WhenResolveInvoked_ThenParametersShouldBeAddedToActionDescriptor()
        {
            _parameterBindings.Add(new BodyAttribute("Test"));
            _parameterBindings.Add(new HeaderAttribute("Test"));
            _parameterBindings.Add(new UriAttribute("Test"));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            foreach (var param in _parameterBindings)
            {
                var exists = resourceActionDescriptor
                    .Parameters
                    .Any(p => p.Name == param.Name &&
                        p.Type == param.ParameterType);

                Assert.IsTrue(exists);
            }
        }

        [Test]
        public void GivenThereAreParameterBindingAttributesWithValue_WhenResolveInvoked_ThenParametersShouldBeAddedToActionDescriptorWithCorrectValueResolver()
        {
            _parameterBindings.Add(new BodyAttribute("Test") { Value = "BodyValue" });
            _parameterBindings.Add(new HeaderAttribute("Test", "Value"));

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            foreach (var param in _parameterBindings)
            {
                var exists = resourceActionDescriptor
                    .Parameters
                    .Any(p => p.Name == param.Name &&
                        p.Type == param.ParameterType &&
                        p.ValueResolver.GetValue(null, null).ToString() == param.Value);

                Assert.IsTrue(exists);
            }
        }

        private static Mock<ParameterInfo> GetMockParameter(string name, Type returnType)
        {
            var param = new Mock<ParameterInfo>();
            param.SetupGet(p => p.Name).Returns(name);
            param.SetupGet(p => p.ParameterType).Returns(returnType);
            return param;
        }

        [Test]
        public void GivenMethodInfoHasArguments_WhenResolveInvoked_ThenArgumentsShouldBeAddedToResourcActionParametersWithArgumentResolver()
        {
            var firstParam = GetMockParameter("Param1", typeof(int));
            var secondParam = GetMockParameter("Param2", typeof(int));
            var parameters = new[]
            {
                firstParam.Object,
                secondParam.Object
            };

            _methodInfo.Setup(p => p.GetParameters()).Returns(parameters);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            foreach (var param in parameters)
            {
                var exists = resourceActionDescriptor
                    .Parameters
                    .Any(p => p.Name == param.Name &&
                        p.ValueResolver.GetType() == typeof(ArgumentValueResolver));

                Assert.IsTrue(exists);
            }
        }

        [TestCase(typeof(int), HttpParameterType.Uri)]
        [TestCase(typeof(string), HttpParameterType.Uri)]
        [TestCase(typeof(bool), HttpParameterType.Uri)]
        [TestCase(typeof(object), HttpParameterType.Body)]
        public void GivenMethodInfoHasArgumentsWithoutParameterBindingAttribute_WhenResolveInvoked_ThenResourcActionParameterTypesShouldBeDerivedFromArgumentType(Type argumentType, HttpParameterType parameterType)
        {
            var firstParam = GetMockParameter("Param1", argumentType);
            var parameters = new[]
            {
                firstParam.Object,
            };
            var argumentResolver = new ArgumentValueResolver(0);
            _methodInfo.Setup(p => p.GetParameters()).Returns(parameters);
            _dependencyResolver.Setup(p => p.Resolve<ArgumentValueResolver>()).Returns(argumentResolver);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            foreach (var param in parameters)
            {
                var exists = resourceActionDescriptor
                    .Parameters
                    .Any(p => p.Name == param.Name &&
                        p.Type == parameterType);

                Assert.IsTrue(exists);
            }
        }

        [TestCase("Id", HttpParameterType.Uri)]
        [TestCase("Name", HttpParameterType.Header)]
        [TestCase("Test", HttpParameterType.Cookie)]
        [TestCase("test", HttpParameterType.Body)]
        public void GivenMethodInfoHasArgumentsWithParameterBindingAttribute_WhenResolveInvoked_ThenResourcActionParameterTypesAndNamesShouldBeDerivedParamterbindingAttribute(string name, HttpParameterType parameterType)
        {
            var firstParam = GetMockParameter("Param1", typeof(object));
            var parameters = new[]
            {
                firstParam.Object,
            };

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(firstParam.Object, false))
                .Returns(new List<ParameterBindingAttribute>() { new ParameterBindingAttribute(parameterType, name) });

            var argumentResolver = new ArgumentValueResolver(0);
            _methodInfo.Setup(p => p.GetParameters()).Returns(parameters);
            _dependencyResolver.Setup(p => p.Resolve<ArgumentValueResolver>()).Returns(argumentResolver);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            var exists = resourceActionDescriptor
                .Parameters
                .Any(p => p.Name == name &&
                    p.Type == parameterType);

            Assert.IsTrue(exists);
        }

        [Test]
        public void GivenThereDuplicateParametersInResourceAndAction_WhenResolveInvoked_ThenParameterResolverShouldBeReplacedWithTheOneInAction()
        {
            var firstParam = GetMockParameter("Param", typeof(string));//string will be resolved to url parameter

            var parameters = new[]
            {
                firstParam.Object,
            };

            _methodInfo.Setup(p => p.GetParameters()).Returns(parameters);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            var exists = resourceActionDescriptor
                .Parameters
                .Any(p => p.Name == "Param" &&
                    p.Type == HttpParameterType.Uri &&
                    p.ValueResolver.GetType() == typeof(ArgumentValueResolver));

            Assert.IsTrue(exists);
        }


        [Test]
        public void WhenResolveInvoked_ThenResultDataTypeShouldComeFromResultDataTypeResolver()
        {
            var resultDataType = new ActionResultDataType();

            _actionResultDataTypeResolver
                .Setup(p => p.Resolve(It.IsAny<MethodInfo>()))
                .Returns(resultDataType);

            var resourceActionDescriptor = _resourceActionDescriptorResolver.Resolve(_methodInfo.Object, _resourceDescriptor.Object);

            Assert.AreEqual(resultDataType, resourceActionDescriptor.ResultDataType);
        }

        [Test]
        public void GivenTypeActivatorCannotActivateActionParameterValueResolverType_WhenResolvedInvoked_ThenInvalidOperationExceptionShouldBrThrown()
        {
            var firstParam = GetMockParameter("Param1", typeof(object));
            var parameters = new[]
            {
                firstParam.Object,
            };

            var parameterValueResolver = new Mock<ParameterValueResolver>();

            var parameter = new HeaderAttribute("param1") { ValueResolver = parameterValueResolver.Object.GetType() };

            _typeActivator.Setup(p => p.Activate(It.IsAny<Type>())).Returns(null);

            _attributeFinder
                .Setup(p => p.GetAttributes<ParameterBindingAttribute>(firstParam.Object, false))
                .Returns(new List<ParameterBindingAttribute>() { parameter });

            var argumentResolver = new ArgumentValueResolver(0);
            _methodInfo.Setup(p => p.GetParameters()).Returns(parameters);
            _dependencyResolver.Setup(p => p.Resolve<ArgumentValueResolver>()).Returns(argumentResolver);

            Assert.Throws<InvalidOperationException>(
                () => _resourceActionDescriptorResolver
                .Resolve(_methodInfo.Object, _resourceDescriptor.Object));
        }
    }
}
