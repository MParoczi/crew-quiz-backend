using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;

namespace Backend.Utils;

public class ServiceDispatcher : IServiceDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IServiceMethodDispatcher<TService> For<TService>() where TService : IServiceBase
    {
        var serviceType = typeof(TService);

        var implementations = _serviceProvider.GetServices(serviceType).ToList();

        if (implementations.Count == 0) throw new InvalidOperationException($"No implementation found for {serviceType}");

        var concreteService = implementations.FirstOrDefault();

        var dispatcher = (IServiceMethodDispatcher<TService>)Activator.CreateInstance(
            typeof(ServiceMethodDispatcher<>).MakeGenericType(serviceType), concreteService)!;

        return dispatcher;
    }
}