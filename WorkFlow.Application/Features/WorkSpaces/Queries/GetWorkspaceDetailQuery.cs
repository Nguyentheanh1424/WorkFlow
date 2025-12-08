using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Queries
{
    public record GetWorkspaceDetailQuery(Guid Id) : IRequest<Result<WorkSpaceDto>>;

    public class GetWorkspaceDetailQueryHandler : IRequestHandler<GetWorkspaceDetailQuery, Result<WorkSpaceDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public GetWorkspaceDetailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<WorkSpaceDto>> Handle(GetWorkspaceDetailQuery request, CancellationToken cancellationToken)
        {
            var workspace = await _repository.GetByIdAsync(request.Id);

            if (workspace == null)
                return Result<WorkSpaceDto>.Failure("Workspace không tồn tại.");

            var dto = _mapper.Map<WorkSpaceDto>(workspace);
            return Result<WorkSpaceDto>.Success(dto);
        }
    }
}
