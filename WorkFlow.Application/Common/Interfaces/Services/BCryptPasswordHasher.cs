using BCrypt.Net;

namespace WorkFlow.Application.Common.Interfaces.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private const int WorkFactor = 12; // Mức độ bảo mật

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
