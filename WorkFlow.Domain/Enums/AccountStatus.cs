using System.ComponentModel;

namespace WorkFlow.Domain.Enums
{
    public enum AccountStatus
    {
        [Description("Đã kích hoạt")]
        Actived = 0,

        [Description("Bị khóa")]
        Locked = 1,

        [Description("Bị cấm vĩnh viễn")]
        Banned = 2,

        [Description("Đã xóa")]
        Deleted = 3,
    }
}
