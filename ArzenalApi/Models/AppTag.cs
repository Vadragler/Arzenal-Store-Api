using System.Text.Json.Serialization;

namespace ArzenalApi.Models
{
    public class AppTag
    {
        public Guid AppId { get; set; }

        [JsonIgnore]
        public App App { get; set; } = null!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }

}
