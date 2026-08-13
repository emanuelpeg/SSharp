using System;

public interface Tree {}
public record Leaf(int value) : Tree;
public record Node(Tree left, Tree right) : Tree;

public static class Program
{
    public static void Main()
    {
        Node t = new Node(new Leaf(1), new Leaf(2));
        Console.WriteLine(t != null);
    }
}
