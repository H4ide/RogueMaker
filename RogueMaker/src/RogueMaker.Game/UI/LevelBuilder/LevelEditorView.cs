using Godot;

/// <summary>
/// Controls the camera used to navigate the level grid inside its sub-viewport. 
/// So, it doesn't work if you hover the grid outside of the sub-viewport. e.g if you hover on tools menu. 
/// The camera can be zoomed in and out with the mouse wheel, and dragged with the middle mouse button. 
/// The camera is clamped to the level grid bounds, so it cannot be moved outside of the grid.
/// </summary>
public partial class LevelEditorView : Camera2D
{
    private const float MinimumZoom = 0.2f;
    private const float MaximumZoom = 1.0f;
    private const float ZoomStep = 0.2f;

    private LevelGrid _levelGrid = null!;
    private bool _isDragging;

    /// <inheritdoc />
    public override void _Ready()
    {
        _levelGrid = GetNode<LevelGrid>("../LevelGrid");
        MakeCurrent();
    }

    /// <summary>Restores the default zoom and centers the current level.</summary>
    public void ResetView()
    {
        Zoom = Vector2.One;
        Position = _levelGrid.ContentSize * 0.5f;
        _isDragging = false;
    }

    /// <inheritdoc />
    public override void _Input(InputEvent @event)
    {
        switch (@event)
        {
            case InputEventMouseButton button
                when button.Pressed
                    && button.ButtonIndex is MouseButton.WheelUp
                        or MouseButton.WheelDown:
                ZoomAt(
                    button.Position,
                    button.ButtonIndex == MouseButton.WheelUp);
                GetViewport().SetInputAsHandled();
                break;

            case InputEventMouseButton button
                when button.ButtonIndex == MouseButton.Middle:
                _isDragging = button.Pressed;
                GetViewport().SetInputAsHandled();
                break;

            case InputEventMouseMotion motion when _isDragging:
                if ((motion.ButtonMask & MouseButtonMask.Middle) == 0)
                {
                    _isDragging = false;
                    break;
                }

                Position -= motion.Relative / Zoom.X;
                ClampCameraPosition();
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    /// <summary>Changes zoom while preserving the map point under the cursor.</summary>
    private void ZoomAt(Vector2 positionInView, bool zoomIn)
    {
        Vector2 mapPoint = ViewToMap(positionInView);

        if (!new Rect2(Vector2.Zero, _levelGrid.ContentSize).HasPoint(mapPoint))
            return;

        float step = zoomIn ? ZoomStep : -ZoomStep;
        float newZoom = Mathf.Clamp(
            Zoom.X + step,
            MinimumZoom,
            MaximumZoom);

        if (Mathf.IsEqualApprox(newZoom, Zoom.X))
            return;

        Zoom = Vector2.One * newZoom;
        Position = mapPoint - ((positionInView - ViewCenter) / newZoom);
        ClampCameraPosition();
    }

    /// <summary>
    /// Converts a cursor position from viewport-local pixels to map-local pixels.
    /// <see cref="Position"/> is the camera center: the point it is looking at
    /// relative to the map, whose pixel size is the 80-pixel cell size multiplied
    /// by the number of cells. <paramref name="positionInView"/> is the cursor
    /// position inside the current viewport, while <see cref="ViewCenter"/> is
    /// the center coordinate of that viewport.
    /// </summary>
    private Vector2 ViewToMap(Vector2 positionInView)
        => Position + ((positionInView - ViewCenter) / Zoom.X);

    /// <summary>The center of the sub-viewport in viewport-local pixels.</summary>
    private Vector2 ViewCenter
        => GetViewportRect().Size * 0.5f;

    /// <summary>Keeps the camera within one visible area of the map bounds.</summary>
    private void ClampCameraPosition()
    {
        Vector2 halfViewInMap = ViewCenter / Zoom.X;
        Vector2 minimum = -halfViewInMap;
        Vector2 maximum = _levelGrid.ContentSize + halfViewInMap;

        Position = new Vector2(
            Mathf.Clamp(Position.X, minimum.X, maximum.X),
            Mathf.Clamp(Position.Y, minimum.Y, maximum.Y));
    }
}
