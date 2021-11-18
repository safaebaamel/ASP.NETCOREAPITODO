using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<ApiDbContext>(options => 
    options.UseSqlite(connectionString));

// builder.Services.AddSingleton<ItemRepository>();

var app = builder.Build();

app.MapGet("/items", async (ApiDbContext db) => {
    return await db.Items.ToListAsync();
});

app.MapPost("/items", async (ApiDbContext db, Item item) => {
    if(await db.Items.FirstOrDefaultAsync(x => x.Id == item.Id) != null) {
        return Results.BadRequest();
    } 
    
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/Items/{item.Id}", item);
});

app.MapGet("/items/{id}", async (ApiDbContext db, int id) => {
    var item = await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    return item == null ? Results.NotFound() : Results.Ok(item);
});

app.MapPut("/items/{id}", async (ApiDbContext db, int id, Item item) => {
    var existItem =  await db.Items.FirstOrDefaultAsync(x => x.Id == id);
    if(existItem == null) {
        return Results.NotFound();
    }
    existItem.title = item.title;
    existItem.IsCompleted = item.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/items/{id}", async (ApiDbContext db, int id) => {
    var existItem = await db.Items.FirstOrDefaultAsync(x => x.Id == id); 
    if(existItem == null) {
        return Results.NotFound();
    }
    db.Items.Remove(existItem);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapGet("/", () => "Hello From Minimal Api");
app.Run();

// record Item(int id, string title, bool IsCompleted);

class Item {
    public int Id { get; set; }
    public string? title { get; set; }
    public bool IsCompleted { get; set; }
}

// class ItemRepository 
// {
    // private Dictionary<int, Item> items = new Dictionary<int, Item>();

    // public ItemRepository()
    // {
    //     var item1 = new Item(1, "Buy Groceries ", false);
    //     var item2 = new Item(2, "Drink 1L of water ", true);
    //     var item3 = new Item(3, "Pay bills ", false);
    //     items.Add(item1.id, item1);
    //     items.Add(item2.id, item2);
    //     items.Add(item3.id, item3);
    // }

    // public IEnumerable<Item> GetAll() => items.Values;

    // public Item? GetById(int id) {
    //   if(items.ContainsKey(id)) {
    //     return items[id];
    //   }
    //   return null;
    // }

    // public void Add(Item item) => items.Add(item.id, item);

    // public void Update(Item item) => items[item.id] = item;

    // public void Delete(int id) => items.Remove(id);

// }

class ApiDbContext: DbContext
{
    public DbSet<Item>? Items { get; set; }
    
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {

    }

}
