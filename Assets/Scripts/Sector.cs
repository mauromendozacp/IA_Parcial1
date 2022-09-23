using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

public class Sector
{
    #region PRIVATE_FIELDS
    private Vector2 point = Vector2.zero;
    private Color color = Color.white;
    private List<Segment> segments = null;
    private List<Vector2> intersections = null;
    #endregion

    #region CONSTRUCTORS
    public Sector(Vector2 point)
    {
        this.point = point;

        color = Random.ColorHSV();
        color.a = 0.35f;

        segments = new List<Segment>();
        intersections = new List<Vector2>();
    }
    #endregion

    #region PUBLIC_METHODS
    public void AddSegment(Vector2 origin, Vector2 final)
    {
        segments.Add(new Segment(origin, final));
    }

    public void SetIntersections()
    {
        intersections.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                segments[i].GetTwoPoints(out Vector2 p1, out Vector2 p2);
                segments[j].GetTwoPoints(out Vector2 p3, out Vector2 p4);
                Vector2 intersectionPoint = Segment.GetIntersection(p1, p2, p3, p4);

                if (intersections.Contains(intersectionPoint)) continue;

                float maxDist = Vector2.Distance(intersectionPoint, segments[i].Origin);

                bool hasOtherPoint = false;
                for (int k = 0; k < segments.Count; k++)
                {
                    if (k == i || k == j) continue;

                    if (HasOtherPointInIntersection(intersectionPoint, segments[k].Final, maxDist))
                    {
                        hasOtherPoint = true;
                        break;
                    }
                }

                if (!hasOtherPoint)
                {
                    intersections.Add(intersectionPoint);
                    segments[i].Intersections.Add(intersectionPoint);
                    segments[j].Intersections.Add(intersectionPoint);
                }
            }
        }

        UpdateSegments();
        SortIntersections();
    }

    public void AddSegmentLimits(List<Limit> limits)
    {
        for (int i = 0; i < limits.Count; i++)
        {
            Vector2 origin = point;
            Vector2 final = limits[i].GetOpositePosition(origin);

            segments.Add(new Segment(origin, final));
        }
    }

    public void DrawSector()
    {
        Vector3[] points = new Vector3[intersections.Count + 1];

        for (int i = 0; i < intersections.Count; i++)
        {
            points[i] = new Vector3(intersections[i].x, intersections[i].y, 0f);
        }
        points[intersections.Count] = points[0];

        Handles.color = color;
        Handles.DrawAAConvexPolygon(points);

        Handles.color = Color.black;
        Handles.DrawPolyLine(points);
    }
    #endregion

    #region PRIVATE_METHODS
    private bool HasOtherPointInIntersection(Vector2 intersectionPoint, Vector2 pointEnd, float maxDistance)
    {
        float distance = Vector2.Distance(intersectionPoint, pointEnd);
        return distance < maxDistance;
    }

    private void UpdateSegments()
    {
        List<Segment> noIntersectionSegments = new List<Segment>();
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].Intersections.Count != 2)
            {
                noIntersectionSegments.Add(segments[i]);
            }
        }

        segments = segments.Except(noIntersectionSegments).ToList();
    }

    private void SortIntersections()
    {
        intersections.Clear();

        if (segments.Count == 0) return;

        Vector2 lastIntersection = segments[0].Intersections[0];
        intersections.Add(lastIntersection);

        Vector2 firstIntersection;
        Vector2 secondIntersection;

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                firstIntersection = segments[j].Intersections[0];
                secondIntersection = segments[j].Intersections[1];

                if (!intersections.Contains(secondIntersection))
                {
                    if (firstIntersection == lastIntersection)
                    {
                        intersections.Add(secondIntersection);
                        lastIntersection = secondIntersection;
                        break;
                    }
                }

                if (!intersections.Contains(firstIntersection))
                {
                    if (secondIntersection == lastIntersection)
                    {
                        intersections.Add(firstIntersection);
                        lastIntersection = firstIntersection;
                        break;
                    }
                }
            }
        }

        firstIntersection = segments[^1].Intersections[0];
        if (!intersections.Contains(firstIntersection))
        {
            intersections.Add(firstIntersection);
        }

        secondIntersection = segments[^1].Intersections[1];
        if (!intersections.Contains(secondIntersection))
        {
            intersections.Add(secondIntersection);
        }
    }
    #endregion
}
