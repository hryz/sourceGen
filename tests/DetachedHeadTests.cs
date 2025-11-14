using System;
using System.Collections.Generic;
using System.Linq;
using db;
using FluentAssertions;
using Xunit;

namespace tests;

public class DetachedHeadTests
{
    private record Model(int X, int Y);
        
    [Fact]
    public void ZeroElementsBehavior()
    {
        var dhm = new DetachedHeadMap<int, Model>();
        Func<Model> a = () => dhm[1];
        a.Should().Throw<Exception>();
            
        dhm.ContainsKey(1).Should().BeFalse();
        dhm.TryGetValue(1, out var v).Should().BeFalse(); v.Should().Be(default);
        dhm.IsEmpty.Should().BeTrue();
        dhm.Count.Should().Be(0);
        dhm.Keys.Should().BeEmpty();
        dhm.Values.Should().BeEmpty();
        dhm.Should().BeEmpty();

        dhm.TryRemove(1, out var remVal).Should().BeFalse(); remVal.Should().Be(default);
    }
        
    [Fact]
    public void OneElementBehavior()
    {
        var value = new Model(1, 1);
        var dhm = new DetachedHeadMap<int, Model>();
        dhm[1] = value;

        dhm[1].Should().Be(value);
        dhm.ContainsKey(1).Should().BeTrue();
        dhm.ContainsKey(2).Should().BeFalse();
        dhm.TryGetValue(1, out var v).Should().BeTrue(); v.Should().Be(value);
        dhm.TryGetValue(2, out var v2).Should().BeFalse(); v2.Should().Be(default);
        dhm.IsEmpty.Should().BeFalse();
        dhm.Count.Should().Be(1);
        dhm.Keys.Should().Contain(1);
        dhm.Values.Should().Contain(value);
        dhm.Any(x => x.Key == 1).Should().BeTrue();

        var value2 = new Model(2, 2);
        dhm[1] = value2;
        dhm[1].Should().Be(value2);

        Func<Model> getMissing = () => dhm[2];
        getMissing.Should().Throw<Exception>();
            
        dhm.TryRemove(2, out var remVal2).Should().BeFalse(); remVal2.Should().Be(default);
            
        dhm.TryRemove(1, out var remVal).Should().BeTrue(); remVal.Should().Be(value2);
    }
        
    [Fact]
    public void ManyElementsBehavior()
    {
        var value = new Model(1, 1);
        var value99 = new Model(99,99);
        var dhm = new DetachedHeadMap<int, Model>();
        dhm[1] = value;
        dhm[99] = value99;

        dhm[1].Should().Be(value);
        dhm.ContainsKey(1).Should().BeTrue();
        dhm.ContainsKey(99).Should().BeTrue();
        dhm.ContainsKey(2).Should().BeFalse();
        dhm.TryGetValue(1, out var v).Should().BeTrue(); v.Should().Be(value);
        dhm.TryGetValue(99, out var v99).Should().BeTrue(); v99.Should().Be(value99);
        dhm.TryGetValue(2, out var v2).Should().BeFalse(); v2.Should().Be(default);
        dhm.IsEmpty.Should().BeFalse();
        dhm.Count.Should().Be(2);
        dhm.Keys.Should().Contain(1);
        dhm.Keys.Should().Contain(99);
        dhm.Values.Should().Contain(value);
        dhm.Values.Should().Contain(value99);
        dhm.Any(x => x.Key == 1).Should().BeTrue();
        dhm.Any(x => x.Key == 99).Should().BeTrue();
            
        var value2 = new Model(2, 2);
        dhm[1] = value2;
        dhm[1].Should().Be(value2);
            
        var value100 = new Model(100, 100);
        dhm[99] = value100;
        dhm[99].Should().Be(value100);

        Func<Model> getMissing = () => dhm[2];
        getMissing.Should().Throw<Exception>();
            
        dhm.TryRemove(2, out var remVal2).Should().BeFalse(); remVal2.Should().Be(default);
            
        dhm.TryRemove(1, out var remVal).Should().BeTrue(); remVal.Should().Be(value2);
            
        dhm.TryRemove(99, out var remVal99).Should().BeTrue(); remVal99.Should().Be(value100);
    }

    [Fact]
    public void StateTransitions()
    {
        var value1 = new Model(1,1);
        var value2 = new Model(2,2);
        var dhm = new DetachedHeadMap<int, Model>();

        // zero:
        dhm.ZeroElements().Should().BeTrue();
        dhm.OneElement().Should().BeFalse();
        dhm.ManyElements().Should().BeFalse();

        // one:
        dhm[1] = value1;
        dhm.ZeroElements().Should().BeFalse();
        dhm.OneElement().Should().BeTrue();
        dhm.ManyElements().Should().BeFalse();
            
        // many:
        dhm[2] = value2;
        dhm.ZeroElements().Should().BeFalse();
        dhm.OneElement().Should().BeFalse();
        dhm.ManyElements().Should().BeTrue();
            
        // one' (shrink back doesn't work!)
        dhm.TryRemove(2, out _);
        dhm.Count.Should().Be(1);
        dhm.ZeroElements().Should().BeFalse();
        dhm.OneElement().Should().BeFalse();
        dhm.ManyElements().Should().BeTrue();
            
        // zero' (shrink back doesn't work!)
        dhm.TryRemove(1, out _);
        dhm.Count.Should().Be(0);
        dhm.IsEmpty.Should().BeTrue();
        dhm.ZeroElements().Should().BeFalse();
        dhm.OneElement().Should().BeFalse();
        dhm.ManyElements().Should().BeTrue();

    }
}