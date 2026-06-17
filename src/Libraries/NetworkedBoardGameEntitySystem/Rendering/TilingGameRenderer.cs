using System;
using System.Collections.Generic;
using Godot;
using TT2026.Game;
using TT2026.Game.Entities;
using TT2026.libraries.Izzy.Geometry;

namespace TT2026.libraries.NetworkedBoardGameEntitySystem.Rendering;

/// <summary>
/// A base renderer variant that shows the game board as a grid of square tiles, which the implementing
/// renderer can then color as it pleases
/// </summary>
public abstract partial class TilingGameRenderer : GameRenderer
{
    [Export] private int _width;
    [Export] private int _height;
    [Export] private QuadMesh _tileMesh;
    [Export] private StandardMaterial3D _tileMaterial;

    private MeshInstance3D[] _tiles;
    private Dictionary<ICoordinate2d, int> _tileIDByCoordinate = new ();

    public override void Initialize()
    {
        RandomNumberGenerator random = new();
        if (_width == 0 || _height == 0 || _tileMesh is null || _tileMaterial is null)
        {
            throw new ArgumentException($"Not all TilingGameRenderer fields have been set up");
        }
        
        _tiles = new MeshInstance3D[_width * _height];
        int i = 0;
        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                _tileIDByCoordinate.Add(new SquareCoordinate(x, y), i);
                
                var meshInstance = new MeshInstance3D();
                _tiles[i] = meshInstance;
                AddChild(meshInstance);
                meshInstance.SetPosition(new Vector3(x, 0, y));
                meshInstance.SetMesh(_tileMesh);
                StandardMaterial3D material = (StandardMaterial3D)_tileMaterial.Duplicate();
                material.AlbedoColor = new Color(random.Randf(), random.Randf(), random.Randf());
                meshInstance.SetSurfaceOverrideMaterial(0, material);
                meshInstance.SetRotationDegrees(new Vector3(-90, 0, 0));
                i++;
            }
    }
    
    protected abstract void OnTileClicked(int x, int y);

    public override void FullRefresh()
    {
        
    }

    public MeshInstance3D GetTile(int tileId) => _tiles[tileId];
    
    public MeshInstance3D GetTile(ICoordinate2d coordinate)
    {
        return _tiles[_tileIDByCoordinate[coordinate]];
    }
    
    public int GetTileId(ICoordinate2d coordinate) => _tileIDByCoordinate[coordinate];

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
            {
                Vector2 clickPosition = CameraController.RaycastFromScreenToGameMap(CameraController.Camera.GetViewport().GetMousePosition());
                int roundedX = Mathf.Clamp(Mathf.RoundToInt(clickPosition.X), 0,  _width - 1);
                int roundedY = Mathf.Clamp(Mathf.RoundToInt(clickPosition.Y), 0, _height - 1);
                
                OnTileClicked(roundedX, roundedY);
            }
        }
    }
}