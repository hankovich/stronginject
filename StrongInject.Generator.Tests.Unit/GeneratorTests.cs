﻿using FluentAssertions;
using Microsoft.CodeAnalysis;
using StrongInject.Runtime;
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
        public void InstancePerDependencyDependencies()
        {
            string userSource = @"
using StrongInject.Runtime;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
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
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = new global::C();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithCasts()
        {
            string userSource = @"
using StrongInject.Runtime;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
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
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = new global::C();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::IC)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::IC)_2);
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresInitialization
{
    ValueTask IRequiresInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = new global::C();
        await ((global::StrongInject.Runtime.IRequiresInitialization)_2).InitializeAsync();
        var _3 = new global::D((global::C)_2);
        await ((global::StrongInject.Runtime.IRequiresInitialization)_3).InitializeAsync();
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        await ((global::StrongInject.Runtime.IRequiresInitialization)_0).InitializeAsync();
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

[Registration(typeof(A), typeof(IFactory<AFactoryTarget>))]
[Registration(typeof(B), typeof(IFactory<BFactoryTarget>))]
[Registration(typeof(C), typeof(C), typeof(IFactory<CFactoryTarget>))]
[Registration(typeof(D), typeof(IFactory<DFactoryTarget>))]
public partial class Container : IContainer<AFactoryTarget>
{
}

public class A : IFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _4 = new global::C();
        var _7 = await ((global::StrongInject.Runtime.IFactory<global::CFactoryTarget>)_4).CreateAsync();
        var _6 = new global::D(_7);
        var _5 = await ((global::StrongInject.Runtime.IFactory<global::DFactoryTarget>)_6).CreateAsync();
        var _3 = new global::B((global::C)_4, _5);
        var _2 = await ((global::StrongInject.Runtime.IFactory<global::BFactoryTarget>)_3).CreateAsync();
        var _1 = new global::A(_2, _7);
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::AFactoryTarget>)_1).CreateAsync();
        var result = await func((global::AFactoryTarget)_0, param);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_0);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_7);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_2);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_5);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_7);
        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject.Runtime;

[Registration(typeof(A), Lifetime.SingleInstance)]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D), Lifetime.SingleInstance)]
public partial class Container : IContainer<A>
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
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::A _singleInstanceField0;
    private global::D _singleInstanceField1;
    private async System.Threading.Tasks.ValueTask<D> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        var _1 = new global::C();
        var _0 = new global::D((global::C)_1);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField1, _0, null);
        return _singleInstanceField1;
    }

    private async System.Threading.Tasks.ValueTask<A> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _2 = new global::C();
        var _3 = await GetSingleInstanceField1();
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await GetSingleInstanceField0();
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWihCasts()
        {
            string userSource = @"
using StrongInject.Runtime;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), Lifetime.SingleInstance, typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
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
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::C _singleInstanceField0;
    private async System.Threading.Tasks.ValueTask<C> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _0 = new global::C();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = await GetSingleInstanceField0();
        var _3 = new global::D((global::C)_2);
        var _1 = new global::B((global::IC)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::IC)_2);
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

[Registration(typeof(A), Lifetime.SingleInstance)]
[Registration(typeof(B))]
[Registration(typeof(C), Lifetime.SingleInstance)]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresInitialization
{
    ValueTask IRequiresInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::A _singleInstanceField0;
    private global::C _singleInstanceField1;
    private async System.Threading.Tasks.ValueTask<C> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        var _0 = new global::C();
        await ((global::StrongInject.Runtime.IRequiresInitialization)_0).InitializeAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField1, _0, null);
        return _singleInstanceField1;
    }

    private async System.Threading.Tasks.ValueTask<A> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _2 = await GetSingleInstanceField1();
        var _3 = new global::D((global::C)_2);
        await ((global::StrongInject.Runtime.IRequiresInitialization)_3).InitializeAsync();
        var _1 = new global::B((global::C)_2, (global::D)_3);
        var _0 = new global::A((global::B)_1, (global::C)_2);
        await ((global::StrongInject.Runtime.IRequiresInitialization)_0).InitializeAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await GetSingleInstanceField0();
        var result = await func((global::A)_0, param);
        return result;
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

[Registration(typeof(A), Lifetime.SingleInstance, Lifetime.InstancePerDependency, typeof(IFactory<AFactoryTarget>))]
[Registration(typeof(B), Lifetime.SingleInstance, Lifetime.SingleInstance, typeof(IFactory<BFactoryTarget>))]
[Registration(typeof(C), Lifetime.InstancePerDependency, Lifetime.SingleInstance, typeof(C), typeof(IFactory<CFactoryTarget>))]
[Registration(typeof(D), Lifetime.InstancePerDependency, Lifetime.InstancePerDependency, typeof(IFactory<DFactoryTarget>))]
public partial class Container : IContainer<AFactoryTarget>
{
}

public class A : IFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::A _singleInstanceField0;
    private global::BFactoryTarget _singleInstanceField1;
    private global::B _singleInstanceField2;
    private global::CFactoryTarget _singleInstanceField3;
    private async System.Threading.Tasks.ValueTask<CFactoryTarget> GetSingleInstanceField3()
    {
        if (!object.ReferenceEquals(_singleInstanceField3, null))
            return _singleInstanceField3;
        var _1 = new global::C();
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::CFactoryTarget>)_1).CreateAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField3, _0, null);
        return _singleInstanceField3;
    }

    private async System.Threading.Tasks.ValueTask<B> GetSingleInstanceField2()
    {
        if (!object.ReferenceEquals(_singleInstanceField2, null))
            return _singleInstanceField2;
        var _1 = new global::C();
        var _4 = await GetSingleInstanceField3();
        var _3 = new global::D(_4);
        var _2 = await ((global::StrongInject.Runtime.IFactory<global::DFactoryTarget>)_3).CreateAsync();
        var _0 = new global::B((global::C)_1, _2);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField2, _0, null);
        return _singleInstanceField2;
    }

    private async System.Threading.Tasks.ValueTask<BFactoryTarget> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        var _1 = await GetSingleInstanceField2();
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::BFactoryTarget>)_1).CreateAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField1, _0, null);
        return _singleInstanceField1;
    }

    private async System.Threading.Tasks.ValueTask<A> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _1 = await GetSingleInstanceField1();
        var _2 = await GetSingleInstanceField3();
        var _0 = new global::A(_1, _2);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _1 = await GetSingleInstanceField0();
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::AFactoryTarget>)_1).CreateAsync();
        var result = await func((global::AFactoryTarget)_0, param);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_0);
        return result;
    }
}");
        }

        [Fact]
        public void MultipleResolvesShareSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject.Runtime;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C), Lifetime.SingleInstance, typeof(C), typeof(IC))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>, IContainer<B>
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
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::C _singleInstanceField0;
    private async System.Threading.Tasks.ValueTask<C> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _0 = new global::C();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _1 = await GetSingleInstanceField0();
        var _0 = new global::A((global::IC)_1);
        var result = await func((global::A)_0, param);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::B>.RunAsync<TResult, TParam>(global::System.Func<global::B, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _1 = await GetSingleInstanceField0();
        var _2 = new global::D((global::C)_1);
        var _0 = new global::B((global::C)_1, (global::D)_2);
        var result = await func((global::B)_0, param);
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
                // (1,1): Error SI0201: Missing Type 'StrongInject.Runtime.RegistrationAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Runtime.ModuleRegistrationAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Runtime.IRequiresInitialization'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Runtime.Helpers'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void RegistersInstanceProviderFields()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IContainer<C>, IContainer<D>, IContainer<int[]>
{
    public InstanceProvider _instanceProvider1;
    internal IInstanceProvider _instanceProvider2;
    private IInstanceProvider<int[]> _instanceProvider3;
}

public class A {}
public class B {}
public class C {}
public class D {}

public class InstanceProvider : IInstanceProvider<A>, IInstanceProvider<B>
{
    public ValueTask<A> GetAsync() => throw null;
    ValueTask<B> IInstanceProvider<B>.GetAsync() => throw null;
}

public interface IInstanceProvider : IInstanceProvider<C>, IInstanceProvider<D>
{
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (8,32): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 32),
                // (9,38): Warning CS0649: Field 'Container._instanceProvider3' is never assigned to, and will always have its default value null
                // _instanceProvider3
                new DiagnosticResult("CS0649", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 38));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::A>)this._instanceProvider1).GetAsync();
        var result = await func((global::A)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::A>)this._instanceProvider1).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::B>.RunAsync<TResult, TParam>(global::System.Func<global::B, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::B>)this._instanceProvider1).GetAsync();
        var result = await func((global::B)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::B>)this._instanceProvider1).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::C>)this._instanceProvider2).GetAsync();
        var result = await func((global::C)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::C>)this._instanceProvider2).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::D>)this._instanceProvider2).GetAsync();
        var result = await func((global::D)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::D>)this._instanceProvider2).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::System.Int32[]>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32[], TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32[]>)this._instanceProvider3).GetAsync();
        var result = await func((global::System.Int32[])_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32[]>)this._instanceProvider3).ReleaseAsync(_0);
        return result;
    }
}");
        }

        [Fact]
        public void IgnoresStaticInstanceProviderFields()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IContainer<C>, IContainer<D>, IContainer<int[]>
{
    public static InstanceProvider _instanceProvider1;
    internal static IInstanceProvider _instanceProvider2;
    private static IInstanceProvider<int[]> _instanceProvider3;
}

public class A {}
public class B {}
public class C {}
public class D {}

public class InstanceProvider : IInstanceProvider<A>, IInstanceProvider<B>
{
    public ValueTask<A> GetAsync() => throw null;
    ValueTask<B> IInstanceProvider<B>.GetAsync() => throw null;
}

public interface IInstanceProvider : IInstanceProvider<C>, IInstanceProvider<D>
{
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out _, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error whilst resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container").WithLocation(5, 22),
                // (5,22): Error SI0102: Error whilst resolving dependencies for 'B': We have no source for instance of type 'B'
                // Container
                new DiagnosticResult("SI0102", @"Container").WithLocation(5, 22),
                // (5,22): Error SI0102: Error whilst resolving dependencies for 'C': We have no source for instance of type 'C'
                // Container
                new DiagnosticResult("SI0102", @"Container").WithLocation(5, 22),
                // (5,22): Error SI0102: Error whilst resolving dependencies for 'D': We have no source for instance of type 'D'
                // Container
                new DiagnosticResult("SI0102", @"Container").WithLocation(5, 22),
                // (5,22): Error SI0102: Error whilst resolving dependencies for 'int[]': We have no source for instance of type 'int[]'
                // Container
                new DiagnosticResult("SI0102", @"Container").WithLocation(5, 22));
            comp.GetDiagnostics().Verify(
                // (5,34): Error CS0535: 'Container' does not implement interface member 'IContainer<A>.ResolveAsync()'
                // IContainer<A>
                new DiagnosticResult("CS0535", @"IContainer<A>", DiagnosticSeverity.Error).WithLocation(5, 34),
                // (5,49): Error CS0535: 'Container' does not implement interface member 'IContainer<B>.ResolveAsync()'
                // IContainer<B>
                new DiagnosticResult("CS0535", @"IContainer<B>", DiagnosticSeverity.Error).WithLocation(5, 49),
                // (5,64): Error CS0535: 'Container' does not implement interface member 'IContainer<C>.ResolveAsync()'
                // IContainer<C>
                new DiagnosticResult("CS0535", @"IContainer<C>", DiagnosticSeverity.Error).WithLocation(5, 64),
                // (5,79): Error CS0535: 'Container' does not implement interface member 'IContainer<D>.ResolveAsync()'
                // IContainer<D>
                new DiagnosticResult("CS0535", @"IContainer<D>", DiagnosticSeverity.Error).WithLocation(5, 79),
                // (5,94): Error CS0535: 'Container' does not implement interface member 'IContainer<int[]>.ResolveAsync()'
                // IContainer<int[]>
                new DiagnosticResult("CS0535", @"IContainer<int[]>", DiagnosticSeverity.Error).WithLocation(5, 94),
                // (8,39): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 39),
                // (9,45): Warning CS0169: The field 'Container._instanceProvider3' is never used
                // _instanceProvider3
                new DiagnosticResult("CS0169", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 45));
        }

        [Fact]
        public void DependenciesAreOverriddenByInstanceProviderFields()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

[Registration(typeof(A))]
[Registration(typeof(B))]
[Registration(typeof(C))]
[Registration(typeof(D))]
public partial class Container : IContainer<A>
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

public class InstanceProvider : IInstanceProvider<IC>, IInstanceProvider<D>
{
    public ValueTask<IC> GetAsync() => throw null;
    ValueTask<D> IInstanceProvider<D>.GetAsync() => throw null;
}
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = new global::C();
        var _3 = await ((global::StrongInject.Runtime.IInstanceProvider<global::D>)this._instanceProvider).GetAsync();
        var _1 = new global::B((global::C)_2, _3);
        var _4 = await ((global::StrongInject.Runtime.IInstanceProvider<global::IC>)this._instanceProvider).GetAsync();
        var _0 = new global::A((global::B)_1, _4);
        var result = await func((global::A)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::IC>)this._instanceProvider).ReleaseAsync(_4);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::D>)this._instanceProvider).ReleaseAsync(_3);
        return result;
    }
}");
        }

        [Fact]
        public void ErrorIfMultipleInstanceProviderFieldsProvideSameType()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;

public partial class Container : IContainer<int>, IContainer<string>, IContainer<bool>
{
    public InstanceProvider1 _instanceProvider1;
    internal InstanceProvider2 _instanceProvider2;
    private IInstanceProvider<int> _instanceProvider3;
}

public class InstanceProvider1 : IInstanceProvider<int>, IInstanceProvider<bool>
{
    public ValueTask<bool> GetAsync() => throw null;
    ValueTask<int> IInstanceProvider<int>.GetAsync() => throw null;
}

public class InstanceProvider2 : IInstanceProvider<string>, IInstanceProvider<bool>
{
    public ValueTask<string> GetAsync() => throw null;
    ValueTask<bool> IInstanceProvider<bool>.GetAsync() => throw null;
}

";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
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
                // (9,36): Error SI0301: Both fields 'Container._instanceProvider1' and 'Container._instanceProvider3' are instance providers for 'int'
                // _instanceProvider3
                new DiagnosticResult("SI0301", @"_instanceProvider3", DiagnosticSeverity.Error).WithLocation(9, 36));
            comp.GetDiagnostics().Verify(
                // (8,32): Warning CS0649: Field 'Container._instanceProvider2' is never assigned to, and will always have its default value null
                // _instanceProvider2
                new DiagnosticResult("CS0649", @"_instanceProvider2", DiagnosticSeverity.Warning).WithLocation(8, 32),
                // (9,36): Warning CS0169: The field 'Container._instanceProvider3' is never used
                // _instanceProvider3
                new DiagnosticResult("CS0169", @"_instanceProvider3", DiagnosticSeverity.Warning).WithLocation(9, 36));

            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::System.Int32>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32>)this._instanceProvider1).GetAsync();
        var result = await func((global::System.Int32)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32>)this._instanceProvider1).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::System.String>.RunAsync<TResult, TParam>(global::System.Func<global::System.String, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.String>)this._instanceProvider2).GetAsync();
        var result = await func((global::System.String)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::System.String>)this._instanceProvider2).ReleaseAsync(_0);
        return result;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _0 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Boolean>)this._instanceProvider1).GetAsync();
        var result = await func((global::System.Boolean)_0, param);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Boolean>)this._instanceProvider1).ReleaseAsync(_0);
        return result;
    }
}");
        }

        [Fact]
        public void CorrectDisposal()
        {
            string userSource = @"
using StrongInject.Runtime;
using System.Threading.Tasks;
using System;

[Registration(typeof(A), typeof(IFactory<AFactoryTarget>))]
[Registration(typeof(B), Lifetime.SingleInstance, Lifetime.SingleInstance, typeof(IFactory<BFactoryTarget>))]
[Registration(typeof(C), Lifetime.InstancePerDependency, Lifetime.SingleInstance, typeof(C), typeof(IFactory<CFactoryTarget>))]
[Registration(typeof(D), Lifetime.InstancePerDependency, Lifetime.InstancePerDependency, typeof(IFactory<DFactoryTarget>))]
[Registration(typeof(E))]
[Registration(typeof(F))]
[Registration(typeof(G))]
[Registration(typeof(H))]
[Registration(typeof(I), Lifetime.SingleInstance)]
public partial class Container : IContainer<AFactoryTarget>
{
    IInstanceProvider<int> instanceProvider;
}

public class A : IFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c, E e, int i){}
    ValueTask<AFactoryTarget> IFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IFactory<BFactoryTarget>, IDisposable
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
    public void Dispose() {}
}
public class BFactoryTarget {}
public class C : IFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
public class E : IDisposable { public E(F f) {} public void Dispose() {} }
public class F : IAsyncDisposable { public F(G g) {} ValueTask IAsyncDisposable.DisposeAsync() => default; }
public class G : IDisposable, IAsyncDisposable { public G(H h) {} void IDisposable.Dispose() {} public ValueTask DisposeAsync() => default; }
public class H { public H(I i) {} }
public class I : IDisposable { public I(int i) {} public void Dispose() {} }
";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out var generated, MetadataReference.CreateFromFile(typeof(IContainer<>).Assembly.Location));
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (17,28): Warning CS0649: Field 'Container.instanceProvider' is never assigned to, and will always have its default value null
                // instanceProvider
                new DiagnosticResult("CS0649", @"instanceProvider", DiagnosticSeverity.Warning).WithLocation(17, 28));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private global::BFactoryTarget _singleInstanceField0;
    private global::B _singleInstanceField1;
    private global::CFactoryTarget _singleInstanceField2;
    private async System.Threading.Tasks.ValueTask<CFactoryTarget> GetSingleInstanceField2()
    {
        if (!object.ReferenceEquals(_singleInstanceField2, null))
            return _singleInstanceField2;
        var _1 = new global::C();
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::CFactoryTarget>)_1).CreateAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField2, _0, null);
        return _singleInstanceField2;
    }

    private async System.Threading.Tasks.ValueTask<B> GetSingleInstanceField1()
    {
        if (!object.ReferenceEquals(_singleInstanceField1, null))
            return _singleInstanceField1;
        var _1 = new global::C();
        var _4 = await GetSingleInstanceField2();
        var _3 = new global::D(_4);
        var _2 = await ((global::StrongInject.Runtime.IFactory<global::DFactoryTarget>)_3).CreateAsync();
        var _0 = new global::B((global::C)_1, _2);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField1, _0, null);
        return _singleInstanceField1;
    }

    private async System.Threading.Tasks.ValueTask<BFactoryTarget> GetSingleInstanceField0()
    {
        if (!object.ReferenceEquals(_singleInstanceField0, null))
            return _singleInstanceField0;
        var _1 = await GetSingleInstanceField1();
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::BFactoryTarget>)_1).CreateAsync();
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField0, _0, null);
        return _singleInstanceField0;
    }

    private global::I _singleInstanceField3;
    private async System.Threading.Tasks.ValueTask<I> GetSingleInstanceField3()
    {
        if (!object.ReferenceEquals(_singleInstanceField3, null))
            return _singleInstanceField3;
        var _1 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32>)this.instanceProvider).GetAsync();
        var _0 = new global::I(_1);
        global::System.Threading.Interlocked.CompareExchange(ref _singleInstanceField3, _0, null);
        return _singleInstanceField3;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.Runtime.IContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        var _2 = await GetSingleInstanceField0();
        var _3 = await GetSingleInstanceField2();
        var _8 = await GetSingleInstanceField3();
        var _7 = new global::H((global::I)_8);
        var _6 = new global::G((global::H)_7);
        var _5 = new global::F((global::G)_6);
        var _4 = new global::E((global::F)_5);
        var _9 = await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32>)this.instanceProvider).GetAsync();
        var _1 = new global::A(_2, _3, (global::E)_4, _9);
        var _0 = await ((global::StrongInject.Runtime.IFactory<global::AFactoryTarget>)_1).CreateAsync();
        var result = await func((global::AFactoryTarget)_0, param);
        await global::StrongInject.Runtime.Helpers.DisposeAsync(_0);
        await ((global::StrongInject.Runtime.IInstanceProvider<global::System.Int32>)this.instanceProvider).ReleaseAsync(_9);
        ((global::System.IDisposable)_4).Dispose();
        await ((global::System.IAsyncDisposable)_5).DisposeAsync();
        await ((global::System.IAsyncDisposable)_6).DisposeAsync();
        return result;
    }
}");
        }
    }
}
