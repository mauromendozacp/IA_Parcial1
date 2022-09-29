using UnityEngine;

public class IntersectionPoint
{
    #region PRIVATE_FIELDS
    private Vector2 position = Vector2.zero;
    private float angle = 0f;
    #endregion

    #region PROPERTIES
    public Vector2 Position { get => position; }
    public float Angle { get => angle; set => angle = value; }
    #endregion

    #region CONSTRUCTORS
    public IntersectionPoint(Vector2 position)
    {
        this.position = position;

        angle = 0f;
    }
    #endregion
}
