namespace TreeViewList
{
    public class Cell(object? value)
    {
        public virtual object? Value { get; set; } = value;
        public string ValueAsString => Value?.ToString() ?? string.Empty;

        public override string ToString() => ValueAsString;
    }
}
