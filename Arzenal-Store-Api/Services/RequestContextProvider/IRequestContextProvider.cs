namespace ArzenalStoreApi.Services.RequestContextProvider
{
    public interface IRequestContextProvider
    {
        ClientContext Get(HttpContext http);
    }
}
