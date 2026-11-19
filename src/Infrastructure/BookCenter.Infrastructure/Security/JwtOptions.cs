namespace BookCenter.Infrastructure.Security
{
    /// <summary>Bound from the "Jwt" configuration section.</summary>
    public class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "BookCenter";
        public string Audience { get; set; } = "BookCenter.Client";

        /// <summary>HMAC-SHA256 signing secret, at least 32 characters. Never commit a production key.</summary>
        public string Key { get; set; } = "";

        public int ExpiryMinutes { get; set; } = 120;
    }
}
