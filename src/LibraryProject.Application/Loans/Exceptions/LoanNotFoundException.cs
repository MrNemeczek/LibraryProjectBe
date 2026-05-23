using System.Net;
using LibraryProject.Application.Common.Exceptions;

namespace LibraryProject.Application.Loans.Exceptions;

public sealed class LoanNotFoundException(int id) : ApplicationExceptionBase(
    "LOAN_NOT_FOUND", "Loan was not found.",
    (int)HttpStatusCode.NotFound,
    new Dictionary<string, string[]> { [nameof(id)] = ["Loan does not exist."] })
{
    public int Id { get; } = id;
}
