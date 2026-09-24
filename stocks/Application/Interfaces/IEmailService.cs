interface IEmailService
{
    Task SendEmailAsync(string subject, string body);
}