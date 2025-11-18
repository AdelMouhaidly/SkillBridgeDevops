using Microsoft.EntityFrameworkCore;
using SkillBridge.API.Data;

namespace SkillBridge.Tests.Common;

public static class TestDbContextFactory
{
    public static SkillBridgeContext CreateContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<SkillBridgeContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new SkillBridgeContext(options);
    }
}
