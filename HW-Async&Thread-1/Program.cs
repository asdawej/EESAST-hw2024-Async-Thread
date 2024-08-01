namespace HW_Async_Thread;

public class Program
{
    public static void Main()
    {
        // 测试用例: (a + b) + (c + d)
        // 可以自行修改测试用例
        ValueExpr a = new(1);
        ValueExpr b = new(2);
        ValueExpr c = new(3);
        ValueExpr d = new(4);
        AddExpr add1 = new(a, b);
        AddExpr add2 = new(c, d);
        AddExpr add3 = new(add1, add2);
        Console.WriteLine(add3.Val);
        a.NewVal = 5;
        Console.WriteLine(add3.Val);
    }
}

/// <summary>
/// 表达式结点抽象类，用于构造表达式树
/// </summary>
public abstract class Expr
{
    protected Expr? parent = null;
    protected readonly object lockObj = new();

    public abstract int Val { get; }

    public abstract Task Update();

    public abstract void Register(Expr parent);
}

/// <summary>
/// 数据结点
/// </summary>
/// <param name="initVal">初始值</param>
public class ValueExpr(int initVal) : Expr
{
    private int val;

    public ValueExpr(int initVal)
    {
        val = initVal;
    }

    public override int Val
    {
        get
        {
            lock (lockObj)
            {
                return val;
            }
        }
    }

    public int NewVal
    {
        set
        {
            lock (lockObj)
            {
                val = value;
            }
            _ = Update(); // Trigger asynchronous update
        }
    }

    public override async Task Update()
    {
        if (parent != null)
        {
            await Task.Delay(100); // Simulate delay
            await parent.Update();
        }
    }

    public override void Register(Expr parent)
    {
        this.parent = parent;
        _ = parent.Update();
    }
}

/// <summary>
/// 加法表达式结点
/// 可以根据自己的想法创造更多种类的结点
/// </summary>
public class AddExpr : Expr
{
    private int val;
    public Expr ExprA { get; }
    public Expr ExprB { get; }

    public AddExpr(Expr A, Expr B)
    {
        ExprA = A;
        ExprB = B;
        A.Register(this);
        B.Register(this);
        _ = Update(); // Initialize the value
    }

    public override int Val
    {
        get
        {
            lock (lockObj)
            {
                return val;
            }
        }
    }

    public override async Task Update()
    {
        int newVal;
        await Task.Delay(100); // Simulate delay
        lock (lockObj)
        {
            newVal = ExprA.Val + ExprB.Val;
            val = newVal;
        }
        if (parent != null)
        {
            await parent.Update();
        }
    }

    public override void Register(Expr parent)
    {
           this.parent = parent;
        _ = parent.Update();
    }
}
