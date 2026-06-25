using Godot;

namespace TT2026.Game.Rendering.BoardObjects;

public partial class StaticBoardObject : Node3D
{
    [Export] public MeshInstance3D BackgroundMesh;
    [Export] public MeshInstance3D IconMesh;
    [Export] public Label3D IconLabel;

    public StandardMaterial3D IconMaterial => (StandardMaterial3D)IconMesh.GetSurfaceOverrideMaterial(0);
    public StandardMaterial3D BackgroundMaterial => (StandardMaterial3D)BackgroundMesh.GetSurfaceOverrideMaterial(0);
}