using Nop.Core;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

public interface ICodePoolService
{
    Task<IPagedList<CodePool>> SearchAsync(string name = null, int? productId = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<CodePool> GetByIdAsync(int id);

    Task InsertAsync(CodePool entity);

    Task UpdateAsync(CodePool entity);

    Task DeleteAsync(CodePool entity);
}