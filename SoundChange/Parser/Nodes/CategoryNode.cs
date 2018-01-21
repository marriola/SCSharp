﻿using System.Collections.Generic;
using System.Linq;

namespace SoundChange.Parser.Nodes
{
    class CategoryNode : Node
    {
        public string Name { get; private set; }

        public HashSet<string> Members { get; private set; }

        public BuilderNode BuilderTree { get; private set; }

        public CategoryNode(string name, HashSet<string> members)
        {
            Name = name;
            Members = members;
            BuilderTree = BuilderNode.TreeFrom(members);
        }


        public override string ToString()
        {
            return $"{Name} {{ {string.Join(string.Empty, Members.ToList())} }}";
        }
    }
}