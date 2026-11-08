using eBike.Application.Common.Interfaces;
using eBike.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace eBike.Application.Common.Helpers;

public static class SisterConcernFamilyHelper
{
    /// <summary>
    /// Gets connected sister concerns (family tree) excluding the current concern itself,
    /// precisely matching legacy MCSRMIMSWEB GetFamilyTree logic:
    /// - If concern has ParentID > 0, family is Parent + siblings (excluding current concern)
    /// - If concern has ParentID == 0, family is current concern + all its child branches (excluding current concern)
    /// </summary>
    public static async Task<List<SisterConcern>> GetConnectedFamilyConcernsAsync(
        IApplicationDbContext context, 
        int currentConcernId, 
        CancellationToken cancellationToken = default)
    {
        var currentConcern = await context.SisterConcerns
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.ConcernID == currentConcernId, cancellationToken);

        if (currentConcern == null)
        {
            return new List<SisterConcern>();
        }

        int rootParentId = currentConcern.ParentID > 0 ? currentConcern.ParentID : currentConcern.ConcernID;

        return await context.SisterConcerns
            .AsNoTracking()
            .Where(sc => (sc.ConcernID == rootParentId || sc.ParentID == rootParentId) && sc.ConcernID != currentConcernId)
            .ToListAsync(cancellationToken);
    }
}
