using System;

namespace Vk.Post.Predict.Entities;

public readonly struct MessageId : IEquatable<MessageId>
{
    public int Id { get; }

    public int OwnerId { get; }

    public MessageId(int id, int ownerId)
    {
        Id = id;
        OwnerId = ownerId;
    }

    public bool Equals(MessageId other)
    {
        return other.OwnerId == OwnerId && other.Id == Id;
    }

    public override string ToString()
    {
        return $"{OwnerId}_{Id}";
    }

    public override bool Equals(object obj)
    {
        return obj is MessageId id && Equals(id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, OwnerId);
    }

    public static bool operator ==(MessageId left, MessageId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MessageId left, MessageId right)
    {
        return !(left == right);
    }
}
