using System;
using System.Collections.Generic;

namespace stdTernary;

public sealed class TernarySearchTree<TValue>
{
    private sealed class Node
    {
        public char Character;
        public Node? Left;
        public Node? Middle;
        public Node? Right;
        public bool HasValue;
        public TValue? Value;
    }

    private Node? _root;
    public int Count { get; private set; }

    public void Put(string key, TValue value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key must have at least one character.", nameof(key));

        _root = Put(_root, key, value, 0);
    }

    public bool TryGetValue(string key, out TValue value)
    {
        value = default!;
        if (string.IsNullOrEmpty(key))
            return false;

        Node? node = Get(_root, key, 0);
        if (node is { HasValue: true })
        {
            value = node.Value!;
            return true;
        }
        return false;
    }

    public IEnumerable<(string Key, TValue Value)> Items()
    {
        return Collect(_root, new List<char>());
    }

    public IEnumerable<(string Key, TValue Value)> KeysWithPrefix(string prefix)
    {
        if (prefix is null)
            throw new ArgumentNullException(nameof(prefix));

        if (prefix.Length == 0)
            return Collect(_root, new List<char>());

        Node? node = Get(_root, prefix, 0);
        if (node is null)
            return Array.Empty<(string, TValue)>();

        var list = new List<(string, TValue)>();
        if (node.HasValue)
            list.Add((prefix, node.Value!));

        list.AddRange(Collect(node.Middle, new List<char>(prefix)));
        return list;
    }

    private Node Put(Node? node, string key, TValue value, int depth)
    {
        char current = key[depth];
        node ??= new Node { Character = current };

        Trit comparison = current.Spaceship(node.Character);
        comparison
            .Negative(() => node.Left = Put(node.Left, key, value, depth))
            .Positive(() => node.Right = Put(node.Right, key, value, depth))
            .Zero(() =>
            {
                if (depth + 1 == key.Length)
                {
                    if (!node.HasValue)
                        Count++;
                    node.HasValue = true;
                    node.Value = value;
                }
                else
                {
                    node.Middle = Put(node.Middle, key, value, depth + 1);
                }
            });

        return node;
    }

    private Node? Get(Node? node, string key, int depth)
    {
        if (node is null)
            return null;

        char current = key[depth];
        Trit comparison = current.Spaceship(node.Character);
        if (comparison.Value == TritVal.n)
            return Get(node.Left, key, depth);
        if (comparison.Value == TritVal.p)
            return Get(node.Right, key, depth);

        if (depth + 1 == key.Length)
            return node;
        return Get(node.Middle, key, depth + 1);
    }

    private IEnumerable<(string Key, TValue Value)> Collect(Node? node, List<char> path)
    {
        if (node is null)
            yield break;

        foreach (var pair in Collect(node.Left, path))
            yield return pair;

        path.Add(node.Character);
        if (node.HasValue)
            yield return (new string(path.ToArray()), node.Value!);

        foreach (var pair in Collect(node.Middle, path))
            yield return pair;
        path.RemoveAt(path.Count - 1);

        foreach (var pair in Collect(node.Right, path))
            yield return pair;
    }
}
