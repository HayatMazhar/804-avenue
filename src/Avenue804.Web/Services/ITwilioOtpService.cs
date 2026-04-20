namespace Avenue804.Web.Services;

public interface ITwilioOtpService
{
    Task<bool> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default);
    bool VerifyOtp(string phoneNumber, string code);
}
