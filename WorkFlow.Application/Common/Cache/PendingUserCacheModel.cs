namespace WorkFlow.Application.Common.Cache
{
    public class PendingUserCacheModel : CacheModelBase
    {
        public string Email { get; private set; }
        public string Name { get; private set; }
        public string PasswordHash { get; private set; }
        public string Salt { get; private set; }


        public PendingUserCacheModel(string email, string name, string passwordHash, string salt, TimeSpan ttl)
            : base($"pending-user:{email}", absoluteTtl: ttl)
        {
            Email = email;
            Name = name;
            PasswordHash = passwordHash;
            Salt = salt;
        }
    }
}
