using MediatR;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Tenancy.Queries.GetDepartmentsByClinic;

public class GetDepartmentsByClinicQueryHandler : IRequestHandler<GetDepartmentsByClinicQuery, IReadOnlyList<DepartmentDto>>
{
    private readonly IDepartmentRepository _departmentRepository;
    public GetDepartmentsByClinicQueryHandler(IDepartmentRepository departmentRepository) =>
        _departmentRepository = departmentRepository;

    public async Task<IReadOnlyList<DepartmentDto>> Handle(GetDepartmentsByClinicQuery request, CancellationToken ct)
    {
        var departments = await _departmentRepository.GetByClinicAsync(request.ClinicId, ct);
        return departments.Select(d => new DepartmentDto(d.Id, d.Name, d.Description)).ToList();
    }
}
