using System;
using System.Threading;

// https://devblogs.microsoft.com/oldnewthing/20130913-00/?p=3243

// decimal muss ich boxen
public class Box<T> where T:struct
{
    private readonly T boxed;

    public Box(T seed) => boxed = seed;

    public Box() => boxed = default(T);

    public static implicit operator T(Box<T> value) => value.boxed;
}

public class BankAccount
{

    private Box<decimal> balance = new Box<decimal>();

    private bool isOpen = false;

    public void Open()
    {
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
    }

    public decimal Balance 
    { 
        get 
        { 
            if (!this.isOpen) 
                throw new InvalidOperationException();
            return this.balance;
        }
    }

    public void UpdateBalance(decimal change)
    {
        Box<decimal> oldValue;
        Box<decimal> newValue;
        do
        {
            oldValue = this.balance;
            newValue = new Box<decimal>(change + oldValue);
        } while (Interlocked.CompareExchange(ref this.balance, newValue, oldValue) != oldValue);
        // super schnell da atomare CPU operation !!! keine Betriebsystem Calls !!!! 
    }
}
