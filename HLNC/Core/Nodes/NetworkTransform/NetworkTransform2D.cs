using Godot;
using HLNC.Utils;

namespace HLNC.Utilities
{
    public partial class NetworkTransform2D : NetworkNode
    {
        [Export]
        public Node2D SourceNode { get; set; }

        [Export]
        public Node2D TargetNode { get; set; }

        [NetworkProperty]
        public bool IsTeleporting { get; set; }

        [NetworkProperty]
        public Vector2 NetPosition { get; set; }

        [NetworkProperty]
        public float NetRotation { get; set; }

        private bool _isTeleporting = false;
        public void OnNetworkChangeIsTeleporting(Tick tick, bool from, bool to)
        {
            _isTeleporting = true;
        }

        /// <inheritdoc/>
        public override void _WorldReady()
        {
            base._WorldReady();
            TargetNode ??= GetParent2D();
            SourceNode ??= GetParent2D();
            if (GetMeta("import_from_external", false).AsBool())
            {
                SourceNode.Position = NetPosition;
                SourceNode.Rotation = NetRotation;
                TargetNode.Position = NetPosition;
                TargetNode.Rotation = NetRotation;
            }
        }

        public Node2D GetParent2D()
        {
            var parent = GetParent();
            if (parent is Node2D)
            {
                return (Node2D)parent;
            }
            Debugger.Log("NetworkTransform parent is not a Node2D", Debugger.DebugLevel.ERROR);
            return null;
        }

        bool teleportExported = false;

        /// <inheritdoc/>
        public override void _NetworkProcess(int tick)
        {
            base._NetworkProcess(tick);
            if (NetworkRunner.Instance.IsClient)
            {
                return;
            }
            NetPosition = SourceNode.Position;
            NetRotation = SourceNode.Rotation;
            if (IsTeleporting)
            {
                if (teleportExported)
                {
                    IsTeleporting = false;
                    teleportExported = false;
                }
                else
                {
                    teleportExported = true;
                }
            }
        }

        public double NetworkLerpNetPosition(Variant from, Variant to, double weight)
        {
            if (_isTeleporting)
            {
                NetPosition = (Vector2)to;
                _isTeleporting = false;
                return 1;
            }

            return -1;
        }

        public double NetworkLerpNetRotation(Variant from, Variant to, double weight)
        {
            float start = (float)from;
            float end = (float)to;
            NetRotation = Mathf.LerpAngle(start, end, (float)weight);
            return weight;
        }

        /// <inheritdoc/>
        public override void _PhysicsProcess(double delta)
        {
            if (Engine.IsEditorHint())
            {
                return;
            }
            base._PhysicsProcess(delta);
            if (NetworkRunner.Instance.IsServer)
            {
                return;
            }
            if (!Network.IsWorldReady) return;
            TargetNode.Position = NetPosition;
            TargetNode.Rotation = NetRotation;
        }

        public void Teleport(Vector2 incoming_position)
        {
            TargetNode.Position = incoming_position;
            IsTeleporting = true;
        }
    }
}