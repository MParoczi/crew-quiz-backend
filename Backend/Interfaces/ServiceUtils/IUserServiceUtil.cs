namespace Backend.Interfaces.ServiceUtils;

public interface IUserServiceUtil : IServiceUtilBase
{
    public string HashPassword(string passwordMd5);
}