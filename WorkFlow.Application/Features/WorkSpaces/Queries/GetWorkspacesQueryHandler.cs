using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.WorkSpaces.Dtos;
using WorkFlow.Domain.Common;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.WorkSpaces.Queries
{
    public class GetWorkspacesQueryHandler : IRequestHandler<GetWorkspacesQuery, Result<List<WorkSpaceDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<WorkSpace, Guid> _repository;
        private readonly IMapper _mapper;

        public GetWorkspacesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _repository = _unitOfWork.GetRepository<WorkSpace, Guid>();
            _mapper = mapper;
        }

        public async Task<Result<List<WorkSpaceDto>>> Handle(GetWorkspacesQuery request, CancellationToken cancellationToken)
        {
            var query = _repository.GetAll();

            // Support search by name
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(w => w.Name.Contains(request.Search));
            }

            var workspaces = await _repository.GetAllAsync();
            
            // Apply search filter if needed
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                workspaces = workspaces
                    .Where(w => w.Name.Contains(request.Search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var dtos = _mapper.Map<List<WorkSpaceDto>>(workspaces);
            return Result<List<WorkSpaceDto>>.Success(dtos);
        }
    }
}
