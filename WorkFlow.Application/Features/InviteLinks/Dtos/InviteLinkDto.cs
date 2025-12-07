using AutoMapper;
using WorkFlow.Domain.Entities;
using WorkFlow.Domain.Enums;
using WorkFlow.Application.Common.Mappings;

public class InviteLinkDto : IMapFrom<InviteLink>
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public InviteLinkType Type { get; set; }
    public InviteLinkStatus Status { get; set; }
    public Guid TargetId { get; set; }
    public DateTime? ExpiredAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<InviteLink, InviteLinkDto>()
               .ForMember(dest => dest.TargetId,
                          opt => opt.MapFrom(src => src.WorkSpaceId ?? src.BoardId ?? Guid.Empty));
    }
}
