using Nop.Core.Domain.Messages;
using Nop.Core.Events;
using Nop.Services.Events;
using Nop.Plugin.Misc.DigitalCodes.Infrastructure;

namespace Nop.Plugin.Misc.DigitalCodes.Services;

/// <summary>
/// Allowed token listesini genişletmek için tüketici
/// </summary>
public class TokenEventConsumer : IConsumer<AdditionalTokensAddedEvent>
{
    public Task HandleEventAsync(AdditionalTokensAddedEvent eventMessage)
    {
        // Özel token'ı her durumda kullanılabilir yap
        eventMessage.AddTokens(DigitalCodesDefaults.ItemsToken);
        return Task.CompletedTask;
    }
}