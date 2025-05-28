using System.Text.Json.Serialization;

namespace ArzenalStoreApi.Models
{
    public class AppLanguage
    {
        public Guid AppId { get; set; }

        [JsonIgnore]
        public App App { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }
    }

}
