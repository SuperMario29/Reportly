using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class CaptchaService
{
    private readonly string _secretKey;
    private readonly HttpClient _httpClient;

    public CaptchaService(string secretKey)
    {
        _secretKey = secretKey;
        _httpClient = new HttpClient();
    }

    public async Task<bool> VerifyCaptcha(string token)
    {
        var response = await _httpClient.PostAsync(
            $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={token}",
            null);

        var result = JsonConvert.DeserializeObject<CaptchaResponse>(
            await response.Content.ReadAsStringAsync());

        return result.Success && result.Score > 0.5;
    }

    private class CaptchaResponse
    {
        public bool Success { get; set; }
        public float Score { get; set; }
    }
}