using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HW_Async_Thread
{
    public class Program
    {
        public static async Task Main()
        {
            // 测试用例: (a + b) + (c + d)
            ValueExpr a = new ValueExpr(1);
            ValueExpr b = new ValueExpr(2);
            ValueExpr c = new ValueExpr(3);
            ValueExpr d = new ValueExpr(4);
            AddExpr add1 = new AddExpr(a, b);
            AddExpr add2 = new AddExpr(c, d);
            AddExpr add3 = new AddExpr(add1, add2);


            Console.WriteLine(await add3.Val); 

            a.NewVal = 5;
            await add3.Update(); 

          
            Console.WriteLine(await add3.Val); 
        }
    }

    public abstract class Expr
    {
        protected readonly List<Expr> parents = new List<Expr>();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        protected bool isDirty = true;

        public abstract Task<int> GetValAsync();

        public Task<int> Val
        {
            get
            {
                return GetValAsync();
            }
        }

        public abstract Task Update();

        public abstract void Register(Expr parent);

        protected void NotifyParents()
        {
            foreach (var parent in parents)
            {
                _ = parent.Update();
            }
        }

        protected async Task WaitForUpdate()
        {
            await semaphore.WaitAsync();
            try
            {
                await Update();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    public class ValueExpr : Expr
    {
        private int val;

        public ValueExpr(int initVal)
        {
            val = initVal;
        }

        public override Task<int> GetValAsync()
        {
            return Task.FromResult(val);
        }

        public int NewVal
        {
            set
            {
                if (val != value)
                {
                    val = value;
                    NotifyParents();
                }
            }
        }

        public override Task Update()
        {
            return Task.CompletedTask;
        }

        public override void Register(Expr parent)
        {
            parents.Add(parent);
        }
    }

    public class AddExpr : Expr
    {
        private int val;
        private readonly Expr exprA;
        private readonly Expr exprB;

        public AddExpr(Expr A, Expr B)
        {
            exprA = A;
            exprB = B;
            A.Register(this);
            B.Register(this);
        }

        public override async Task<int> GetValAsync()
        {
            if (isDirty)
            {
                await WaitForUpdate();
            }
            return val;
        }

        public override async Task Update()
        {
            await Task.WhenAll(exprA.GetValAsync(), exprB.GetValAsync());
            val = await exprA.GetValAsync() + await exprB.GetValAsync();
            isDirty = false;
            NotifyParents();
        }

        public override void Register(Expr parent)
        {
            parents.Add(parent);
        }
    }
}


