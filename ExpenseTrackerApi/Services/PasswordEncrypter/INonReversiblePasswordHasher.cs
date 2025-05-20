namespace ExpenseTrackerApi.Services.PasswordEncrypter
{
    public interface INonReversiblePasswordHasher
    {
        public string ComputeHash(string password, string salt, string pepper, int iteration = 2);

        public string GenerateSalt();
    }
}
