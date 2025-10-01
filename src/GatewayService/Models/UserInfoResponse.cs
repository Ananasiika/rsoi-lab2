namespace GatewayService.Models;

public class UserInfoResponse
{
    public List<TicketResponse> Tickets { get; set; } = new();
    public PrivilegeShortInfo Privilege { get; set; } = new();
}