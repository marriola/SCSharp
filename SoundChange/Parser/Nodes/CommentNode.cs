namespace SoundChange.Parser.Nodes
{
    class CommentNode : Node
    {
        public string Comment { get; private set; }

        public CommentNode(string comment)
        {
            Comment = comment;
        }
    }
}
