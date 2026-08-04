using MediatR;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.CreateDepartment;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, Result<Guid>>
{
    private readonly IDepartmentRepository _departmentRepository;
    public CreateDepartmentCommandHandler(IDepartmentRepository departmentRepository) =>
        _departmentRepository = departmentRepository;

    public async Task<Result<Guid>> Handle(CreateDepartmentCommand request, CancellationToken ct)
    {
        var existing = await _departmentRepository.GetByClinicAndNameAsync(request.ClinicId, request.Name, ct);
        if (existing is not null)
            return Result<Guid>.Failure("A department with this name already exists at this clinic.");

        try
        {
            var department = Department.Create(request.ClinicId, request.Name, request.Description);
            await _departmentRepository.AddAsync(department, ct);
            await _departmentRepository.SaveChangesAsync(ct);
            return Result<Guid>.Success(department.Id);
        }
        catch (ArgumentException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}