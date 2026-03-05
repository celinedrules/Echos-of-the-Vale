// Done
namespace Core.Interfaces
{
    public interface ICounterable
    {
        public bool CanBeCountered { get; }
        public void HandleCounter();
    }
}