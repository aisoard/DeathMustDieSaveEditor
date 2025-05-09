using Newtonsoft.Json;
using DeathMustDieSaveEditor.Core.Models.SaveStructure;
using System.Text.Json.Serialization;

namespace DeathMustDieSaveEditor.Core.Models.SaveStructure
{
    [Serializable]
    public class SerializedSaveData
    {
        public IList<string> keys { get; set; }
        public IList<string> values { get; set; }

        public Progression GetProgression()
        {
            string prog = values[0];
            var res = JsonConvert.DeserializeObject<Progression>(prog);
            return res;
        }

        public string GetNotParsedProgression()
        {
            string prog = values[0];
            return prog;
        }

        public void SaveProgression(Progression progression)
        {
            var serializedProgression = JsonConvert.SerializeObject(progression);
            this.values[0] = serializedProgression;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int Version { get; set; }
        public bool IsAutosave { get; set; }
        public SerializedSaveData serializedSaveData { get; set; }
        public string thumbnailBase64 { get; set; }
        public string playthroughGuidString { get; set; }
    }

    [Serializable]
    public class InventoryData
    {
        public string CharacterCode { get; set; }
        public string Json { get; set; }
    }

    [Serializable]
    public class EquipmentStateWrapper
    {
        public IList<EquipmentState> LoadoutStates { get; set; }
        public IList<bool> LoadoutUnlocks { get; set; }
        public int SelectedLoadout { get; set; }
        public EquipmentState EquipmentState { get; set; }
    }

    [Serializable]
    public class EquipmentState
    {
        public IList<Item> Items { get; set; }
        public IList<ItemSlot> ItemSlots { get; set; }
    }

    [Serializable]
    public class Affix
    {
        public string Code { get; set; }
        public int Levels { get; set; }
    }

    [Serializable]
    public class StashState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<Item> Items { get; set; }
        public IList<ItemSlot> ItemSlots { get; set; }
    }

    [Serializable]
    public class Item
    {
        public string Code { get; set; }
        public int Type { get; set; }
        public int Rarity { get; set; }
        public int TierIndex { get; set; }
        public bool IsUnique { get; set; }
        public string SubtypeCode { get; set; }
        public string IconVariant { get; set; }
        public string DropVariant { get; set; }
        public IList<Affix> Affixes { get; set; }
        public bool WasOwnedByPlayer { get; set; }
    }

    [Serializable]
    public class Id
    {
        public int _value { get; set; }
    }

    [Serializable]
    public class ItemSlot
    {
        public bool IsEmpty { get; set; }
        public Id Id { get; set; }
    }

    [Serializable]
    public class Entry
    {
        public Id Id { get; set; }
        public Item Item { get; set; }
    }

    [Serializable]
    public class RepoState
    {
        public Id LastId { get; set; }
        public IList<Entry> Entries { get; set; }

        public Item? GetItemById(Id id)
        {
            var item = Entries.FirstOrDefault(x => x.Id._value == id._value);
            return item?.Item;
        }
    }

    [Serializable]
    public class LibraryState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<Item> Items { get; set; }
        public IList<ItemSlot> ItemSlots { get; set; }
    }

    [Serializable]
    public class BackpackState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<Item> Items { get; set; }
        public IList<ItemSlot> ItemSlots { get; set; }
    }

    [Serializable]
    public class Stashes
    {
        public IList<string> StashJsons { get; set; }
    }

    [Serializable]
    public class Progression
    {
        public string ProgressionJson { get; set; }
        public string DarknessJson { get; set; }
        public string DarknessModesJson { get; set; }
        public string AchievementsJson { get; set; }
        public string TalentsJson { get; set; }
        public string UnlocksJson { get; set; }
        public RepoState PlayerRepoState { get; set; }
        public RepoState ShopRepoState { get; set; }
        public IList<InventoryData> InventoryData { get; set; }
        public string ShopJson { get; set; }
        public Stashes Stashes { get; set; }
        public string StashJson { get; set; }
        public StashState StashState { get; set; }
        public LibraryState LibraryState { get; set; }
        public BackpackState BackpackState { get; set; }
        public string StashUpgradesJson { get; set; }
        public int Gold { get; set; }

        public IEnumerable<Item> GetEquippedItems(string charecterCode)
        {
            var charEquipped = this.InventoryData.Where(x => x.CharacterCode == charecterCode).FirstOrDefault();

            var res = JsonConvert.DeserializeObject<EquipmentStateWrapper>(charEquipped.Json);

            var loadout = res.LoadoutStates[res.SelectedLoadout];
            var itemSlots = loadout.ItemSlots;
            var playerRepo = this.PlayerRepoState;

            var items = itemSlots.Select(x => playerRepo.GetItemById(x.Id)).ToList();

            return items;
        }

        public void SetEquippedItems(string charecterCode, IEnumerable<Item> items)
        {
            var charEquipped = this.InventoryData.Where(x => x.CharacterCode == charecterCode).FirstOrDefault();

            var res = JsonConvert.DeserializeObject<EquipmentStateWrapper>(charEquipped.Json);
            res.EquipmentState.Items = items.ToList();

            var serializedEquippment = JsonConvert.SerializeObject(res);
            charEquipped.Json = serializedEquippment;
        }
    }
}
