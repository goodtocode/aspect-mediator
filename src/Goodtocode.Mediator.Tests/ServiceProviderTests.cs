using Goodtocode.Mediator;

namespace Goodtocode.Mediator.Tests;

[TestClass]
public class ServiceProviderTests
{
    #pragma warning disable CA2263
    private class TestService { }
    private class AnotherService { }

    private class SimpleServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new();

        public void AddService(Type type, object instance) => _services[type] = instance;

        public object? GetService(Type serviceType) =>
            _services.TryGetValue(serviceType, out var instance) ? instance : null;
    }

    [TestMethod]
    public void GetRequiredServiceThrowsOnNullProvider()
    {
        IServiceProvider provider = null!;
        Assert.Throws<ArgumentNullException>(() => provider.GetRequiredService(typeof(TestService)));
    }

    [TestMethod]
    public void GetRequiredServiceThrowsOnNullType()
    {
        var provider = new SimpleServiceProvider();
        Assert.Throws<ArgumentNullException>(() => provider.GetRequiredService(null!));
    }

    [TestMethod]
    public void GetRequiredServiceThrowsIfNotRegistered()
    {
        var provider = new SimpleServiceProvider();
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService(typeof(TestService)));
    }

    [TestMethod]
    public void GetRequiredServiceReturnsRegisteredService()
    {
        var provider = new SimpleServiceProvider();
        var service = new TestService();
        provider.AddService(typeof(TestService), service);

        var result = provider.GetRequiredService(typeof(TestService));
        Assert.AreSame(service, result);
    }

    [TestMethod]
    public void GetRequiredServiceTReturnsRegisteredService()
    {
        var provider = new SimpleServiceProvider();
        var service = new TestService();
        provider.AddService(typeof(TestService), service);

        var result = provider.GetRequiredService<TestService>();
        Assert.AreSame(service, result);
    }

    [TestMethod]
    public void GetServicesThrowsOnNullProvider()
    {
        IServiceProvider provider = null!;
        Assert.Throws<ArgumentNullException>(() => provider.GetServices(typeof(TestService)));
    }

    [TestMethod]
    public void GetServicesThrowsOnNullType()
    {
        var provider = new SimpleServiceProvider();
        Assert.Throws<ArgumentNullException>(() => provider.GetServices(null!));
    }

    [TestMethod]
    public void GetServicesReturnsAllRegisteredServices()
    {
        var provider = new SimpleServiceProvider();
        var services = new List<TestService> { new TestService(), new TestService() };
        provider.AddService(typeof(IEnumerable<TestService>), services);

        var result = provider.GetServices(typeof(TestService)).Cast<TestService>().ToList();
        CollectionAssert.AreEqual(services, result);
    }

    [TestMethod]
    public void GetServicesTReturnsAllRegisteredServices()
    {
        var provider = new SimpleServiceProvider();
        var services = new List<TestService> { new TestService(), new TestService() };
        provider.AddService(typeof(IEnumerable<TestService>), services);

        var result = provider.GetServices<TestService>().ToList();
        CollectionAssert.AreEqual(services, result);
    }
    #pragma warning restore CA2263
}
