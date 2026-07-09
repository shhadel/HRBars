namespace HRBars.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendCredentialsAsync(string email, string fullName, string password);
    }
}