namespace ASCTracInterfaceService.Filters
{
    internal interface IUserServices
    {
        int Authenticate(string token, string param);
    }
}