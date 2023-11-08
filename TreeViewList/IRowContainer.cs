namespace TreeViewList
{
    public interface IRowContainer
    {
        TreeViewListControl Control { get; }
        RowCollection Rows { get; }
    }
}