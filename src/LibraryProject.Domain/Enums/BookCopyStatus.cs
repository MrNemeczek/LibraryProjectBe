namespace LibraryProject.Domain.Enums;

/// <summary>
/// Describes the current availability state of a physical book copy.
/// </summary>
public enum BookCopyStatus
{
    /// <summary>
    /// The copy is available and can be borrowed.
    /// </summary>
    Available = 1,

    /// <summary>
    /// The copy is reserved and waiting for pickup.
    /// </summary>
    Reserved = 2,

    /// <summary>
    /// The copy is currently borrowed by a reader.
    /// </summary>
    Borrowed = 3,

    /// <summary>
    /// The copy has been withdrawn from circulation.
    /// </summary>
    Withdrawn = 4
}
