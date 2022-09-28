using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

public class Sector
{
    #region PRIVATE_FIELDS
    private Mine mine = null;
    private Color color = Color.white;
    private List<Segment> segments = null;
    private List<Vector2> intersections = null;
    private Vector3[] points = null;
    #endregion

    #region PROPERTIES
    public Mine Mine { get => mine; }
    #endregion

    #region CONSTRUCTORS
    public Sector(Mine mine)
    {
        this.mine = mine;

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

    public void DrawSegments()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].Draw();
        }
    }

    public void DrawSector()
    {
        Handles.color = color;
        Handles.DrawAAConvexPolygon(points);

        Handles.color = Color.black;
        Handles.DrawPolyLine(points);
    }

    public void SetIntersections()
    {
        intersections.Clear();

        for (int i = 0; i < segments.Count; i++)
        {
            for (int j = 0; j < segments.Count; j++)
            {
                if (i == j) continue;

                Vector2 intersectionPoint = GetIntersection(segments[i], segments[j]);

                if (intersections.Contains(intersectionPoint)) continue;

                float maxDistance = Vector2.Distance(intersectionPoint, segments[i].Origin);
                for (int k = 0; k < segments.Count; k++)
                {
                    if (k == j) continue;

                    float dist = Vector2.Distance(intersectionPoint, segments[k].Origin);
                    if (dist < maxDistance)
                    {
                        maxDistance = dist;
                    }
                }

                bool hasOtherPoint = false;
                for (int k = 0; k < segments.Count; k++)
                {
                    if (k == i || k == j) continue;

                    if (HasOtherPointInIntersection(intersectionPoint, segments[k].Final, maxDistance))
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

        CleanUnusedSegments();
        SortIntersections();
        SetPointsInSector();
    }

    public void AddSegmentLimits(List<Limit> limits)
    {
        for (int i = 0; i < limits.Count; i++)
        {
            Vector2 origin = mine.transform.position;
            Vector2 final = limits[i].GetOpositePosition(origin);

            segments.Add(new Segment(origin, final));
        }
    }

    public bool CheckPointInSector(Vector3 point)
    {
        bool inside = false;

        if (points == null) return false;

        Vector2 endPoint = points[^1];
        float endX = endPoint.x;
        float endY = endPoint.y;

        for (int i = 0; i < points.Length; i++)
        {
            float startX = endX; 
            float startY = endY;

            endPoint = points[i];

            endX = endPoint.x; 
            endY = endPoint.y;

            inside ^= (endY > point.y ^ startY > point.y) && ((point.x - endX) < (point.y - endY) * (startX - endX) / (startY - endY));
        }

        return inside;
    }
    #endregion

    #region PRIVATE_METHODS
    private bool HasOtherPointInIntersection(Vector2 intersectionPoint, Vector2 pointEnd, float maxDistance)
    {
        float distance = Vector2.Distance(intersectionPoint, pointEnd);
        return distance < maxDistance;
    }

    private void CleanUnusedSegments()
    {
        List<Segment> unusedSegments = new List<Segment>();
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].Intersections.Count != 2)
            {
                unusedSegments.Add(segments[i]);
            }
        }

        segments = segments.Except(unusedSegments).ToList();
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

    private void SetPointsInSector()
    {
        points = new Vector3[intersections.Count + 1];

        for (int i = 0; i < intersections.Count; i++)
        {
            points[i] = new Vector3(intersections[i].x, intersections[i].y, 0f);
        }
        points[intersections.Count] = points[0];
    }

    public Vector2 GetIntersection(Segment seg1, Segment seg2)
    {
        Vector2 intersection = Vector2.zero;

        Vector2 p1 = seg1.Mediatrix;
        Vector2 p2 = seg1.Mediatrix + seg1.Direction * NodeUtils.mapSize.magnitude;
        Vector2 p3 = seg2.Mediatrix;
        Vector2 p4 = seg2.Mediatrix + seg2.Direction * NodeUtils.mapSize.magnitude;

        if (((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x)) == 0) return intersection;

        intersection.x = ((p1.x * p2.y - p1.y * p2.x) * (p3.x - p4.x) - (p1.x - p2.x) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));
        intersection.y = ((p1.x * p2.y - p1.y * p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x * p4.y - p3.y * p4.x)) / ((p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x));

        return intersection;
    }
    #endregion
}
