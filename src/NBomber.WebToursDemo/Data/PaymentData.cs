namespace NBomber.WebToursDemo.Data;

public sealed record PaymentData(
    string FirstName,
    string LastName,
    string Address1,
    string Address2,
    string CreditCard,
    string ExpDate)
{
    public string PassengerName => $"{FirstName} {LastName}";
}
