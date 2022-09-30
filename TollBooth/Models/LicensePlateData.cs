
namespace TollBooth.Models;

public class LicensePlateData
{
    public string FileName { get; set; }
    public string LicensePlateText { get; set; }
    public DateTime TimeStamp { get; set; }
    public bool LicensePlateFound => !string.IsNullOrWhiteSpace( this.LicensePlateText );
}
