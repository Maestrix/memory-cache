namespace MemoryCache.Models;

public class UserProfile
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsWorker{ get; set; }
}
