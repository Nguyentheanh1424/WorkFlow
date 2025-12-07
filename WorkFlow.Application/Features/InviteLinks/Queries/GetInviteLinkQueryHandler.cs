using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.InviteLinks.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.InviteLinks.Queries
{
    public class GetInviteLinkQueryHandler
        : IRequestHandler<GetInviteLinkQuery, Result<InviteLinkDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<InviteLink, Guid> _repository;
        private readonly IMapper _mapper;

        public GetInviteLinkQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<InviteLink, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<InviteLinkDto>> Handle(GetInviteLinkQuery request, CancellationToken cancellationToken)
        {
            var entity = (await _repository.FindAsync(x => x.Token == request.Token)).FirstOrDefault();

            if (entity == null)
                return Result<InviteLinkDto>.Failure("Link không tồn tại.");

            return Result<InviteLinkDto>.Success(_mapper.Map<InviteLinkDto>(entity));
        }
    }
}
