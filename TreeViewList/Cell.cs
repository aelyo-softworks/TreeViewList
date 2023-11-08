namespace TreeViewList
{
    public class Cell
    {
        public Cell(object? value)
        {
            Value = value;
        }

        public virtual object? Value { get; set; }
        public string ValueAsString => Value?.ToString() ?? string.Empty;

        public override string ToString() => ValueAsString;
    }
}
