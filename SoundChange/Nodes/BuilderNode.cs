using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundChange.Nodes
{
    /// <summary>
    /// Represents a tree structure filled by breaking a list of strings down character by character.
    /// The leaves of the tree are the elements of the original list.
    /// </summary>
    class BuilderNode
    {
        public string Value { get; private set; }

        public char? Character { get; private set; }

        public List<BuilderNode> Children { get; private set; }

        public BuilderNode(string value = null, char? character = null)
        {
            Value = value;
            Character = character;
            Children = new List<BuilderNode>();
        }

        public static BuilderNode TreeFrom(IEnumerable<string> keys, string head = "", char? character = null)
        {
            var root = new BuilderNode(
                head == string.Empty
                    ? null
                    : head,
                character);

            foreach (var group in keys.GroupBy(key => key[0]))
            {
                var follow = group
                    .Where(x => x.Length > 1)
                    .Select(x => x.Substring(1));

                root.Children.Add(TreeFrom(follow, head + group.Key, group.Key));
            }

            return root;
        }
    }
}
