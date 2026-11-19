namespace BookCenter.Application.DTOs.Authors
{
    public class CreateAuthorDto
    {
        public string Name { get; set; } = null!;
        public string? Biography { get; set; }
    }
}
