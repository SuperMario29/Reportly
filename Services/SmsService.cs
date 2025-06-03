using System;
using System.Threading.Tasks;
using Reportly;
using SQLitePCL;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

public class SmsService
{
    private readonly string _accountSid;
    private readonly string _authToken;
    private readonly string _twilioPhoneNumber;

    private readonly string _connectionString;

    public SmsService(string accountSid, string authToken, string twilioPhoneNumber, string connectionString)
    {
        _accountSid = accountSid;
        _authToken = authToken;
        _twilioPhoneNumber = twilioPhoneNumber;
        _connectionString = connectionString;
    }

    public async Task<bool> Send2FaCode(string username)
    {
        var auth = new AuthService(_connectionString);
        var user = auth.GetUserByUsername(username);
        if (user == null || !user.TwoFactorEnabled) return false;

        var code = new Random().Next(100000, 999999).ToString();
        user.TwoFactorSecret = code; // In production, use TOTP instead

        auth.UpdateUserTwoFactor(user);

        await Send2FaCode(user.PhoneNumber, code);
        return true;
    }

    // Verify 2FA code
    public bool Verify2Fa(string username, string code)
    {
        var auth = new AuthService(_connectionString);
        var user = auth.GetUserByUsername(username);
        return user?.TwoFactorSecret == code;
    }

    public async Task Send2FaCode(string phoneNumber, string code)
    {
        await MessageResource.CreateAsync(
            to: new PhoneNumber(phoneNumber),
            from: new PhoneNumber(_twilioPhoneNumber),
            body: $"Your verification code: {code}"
        );
    }
}