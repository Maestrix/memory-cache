using Serializator;

namespace MemoryCache.Models;

[GenerateBinarySerializer]
public partial class UserProfile
{
    public int Id { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsWorker{ get; set; }
}
