
namespace TollBooth.Models;

public class LicensePlateProcessedEvent<T> where T : class
{
    public string Id { get; set; }
    public string Subject { get; set; }
    public string EventType { get; set; }
    public T Data { get; set; }
    public DateTime EventTime { get; set; }
}
