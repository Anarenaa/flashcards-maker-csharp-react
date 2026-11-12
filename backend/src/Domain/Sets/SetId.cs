namespace Domain.Sets;

public record SetId(long Value)
{
    public static SetId Empty()
    {
        return new SetId(0);
    }

    public static SetId New()
    {
        return new SetId(0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}