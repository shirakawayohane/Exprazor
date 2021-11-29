using Xunit;
using Exprazor;
using static Exprazor.ExprazorApp;
using System.Collections.Generic;
using System;

namespace Exprazor.Tests
{
    //public class TestPropsChanged
    //{
    //    [Fact]
    //    public void TestReturnsTrueWhenChangedWithSameKeys()
    //    {
    //        Action oldOnclickAction = () => { Console.WriteLine("clicked!"); };
    //        Action newOnclickAction = () => { Console.WriteLine("clicked!"); };
    //        Dictionary<string, object> props1 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = oldOnclickAction
    //        };

    //        Dictionary<string, object> props2 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = newOnclickAction
    //        };

    //        Assert.True(PropsChanged(props1, props2));
    //    }
    //    [Fact]
    //    public void TestReturnsTrueWhenChangedWithDifferentKeys()
    //    {
    //        Action oldOnclickAction = () => { Console.WriteLine("clicked!"); };
    //        Action newOnclickAction = () => { Console.WriteLine("clicked!"); };
    //        Dictionary<string, object> props1 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = oldOnclickAction
    //        };

    //        Dictionary<string, object> props2 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = newOnclickAction,
    //            ["data-foo"] = "bar"
    //        };

    //        Assert.True(PropsChanged(props1, props2));
    //    }

    //    [Fact]
    //    public void TestReturnsFalseWhenNotChanged()
    //    {
    //        Action onclickAction = () => { Console.WriteLine("clicked!"); };

    //        Dictionary<string, object> props1 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = onclickAction
    //        };

    //        Dictionary<string, object> props2 = new()
    //        {
    //            ["class"] = "btn btn-primary",
    //            ["onclick"] = onclickAction
    //        };

    //        Assert.False(PropsChanged(props1, props2));
    //    }

    //    [Fact]
    //    public void TestReturnsFalseWhenBothNull()
    //    {
    //        Assert.False(PropsChanged(null, null));
    //    }
    //    [Fact]
    //    public void TestReturnsTrueWhenEitherOneNull()
    //    {
    //        Dictionary<string, object> props = new()
    //        {
    //            ["class"] = "icon chevron-right",
    //            ["aria-expanded"] = false,
    //        };

    //        Assert.True(PropsChanged(props, null));
    //        Assert.True(PropsChanged(null, props));
    //    }
    //}
}