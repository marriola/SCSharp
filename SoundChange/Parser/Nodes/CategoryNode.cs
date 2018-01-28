using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class CategoryNode : Node
    {
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the set of phonemes present in this category.
        /// </summary>
        public HashSet<string> Members { get; private set; }

        /// <summary>
        /// Gets or sets the list of sets included in this category.
        /// </summary>
        public List<string> Includes { get; private set; }

        /// <summary>
        /// Gets or sets the builder tree matching the elements of this category.
        /// </summary>
        public BuilderNode BuilderTree { get; private set; }

        public CategoryNode(string name, HashSet<string> members, List<string> includes)
        {
            Name = name;
            Members = members;
            Includes = includes;
            BuilderTree = BuilderNode.TreeFrom(members);
        }

        public void ResolveIncludes(List<FeatureSetNode> features, List<CategoryNode> categories)
        {
            var featureMembers = features
                .Where(x => Includes.Contains(x.Name))
                .SelectMany(x => x.Members);

            var categoryMembers = categories
                .Where(x => Includes.Contains(x.Name))
                .SelectMany(x => x.Members);

            foreach (var member in featureMembers.Concat(categoryMembers))
            {
                Members.Add(member);
            }

            BuilderTree = BuilderNode.TreeFrom(Members);
        }


        public override string ToString()
        {
            return $"{Name} {{ {string.Join(string.Empty, Members.ToList())} }}";
        }
    }
}
