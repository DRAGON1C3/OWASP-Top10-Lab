public sealed class AddBasketItemRequest
{
    public int ProductId { get; init; }
    public int BasketId { get; init; }
    public int Quantity { get; init; }
}

[HttpPost("/api/BasketItems")]
public async Task<IActionResult> AddItem(
    AddBasketItemRequest request,
    CancellationToken cancellationToken)
{
    var currentUserId = User.GetUserId();

    var basket = await dbContext.Baskets
        .SingleOrDefaultAsync(
            basket =>
                basket.Id == request.BasketId &&
                basket.UserId == currentUserId,
            cancellationToken);

    if (basket is null)
    {
        return NotFound();
    }

    basket.Items.Add(new BasketItem
    {
        ProductId = request.ProductId,
        Quantity = request.Quantity
    });

    await dbContext.SaveChangesAsync(cancellationToken);

    return Ok();
}