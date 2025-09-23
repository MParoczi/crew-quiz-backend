using Backend.Interfaces.Services;
using Backend.Interfaces.Utils;

namespace Backend.Utils;

public class ServiceMethodDispatcher<TService> : IServiceMethodDispatcher<TService> where TService : IServiceBase
{
    private readonly TService _service;

    public ServiceMethodDispatcher(TService service)
    {
        _service = service;
    }

    public void Dispatch(Func<TService, Task> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        method(_service);
    }

    public T Dispatch<T>(Func<TService, T> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        var result = method(_service);
        return result;
    }

    public async Task DispatchAsync(Func<TService, Task> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        await method(_service);
    }

    public async Task<T> DispatchAsync<T>(Func<TService, Task<T>> method)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));

        var result = await method(_service);
        return result;
    }
}