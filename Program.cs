using Google.Protobuf.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.ValueConversion;

await using var ctx = new AppContext();
await ctx.Database.EnsureDeletedAsync();
await ctx.Database.EnsureCreatedAsync();

var myEntities = await ctx.MyEntities.AsNoTracking().ToListAsync();

public class AppContext : DbContext
{
    public DbSet<MyEntity> MyEntities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .UseNpgsql(@"Host=127.0.0.1;Port=5432;Username=test;Password=test;Database=test")
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<MyEntity>(b =>
        {
            b.Property(p => p.Tags)
            .HasConversion(
                new NpgsqlArrayConverter<RepeatedField<string>, string[]>(
                    new ValueConverter<RepeatedField<string>, string[]>(
                        rf => rf.ToArray(),
                        values => MyRepeatedFieldExtensions.FromCollection(values)
                    )
                )
            )
            .HasColumnType("text[]");
        });
    }
}

public record MyEntity(int Id, RepeatedField<string> Tags);

public static class MyRepeatedFieldExtensions
{
    public static RepeatedField<T> FromCollection<T>(IEnumerable<T> values)
    {
        var rf = new RepeatedField<T>();
        rf.AddRange(values);
        return rf;
    }
}
