using System;
using Godot;

namespace TT2026.Game;

public partial class CameraController : Camera3D
{
    // Singleton
    public static CameraController Camera;
    
    // Camera control
    private float _panSpeed = 0.4f;
    private float _zoomSpeed = 12f;
    private Vector3 _queuedMovement;
    private Vector3 _internalPosition;
    private bool _cameraChangedSinceLastUpdate;
    private float _zoom = 2f;
    private float _perspectiveZoomExponant = 2;
    private float _orthagonalY;
    private Plane ProjectionPlane;
    private float PerspectiveAdjustedZoom => Mathf.Pow(_zoom, _perspectiveZoomExponant);
    private const float BaseRotationAngle = -60f;

    public override void _EnterTree()
    {
        if (Camera is not null) throw new InvalidOperationException();
        Camera = this;
    }

    override public void _ExitTree()
    {
        Camera = null;
    }

    public override void _Ready()
    {
        base._Ready();
        ProjectionPlane = new Plane(new Vector3(0, 1, 0));
        _internalPosition = new Vector3(0, 10, 0);
    }

    public static Vector2 RaycastFromScreenToGameMap(Vector2 screenPoint)
    {
        Vector3 rayOrigin = Camera.ProjectRayOrigin(screenPoint);
        Vector3 rayDirection = Camera.ProjectRayNormal(screenPoint);

        // Find the intersection point
        Vector3? intersection = Camera.ProjectionPlane.IntersectsRay(rayOrigin, rayDirection);

        if (intersection.HasValue)
        {
            return new Vector2(intersection.Value.X,  intersection.Value.Z);
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    public void MoveCamera(float x = 0, float y = 0, float zoom = 0)
    {
        _queuedMovement += new Vector3(x * _panSpeed, zoom * _zoomSpeed, y * _panSpeed);
        _cameraChangedSinceLastUpdate = true;
    }

    
    public override void _Process(double deltaTime)
    {
        if (Input.IsPhysicalKeyPressed(Key.W)) MoveCamera(0, -1);
        if (Input.IsPhysicalKeyPressed(Key.S)) MoveCamera(0, 1);
        if (Input.IsPhysicalKeyPressed(Key.A)) MoveCamera(-1, 0);
        if (Input.IsPhysicalKeyPressed(Key.D)) MoveCamera(1, 0);
        if (_cameraChangedSinceLastUpdate)
        {
            _zoom += _queuedMovement.Z;
            if (_zoom < 0.1f) _zoom = 0.1f;
            _internalPosition += new Vector3(_queuedMovement.X, _queuedMovement.Y, 0);
            _queuedMovement = Vector3.Zero;

            switch (Projection)
            {
                case Camera3D.ProjectionType.Orthogonal:
                    float rotationAngle = Mathf.DegToRad(BaseRotationAngle);
                    Rotation = new Vector3(rotationAngle, 0, 0);
                    Size = Mathf.Exp(_zoom * 0.1f);
                    Position = (new Vector3(0, -Mathf.Sin(rotationAngle),
                        Mathf.Cos(rotationAngle)) * Size) + _internalPosition;
                    break;
                case Camera3D.ProjectionType.Perspective:
                    Rotation = new Vector3(Mathf.DegToRad(BaseRotationAngle), 0, 0);
                    Position = new Vector3(_internalPosition.X, _internalPosition.Y, _zoom);
                    break;
                default: throw new NotSupportedException();
            }
        }

        _cameraChangedSinceLastUpdate = false;
    }

    public void CenterOrthagonalPosition(float mapSize)
    {
        if (Projection == ProjectionType.Orthogonal)
        {
            float rotationAngle = Mathf.DegToRad(BaseRotationAngle);
            if (Projection == ProjectionType.Orthogonal) {
            }
        }
    }
    
    
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        // Most mouse wheels are read in godot as button inputs, so we check for those...
        if (@event is InputEventMouseButton mouseInput){
            if (mouseInput.ButtonIndex == MouseButton.WheelUp){
                MoveCamera(0, 0, -0.1f);
            }
            if (mouseInput.ButtonIndex == MouseButton.WheelDown){
                MoveCamera(0, 0, 0.1f);
            }
        }

        // ...but on mac, it's a gesture. So we check for that as well
        if (@event is InputEventPanGesture gesture)
        {
            MoveCamera(0, 0, gesture.Delta.Y);
        }
    }
}