using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RuntimeInvestigation.Application.Features.Templates;
using RuntimeInvestigation.Domain.Entities;

namespace RuntimeInvestigation.Infrastructure.Persistence.Repositories;

public sealed class InMemoryTemplateRepository : ITemplateRepository
{
    private readonly ConcurrentDictionary<string, InvestigationTemplate> _templates = new();

    public Task<InvestigationTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        _templates.TryGetValue(id, out var template);
        return Task.FromResult(template);
    }

    public Task<IReadOnlyList<InvestigationTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = _templates.Values.OrderBy(t => t.Name).ToArray();
        return Task.FromResult<IReadOnlyList<InvestigationTemplate>>(list);
    }

    public Task CreateAsync(InvestigationTemplate template, CancellationToken cancellationToken = default)
    {
        _templates[template.Id] = template;
        return Task.CompletedTask;
    }

    public void Clear() => _templates.Clear();
}
