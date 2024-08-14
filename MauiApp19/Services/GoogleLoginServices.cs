using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
namespace Services.MauiApp19.Helpers
{
    public class GoogleAuthHelper
    {
        private const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenUrl = "https://oauth2.googleapis.com/token";
        private readonly string _clientId = "218858434869-idj0lukpqiiq89s0qmv3gml6ooeercvq.apps.googleusercontent.com";
        private readonly string _clientSecret = "GOCSPX-GA-cqTFctOZxcf0TgpsdiMtygaG";
        private readonly string _redirectUri = "https://localhost/oauth2redirect";
        public GoogleAuthHelper(string clientId, string clientSecret, string redirectUri)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
            _redirectUri = redirectUri;
        }
        public async Task<(string idToken, string scope)> Authenticate()
        {
            var state = Guid.NewGuid().ToString("N");
            var authorizationRequest = new Uri($"{AuthorizeUrl}?response_type=code&client_id={_clientId}&redirect_uri={Uri.EscapeDataString(_redirectUri)}&scope=openid%20profile%20email&state={state}");
            var result = await WebAuthenticator.AuthenticateAsync(authorizationRequest, new Uri(_redirectUri));
            if (result.Properties.TryGetValue("code", out var code))
            {
                var tokenResult = await GetTokenAsync(code);
                return tokenResult;
            }
            throw new Exception("Kimlik doğrulama başarısız oldu.");
        }
        private async Task<(string idToken, string scope)> GetTokenAsync(string code)
        {
            using var client = new HttpClient();
            var response = await client.PostAsync(TokenUrl, new FormUrlEncodedContent(new Dictionary<string, string>
    {
        {"code", code},
        {"client_id", _clientId},
        {"client_secret", _clientSecret},
        {"redirect_uri", _redirectUri},
        {"grant_type", "authorization_code"}
    }));
            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var idToken = tokenResponse.GetProperty("id_token").GetString();
            var scope = tokenResponse.GetProperty("scope").GetString();
            return (idToken, scope);
        }
        public static GoogleUserInfo GetUserInfoFromToken(string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(idToken) as JwtSecurityToken;
            return new GoogleUserInfo
            {
                Email = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value,
                Name = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value,
                Picture = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "picture")?.Value
            };
        }
    }
    public class GoogleUserInfo
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
    }
}