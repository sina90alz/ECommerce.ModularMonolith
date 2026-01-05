namespace Products.Contracts;

public record ProductSnapshotDto(
    Guid Id,
    string Name,
    decimal Price);
