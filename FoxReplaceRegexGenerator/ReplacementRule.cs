record ReplacementRule
{

    public ReplacementRule(string input, string output)
    {
        Input = input;
        Output = output;
        InputPlural = Input + "s";
        OutputPlural = Output + "s";
    }

    public string Input { get; set; }
    public string Output { get; set; }
    public bool WholeWords { get; set; } = true;
    
    /// <summary>
    /// If neither is null or empty, generated rules will also replace InputPlural with OutputPlural. Defaults to Input + "s"
    /// </summary>
    public string InputPlural { get; set; }
    /// <summary>
    /// If neither is null or empty, generated rules will also replace InputPlural with OutputPlural. Defaults to Output + "s"
    /// </summary>
    public string OutputPlural { get; set; }
    public CapitalisationHandling CapitalisationHandling {get; set;} = CapitalisationHandling.Preserve;

    /// <summary>
    /// If true, two rules will be generated, one that replaces the input with a UID and another with later ordering that replaces the UID with the final result. This ensures that the output of one rule cannot be modified by other rules.
    /// </summary>
    public bool ReplaceViaUid { get; set; } = true;
    /// <summary>
    /// If true, will also replace the output with the input. Requires ReplaceViaUid, otherwise the second replacement will undo the first
    /// </summary>
    public bool Bidirectional { get; set; } = false;

}

public enum CapitalisationHandling
{
    /// <summary>
    /// Match any input capitalisation, output with capitalisation as given in the rule
    /// </summary>
    Ignore,
    /// <summary>
    /// Match only the input capitalisatino specified in the rule, output with capitalisation as given in the rule
    /// </summary>
    MatchExact,
    /// <summary>
    /// Match any input capitalisation, attempt output with matching capitalisation with the following cases in priority order:
    /// 1. all uppercase (if input all uppercase)
    /// 2. first letter only uppercase (else if input starts with uppercase)
    /// 3. all lowercase
    /// </summary>
    Preserve
}