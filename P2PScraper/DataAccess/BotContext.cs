namespace P2PScraper.DataAccess;

using System.Reflection;
using Microsoft.EntityFrameworkCore;
using P2PScraper.DataAccess.Entities;

public sealed class BotContext : DbContext
{
    public BotContext(DbContextOptions<BotContext> options) : base(options)
    {
        //this.Database.EnsureDeletedAsync();
        this.Database.EnsureCreatedAsync();
    }

    public DbSet<Chat> Chats { get; init; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={Assembly.GetExecutingAssembly().GetName().Name}.db");
        base.OnConfiguring(optionsBuilder);
    }
}