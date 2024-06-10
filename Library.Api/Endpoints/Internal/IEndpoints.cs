namespace Library.Api.Endpoints.Internal;

public interface IEndpoints
{
    static abstract void DefineEndpoints(IEndpointRouteBuilder app);
    static abstract void AddServices(IServiceCollection services, IConfiguration configuration);
}