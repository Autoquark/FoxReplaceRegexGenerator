record ReplacementRule
{

    public ReplacementRule(string input, string output, string? inputPlural = null, string? outputPlural = null) : this([input], output, inputPlural, outputPlural)
    {   
    }

    public ReplacementRule(IEnumerable<string> inputs, string output, string? inputPlural = null, string? outputPlural = null) : this(inputs, [output])
    {
        // If inputPlural is not defined, InputsPlural defaults to inputs with "s" appended. To disable plurals completely, set InputsPlural to empty after construction
        if(!string.IsNullOrEmpty(inputPlural))
        {
            InputsPlural = [inputPlural];
        }
        else
        {
            InputsPlural = Inputs.Select(x => x + "s").ToList();
        }

        // If outputPlural is not defined, ReverseInputsPlural defaults to ReverseInputs with "s" appended. To disable plurals completely, set InputsPlural to empty after construction
        if(!string.IsNullOrEmpty(outputPlural))
        {
            ReverseInputsPlural = [outputPlural];
        }
        else
        {
            ReverseInputsPlural = ReverseInputs.Select(x => x + "s").ToList();
        }
    }

    public ReplacementRule(IEnumerable<string> inputs, IEnumerable<string> outputs)
    {
        if(!inputs.Any() || !outputs.Any())
        {
            throw new Exception("Rules must have at least one input and output");
        }

        Inputs = inputs.ToList();
        ReverseInputs = outputs.ToList();
    }

    // Chainable methods for convenient config
    public ReplacementRule NoPluralHandling()
    {
        InputsPlural = ReverseInputsPlural = [];
        return this;
    }

    /// <summary>
    /// Input strings that should be matched. If Bidirectional is true, the first input is the output of the reverse rule
    /// </summary>
    public List<string> Inputs { get; set; }
    /// <summary>
    /// Output of the reverse rule, if Bidirectional is true
    /// </summary>
    public string ReverseOutput => Inputs[0];

    /// <summary>
    /// Output of the rule
    /// </summary>
    public string Output => ReverseInputs[0];
    /// <summary>
    /// Input strings that should be matched by the reverse rule. The first entry is also the output of the forward rule.
    /// </summary>
    public List<string> ReverseInputs { get; set; }

    /// <summary>
    /// If true, a word boundary is required at each end of the input
    /// </summary>
    public bool WholeWords { get; set; } = true;
    
    /// <summary>
    /// If not empty, generated rules will also replace anything in InputsPlural with OutputPlural. Defaults to Input + "s"
    /// </summary>
    public List<string> InputsPlural { get; set; } = [];

    public string? OutputPlural => ReverseInputsPlural.Any() ? ReverseInputsPlural[0] : null;
    /// <summary>
    /// If neither is null or empty, generated rules will also replace InputPlural with OutputPlural. Defaults to Output + "s"
    /// </summary>
    public List<string> ReverseInputsPlural { get; set; } = [];
    public CapitalisationHandling CapitalisationHandling { get; set; } = CapitalisationHandling.Preserve;

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
    /// Match only the input capitalisation specified in the rule, output with capitalisation as given in the rule
    /// </summary>
    MatchExact,
    /// <summary>
    /// Match any input capitalisation, attempt to output with matching capitalisation with the following cases in priority order:
    /// 1. all uppercase (if input all uppercase)
    /// 2. first letter only uppercase (if input starts with uppercase)
    /// 3. all lowercase
    /// </summary>
    Preserve
}