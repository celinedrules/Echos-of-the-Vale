// Done
using SaveSystem;

namespace Core.Interfaces
{
    public interface ISavable
    {
        public void LoadData(GameData gameData);
        public void SaveData(ref GameData gameData);
    }
}