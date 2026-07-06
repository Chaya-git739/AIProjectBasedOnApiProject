namespace BffService.Services;

public interface IProductClient
{
    Task<HttpClientResult<CatalogServiceContract.GiftDto>> GetGiftAsync(int giftId, string correlationId, CancellationToken ct);
}
