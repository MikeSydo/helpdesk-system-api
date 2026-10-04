namespace HelpDesk.Domain.Entities;

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public List<Ticket> Tickets { get; private set; } = [];

    private Category(string name, string? description)
    {
        Name = name;
        Description = description;
    }
    
    public static Category Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Category(name.Trim(), description?.Trim());
    }
    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description?.Trim();
    }
}