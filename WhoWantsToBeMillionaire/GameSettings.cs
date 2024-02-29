using System.Collections.Generic;

namespace WhoWantsToBeMillionaire
{
    enum GSettings
    {
        IsPlaySounds,
        SequentialDisplayOptions,
        FastHintResponse
    }

    class GameSettings
    {
        private readonly Dictionary<GSettings, bool> settings;

    }
}
