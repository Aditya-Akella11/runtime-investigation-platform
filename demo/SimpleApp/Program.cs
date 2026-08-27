using System;
using System.Diagnostics;
using System.Threading;

namespace SimpleApp;

public static class Calculator
{
    public static int Add(int a, int b)
    {
        return a + b;
    }

    public static int Multiply(int a, int b)
    {
        return a * b;
    }
}

internal class Program
{
    private static void Main(string[] args)
    {
        int pid = Process.GetCurrentProcess().Id;
        Console.WriteLine($"SimpleApp PID: {pid}");

        // Loop for 110 seconds, calling Calculator methods repeatedly
        for (int i = 0; i < 110; i++)
        {
            int sum = Calculator.Add(i, 10);
            int product = Calculator.Multiply(i, 2);
            Console.WriteLine($"[{i}] Calculator.Add({i}, 10) = {sum}, Calculator.Multiply({i}, 2) = {product}");
            Thread.Sleep(1000);
        }
    }
}
