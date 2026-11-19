namespace BookCenter.Domain.Base
{
    public class BaseNameableEntity : BaseEntity
    {
        public string Name { get; set; } = null!;
    }
}
