namespace Backend.Interfaces.Utils;

public interface IServiceMethodDispatcher<TService>
{
    public void Dispatch(Func<TService, Task> method);
    public T Dispatch<T>(Func<TService, T> method);
    public Task DispatchAsync(Func<TService, Task> method);
    public Task<T> DispatchAsync<T>(Func<TService, Task<T>> method);
}