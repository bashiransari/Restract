# Restract

[![NuGet](http://img.shields.io/nuget/v/Restract.svg)](https://www.nuget.org/packages/Restract/)

What is Restract?
--------------------------------
Restract is a contract based easy to use rest client .

# Installation using nuet :  

```
PM> Install-Package Restract
```

## Features

- .Net Core support
- Interceptor support (for customising requests/response before request sent or after response received)
- Async support
- DI friendly

## How to use

You just need to create the service contract and use RestClientFactory to create the http proxy.

### Contract

A contract is an interface describing a resource information with all actions signature.
Resource information is represented by IResourceDescriptor that has tow important properties: ResourceUrl and Paramters .
Action signature is represented by IResourceActionDescriptor that four  important properties: ActionPath, Method, Parameters, ResultDataType
Parameters can be of type Uri, Body, Header, Cookie, CancellationToken.

```#!csharp

[Header("HeaderItem1", "FixedValue")] // a header parameter in resource with a fix value
[Resource("api/customers")] // ResourceUrl
public interface ICustomersService
{
	//describing Get action for api/customers resource
	[Header("HeaderItem2", "FixedValue")] // a header parameter in action with a fix value
	IEnumerable<Customer> Get();

	// GET api/customers?id=5
	Customer Get(int id); // Get action with id paramter as url parameter (parameters with primitive type will be considered as url parameter by default)

	// POST api/customers
	void Post(Customer customer); // Post action with Customer object as body url (parameters with complex type will be considered as body parameter by default)

	// PUT api/customers?id=5
	void Put(int id, Customer customer);

	[ResourceAction(Method.Put, "/updateName/{id}")]
	void UpdateName(int id, [Body]string name); // Put action with id parameter in url and string paramter in body

	// DELETE api/customers
	[ResourceAction(Method.Delete)]
	void DeleteAll([Header]string secret); // Delete action with secret parameter in header
}

```

## Create proxy with inline config

By using a RestClientFactory object instance you would be able to create actual proxies out of your contracts.

```#!csharp
var customerServiceFactory = new RestClientFactory("http://localhost/testapi"); // create a client factory with "http://localhost/testapi" as baseUrl.

var client = customerServiceFactory.CreateClient<ICustomersService>(); // create the http proxy object

var customers = client.Get(); // Do actual http call and receive customers. Easy ;-)
```

## Create proxy with Interceptor

Interceptors can have multiple purpose, modify the http request or response, logging, mocking, exception hadling, ...
An Intercetor is basically a DelegatingHandler used by HttpClient internally, 

```#!csharp
var config = new RestClientConfiguration("http://localhost/testapi", new MyAuthorizationInterceptor());

var customerServiceFactory = new RestClientFactory(config);
customerServiceFactory.Configuration.AddInterceptor<MyAuthInterceptor>();

```

## Calling api endpoints

Once you have created the client by using **factory.CreateClient()** you can use all contract methods.

```#!csharp
var customers = client.Get();
var myCustomer = client.Get(1);

client.Post(new Customer()
{
	Id = 1000,
	Name = "New",
	Family = "2",
	BirthDate = new DateTime(2000, 1, 1)
});
```

## Getting http response details

You can use HttpResponseMessage or HttpResponseMessage<> in contract for accessing http response object on the consumer side

```#!csharp
[Resource("api/customers")]
public interface ICustomersService
{
	IEnumerable<Customer> Get();

	// GET api/customers
	HttpResponseMessage<IEnumerable<Customer>> GetWithResponse();

	// POST api/customers
	HttpResponseMessage Post(Customer customer);

	// GET api/customers/5
	HttpResponseMessage<Customer> GetWithResponse(int id);
}

```

## Async method

You can use Task or Task<> in contract for calling rest endpoint in async manner also you can add CancellationToken as parameter to your action.

```#!csharp
[Resource("api/customers")]
public interface ICustomersService
{
	// GET api/customers
	IEnumerable<Customer> Get();

	// GET api/customers
	Task<IEnumerable<Customer>> GetAsync();

	// GET api/customers
	[ResourceAction(Method.Get)]
	Task<HttpResponseMessage<IEnumerable<Customer>>> GetWithResponseAsync();

	// GET api/customers/5
	Task<RestResponse<Customer>> GetAsync(int id, CancellationToken token);

	// POST api/customers
	Task Post(Customer customer);

	// POST api/customers
	Task<HttpResponseMessage> Post(Customer customer);
}

```

## Creating Interceptor

```#!csharp
public class MyAuthorizationInterceptor : HttpMessageInterceptor
{
	public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, IResourceActionDescriptor resourceActionDescriptor, MethodCallInfo methodCallInfo, CancellationToken cancellationToken)
	{
		request.Headers.Add("my-auth", "123");

		var resp = await base.SendAsync(request, resourceActionDescriptor, methodCallInfo, cancellationToken)
			.ConfigureAwait(resourceActionDescriptor.ResultDataType.IsAsync);

		Console.WriteLine("Response:" + resp.StatusCode);
		return resp;
	}
}
```

## ParameterValueResolver

You can define custom parameters in resource/action level and use ParameterValueResolver to get the values in every call

```#!csahrp
class Program {
	void main() {
		var config = new RestClientConfiguration("http://localhost/test/");

		var customerServiceFactory = new RestClientFactory(config);
		var client = customerServiceFactory.CreateClient<ICustomersService>();
		...
	}
}

[Resource("api/subscriptions/{subscriptionId}/customers/{customerId}/accounts")]
[Uri("subscriptionId", ValueResolver = typeof(AppSettingsValueResolver))]
[Header("CorrelationId")]
[Header("HeaderItem", "StaticValue")]
public interface IAccountsService
{
	IEnumerable<Account> Get(string customerId, [Header]string correlationId);
}


public class AppSettingsValueResolver : ParameterValueResolver
{
	public override object GetValue(string parameterName, MethodCallInfo callInfo)
	{
		return ConfigurationManager.AppSettings[parameterName];
	}
}

```

## Using DI in intercetors and ParamterValueResolvers

RestClientProxy uses an internal service(ITypeActivator) for activating Intercetors and ParamterValueResolvers. You can replace this service with your own version by using RectClientFactory.UerTypeActivator() extension method.
This way you can use your own DI (Autofac, Ninject, AspnetCoreDI, ...) to activate the type.

### Asp.net core sample

### Startup.cs

```#!csahrp
    private RestClientFactory factory;
    public void ConfigureServices(IServiceCollection services)
    {
        ...
        //configuring restclientfactory
        factory = new RestClientFactory("http://localhost/RestClientProxyTestWeb");
        factory.Configuration.AddInterceptor<MyAuthInterceptor>();

        //register IConfigReader as service 
        services.AddTransient<IConfigReader, ConfigReader>();

        //register MyAuthInterceptor as service 
        services.AddTransient<MyAuthInterceptor, MyAuthInterceptor>();

        services.AddTransient(p => factory.CreateClient<ICustomerService>());

        services.AddMvc();
        ...
    }

    public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
    {
        loggerFactory.AddConsole(Configuration.GetSection("Logging"));
        loggerFactory.AddDebug();
        ...

        factory.UseTypeActivator(type => serviceProvider.GetService(type)); //using serviceProvider to activate MyAuthInterceptor.

        ...
        app.UseMvcWithDefaultRoute();
    }
```

### MyAuthInterceptor

```#!csahrp
    public class MyAuthInterceptor : HttpMessageInterceptor
    {
        private IConfigReader _configReader;

        public MyAuthInterceptor(IConfigReader configReader)
        {
            _configReader = configReader;
        }

        public override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, IResourceActionDescriptor resourceActionDescriptor, MethodCallInfo methodCallInfo, CancellationToken cancellationToken)
        {
            request.Headers.Add("MyAuth", _configReader["MyAuthValue"]);
            return base.SendAsync(request, resourceActionDescriptor, methodCallInfo, cancellationToken);
        }
    }
```


## Dependency Resolver

You can replace internal services with your own custom service using RestClientFactory.DependencyResolver property

```
using RestApiClientProxy;
class Program {
	void main() {
		var customerServiceFactory = new RestClientFactory("http://localhost/test/");

		customerServiceFactory.DependencyResolver.AddSingletone<IResourceDescriptorResolver, MyResourceDescriptorResolver>();

		var client = customerServiceFactory.CreateClient<ICustomersService>();
		...
	}
}
```

## Replace internal services

Here are some useful internal service that you might want to replace them with your own implementation.
To see full list of internal services hafve a look at InitDependencyResolver() method in RestClientFactory class.

### IResourceDescriptorResolver

This service is responsible for resolving ResourceDescriptor object from resource type.

Default implementation : ResourceDescriptorResolver

```
public interface IResourceDescriptorResolver
{
	IResourceDescriptor Resolve(Type resrouceType, RestClientConfiguration configuration);
}
```

### IResourceActionDescriptorResolver

This service is responsible for resolving ResourceActionDescriptor object from action MethodInfo.

Default implementation : ResourceActionDescriptorResolver

```
public interface IResourceActionDescriptorResolver
{
	ResourceActionDescriptor Resolve(MethodInfo methodInfo, RestClientConfiguration configuration, ResourceDescriptor resourceDescriptor);
}
```

### IHttpRequestMessageFactory

This service is responsible for creating HttpRequestMessage object by using resourceActionDescriptor and invocation.

Default implementation : HttpRequestMessageFactory

```
public interface IHttpRequestMessageFactory
{
	HttpRequestMessage GetHttpRequest(ResourceActionDescriptor resourceActionDescriptor, IMethodCallMessage invocation, RestClientConfiguration restClientConfiguration);
}
```

### ISerializer

This service is responsible for serialzing/deserializing request content/response content.

Default implementation : NewtonsoftJsonSerializer

```
public interface ISerializer
{
	string Serialize(object obj);
	T Deserialize<T>(string objString) where T : new();
	object Deserialize(string objString, Type objectType);
}
```

## Next release features

- Configuration file support