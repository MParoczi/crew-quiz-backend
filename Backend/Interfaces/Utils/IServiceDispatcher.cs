using Backend.Interfaces.Services;

namespace Backend.Interfaces.Utils;

public interface IServiceDispatcher
{
    public IServiceMethodDispatcher<TService> For<TService>() where TService : IServiceBase;
}