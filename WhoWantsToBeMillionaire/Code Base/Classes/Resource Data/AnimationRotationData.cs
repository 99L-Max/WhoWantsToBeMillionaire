using Newtonsoft.Json;

namespace WhoWantsToBeMillionaire
{
    class AnimationRotationData
    {
        [JsonConstructor]
        public AnimationRotationData(float scale, float compression, bool isFront)
        {
            Scale = scale;
            Compression = compression;
            IsFront = isFront;
        }

        public float Scale { get; }
        public float Compression { get; }
        public bool IsFront { get; }
    }
}
