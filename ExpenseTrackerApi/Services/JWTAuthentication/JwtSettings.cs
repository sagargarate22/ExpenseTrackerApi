﻿namespace ExpenseTrackerApi.Services.JWTAuthentication
{
    public class JwtSettings
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public string SecretKey { get; set; }
    }
}
