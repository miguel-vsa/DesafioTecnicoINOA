class EmailConfiguration
{
    public string Recipient { get; set; } = "";
    public string SmtpServer { get; set; } = "";
    public int SmtpPort { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Recipient)
            && !string.IsNullOrWhiteSpace(SmtpServer)
            && SmtpPort > 0
            && !string.IsNullOrWhiteSpace(Username)
            && !string.IsNullOrWhiteSpace(Password);
    }
}