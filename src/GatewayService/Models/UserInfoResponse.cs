namespace GatewayService.Models;

public class UserInfoResponse
{
    public List<TicketResponse> Tickets { get; set; } = new();
    public UserPrivilegeInfo Privilege { get; set; } = new();
}

public class UserPrivilegeInfo
{
    public int Balance { get; set; }
    public string Status { get; set; } = string.Empty;
}