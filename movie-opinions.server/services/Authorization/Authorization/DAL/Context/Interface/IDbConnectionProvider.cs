namespace Authorization.DAL.Context.Interface
{
    public interface IDbConnectionProvider
    {
        string GetConnectionString();
    }
}
