using System;
using System.Collections.Generic;
using System.Text;

namespace SmartCare.SharedKernel;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();  // generated in C#, works on SQLite AND Postgres later

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}
