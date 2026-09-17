using System;
using System.Collections.Generic;

namespace RuntimeInvestigation.Domain.Entities;

public class InvestigationTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<ProbeTemplate> ProbeTemplates { get; set; } = new();

    public InvestigationTemplate() { }

    public InvestigationTemplate(string name, string description, string category, IEnumerable<ProbeTemplate>? probes = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        Id = Guid.NewGuid().ToString("N");
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        Category = category?.Trim() ?? "General";
        if (probes != null)
        {
            ProbeTemplates.AddRange(probes);
        }
    }
}
