using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

public class CodeItemService : ICodeItemService
{
    private readonly IRepository<CodeItem> _codeItemRepository;

    public CodeItemService(IRepository<CodeItem> codeItemRepository)
    {
        _codeItemRepository = codeItemRepository;
    }

    public async Task<IPagedList<CodeItem>> SearchAsync(int? codePoolId = null, int? status = null, int? orderItemId = null, string searchCode = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _codeItemRepository.GetAllPagedAsync(query =>
        {
            if (codePoolId.HasValue)
                query = query.Where(x => x.CodePoolId == codePoolId.Value);
            if (status.HasValue)
                query = query.Where(x => x.Status == status.Value);
            if (orderItemId.HasValue)
                query = query.Where(x => x.OrderItemId == orderItemId.Value);
            if (!string.IsNullOrWhiteSpace(searchCode))
                query = query.Where(x => x.Code.Contains(searchCode));

            query = query.OrderByDescending(x => x.CreatedOnUtc).ThenBy(x => x.Id);
            return query;
        }, pageIndex, pageSize);
    }

    public Task<CodeItem> GetByIdAsync(int codeItemId)
    {
        return _codeItemRepository.GetByIdAsync(codeItemId);
    }

    public Task InsertAsync(CodeItem codeItem)
    {
        codeItem.CreatedOnUtc = DateTime.UtcNow;
        return _codeItemRepository.InsertAsync(codeItem);
    }

    public Task UpdateAsync(CodeItem codeItem)
    {
        codeItem.UpdatedOnUtc = DateTime.UtcNow;
        return _codeItemRepository.UpdateAsync(codeItem);
    }

    public Task DeleteAsync(CodeItem codeItem)
    {
        return _codeItemRepository.DeleteAsync(codeItem);
    }

    public async Task BulkInsertAsync(IList<CodeItem> codeItems)
    {
        foreach (var item in codeItems)
        {
            item.CreatedOnUtc = DateTime.UtcNow;
        }
        await _codeItemRepository.InsertAsync(codeItems);
    }

    public async Task<int> GetAvailableCountAsync(int codePoolId)
    {
        return await _codeItemRepository.Table
            .Where(x => x.CodePoolId == codePoolId && x.Status == (int)CodeItemStatus.Available)
            .CountAsync();
    }

    public async Task<int> GetTotalCountAsync(int codePoolId)
    {
        return await _codeItemRepository.Table
            .Where(x => x.CodePoolId == codePoolId)
            .CountAsync();
    }
}