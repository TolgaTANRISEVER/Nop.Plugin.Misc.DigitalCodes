using Nop.Core;
using Nop.Plugin.Misc.DigitalCodes.Domain;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

/// <summary>
/// CodeItem yönetimi için servis arayüzü
/// </summary>
public interface ICodeItemService
{
    /// <summary>
    /// Kod kalemi ara (filtreleme ve sayfalama ile)
    /// </summary>
    /// <param name="codePoolId">Havuz ID filtresi</param>
    /// <param name="status">Durum filtresi</param>
    /// <param name="orderItemId">Sipariş kalemi ID filtresi</param>
    /// <param name="searchCode">Kod arama terimi</param>
    /// <param name="pageIndex">Sayfa indeksi</param>
    /// <param name="pageSize">Sayfa boyutu</param>
    /// <returns>Bulunan kod kalemleri</returns>
    Task<IPagedList<CodeItem>> SearchAsync(int? codePoolId = null, int? status = null, int? orderItemId = null, string searchCode = null, int pageIndex = 0, int pageSize = int.MaxValue);

    /// <summary>
    /// ID ile kod kalemi getir
    /// </summary>
    /// <param name="codeItemId">Kod kalemi ID</param>
    /// <returns>Kod kalemi</returns>
    Task<CodeItem> GetByIdAsync(int codeItemId);

    /// <summary>
    /// Kod kalemi ekle
    /// </summary>
    /// <param name="codeItem">Kod kalemi</param>
    Task InsertAsync(CodeItem codeItem);

    /// <summary>
    /// Kod kalemi güncelle
    /// </summary>
    /// <param name="codeItem">Kod kalemi</param>
    Task UpdateAsync(CodeItem codeItem);

    /// <summary>
    /// Kod kalemi sil
    /// </summary>
    /// <param name="codeItem">Kod kalemi</param>
    Task DeleteAsync(CodeItem codeItem);

    /// <summary>
    /// Kod kalemleri çoklu ekleme (CSV import için)
    /// </summary>
    /// <param name="codeItems">Kod kalemleri</param>
    Task BulkInsertAsync(IList<CodeItem> codeItems);

    /// <summary>
    /// Belirli havuzdaki kullanılabilir kod sayısını getir
    /// </summary>
    /// <param name="codePoolId">Havuz ID</param>
    /// <returns>Kullanılabilir kod sayısı</returns>
    Task<int> GetAvailableCountAsync(int codePoolId);

    /// <summary>
    /// Belirli havuzdaki toplam kod sayısını getir
    /// </summary>
    /// <param name="codePoolId">Havuz ID</param>
    /// <returns>Toplam kod sayısı</returns>
    Task<int> GetTotalCountAsync(int codePoolId);
}