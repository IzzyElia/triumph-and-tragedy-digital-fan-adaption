using Godot;

namespace TT2026.Game.Rendering.BoardObjects;

public partial class UnitBoardObject : Node3D
{
    private const string _prefabUid = "uid://b8sxti0tx75u7";
    public static PackedScene Prefab => GD.Load<PackedScene>(_prefabUid);
    
    [Export] private MeshInstance3D _backgroundMesh;
    public ShaderMaterial BackgroundMaterial => (ShaderMaterial)_backgroundMesh.GetSurfaceOverrideMaterial(0);

    [Export] private MeshInstance3D _iconMesh;
    public StandardMaterial3D IconMaterial => (StandardMaterial3D)_iconMesh.GetSurfaceOverrideMaterial(0);
    
    [Export] private MeshInstance3D _pipsMesh;
    public ShaderMaterial PipsMaterial => (ShaderMaterial)_pipsMesh.GetSurfaceOverrideMaterial(0);



    public int BoardSpaceId = -1;
}