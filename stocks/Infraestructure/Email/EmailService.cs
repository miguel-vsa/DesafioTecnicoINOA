using MailKit.Net.Smtp;
using MimeKit;

class EmailService : IEmailService
{
    private readonly EmailConfiguration _configuration;

    public EmailService(EmailConfiguration configuration)
    {
        _configuration = configuration;
    }

        public async Task SendEmailAsync(string subject, string body)
    {
        MimeMessage message = new MimeMessage();

        message.From.Add(MailboxAddress.Parse(_configuration.Username));
        message.To.Add(MailboxAddress.Parse(_configuration.Recipient));

        message.Subject = subject;

        message.Body = new TextPart("plain")
        {
            Text = body
        };

        using SmtpClient client = new SmtpClient();

        await client.ConnectAsync(
            _configuration.SmtpServer,
            _configuration.SmtpPort,
            MailKit.Security.SecureSocketOptions.StartTls
        );

        await client.AuthenticateAsync(
            _configuration.Username,
            _configuration.Password
        );

        await client.SendAsync(message);

        await client.DisconnectAsync(true);
    }
}