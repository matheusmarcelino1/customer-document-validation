namespace CustomerDocumentValidation.SharedKernel.Primitives;

public abstract class Entity
{
    protected Entity(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Entity id cannot be empty.", nameof(id));

        Id = id;
    }

    public string Id { get; }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Id == other.Id && GetType() == other.GetType();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(Entity? left, Entity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Entity? left, Entity? right)
    {
        return !Equals(left, right);
    }
}