﻿using FluentAssertions;
using Microsoft.CodeAnalysis;
using StrongInject;
using Xunit;
using Xunit.Abstractions;

namespace StrongInject.Generator.Tests.Unit
{
    public class GeneratorTests : TestBase
    {
        public GeneratorTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void InstancePerResolutionDependencies()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerResolutionDependenciesWithCasts()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::IC)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::IC)_2);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
    }

        [Fact]
        public void InstancePerResolutionDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        await ((global::StrongInject.IRequiresAsyncInitialization)_2).InitializeAsync();
        var _3 = new global::D((global::C)_2);
        await ((global::StrongInject.IRequiresAsyncInitialization)_3).InitializeAsync();
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerResolutionDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(A))]
[FactoryRegistration(typeof(B))]
[FactoryRegistration(typeof(C))]
[FactoryRegistration(typeof(D))]
[Registration(typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (9,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Registration(typeof(C))
                new DiagnosticResult("SI1001", @"Registration(typeof(C))", DiagnosticSeverity.Warning).WithLocation(9, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _4 = new global::C();
        var _7 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_4).CreateAsync();
        var _6 = new global::D(_7);
        var _5 = await ((global::StrongInject.IAsyncFactory<global::DFactoryTarget>)_6).CreateAsync();
        var _3 = new global::B((global::C)_4, _5);
        var _2 = await ((global::StrongInject.IAsyncFactory<global::BFactoryTarget>)_3).CreateAsync();
        var _1 = new global::A(_2, _7);
        var _0 = await ((global::StrongInject.IAsyncFactory<global::AFactoryTarget>)_1).CreateAsync();
        TResult result;
        try
        {
            result = await func((global::AFactoryTarget)_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(_0);
            await global::StrongInject.Helpers.DisposeAsync(_7);
            await global::StrongInject.Helpers.DisposeAsync(_2);
            await global::StrongInject.Helpers.DisposeAsync(_5);
            await global::StrongInject.Helpers.DisposeAsync(_7);
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependencies()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A), Scope.InstancePerDependency)]
[Registration(typeof(B))]
[Registration(typeof(C), Scope.InstancePerDependency)]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        var _4 = new global::C();
        var _3 = new global::D((global::C)_4);
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _5 = new global::C();
        var _0 = new global::A((global::B)_1, (global::C)_5);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithCasts()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), Scope.InstancePerDependency, typeof(C), typeof(IC))]
[Registration(typeof(D), Scope.InstancePerDependency)]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        var _4 = new global::C();
        var _3 = new global::D((global::C)_4);
        var _1 = new global::B((global::IC)_2, (global::D)_3);
        var _5 = new global::C();
        var _0 = new global::A((global::B)_1, (global::IC)_5);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A), Scope.InstancePerDependency)]
[Registration(typeof(B), Scope.InstancePerDependency)]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c, B b1){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        await ((global::StrongInject.IRequiresAsyncInitialization)_2).InitializeAsync();
        var _3 = new global::D((global::C)_2);
        await ((global::StrongInject.IRequiresAsyncInitialization)_3).InitializeAsync();
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _4 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2, (global::B)_4);
        await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(A))]
[FactoryRegistration(typeof(B), Scope.InstancePerDependency)]
[FactoryRegistration(typeof(C), Scope.InstancePerResolution, Scope.InstancePerDependency)]
[FactoryRegistration(typeof(D), Scope.InstancePerDependency, Scope.InstancePerDependency)]
[Registration(typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c, DFactoryTarget d){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (9,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Registration(typeof(C))
                new DiagnosticResult("SI1001", @"Registration(typeof(C))", DiagnosticSeverity.Warning).WithLocation(9, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _4 = new global::C();
        var _7 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_4).CreateAsync();
        var _6 = new global::D(_7);
        var _5 = await ((global::StrongInject.IAsyncFactory<global::DFactoryTarget>)_6).CreateAsync();
        var _3 = new global::B((global::C)_4, _5);
        var _2 = await ((global::StrongInject.IAsyncFactory<global::BFactoryTarget>)_3).CreateAsync();
        var _8 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_4).CreateAsync();
        var _11 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_4).CreateAsync();
        var _10 = new global::D(_11);
        var _9 = await ((global::StrongInject.IAsyncFactory<global::DFactoryTarget>)_10).CreateAsync();
        var _1 = new global::A(_2, _8, _9);
        var _0 = await ((global::StrongInject.IAsyncFactory<global::AFactoryTarget>)_1).CreateAsync();
        TResult result;
        try
        {
            result = await func((global::AFactoryTarget)_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(_0);
            await global::StrongInject.Helpers.DisposeAsync(_9);
            await global::StrongInject.Helpers.DisposeAsync(_11);
            await global::StrongInject.Helpers.DisposeAsync(_8);
            await global::StrongInject.Helpers.DisposeAsync(_2);
            await global::StrongInject.Helpers.DisposeAsync(_5);
            await global::StrongInject.Helpers.DisposeAsync(_7);
        }

        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A), Scope.SingleInstance)]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D), Scope.SingleInstance)]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::A _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::D _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::D GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = new global::C();
            var _0 = new global::D((global::C)_1);
            this._singleInstanceField1 = _0;
            this._disposeAction1 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    private global::A GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _2 = new global::C();
            var _3 = GetSingleInstanceField1();
            var _1 = new global::B((global::C)_2, (global::D)_3);
            var _0 = new global::A((global::B)_1, (global::C)_2);
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = GetSingleInstanceField0();
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWihCasts()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), Scope.SingleInstance, typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::C();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = GetSingleInstanceField0();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::IC)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::IC)_2);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A), Scope.SingleInstance)]
[Registration(typeof(B))]
[Registration(typeof(C), Scope.SingleInstance)]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::A _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::C> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::C();
            await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
            this._singleInstanceField1 = _0;
            this._disposeAction1 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::A> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _2 = await GetSingleInstanceField1();
            var _3 = new global::D((global::C)_2);
            await ((global::StrongInject.IRequiresAsyncInitialization)_3).InitializeAsync();
            var _1 = new global::B((global::C)_2, (global::D)_3);
            var _0 = new global::A((global::B)_1, (global::C)_2);
            await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await GetSingleInstanceField0();
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(A), Scope.SingleInstance, Scope.InstancePerResolution)]
[FactoryRegistration(typeof(B), Scope.SingleInstance, Scope.SingleInstance)]
[FactoryRegistration(typeof(C), Scope.InstancePerResolution, Scope.SingleInstance)]
[FactoryRegistration(typeof(D), Scope.InstancePerResolution, Scope.InstancePerResolution)]
[Registration(typeof(C), Scope.InstancePerResolution, typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock2.WaitAsync();
        try
        {
            await (this._disposeAction2?.Invoke() ?? default);
        }
        finally
        {
            this._lock2.Release();
        }

        await this._lock3.WaitAsync();
        try
        {
            await (this._disposeAction3?.Invoke() ?? default);
        }
        finally
        {
            this._lock3.Release();
        }
    }

    private global::A _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::BFactoryTarget _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::B _singleInstanceField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction2;
    private global::CFactoryTarget _singleInstanceField3;
    private global::System.Threading.SemaphoreSlim _lock3 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction3;
    private async global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> GetSingleInstanceField3()
    {
        if (!object.ReferenceEquals(_singleInstanceField3, null))
            return _singleInstanceField3;
        await this._lock3.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = new global::C();
            var _0 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_1).CreateAsync();
            this._singleInstanceField3 = _0;
            this._disposeAction3 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(_0);
            }

            ;
        }
        finally
        {
            this._lock3.Release();
        }

        return _singleInstanceField3;
    }

    private async global::System.Threading.Tasks.ValueTask<global::B> GetSingleInstanceField2()
    {
        if (!object.ReferenceEquals(_singleInstanceField2, null))
            return _singleInstanceField2;
        await this._lock2.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = new global::C();
            var _4 = await GetSingleInstanceField3();
            var _3 = new global::D(_4);
            var _2 = await ((global::StrongInject.IAsyncFactory<global::DFactoryTarget>)_3).CreateAsync();
            var _0 = new global::B((global::C)_1, _2);
            this._singleInstanceField2 = _0;
            this._disposeAction2 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(_2);
            }

            ;
        }
        finally
        {
            this._lock2.Release();
        }

        return _singleInstanceField2;
    }

    private async global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = await GetSingleInstanceField2();
            var _0 = await ((global::StrongInject.IAsyncFactory<global::BFactoryTarget>)_1).CreateAsync();
            this._singleInstanceField1 = _0;
            this._disposeAction1 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(_0);
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::A> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = await GetSingleInstanceField1();
            var _2 = await GetSingleInstanceField3();
            var _0 = new global::A(_1, _2);
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = await GetSingleInstanceField0();
        var _0 = await ((global::StrongInject.IAsyncFactory<global::AFactoryTarget>)_1).CreateAsync();
        TResult result;
        try
        {
            result = await func((global::AFactoryTarget)_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(_0);
        }

        return result;
    }
}");
        }

        [Fact]
        public void MultipleResolvesShareSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), Scope.SingleInstance, typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>, IAsyncContainer<B>
{
}

public class A 
{
    public A(IC c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::C();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _0 = new global::A((global::IC)_1);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::B>.RunAsync<TResult, TParam>(global::System.Func<global::B, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _2 = new global::D((global::C)_1);
        var _0 = new global::B((global::C)_1, (global::D)_2);
        TResult result;
        try
        {
            result = await func((global::B)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void ReportMissingTypes()
        {
            string userSource = @"";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(
                // (1,1): Error SI0201: Missing Type 'StrongInject.RegistrationAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.ModuleRegistrationAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IRequiresInitialization'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IRequiresAsyncInitialization'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Helpers'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void RegistersInstanceProviderFields()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<A>, IAsyncContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<int[]>
{
    public InstanceProvider _instanceProvider1;
    internal IAsyncInstanceProvider _instanceProvider2;
    private IAsyncInstanceProvider<int[]> _instanceProvider3;
}

public class A {}
public class B {}
public class C {}
public class D {}

public class InstanceProvider : IAsyncInstanceProvider<A>, IAsyncInstanceProvider<B>
{
    public ValueTask<A> GetAsync() => throw null;
    ValueTask<B> IAsyncInstanceProvider<B>.GetAsync() => throw null;
}

public interface IAsyncInstanceProvider : IAsyncInstanceProvider<C>, IAsyncInstanceProvider<D>
{
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (8,37): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 37),
                // (9,43): Warning CS0649: Field 'Container._instanceProvider3' is never assigned to, and will always have its default value null
                // _instanceProvider3
                new DiagnosticResult("CS0649", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 43));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::A>)this._instanceProvider1).GetAsync();
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::A>)this._instanceProvider1).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::B>.RunAsync<TResult, TParam>(global::System.Func<global::B, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::B>)this._instanceProvider1).GetAsync();
        TResult result;
        try
        {
            result = await func((global::B)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::B>)this._instanceProvider1).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::C>)this._instanceProvider2).GetAsync();
        TResult result;
        try
        {
            result = await func((global::C)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::C>)this._instanceProvider2).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::D>)this._instanceProvider2).GetAsync();
        TResult result;
        try
        {
            result = await func((global::D)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::D>)this._instanceProvider2).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Int32[]>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32[], TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32[]>)this._instanceProvider3).GetAsync();
        TResult result;
        try
        {
            result = await func((global::System.Int32[])_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32[]>)this._instanceProvider3).ReleaseAsync(_0);
        }

        return result;
    }
}");
        }

        [Fact]
        public void IgnoresStaticInstanceProviderFields()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<A>, IAsyncContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<int[]>
{
    public static InstanceProvider _instanceProvider1;
    internal static IAsyncInstanceProvider _instanceProvider2;
    private static IAsyncInstanceProvider<int[]> _instanceProvider3;
}

public class A {}
public class B {}
public class C {}
public class D {}

public class InstanceProvider : IAsyncInstanceProvider<A>, IAsyncInstanceProvider<B>
{
    public ValueTask<A> GetAsync() => throw null;
    ValueTask<B> IAsyncInstanceProvider<B>.GetAsync() => throw null;
}

public interface IAsyncInstanceProvider : IAsyncInstanceProvider<C>, IAsyncInstanceProvider<D>
{
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out _, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Error SI0102: Error while resolving dependencies for 'B': We have no source for instance of type 'B'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Error SI0102: Error while resolving dependencies for 'C': We have no source for instance of type 'C'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Error SI0102: Error while resolving dependencies for 'D': We have no source for instance of type 'D'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Error SI0102: Error while resolving dependencies for 'int[]': We have no source for instance of type 'int[]'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify(
                // (8,44): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 44),
                // (9,50): Warning CS0169: The field 'Container._instanceProvider3' is never used
                // _instanceProvider3
                new DiagnosticResult("CS0169", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 50));
        }

        [Fact]
        public void DependenciesAreOverriddenByInstanceProviderFields()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
    public InstanceProvider _instanceProvider;
}

public class A
{
    public A(B b, IC c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C : IC {}
public interface IC {}
public class D
{
    public D(C c){}
}

public class InstanceProvider : IAsyncInstanceProvider<IC>, IAsyncInstanceProvider<D>
{
    public ValueTask<IC> GetAsync() => throw null;
    ValueTask<D> IAsyncInstanceProvider<D>.GetAsync() => throw null;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::C();
        var _3 = await ((global::StrongInject.IAsyncInstanceProvider<global::D>)this._instanceProvider).GetAsync();
        var _1 = new global::B((global::C)_2, _3);
        var _4 = await ((global::StrongInject.IAsyncInstanceProvider<global::IC>)this._instanceProvider).GetAsync();
        var _0 = new global::A((global::B)_1, _4);
        TResult result;
        try
        {
            result = await func((global::A)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::IC>)this._instanceProvider).ReleaseAsync(_4);
            await ((global::StrongInject.IAsyncInstanceProvider<global::D>)this._instanceProvider).ReleaseAsync(_3);
        }

        return result;
    }
}");
        }

        [Fact]
        public void ErrorIfMultipleInstanceProviderFieldsProvideSameType()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<int>, IAsyncContainer<string>, IAsyncContainer<bool>
{
    public InstanceProvider1 _instanceProvider1;
    internal InstanceProvider2 _instanceProvider2;
    private IAsyncInstanceProvider<int> _instanceProvider3;
}

public class InstanceProvider1 : IAsyncInstanceProvider<int>, IAsyncInstanceProvider<bool>
{
    public ValueTask<bool> GetAsync() => throw null;
    ValueTask<int> IAsyncInstanceProvider<int>.GetAsync() => throw null;
}

public class InstanceProvider2 : IAsyncInstanceProvider<string>, IAsyncInstanceProvider<bool>
{
    public ValueTask<string> GetAsync() => throw null;
    ValueTask<bool> IAsyncInstanceProvider<bool>.GetAsync() => throw null;
}

";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (7,30): Error SI0301: Both fields 'Container._instanceProvider1' and 'Container._instanceProvider2' are instance providers for 'bool'
                // _instanceProvider1
                new DiagnosticResult("SI0301", @"_instanceProvider1", DiagnosticSeverity.Error).WithLocation(7, 30),
                // (7,30): Error SI0301: Both fields 'Container._instanceProvider1' and 'Container._instanceProvider3' are instance providers for 'int'
                // _instanceProvider1
                new DiagnosticResult("SI0301", @"_instanceProvider1", DiagnosticSeverity.Error).WithLocation(7, 30),
                // (8,32): Error SI0301: Both fields 'Container._instanceProvider1' and 'Container._instanceProvider2' are instance providers for 'bool'
                // _instanceProvider2
                new DiagnosticResult("SI0301", @"_instanceProvider2", DiagnosticSeverity.Error).WithLocation(8, 32),
                // (9,41): Error SI0301: Both fields 'Container._instanceProvider1' and 'Container._instanceProvider3' are instance providers for 'int'
                // _instanceProvider3
                new DiagnosticResult("SI0301", @"_instanceProvider3", DiagnosticSeverity.Error).WithLocation(9, 41));
            comp.GetDiagnostics().Verify(
                // (8,32): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 32),
                // (9,41): Warning CS0169: The field 'Container._instanceProvider3' is never used
                // _instanceProvider3
                new DiagnosticResult("CS0169", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 41));

            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Int32>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this._instanceProvider1).GetAsync();
        TResult result;
        try
        {
            result = await func((global::System.Int32)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this._instanceProvider1).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.String>.RunAsync<TResult, TParam>(global::System.Func<global::System.String, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.String>)this._instanceProvider2).GetAsync();
        TResult result;
        try
        {
            result = await func((global::System.String)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::System.String>)this._instanceProvider2).ReleaseAsync(_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.Boolean>)this._instanceProvider1).GetAsync();
        TResult result;
        try
        {
            result = await func((global::System.Boolean)_0, param);
        }
        finally
        {
            await ((global::StrongInject.IAsyncInstanceProvider<global::System.Boolean>)this._instanceProvider1).ReleaseAsync(_0);
        }

        return result;
    }
}");
        }

        [Fact]
        public void CorrectDisposal()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;
using System;

[FactoryRegistration(typeof(A))]
[FactoryRegistration(typeof(B), Scope.SingleInstance, Scope.SingleInstance)]
[FactoryRegistration(typeof(C), Scope.InstancePerResolution, Scope.SingleInstance)]
[FactoryRegistration(typeof(D), Scope.InstancePerResolution, Scope.InstancePerResolution)]
[Registration(typeof(C))]
[Registration(typeof(E))]
[Registration(typeof(F))]
[Registration(typeof(G))]
[Registration(typeof(H))]
[Registration(typeof(I), Scope.SingleInstance)]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
    IAsyncInstanceProvider<int> instanceProvider;
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c, E e, int i){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>, IDisposable
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
    public void Dispose() {}
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
public class E : IDisposable { public E(F f) {} public void Dispose() {} }
public class F : IAsyncDisposable { public F(G g) {} ValueTask IAsyncDisposable.DisposeAsync() => default; }
public class G : IDisposable, IAsyncDisposable { public G(H h) {} void IDisposable.Dispose() {} public ValueTask DisposeAsync() => default; }
public class H { public H(I i) {} }
public class I : IDisposable { public I(int i) {} public void Dispose() {} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (10,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Registration(typeof(C))
                new DiagnosticResult("SI1001", @"Registration(typeof(C))", DiagnosticSeverity.Warning).WithLocation(10, 2));
            comp.GetDiagnostics().Verify(
                // (18,33): Warning CS0649: Field 'Container.instanceProvider' is never assigned to, and will always have its default value null
                // instanceProvider
                new DiagnosticResult("CS0649", @"instanceProvider", DiagnosticSeverity.Warning).WithLocation(18, 33));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock2.WaitAsync();
        try
        {
            await (this._disposeAction2?.Invoke() ?? default);
        }
        finally
        {
            this._lock2.Release();
        }

        await this._lock3.WaitAsync();
        try
        {
            await (this._disposeAction3?.Invoke() ?? default);
        }
        finally
        {
            this._lock3.Release();
        }
    }

    private global::BFactoryTarget _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::B _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::CFactoryTarget _singleInstanceField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction2;
    private async global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> GetSingleInstanceField2()
    {
        if (!object.ReferenceEquals(_singleInstanceField2, null))
            return _singleInstanceField2;
        await this._lock2.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = new global::C();
            var _0 = await ((global::StrongInject.IAsyncFactory<global::CFactoryTarget>)_1).CreateAsync();
            this._singleInstanceField2 = _0;
            this._disposeAction2 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(_0);
            }

            ;
        }
        finally
        {
            this._lock2.Release();
        }

        return _singleInstanceField2;
    }

    private async global::System.Threading.Tasks.ValueTask<global::B> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = new global::C();
            var _4 = await GetSingleInstanceField2();
            var _3 = new global::D(_4);
            var _2 = await ((global::StrongInject.IAsyncFactory<global::DFactoryTarget>)_3).CreateAsync();
            var _0 = new global::B((global::C)_1, _2);
            this._singleInstanceField1 = _0;
            this._disposeAction1 = async () =>
            {
                ((global::System.IDisposable)_0).Dispose();
                await global::StrongInject.Helpers.DisposeAsync(_2);
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = await GetSingleInstanceField1();
            var _0 = await ((global::StrongInject.IAsyncFactory<global::BFactoryTarget>)_1).CreateAsync();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(_0);
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    private global::I _singleInstanceField3;
    private global::System.Threading.SemaphoreSlim _lock3 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction3;
    private async global::System.Threading.Tasks.ValueTask<global::I> GetSingleInstanceField3()
    {
        if (!object.ReferenceEquals(_singleInstanceField3, null))
            return _singleInstanceField3;
        await this._lock3.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this.instanceProvider).GetAsync();
            var _0 = new global::I(_1);
            this._singleInstanceField3 = _0;
            this._disposeAction3 = async () =>
            {
                ((global::System.IDisposable)_0).Dispose();
                await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this.instanceProvider).ReleaseAsync(_1);
            }

            ;
        }
        finally
        {
            this._lock3.Release();
        }

        return _singleInstanceField3;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = await GetSingleInstanceField0();
        var _3 = await GetSingleInstanceField2();
        var _8 = await GetSingleInstanceField3();
        var _7 = new global::H((global::I)_8);
        var _6 = new global::G((global::H)_7);
        var _5 = new global::F((global::G)_6);
        var _4 = new global::E((global::F)_5);
        var _9 = await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this.instanceProvider).GetAsync();
        var _1 = new global::A(_2, _3, (global::E)_4, _9);
        var _0 = await ((global::StrongInject.IAsyncFactory<global::AFactoryTarget>)_1).CreateAsync();
        TResult result;
        try
        {
            result = await func((global::AFactoryTarget)_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(_0);
            await ((global::StrongInject.IAsyncInstanceProvider<global::System.Int32>)this.instanceProvider).ReleaseAsync(_9);
            ((global::System.IDisposable)_4).Dispose();
            await ((global::System.IAsyncDisposable)_5).DisposeAsync();
            await ((global::System.IAsyncDisposable)_6).DisposeAsync();
        }

        return result;
    }
}");
        }

        [Fact]
        public void GeneratesContainerInNamespace()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    [Registration(typeof(A))]
    public partial class Container : IAsyncContainer<A>
    {
    }

    public class A 
    {
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Container
    {
        private int _disposed = 0;
        private bool Disposed => _disposed != 0;
        public async global::System.Threading.Tasks.ValueTask DisposeAsync()
        {
            var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
            if (disposed != 0)
                return;
        }

        async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
        {
            if (Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::N.O.P.A();
            TResult result;
            try
            {
                result = await func((global::N.O.P.A)_0, param);
            }
            finally
            {
            }

            return result;
        }
    }
}");
        }

        [Fact]
        public void GeneratesContainerInNestedType()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    public partial class Outer1
    {
        public partial class Outer2
        {
            [Registration(typeof(A))]
            public partial class Container : IAsyncContainer<A>
            {
            }

            public class A 
            {
            }
        }
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Outer1
    {
        partial class Outer2
        {
            partial class Container
            {
                private int _disposed = 0;
                private bool Disposed => _disposed != 0;
                public async global::System.Threading.Tasks.ValueTask DisposeAsync()
                {
                    var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
                    if (disposed != 0)
                        return;
                }

                async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.Outer1.Outer2.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.Outer1.Outer2.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    var _0 = new global::N.O.P.Outer1.Outer2.A();
                    TResult result;
                    try
                    {
                        result = await func((global::N.O.P.Outer1.Outer2.A)_0, param);
                    }
                    finally
                    {
                    }

                    return result;
                }
            }
        }
    }
}");
        }

        [Fact]
        public void GeneratesContainerInGenericNestedType()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    public partial class Outer1<T>
    {
        public partial class Outer2<T1, T2>
        {
            [Registration(typeof(A))]
            public partial class Container : IAsyncContainer<A>
            {
            }
        }
    }

    public class A 
    {
    }
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Outer1<T>
    {
        partial class Outer2<T1, T2>
        {
            partial class Container
            {
                private int _disposed = 0;
                private bool Disposed => _disposed != 0;
                public async global::System.Threading.Tasks.ValueTask DisposeAsync()
                {
                    var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
                    if (disposed != 0)
                        return;
                }

                async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    var _0 = new global::N.O.P.A();
                    TResult result;
                    try
                    {
                        result = await func((global::N.O.P.A)_0, param);
                    }
                    finally
                    {
                    }

                    return result;
                }
            }
        }
    }
}");
        }

        [Fact]
        public void GeneratesThrowingImplementationForContainerWithMissingDependencies()
        {
            string userSource = @"
using StrongInject;

public class A {}

partial class Container : IAsyncContainer<A>
{
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (6,15): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(6, 15));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer1()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (6,22): Error SI0102: Error while resolving dependencies for 'A': 'A' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer2()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (7,22): Error SI0103: Error while resolving dependencies for 'A': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer3()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(A))]
public partial class Container : IContainer<int>
{
}

public class A : IAsyncFactory<int>
{
    public ValueTask<int> CreateAsync() => default;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (6,22): Error SI0103: Error while resolving dependencies for 'int': 'int' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer4()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(A))]
public partial class Container : IContainer<int>
{
}

public class A : IFactory<int>, IRequiresAsyncInitialization
{
    public int Create() => default;
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (6,22): Error SI0103: Error while resolving dependencies for 'int': 'StrongInject.IFactory<int>' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer5()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<int>
{
    public IAsyncInstanceProvider<int> _instanceProvider;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (4,22): Error SI0103: Error while resolving dependencies for 'int': 'int' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer6()
        {
            string userSource = @"
using StrongInject;

[FactoryRegistration(typeof(A))]
public partial class Container : IContainer<int>
{
    public IAsyncInstanceProvider<B> _instanceProvider;
}

public class A : IFactory<int>
{
    public A(B b) {}
    public int Create() => default;
}
public class B {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (5,22): Error SI0103: Error while resolving dependencies for 'int': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer7()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[FactoryRegistration(typeof(C))]
[Registration(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b) {} }
public class B {}
public class C : IAsyncFactory<B>
{
    public ValueTask<B> CreateAsync() => default;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (7,22): Error SI0103: Error while resolving dependencies for 'A': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(7, 22) );
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void CanGenerateSynchronousContainer()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = new global::B();
        var _0 = new global::A((global::B)_1);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresInitialization { public A(B b){} public void Initialize() {}}
public class B : IRequiresInitialization { public void Initialize() {} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = new global::B();
        ((global::StrongInject.IRequiresInitialization)_1).Initialize();
        var _0 = new global::A((global::B)_1);
        ((global::StrongInject.IRequiresInitialization)_0).Initialize();
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithFactories()
        {
            string userSource = @"
using StrongInject;

[FactoryRegistration(typeof(A))]
[Registration(typeof(B))]
public partial class Container : IContainer<int>
{
}

public class A : IFactory<int> { public A(B b){} public int Create() => default; }
public class B {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _2 = new global::B();
        var _1 = new global::A((global::B)_2);
        var _0 = ((global::StrongInject.IFactory<global::System.Int32>)_1).Create();
        TResult result;
        try
        {
            result = func((global::System.Int32)_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(_0);
        }

        return result;
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithInstanceProviders()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B))]
public partial class Container : IContainer<A>
{
    IInstanceProvider<int> _instanceProvider;
}

public class A { public A(B b, int i){} }
public class B {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (8,28): Warning CS0649: Field 'Container._instanceProvider' is never assigned to, and will always have its default value null
                // _instanceProvider
                new DiagnosticResult("CS0649", @"_instanceProvider", DiagnosticSeverity.Warning).WithLocation(8, 28));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = new global::B();
        var _2 = ((global::StrongInject.IInstanceProvider<global::System.Int32>)this._instanceProvider).Get();
        var _0 = new global::A((global::B)_1, _2);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
            ((global::StrongInject.IInstanceProvider<global::System.Int32>)this._instanceProvider).Release(_2);
        }

        return result;
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::B();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _0 = new global::A((global::B)_1);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void SynchronousAndAsynchronousResolvesCanShareSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B), Scope.SingleInstance)]
[Registration(typeof(C))]
[Registration(typeof(D), Scope.SingleInstance)]
public partial class Container : IContainer<A>, IAsyncContainer<C>
{
}

public class A { public A(B b){} }
public class B {}
public class C : IRequiresAsyncInitialization { public C(B b, D d) {} public ValueTask InitializeAsync() => default; }
public class D : IRequiresAsyncInitialization { public ValueTask InitializeAsync() => default; }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    private global::B _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::B GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::B();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _0 = new global::A((global::B)_1);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }

    private global::D _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::D> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::D();
            await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
            this._singleInstanceField1 = _0;
            this._disposeAction1 = async () =>
            {
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _2 = await GetSingleInstanceField1();
        var _0 = new global::C((global::B)_1, (global::D)_2);
        await ((global::StrongInject.IRequiresAsyncInitialization)_0).InitializeAsync();
        TResult result;
        try
        {
            result = await func((global::C)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void DisposalOfSingleInstanceDependency()
        {
            string userSource = @"
using System;
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IDisposable { public void Dispose(){} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::B();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)_0).Dispose();
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _0 = new global::A((global::B)_1);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void DisposalOfMultipleSingleInstanceDependencies()
        {
            string userSource = @"
using System;
using StrongInject;

[Registration(typeof(A))]
[Registration(typeof(B), Scope.SingleInstance)]
[Registration(typeof(C), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IDisposable { public B(C c){} public void Dispose(){} }
public class C : IDisposable { public void Dispose(){} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }

        this._lock1.Wait();
        try
        {
            this._disposeAction1?.Invoke();
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::B _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::C _singleInstanceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction1;
    private global::C GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::C();
            this._singleInstanceField1 = _0;
            this._disposeAction1 = () =>
            {
                ((global::System.IDisposable)_0).Dispose();
            }

            ;
        }
        finally
        {
            this._lock1.Release();
        }

        return _singleInstanceField1;
    }

    private global::B GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _1 = GetSingleInstanceField1();
            var _0 = new global::B((global::C)_1);
            this._singleInstanceField0 = _0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)_0).Dispose();
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _1 = GetSingleInstanceField0();
        var _0 = new global::A((global::B)_1);
        TResult result;
        try
        {
            result = func((global::A)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }

        [Fact]
        public void DoesNotDisposeUnusedSingleInstanceDependencies()
        {
            string userSource = @"
using System;
using StrongInject;

[Registration(typeof(A), Scope.SingleInstance)]
[Registration(typeof(B), Scope.SingleInstance)]
[Registration(typeof(C), Scope.SingleInstance)]
public partial class Container : IContainer<C>
{
}

public class A : IDisposable { public A(A a){} public void Dispose(){} }
public class B : IDisposable { public void Dispose(){} }
public class C : IDisposable { public void Dispose(){} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IAsyncContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _singleInstanceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::C GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            var _0 = new global::C();
            this._singleInstanceField0 = _0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)_0).Dispose();
            }

            ;
        }
        finally
        {
            this._lock0.Release();
        }

        return _singleInstanceField0;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        var _0 = GetSingleInstanceField0();
        TResult result;
        try
        {
            result = func((global::C)_0, param);
        }
        finally
        {
        }

        return result;
    }
}");
        }
    }
}
