namespace UpgradePortal.Web.Models;

public class Customer
{
    public long CustomerId { get; set; }
    public string CustomerCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string? PrimaryEmail { get; set; }
    public string? RegionCode { get; set; }
}