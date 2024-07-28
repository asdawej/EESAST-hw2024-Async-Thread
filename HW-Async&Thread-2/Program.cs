namespace HW_Async_Thread;

public static class Program
{
    public static void Main()
    {
        // 测试用例:
        TimeVariable tv = new();
        object obj = new();
        Thread thr1 = new(() =>
        {
            Task[] timer = [
                Task.Delay(100),
                Task.Delay(300),
                Task.Delay(700),
                Task.Delay(1300)
            ];
            timer[0].Wait();
            Console.WriteLine(tv.Val);  // 理论值100
            timer[1].Wait();
            Console.WriteLine(tv.Val);  // 理论值400
            timer[2].Wait();
            Console.WriteLine(tv.Val);  // 检测是否超过上限，理论值10000
            timer[3].Wait();
            Console.WriteLine(tv.Val);  // 检测是否超过下限，理论值0
            lock (obj)
                Monitor.Wait(obj);      // 等待强制设置
            Console.WriteLine(tv.Val);  // 理论值5000
        });
        Thread thr2 = new(() =>
        {
            Task[] timer = [
                Task.Delay(200),
                Task.Delay(400),
                Task.Delay(1000),
                Task.Delay(1500)
            ];
            timer[0].Wait();
            tv.Speed = 2;               // 变速
            timer[1].Wait();
            tv.Speed = 100;
            timer[2].Wait();
            tv.Speed = -100;
            timer[3].Wait();
            tv.Speed = 1;
            tv.Val = 5000;              // 强制设置
            lock (obj)
                Monitor.Pulse(obj);     // 通知强制设置完成
        });
        thr1.Start();
        thr2.Start();
        thr1.Join();
        thr2.Join();
    }
}

/// <summary>
/// 随时间变化的变量，默认在0~10000内以+1/ms的速度变化
/// </summary>
/// <param name="initVal">初始值</param>
/// <param name="lowerLimit">变化下限</param>
/// <param name="higherLimit">变化上限</param>
/// <param name="initSpeed">初始变化速度，以毫秒计时</param>
public class TimeVariable(int initVal = 0, int lowerLimit = 0, int higherLimit = 10000, int initSpeed = 1)
{
    /// <summary>
    /// 变化下限
    /// </summary>
    public readonly int LowerLimit = lowerLimit;
    /// <summary>
    /// 变化上限
    /// </summary>
    public readonly int HigherLimit = higherLimit;

    /// <summary>
    /// 计时起点
    /// 思考：如何利用计时起点来完成变量值的更新？
    /// </summary>
    int time = Environment.TickCount;

    int speed = initSpeed;
    /// <summary>
    /// 变化速度，以毫秒计时
    /// </summary>
    static ReaderWriterLockSlim lock_speed=new();
    static ReaderWriterLockSlim lock_val=new();
    public int Speed
    {
        get
        {
            // TODO 1:保护speed的读取
            int sp;
            lock_speed.EnterReadLock();
            try
            {
                sp=speed;
            }
            finally{lock_speed.ExitReadLock();}
            return speed;
        }
        set
        {
            // TODO 2:请思考speed的改变如何体现在val的变化上？
            int t_v=Environment.TickCount;
            int sp=Speed;
            lock_speed.EnterWriteLock();
            try{speed = value;}
            finally{lock_speed.ExitWriteLock();}
            int va,t;
            lock_val.EnterReadLock();
            try
            {
                va=val;t=time;
            }
            finally{lock_val.ExitReadLock();}
            lock_val.EnterWriteLock();
            try
            {
                va+=(t_v-t)*sp;
                if(va>HigherLimit)val=HigherLimit;
                else if(va<LowerLimit)val=LowerLimit;
                else val=va;
                time=t_v;
            }
            finally{lock_val.ExitWriteLock();}
        }
    }

    int val = initVal;
    /// <summary>
    /// 变量的值
    /// </summary>
    public int Val
    {
        get
        {
            // TODO 3:直接返回val是否是这个变量当前时刻的值？当然，可以有不同实现
            int timenow=Environment.TickCount;
            int sp=Speed;
            int va,t;
            lock_val.EnterReadLock();
            try
            {
                va=val;t=time;
            }
            finally{lock_val.ExitReadLock();}
            
            int newval=va+(timenow-t)*sp;
            if(newval<LowerLimit)return LowerLimit;
            else if(newval>HigherLimit)return HigherLimit;
            else return newval;
        }
        set
        {
            // TODO 4:保护val的写入
            if(value<LowerLimit)value=LowerLimit;
            if(value>HigherLimit)value=HigherLimit;
            lock_val.EnterWriteLock();
            try
            {
                val=value;
                time=Environment.TickCount;
            }
            finally{lock_val.ExitWriteLock();}
        }
    }
}
