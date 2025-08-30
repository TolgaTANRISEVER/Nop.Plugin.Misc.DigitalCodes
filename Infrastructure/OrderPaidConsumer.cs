using Microsoft.Extensions.Logging;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.DigitalCodes.Domain;
using Nop.Plugin.Misc.DigitalCodes.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.DigitalCodes.Infrastructure;

/// <summary>
/// Sipariş ödendiğinde dijital kodları ayırıp teslim eden tüketici (idempotent)
/// </summary>
public class OrderPaidConsumer : IConsumer<OrderPaidEvent>
{
    private readonly IRepository<CodeItem> _codeItemRepository;
    private readonly IRepository<CodePool> _codePoolRepository;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly ISettingService _settingService;
    private readonly ILocker _locker;
    private readonly DigitalCodesDeliveryService _deliveryService;
    private readonly ILogger<OrderPaidConsumer> _logger;

    public OrderPaidConsumer(
        IRepository<CodeItem> codeItemRepository,
        IRepository<CodePool> codePoolRepository,
        IOrderService orderService,
        ICustomerService customerService,
        ISettingService settingService,
        ILocker locker,
        DigitalCodesDeliveryService deliveryService,
        ILogger<OrderPaidConsumer> logger)
    {
        _codeItemRepository = codeItemRepository;
        _codePoolRepository = codePoolRepository;
        _orderService = orderService;
        _customerService = customerService;
        _settingService = settingService;
        _locker = locker;
        _deliveryService = deliveryService;
        _logger = logger;
    }

    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        var order = eventMessage.Order;
        if (order == null)
            return;

        // Idempotent kilit
        var lockKey = $"DigitalCodes:Deliver:Order:{order.Id}";
        await _locker.PerformActionWithLockAsync(lockKey, TimeSpan.FromMinutes(3), async () =>
        {
            try
            {
                var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                if (orderItems == null || orderItems.Count == 0)
                    return;

                var orderItemIds = orderItems.Select(oi => oi.Id).ToList();

                // Eğer bu sipariş kalemleri için zaten Delivered kod varsa ikinci kez çalıştırmayalım
                var anyDelivered = await _codeItemRepository.Table
                    .AnyAsync(ci => ci.OrderItemId.HasValue && orderItemIds.Contains(ci.OrderItemId.Value) && ci.Status == (int)CodeItemStatus.Delivered);
                if (anyDelivered)
                {
                    _logger.LogInformation("DigitalCodes: Sipariş {OrderId} için teslimat daha önce tamamlanmış.", order.Id);
                    return;
                }

                // Ürün -> Havuz eşleşmesi olan kalemler için kod ayır
                var reservedCodes = new List<CodeItem>();
                foreach (var item in orderItems)
                {
                    var pool = await _codePoolRepository.Table.FirstOrDefaultAsync(p => p.ProductId == item.ProductId && p.IsActive);
                    if (pool == null)
                        continue;

                    var needCount = item.Quantity;

                    var available = await _codeItemRepository.Table
                        .Where(c => c.CodePoolId == pool.Id && c.Status == (int)CodeItemStatus.Available)
                        .OrderBy(c => c.Id)
                        .Take(needCount)
                        .ToListAsync();

                    if (available.Count < needCount)
                    {
                        _logger.LogWarning("DigitalCodes: Havuzda yeterli kod yok. OrderId={OrderId}, OrderItemId={OrderItemId}, Need={Need}, Found={Found}", order.Id, item.Id, needCount, available.Count);
                        // Yine de bulduklarımızı rezerve edip göndermeyi deneyelim
                    }

                    foreach (var code in available)
                    {
                        code.Status = (int)CodeItemStatus.Reserved;
                        code.OrderItemId = item.Id;
                        code.ReservedUntilUtc = DateTime.UtcNow.AddMinutes(10);
                        code.UpdatedOnUtc = DateTime.UtcNow;
                    }

                    if (available.Count > 0)
                    {
                        await _codeItemRepository.UpdateAsync(available, false);
                        reservedCodes.AddRange(available);
                    }
                }

                if (reservedCodes.Count == 0)
                {
                    _logger.LogInformation("DigitalCodes: Sipariş {OrderId} için ayrılan bir kod yok, teslimat yapılmayacak.", order.Id);
                    return;
                }

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId) ?? new Customer();
                var settings = await _settingService.LoadSettingAsync<DigitalCodesSettings>(order.StoreId);

                // Gönderim: Öncelik Webhook, olmazsa e‑posta (servis sonucu döndürüyor)
                var success = await _deliveryService.DeliverAsync(order, customer, reservedCodes, settings);

                if (!success)
                {
                    _logger.LogWarning("DigitalCodes: OrderId={OrderId} için teslimat başarısız, kodlar rezerve durumda kalacak.", order.Id);
                    return;
                }

                // Başarılı ise kodları Delivered olarak işaretle
                foreach (var code in reservedCodes)
                {
                    code.Status = (int)CodeItemStatus.Delivered;
                    code.ReservedUntilUtc = null;
                    code.UpdatedOnUtc = DateTime.UtcNow;
                }
                await _codeItemRepository.UpdateAsync(reservedCodes, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DigitalCodes: OrderPaid işlemi sırasında hata. OrderId={OrderId}", eventMessage.Order?.Id);
                throw;
            }
        });
    }
}