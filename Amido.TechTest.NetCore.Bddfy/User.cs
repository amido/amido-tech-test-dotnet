using Newtonsoft.Json;

namespace Amido.TechTest.NetCore.Bddfy
{
    public class User
    {
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
