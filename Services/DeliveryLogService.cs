using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

public class DeliveryLogService : IDeliveryLogService
{
    private readonly IRepository<DeliveryLog> _deliveryLogRepository;

    public DeliveryLogService(IRepository<DeliveryLog> deliveryLogRepository)
    {
        _deliveryLogRepository = deliveryLogRepository;
    }

    public async Task<IPagedList<DeliveryLog>> SearchAsync(int? orderId = null, int? orderItemId = null, int? codeItemId = null, string channel = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _deliveryLogRepository.GetAllPagedAsync(query =>
        {
            if (orderId.HasValue)
                query = query.Where(x => x.OrderId == orderId.Value);
            if (orderItemId.HasValue)
                query = query.Where(x => x.OrderItemId == orderItemId.Value);
            if (codeItemId.HasValue)
                query = query.Where(x => x.CodeItemId == codeItemId.Value);
            if (!string.IsNullOrWhiteSpace(channel))
                query = query.Where(x => x.Channel == channel);

            query = query.OrderByDescending(x => x.CreatedOnUtc).ThenBy(x => x.Id);
            return query;
        }, pageIndex, pageSize);
    }

    public Task<DeliveryLog> GetByIdAsync(int id)
    {
        return _deliveryLogRepository.GetByIdAsync(id);
    }

    public Task InsertAsync(DeliveryLog entity)
    {
        entity.CreatedOnUtc = entity.CreatedOnUtc == default ? DateTime.UtcNow : entity.CreatedOnUtc;
        return _deliveryLogRepository.InsertAsync(entity);
    }
}