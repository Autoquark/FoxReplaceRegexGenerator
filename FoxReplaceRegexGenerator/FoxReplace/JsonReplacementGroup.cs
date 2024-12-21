using System.Text.Json;
using System.Text.Json.Serialization;

class JsonReplacementGroup
{
    public string Name {get; set;} = "";
    public List<string> Urls {get; set;} = [];
    public List<JsonSubstitution> Substitutions {get; set;} = [];
    public ReplacementGroupHtmlType Html {get; set;} = ReplacementGroupHtmlType.None;
    public bool Enabled {get; set;} = true;
    public ReplacementGroupMode Mode {get; set;} = ReplacementGroupMode.AutoAndManual;
}

enum ReplacementGroupHtmlType
{
    None,
    Output,
    InputOutput
}

enum ReplacementGroupMode
{
    Auto,
    Manual,
    [JsonStringEnumMemberName("auto&manual")]
    AutoAndManual
}