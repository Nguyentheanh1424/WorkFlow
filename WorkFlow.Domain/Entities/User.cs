using WorkFlow.Domain.Common;

namespace WorkFlow.Domain.Entities
{
    public class User : Entity<Guid>
    {
        //
        private const int NameChangeCooldownDays = 1836;

        // 
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;

        public string AvatarUrl { get; private set; } = string.Empty;
        public DateTime? DateOfBirth { get; private set; }

        public DateTime? NameUpdatedAt { get; private set; }

        protected User() { }

        public User(string name, string email, string avatar)
        {
            Name = name;
            Email = email;
            AvatarUrl = avatar;
        }

        public (bool IsSuccess, int RemainingDays) UpdateName(string name)
        {
            if (NameUpdatedAt.HasValue)
            {
                var daysSinceLastUpdate =
                    (DateTime.UtcNow - NameUpdatedAt.Value).TotalDays;

                if (daysSinceLastUpdate <= NameChangeCooldownDays)
                {
                    var remainingDays =
                        (int)Math.Ceiling(NameChangeCooldownDays - daysSinceLastUpdate);

                    return (false, remainingDays);
                }
            }

            Name = name;
            NameUpdatedAt = DateTime.UtcNow;
            return (true, 0);
        }


        public void UpdatePhoneNumber(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void UpdateDateOfBirth(DateTime dateOfBirth)
        {
            DateOfBirth = dateOfBirth.Date;
        }

        public void UpdateAvatar(string avatar)
        {
            AvatarUrl = avatar;
        }
    }
}
