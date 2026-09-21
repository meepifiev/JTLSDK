using System.Collections.Generic;
using JTLStudio.SDK.Providers;

namespace JTLStudio.SDK.Editor.Toolkit.Data
{
    public class ModuleSlots
    {
        private readonly List<ModuleSlot> _slots = new List<ModuleSlot>
        {
            new ModuleSlot("module.platform", "_platformProvider", typeof(IPlatformProvider)),
            new ModuleSlot("module.ads", "_ads", typeof(IAdsProvider)),
            new ModuleSlot("module.saves", "_data", typeof(IDataProvider)),
            new ModuleSlot("module.purchases", "_payments", typeof(IPaymentsProvider)),
            new ModuleSlot("module.language", "_languageProvider", typeof(ILanguageProvider)),
            new ModuleSlot("module.player", "_player", typeof(IPlayerProvider)),
            new ModuleSlot("module.leaderboards", "_leaderboards", typeof(ILeaderboardsProvider)),
            new ModuleSlot("module.flags", "_flags", typeof(IFlagsProvider)),
            new ModuleSlot("module.time", "_timeProvider", typeof(ITimeProvider)),
            new ModuleSlot("module.gameEvents", "_gameEvents", typeof(IGameEventsProvider)),
            new ModuleSlot("module.review", "_review", typeof(IReviewProvider)),
            new ModuleSlot("module.gameLabel", "_gameLabel", typeof(IGameLabelProvider))
        };

        public IReadOnlyList<ModuleSlot> All => _slots;

        public ModuleSlot Find(string propertyName)
        {
            return _slots.Find(slot => slot.PropertyName == propertyName);
        }
    }
}
