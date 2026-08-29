using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;

namespace Game.Tests;

public class ObjectPoolTests
{
    [Fact]
    public void Pool_ReturnsAndReusesObject()
    {
        var provider = new DefaultObjectPoolProvider();
        var pool = provider.Create(new DefaultPooledObjectPolicy<List<int>>());
        
        var list1 = pool.Get();
        list1.Add(1);
        pool.Return(list1);
        
        var list2 = pool.Get();
        list2.Should().BeSameAs(list1);
        list2.Count.Should().Be(1);
    }
}
