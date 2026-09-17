using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Application.Features.Templates;

public interface ITemplateRepository
{
    Task<InvestigationTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<InvestigationTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task CreateAsync(InvestigationTemplate template, CancellationToken cancellationToken = default);
}
