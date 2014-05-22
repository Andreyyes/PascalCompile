using System;
using System.Collections.Generic;

public class Tree
{
    private List<Tree> child { get; set; }
    public Tree parent { get; set; }
    public Tree next { get; set; }
    public Tree prev { get; set; }
    public string command { get; set; }
    public string type { get; set; }
    public bool wait { get; set; }
    public bool is_root { get; set; }
    public int start { get; set; }
    public int end { get; set; }
    public int line { get; set; }
    public int enters_count { get; set; }
    public bool ignore_enter { get; set; }
    public bool ignore_exit { get; set; }
    public bool ignore_sibling { get; set; }

    public Tree()
    {
        child = new List<Tree>();
        wait = true;
        enters_count = 0;
    }

    public Tree(string _command)
    {
        child = new List<Tree>();
        while (_command.Contains("  "))
            _command = _command.Replace("  ", " ");
        command = _command.Trim();
        wait = true;
        enters_count = 0;
    }

    public Tree(string _command, int _start, int _end, int _line)
    {
        child = new List<Tree>();
        while (_command.Contains("  "))
            _command = _command.Replace("  ", " ");
        command = _command.Trim();
        wait = true;
        start = _start;
        end = _end;
        line = _line;
        enters_count = 0;
    }

    public void Dump(string prefix = "")
    {
        Console.WriteLine(prefix + command/* + "(" + start + ";" + (end - start) + ";"+line+")" + (is_root ? "[ROOT]" : "")*/);
        foreach (Tree tree in child)
            tree.Dump("|   " + prefix);
    }

    /// <summary>
    /// Добавляет потомка в ветку дерева
    /// </summary>
    /// <param name="_child">Потомок</param>
    /// <returns>Потомок</returns>
    public Tree AppendChild(Tree _child)
    {
        wait = false;
        _child.parent = this;
        child.Add(_child);
        int count = child.Count;
        if (count >= 2)
        {
            child[count - 2].next = child[count - 1];
            child[count - 1].prev = child[count - 2];
        }
        return child[count - 1];
    }

    /// <summary>
    /// Возвращается потом по порядковому номеру в дереве
    /// </summary>
    /// <param name="id">Номер потомка</param>
    /// <returns>Потомок</returns>
    public Tree GetChildById(int id)
    {
        if (id < child.Count)
            return child[id];
        return null;
    }

    /// <summary>
    /// Возвращается первый потомок
    /// </summary>
    /// <returns>Потомок</returns>
    public Tree GetFirstChild()
    {
        if (child.Count == 0)
            return null;
        return child[0];
    }

    /// <summary>
    /// Возвращается последний потомок
    /// </summary>
    /// <returns>Потомок</returns>
    public Tree GetLastChild()
    {
        if (child.Count == 0)
            return null;
        return child[child.Count - 1];
    }

    /// <summary>
    /// Возвращает true, если у дерева есть потомки и false, если дерево не имеет потомков
    /// </summary>
    /// <returns></returns>
    public bool HasChild()
    {
        if (child == null)
            return false;
        return child.Count > 0;
    }

    /// <summary>
    /// Возвращает количество потомков у дерева
    /// </summary>
    /// <returns></returns>
    public int GetChildCount()
    {
        return child.Count;
    }
}