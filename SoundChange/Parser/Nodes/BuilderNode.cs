using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    /// <summary>
    /// Represents a tree structure filled by breaking a list of strings down character by character.
    /// The leaves of the tree are the elements of the original list.
    /// </summary>
    class BuilderNode
    {
        public bool IsFinal { get; private set; }

        public string Value { get; private set; }

        public BuilderNode Parent { get; private set; }

        public char? Character { get; private set; }

        public List<BuilderNode> Children { get; private set; }

        public IEnumerable<BuilderNode> Siblings
        {
            get
            {
                return Parent.Children.Where(x => x != this);
            }
        }

        public BuilderNode(string value = null)
        {
            Value = value;
            Character = null;
            Children = new List<BuilderNode>();
            Parent = null;
        }

        private static readonly string EMPTY = StateMachines.RuleMachine.Special.LAMBDA.ToString();

        public static BuilderNode TreeFrom(IEnumerable<string> keys, string head = "", bool isFinal = false, char? character = null, BuilderNode parent = null)
        {
            var root = new BuilderNode
            {
                Value = head == string.Empty
                    ? null
                    : head,
                IsFinal = isFinal,
                Character = character,
                Parent = parent
            };

            var groupedKeys = keys
                .GroupBy(key => key[0]);

            foreach (var group in groupedKeys)
            {
                var follow = group
                    .Where(x => x != EMPTY)
                    .Select(x => x.Length > 1
                        ? x.Substring(1)
                        : EMPTY);

                if (follow.Count() == 1 && follow.First() == EMPTY)
                {
                    follow = new List<string>();
                }

                var next = group.Key == StateMachines.RuleMachine.Special.LAMBDA
                    ? head
                    : head + group.Key;

                root.Children.Add(TreeFrom(
                    follow,
                    head: next,
                    isFinal: keys.Contains(next),
                    character: group.Key,
                    parent: root));
            }

            return root;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
