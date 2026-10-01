using Godot;
using Godot.Collections;
using RogueMaker.Core.Enemies;
using RogueMaker.Core.Map;

[GlobalClass]
public partial class GameTextures : Resource
{
    [Export]
    public Texture2D Player { get; set; } = null!;

    [Export]
    public Dictionary<SurfaceType, Texture2D> Surfaces { get; set; } = new();

    [Export]
    public Dictionary<EnemyTypeId, Texture2D> Enemies { get; set; } = new();
}
