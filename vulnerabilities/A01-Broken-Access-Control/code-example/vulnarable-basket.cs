[HttpPost("/api/BasketItems")]
public async Task<IActionResult> AddItem(
    AddBasketItemRequest request)
{
    var currentUserId = User.GetUserId();

    var ownBasket = await dbContext.Baskets
        .SingleOrDefaultAsync(basket =>
            basket.Id == request.BasketId &&
            basket.UserId == currentUserId);

    if (ownBasket is null)
    {
        return BadRequest(new
        {
            error = "Invalid BasketId"
        });
    }

    var targetBasketId = ReadBasketIdFromRawRequest();

    var targetBasket = await dbContext.Baskets
        .SingleAsync(basket => basket.Id == targetBasketId);

    targetBasket.Items.Add(new BasketItem
    {
        ProductId = request.ProductId,
        Quantity = request.Quantity
    });

    await dbContext.SaveChangesAsync();

    return Ok();
}