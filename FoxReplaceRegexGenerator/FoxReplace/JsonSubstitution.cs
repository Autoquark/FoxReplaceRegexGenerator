public record class JsonSubstitution
{
    public required string Input {get; set;} = "";
    public string InputType {get; set;} = "regexp";

    public required string Output {get; set;} = "";
    public SubstitutionOutputType OutputType {get; set;} = SubstitutionOutputType.Text;

    public bool CaseSensitive {get; set;} = true;
}

public enum SubstitutionOutputType
{
    Text,
    Function
}