using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

public class CodePoolService : ICodePoolService
{
    private readonly IRepository<CodePool> _codePoolRepository;

    public CodePoolService(IRepository<CodePool> codePoolRepository)
    {
        _codePoolRepository = codePoolRepository;
    }

    public async Task<IPagedList<CodePool>> SearchAsync(string name = null, int? productId = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _codePoolRepository.GetAllPagedAsync(query =>
        {
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(x => x.Name.Contains(name));
            if (productId.HasValue)
                query = query.Where(x => x.ProductId == productId.Value);

            query = query.OrderByDescending(x => x.CreatedOnUtc).ThenBy(x => x.Id);
            return query;
        }, pageIndex, pageSize);
    }

    public Task<CodePool> GetByIdAsync(int id)
    {
        return _codePoolRepository.GetByIdAsync(id);
    }

    public Task InsertAsync(CodePool entity)
    {
        entity.CreatedOnUtc = DateTime.UtcNow;
        return _codePoolRepository.InsertAsync(entity);
    }

    public Task UpdateAsync(CodePool entity)
    {
        entity.UpdatedOnUtc = DateTime.UtcNow;
        return _codePoolRepository.UpdateAsync(entity);
    }

    public Task DeleteAsync(CodePool entity)
    {
        return _codePoolRepository.DeleteAsync(entity);
    }
}