using System.Text.Json.Serialization;

namespace ArzenalApi.Models
{
    public class AppOperatingSystem
    {
        public Guid AppId { get; set; }

        [JsonIgnore]
        public App App { get; set; } = null!;

        public int OSId { get; set; }
        public OperatingSystem OperatingSystem { get; set; } = null!;
    }

}
