using System.Collections.Generic;
using UnityEngine;

public class Segment
{
    #region PRIVATE_FIELDS
    private Vector2 origin = Vector2.zero;
    private Vector2 final = Vector2.zero;
    private Vector2 direction = Vector2.zero;
    private Vector2 mediatrix = Vector2.zero;

    private List<Vector2> intersections = null;
    #endregion

    #region CONSTANTS
    private const int maxDist = 10;
    #endregion

    #region PROPERTIES
    public Vector2 Origin { get => origin; }
    public Vector2 Final { get => final; }
    public List<Vector2> Intersections { get => intersections; }
    #endregion

    #region CONSTRUCTORS
    public Segment(Vector2 origin, Vector2 final)
    {
        this.origin = origin;
        this.final = final;

        mediatrix = new Vector2((origin.x + final.x) / 2, (origin.y + final.y) / 2);
        direction = Vector2.Perpendicular(new Vector2(final.x - origin.x, final.y - origin.y));

        intersections = new List<Vector2>();
    }
    #endregion

    #region PUBLIC_METHODS
    public static Vector2 GetIntersection(Vector2 ap1, Vector2 ap2, Vector2 bp1, Vector2 bp2)
    {
        Vector2 intersection = Vector2.zero;

        if (((ap1.x - ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x - bp2.x)) == 0) return intersection;

        intersection.x = ((ap1.x * ap2.y - ap1.y * ap2.x) * (bp1.x - bp2.x) - (ap1.x - ap2.x) * (bp1.x * bp2.y - bp1.y * bp2.x)) / ((ap1.x - ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x - bp2.x));
        intersection.y = ((ap1.x * ap2.y - ap1.y * ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x * bp2.y - bp1.y * bp2.x)) / ((ap1.x - ap2.x) * (bp1.y - bp2.y) - (ap1.y - ap2.y) * (bp1.x - bp2.x));

        return intersection;
    }

    public void GetTwoPoints(out Vector2 p1, out Vector2 p2)
    {
        p1 = mediatrix;
        p2 = mediatrix + direction * maxDist;
    }
    #endregion
}
