using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Books.Exceptions;

public sealed class BookCopyInventoryNumberAlreadyExistsException(string inventoryNumber)
    : ApplicationExceptionBase(
        "BOOKCOPY_INVENTORY_NUMBER_ALREADY_EXISTS",
        $"Book copy with inventory number '{inventoryNumber}' already exists.",
        (int)HttpStatusCode.Conflict,
        new Dictionary<string, string[]>
        {
            [nameof(inventoryNumber)] = ["Inventory number is already assigned to another book copy."]
        })
{
    public string InventoryNumber { get; } = inventoryNumber;
}
