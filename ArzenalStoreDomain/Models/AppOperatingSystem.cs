using System.Text.Json.Serialization;

namespace Arzenal.Store.Api.Domain.Models
{
    public class AppOperatingSystem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppId { get; set; }

        [JsonIgnore]
        public App App { get; set; } = null!;

        public Guid OSId { get; set; }
        public OperatingSystem OperatingSystem { get; set; } = null!;
    }

}
