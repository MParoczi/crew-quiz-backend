using System.Net;
using Backend.Data;

namespace Backend.Middlewares;

public class DatabaseTransactionMiddleware
{
    private readonly RequestDelegate _next;

    public DatabaseTransactionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, CrewQuizContext context)
    {
        try
        {
            // Start the transaction
            await context.Database.BeginTransactionAsync();

            await _next(httpContext); // Continue processing the request

            // Only commit if the response code is OK (2xx)
            if (httpContext.Response.StatusCode < (int)HttpStatusCode.BadRequest)
                await context.Database.CommitTransactionAsync();
            else
                await context.Database.RollbackTransactionAsync();
        }
        catch (Exception)
        {
            // Rollback the transaction in case of any error
            if (context.Database.CurrentTransaction != null) await context.Database.RollbackTransactionAsync();
            throw;
        }
        finally
        {
            // Ensure the transaction is disposed properly
            if (context.Database.CurrentTransaction != null) await context.Database.CurrentTransaction.DisposeAsync();
        }
    }
}