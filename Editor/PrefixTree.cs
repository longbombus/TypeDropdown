using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TypeDropdown.Editor
{
	public class PrefixTree<TValue>
		: IEnumerable<KeyValuePair<IEnumerable<string>, TValue>>
	{
		public readonly Node Root = new();

		public TValue this[string path]
		{
			get => TryLocateNode(path, out var node, out _, out _)
				? node.Value
				: throw new KeyNotFoundException($"Path {path} is missing in the tree.");
			set => Set(path, value);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		public IEnumerator<KeyValuePair<IEnumerable<string>, TValue>> GetEnumerator()
		{
			if (!Root.HasChildren)
				yield break;

			var stack = new Stack<Node>();
			foreach (var child in Root.Children)
				stack.Push(child);

			while (stack.TryPop(out var node))
			{
				if (node.Value != null)
					yield return new KeyValuePair<IEnumerable<string>, TValue>(node.Path, node.Value);

				if (!node.HasChildren)
					continue;

				foreach (var child in node.Children)
					stack.Push(child);
			}
		}

		public bool ContainsKey(string path)
			=> TryLocateNode(path, out _, out _, out _);

		public bool TryGetValue(string key, out TValue value)
		{
			if (TryLocateNode(key, out var node, out _, out _))
			{
				value = node.Value;
				return true;
			}

			value = default;
			return false;
		}

		public void Add(string path, TValue value)
		{
			if (TryCreateNode(path, out var node))
				node.Value = value;
		}

		public void Set(string path, TValue value)
		{
			_ = TryCreateNode(path, out var node);
			node.Value = value;
		}

		public bool Remove(string path)
			=> TryLocateNode(path, out _, out var nodePrefix, out var parentNode)
			&& parentNode.RemoveChild(nodePrefix);

		public void Clear()
		{
			Root.Clear();
		}

		private bool TryLocateNode(string path, out Node node, out string nodeName, out Node parent)
		{
			nodeName = string.Empty;
			parent = null;
			node = Root;

			int start, end;
			for (start = 0, end = 1; end <= path.Length; ++end)
			{
				if (end < path.Length && !IsSeparator(path[end]))
					continue;

				nodeName = path.Substring(start, end - start);
				start = end + 1;

				if (!node.TryGetChild(nodeName, out var childNode))
					return false;

				parent = node;
				node = childNode;
			}

			return true;
		}

		private bool TryCreateNode(string path, out Node node)
		{
			node = Root;

			int start, end;
			for (start = 0, end = 1; end <= path.Length; ++end)
			{
				if (end < path.Length && !IsSeparator(path[end]))
					continue;

				var nodeName = path.Substring(start, end - start);
				start = end + 1;

				node = node.GetOrAddChild(nodeName);
			}

			return true;
		}

		private static bool IsSeparator(char c)
			=> c is '/' or '.';

		public class Node : IComparable<Node>
		{
			private static readonly Node SearchNode = new();

			private readonly Node parent;
			public IEnumerable<string> Path => parent == null ? Array.Empty<string>() : parent.Path.Append(Name);

			private string name;
			private string Name => name;

			public TValue Value { get; set; }

			private SortedSet<Node> children;
			public IReadOnlyCollection<Node> Children => children;
			public bool HasChildren => children != null && children.Count > 0;

			public Node() : this(null, null) { }

			private Node(Node parent, string name)
			{
				this.parent = parent;
				this.name = name;
			}

			public bool TryGetChild(string childName, out Node childNode)
			{
				if (children == null)
				{
					childNode = null;
					return false;
				}

				SearchNode.name = childName;
				return children.TryGetValue(SearchNode, out childNode);
			}

			public Node GetOrAddChild(string childName)
			{
				children ??= new SortedSet<Node>();

				SearchNode.name = childName;
				if (!children.TryGetValue(SearchNode, out var childNode))
				{
					childNode = new Node(this, childName);
					children.Add(childNode);
				}

				return childNode;
			}

			public bool RemoveChild(string childName)
			{
				if (children == null)
					return false;

				SearchNode.name = childName;
				return children.Remove(SearchNode);
			}

			public void Clear()
			{
				children?.Clear();
				Value = default;
			}

			public int CompareTo(Node other)
				=> ReferenceEquals(this, other) ? 0
				: other is null ? -1
				: string.Compare(name, other.name, StringComparison.Ordinal);
		}
	}
}