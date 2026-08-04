#nullable enable

namespace eShop.Basket.API.Extensions;

internal static class ServerCallContextIdentityExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.GetUserId();
    public static string? GetUserName(this ServerCallContext context) => context.GetHttpContext().User.GetUserName();
}
