using System;
using System.Collections.Generic;

namespace stdTernary;

public sealed class BinarySearchTree<TValue>
{
    private sealed class Node
    {
        public required string Key;
        public TValue? Value;
        public Node? Left;
        public Node? Right;
    }

    private Node? _root;
    public int Count { get; private set; }

    public void Put(string key, TValue value)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Key must have at least one character.", nameof(key));

        bool added = false;
        _root = Put(_root, key, value, ref added);
        if (added)
            Count++;
    }

    public bool TryGetValue(string key, out TValue value)
    {
        value = default!;
        if (string.IsNullOrEmpty(key))
            return false;

        Node? current = _root;
        while (current is not null)
        {
            int comparison = string.CompareOrdinal(key, current.Key);
            if (comparison == 0)
            {
                value = current.Value!;
                return true;
            }

            current = comparison < 0 ? current.Left : current.Right;
        }

        return false;
    }

    public IEnumerable<(string Key, TValue Value)> Items()
    {
        return Traverse(_root);
    }

    public IEnumerable<(string Key, TValue Value)> KeysWithPrefix(string prefix)
    {
        if (prefix is null)
            throw new ArgumentNullException(nameof(prefix));

        if (prefix.Length == 0)
            return Traverse(_root);

        return KeysWithPrefix(_root, prefix);
    }

    private static Node Put(Node? node, string key, TValue value, ref bool added)
    {
        if (node is null)
        {
            added = true;
            return new Node
            {
                Key = key,
                Value = value
            };
        }

        int comparison = string.CompareOrdinal(key, node.Key);
        if (comparison < 0)
        {
            node.Left = Put(node.Left, key, value, ref added);
        }
        else if (comparison > 0)
        {
            node.Right = Put(node.Right, key, value, ref added);
        }
        else
        {
            node.Value = value;
        }

        return node;
    }

    private static IEnumerable<(string Key, TValue Value)> Traverse(Node? node)
    {
        if (node is null)
            yield break;

        foreach (var item in Traverse(node.Left))
            yield return item;

        yield return (node.Key, node.Value!);

        foreach (var item in Traverse(node.Right))
            yield return item;
    }

    private static IEnumerable<(string Key, TValue Value)> KeysWithPrefix(Node? node, string prefix)
    {
        if (node is null)
            yield break;

        foreach (var item in KeysWithPrefix(node.Left, prefix))
            yield return item;

        if (node.Key.StartsWith(prefix, StringComparison.Ordinal))
            yield return (node.Key, node.Value!);

        foreach (var item in KeysWithPrefix(node.Right, prefix))
            yield return item;
    }
}

