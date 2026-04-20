using Microsoft.Extensions.Caching.Memory;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Avenue804.Web.Services;

public class TwilioOtpService : ITwilioOtpService
{
    private readonly IConfiguration _cfg;
    private readonly IMemoryCache _cache;
    private readonly ILogger<TwilioOtpService> _log;

    private static readonly TimeSpan OtpTtl = TimeSpan.FromMinutes(10);

    public TwilioOtpService(IConfiguration cfg, IMemoryCache cache, ILogger<TwilioOtpService> log)
    {
        _cfg = cfg;
        _cache = cache;
        _log = log;
    }

    public async Task<bool> SendOtpAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        var sid = _cfg["Auth:Twilio:AccountSid"];
        var token = _cfg["Auth:Twilio:AuthToken"];
        var from = _cfg["Auth:Twilio:FromNumber"];

        if (string.IsNullOrWhiteSpace(sid) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(from))
        {
            _log.LogWarning("Twilio not configured — OTP not sent to {Phone}.", phoneNumber);
            return false;
        }

        var code = Random.Shared.Next(100000, 999999).ToString();
        var cacheKey = OtpCacheKey(phoneNumber);

        _cache.Set(cacheKey, code, OtpTtl);

        TwilioClient.Init(sid, token);
        await MessageResource.CreateAsync(
            to: new Twilio.Types.PhoneNumber(phoneNumber),
            from: new Twilio.Types.PhoneNumber(from),
            body: $"Your 804 Avenue verification code is: {code}. Valid for 10 minutes.");

        _log.LogInformation("OTP sent to {Phone}.", phoneNumber);
        return true;
    }

    public bool VerifyOtp(string phoneNumber, string code)
    {
        var cacheKey = OtpCacheKey(phoneNumber);
        if (_cache.TryGetValue(cacheKey, out string? stored) && stored == code.Trim())
        {
            _cache.Remove(cacheKey);
            return true;
        }
        return false;
    }

    private static string OtpCacheKey(string phone) => $"otp:{phone}";
}
