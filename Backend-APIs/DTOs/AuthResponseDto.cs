namespace Backend_APIs.DTOs
{
    using System.Text.Json.Serialization;

    public class AuthResponseDto
    {
        // Access token string
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; } = null!;
        // New refresh token string
        [JsonPropertyName("refreshToken")]
        public string? RefreshToken { get; set; }
        [JsonPropertyName("user")]
        public UserDto User { get; set; } = null!;
    }
}
