using Nop.Core;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

/// <summary>
/// DeliveryLog sorgulama için servis arayüzü (CRUD gerekmiyor, salt okunur)
/// </summary>
public interface IDeliveryLogService
{
    /// <summary>
    /// Teslimat loglarını ara
    /// </summary>
    /// <param name="orderId">Sipariş ID</param>
    /// <param name="orderItemId">Sipariş kalemi ID</param>
    /// <param name="codeItemId">Kod kalemi ID</param>
    /// <param name="channel">Kanal (Email/Webhook)</param>
    /// <param name="pageIndex">Sayfa indeksi</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    Task<IPagedList<DeliveryLog>> SearchAsync(int? orderId = null, int? orderItemId = null, int? codeItemId = null, string channel = null, int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// ID ile getir
    /// </summary>
    Task<DeliveryLog> GetByIdAsync(int id);

    /// <summary>
    /// Manuel log eklemek gerekirse (opsiyonel)
    /// </summary>
    Task InsertAsync(DeliveryLog entity);
}