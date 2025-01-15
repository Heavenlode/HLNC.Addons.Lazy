using Godot;

namespace HLNC.Serialization
{
    [Tool]
    public partial class CollectedNetworkProperty : GodotObject
    {
        public string NodePath;
        public string Name;
        public Variant.Type Type;
        public byte Index;
        public VariantSubtype Subtype;
        public long InterestMask;
        public Callable NetworkSerialize;
        public Callable NetworkDeserialize;
        public Callable BsonDeserialize;
    }
}