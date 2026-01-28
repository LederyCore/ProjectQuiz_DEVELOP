using System;
using System.Collections;
using System.Collections.Generic;

public class CircularLinkedList<T> : ICollection<T>, IEnumerable<T>
{
    private LinkedList<T> list;
    private LinkedListNode<T> current;

    public int Count => list.Count;
    public bool IsReadOnly => false;
    public LinkedListNode<T> First => list.First;
    public LinkedListNode<T> Last => list.Last;
    public LinkedListNode<T> Current => current;

    public CircularLinkedList()
    {
        list = new LinkedList<T>();
        current = null;
    }

    public CircularLinkedList(IEnumerable<T> collection)
    {
        list = new LinkedList<T>(collection);
        current = list.First;
    }

    // AddAfter
    public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value)
    {
        var newNode = list.AddAfter(node, value);
        if (current == null)
            current = newNode;
        return newNode;
    }

    public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        list.AddAfter(node, newNode);
        if (current == null)
            current = newNode;
    }

    // AddBefore
    public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value)
    {
        var newNode = list.AddBefore(node, value);
        if (current == null)
            current = newNode;
        return newNode;
    }

    public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode)
    {
        list.AddBefore(node, newNode);
        if (current == null)
            current = newNode;
    }

    // AddFirst
    public LinkedListNode<T> AddFirst(T value)
    {
        var newNode = list.AddFirst(value);
        if (current == null)
            current = newNode;
        return newNode;
    }

    public void AddFirst(LinkedListNode<T> node)
    {
        list.AddFirst(node);
        if (current == null)
            current = node;
    }

    // AddLast
    public LinkedListNode<T> AddLast(T value)
    {
        var newNode = list.AddLast(value);
        if (current == null)
            current = newNode;
        return newNode;
    }

    public void AddLast(LinkedListNode<T> node)
    {
        list.AddLast(node);
        if (current == null)
            current = node;
    }

    // Clear
    public void Clear()
    {
        list.Clear();
        current = null;
    }

    // Contains
    public bool Contains(T value)
    {
        return list.Contains(value);
    }

    // CopyTo
    public void CopyTo(T[] array, int arrayIndex)
    {
        list.CopyTo(array, arrayIndex);
    }

    // Find
    public LinkedListNode<T> Find(T value)
    {
        return list.Find(value);
    }

    // FindLast
    public LinkedListNode<T> FindLast(T value)
    {
        return list.FindLast(value);
    }

    // Remove (value)
    public bool Remove(T value)
    {
        var node = list.Find(value);
        if (node == null) return false;

        if (current == node)
        {
            current = GetNextCircular(current);
            if (current == node) // 노드가 하나만 있었던 경우
                current = null;
        }

        list.Remove(node);
        return true;
    }

    // Remove (node)
    public void Remove(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        if (current == node)
        {
            current = GetNextCircular(current);
            if (current == node)
                current = null;
        }

        list.Remove(node);
    }

    // RemoveFirst
    public void RemoveFirst()
    {
        if (list.Count == 0)
            throw new InvalidOperationException("리스트가 비어있습니다.");

        if (current == list.First)
        {
            current = GetNextCircular(current);
            if (list.Count == 1)
                current = null;
        }

        list.RemoveFirst();
    }

    // RemoveLast
    public void RemoveLast()
    {
        if (list.Count == 0)
            throw new InvalidOperationException("리스트가 비어있습니다.");

        if (current == list.Last)
        {
            current = GetPreviousCircular(current);
            if (list.Count == 1)
                current = null;
        }

        list.RemoveLast();
    }

    // ICollection<T> 인터페이스 구현
    public void Add(T item)
    {
        AddLast(item);
    }

    // IEnumerable<T> 인터페이스 구현
    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // 순환 관련 추가 메서드들
    public LinkedListNode<T> GetNextCircular(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        return node.Next ?? list.First;
    }

    public LinkedListNode<T> GetPreviousCircular(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        return node.Previous ?? list.Last;
    }

    public void MoveNext()
    {
        if (current == null)
            throw new InvalidOperationException("리스트가 비어있습니다.");

        current = GetNextCircular(current);
    }

    public void MovePrevious()
    {
        if (current == null)
            throw new InvalidOperationException("리스트가 비어있습니다.");

        current = GetPreviousCircular(current);
    }

    public void ResetCurrent()
    {
        current = list.First;
    }

    public void SetCurrent(LinkedListNode<T> node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));

        current = node;
    }

    public void Display()
    {
        if (list.Count == 0)
        {
            Console.WriteLine("리스트가 비어있습니다.");
            return;
        }

        var node = list.First;
        do
        {
            Console.Write(node.Value);
            if (node == current)
                Console.Write(" [Current]");
            Console.Write(" -> ");

            node = GetNextCircular(node);
        } while (node != list.First);

        Console.WriteLine("(순환)");
    }
}