using UnityEngine;

#region ENUMS
public enum DIRECTION
{
    NONE,
    UP,
    RIGHT,
    DOWN,
    LEFT
}
#endregion

public class Limit
{
    #region PRIVATE_FIELDS
    private Vector2 origin = Vector2.zero;
    private DIRECTION direction = default;
    #endregion

    #region CONSTRUCTORS
    public Limit(Vector2 origin, DIRECTION direction)
    {
        this.origin = origin;
        this.direction = direction;
    }
    #endregion

    #region PUBLIC_METHODS
    public Vector2 GetOutsitePosition(Vector2 pos)
    {
        float distanceX = Mathf.Abs(Mathf.Abs(pos.x) - Mathf.Abs(origin.x)) * 2f;
        float distanceY = Mathf.Abs(Mathf.Abs(pos.y) - Mathf.Abs(origin.y)) * 2f;

        switch (direction)
        {
            case DIRECTION.LEFT:
                pos.x -= distanceX;
                break;
            case DIRECTION.UP:
                pos.y += distanceY;
                break;
            case DIRECTION.RIGHT:
                pos.x += distanceX;
                break;
            case DIRECTION.DOWN:
                pos.y -= distanceY;
                break;
            default:
                pos = Vector2.zero;
                break;
        }

        return pos;
    }
    #endregion
}
