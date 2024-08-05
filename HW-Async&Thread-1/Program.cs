namespace HW_Async_Thread;

public class Program
{
    public static async Task Main()
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
        Console.WriteLine($"Initial Value: {add3.Val}");
        Console.WriteLine($"Update State: {add3.IsUpdated}");
        a.NewVal = 5;
        await Task.Delay(500);
        Console.WriteLine($"Updated Value: {add3.Val}");
        Console.WriteLine($"Update State: {add3.IsUpdated}");

        b.NewVal = 6;
        await Task.Delay(500);
        Console.WriteLine($"Updated(2) Value: {add3.Val}");
        Console.WriteLine($"Update State: {add3.IsUpdated}");
    }
}

/// <summary>
/// 表达式结点抽象类，用于构造表达式树
/// </summary>
public abstract class Expr
{
    /// <summary>
    /// 父结点
    /// </summary>
    protected Expr? parent = null;

    /// <summary>
    /// 表达式的值，只允许返回一个现成的值，可以加锁
    /// 拓展思考：是否所有时候读取到的值都是其正确的值？如何避免？
    /// </summary>
    public abstract int Val { get; }

    /// <summary>
    /// 异步方法，它的作用是启动一个任务，推动结点自身及其父结点更新值
    /// 可以根据自身需求适当修改方法签名
    /// </summary>
    public abstract Task Update();

    /// <summary>
    /// 注册父结点
    /// 思考：当父结点被注册后，父结点的值是否需要更新？
    /// </summary>
    /// <param name="parent">待注册的父结点</param>
    public abstract void Register(Expr parent);
}

/// <summary>
/// 数据结点
/// </summary>
/// <param name="initVal">初始值</param>
public class ValueExpr(int initVal) : Expr
{
    int val = initVal;
    bool isUpdated = false;
    public override int Val
    {
        get
        {
            // TODO 1:读取操作
            return val;
        }
    }

    public bool IsUpdated => isUpdated;
    /// <summary>
    /// 修改数据
    /// 思考：修改数据后，父结点是否也需要更新？
    /// </summary>
    public int NewVal
    {
        set
        {
            // TODO 2:修改操作
            val = value;
            isUpdated = false;
            _ = Update();  // 对异步方法调用结果进行忽略

        }
    }

    public override async Task Update()
    {
        // TODO 3:更新操作
        await Task.Delay(100);
        isUpdated = true;
        parent?.Update();
    }

    public override void Register(Expr parent)
    {
        // TODO 4:注册操作
        this.parent = parent;
        parent?.Update();
    }


}

/// <summary>
/// 加法表达式结点
/// 可以根据自己的想法创造更多种类的结点
/// </summary>
public class AddExpr : Expr
{
    int val = 0;
    bool isUpdated = false;
    public override int Val
    {
        get
        {
            // TODO 5:读取操作
            return val;
        }
    }

    public bool IsUpdated => isUpdated;

    public Expr ExprA, ExprB;
    public AddExpr(Expr A, Expr B)
    {
        ExprA = A;
        ExprB = B;
        A.Register(this);
        B.Register(this);
    }

    public override async Task Update()
    {
        // TODO 6:更新操作
        await Task.Delay(100);
        isUpdated = true;
        Compute();
        parent?.Update();
    }

    public override void Register(Expr parent)
    {
        // TODO 7:注册操作
        this.parent = parent;
        parent?.Update();
    }

    public async void Compute()
    {
        // 计算表达式的值
        await Task.Delay(100);
        val = ExprA.Val + ExprB.Val;
    }
}
