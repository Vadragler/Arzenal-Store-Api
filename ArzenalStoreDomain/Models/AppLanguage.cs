using System.Text.Json.Serialization;

namespace Arzenal.Store.Api.Domain.Models
{
    public class AppLanguage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid AppId { get; set; }

        [JsonIgnore]
        public App App { get; set; }

        public Guid LanguageId { get; set; }
        public Language Language { get; set; }
    }

}
