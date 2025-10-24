using AutoMapper;
using MediatR;
using WorkFlow.Application.Common.Interfaces.Repository;
using WorkFlow.Application.Features.Workflows.Dto;
using WorkFlow.Domain.Entities;

namespace WorkFlow.Application.Features.Workflows.Queries
{
    public record GetWorkflowByIdQuery(Guid Id) : IRequest<WorkflowDto>;

    public class GetWorkflowByIdHandler : IRequestHandler<GetWorkflowByIdQuery, WorkflowDto>
    {
        private readonly IRepository<Workflow, Guid> _workflowRepository;
        private readonly IMapper _mapper;

        public GetWorkflowByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _workflowRepository = unitOfWork.GetRepository<Workflow, Guid>();
            _mapper = mapper;
        }

        public async Task<WorkflowDto> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
        {
            var entity = await _workflowRepository.GetByIdAsync(request.Id)
                ?? throw new KeyNotFoundException($"Workflow with Id {request.Id} not found.");

            return _mapper.Map<WorkflowDto>(entity);
        }
    }
}
