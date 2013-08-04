/* -*- mode: csharp; c-basic-offset: 2 -*- 
 * Copyright 2013 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;

using CamBam.CAD;
using CamBam.Geom;
using CamBam.Geom.BSP;

namespace VEngraveForCamBam {
  using CamBamExtensions;

  namespace CamBamExtensions {
    public static class Geom {
      const double TOL = 1e-10;
      //public static Point2F Add(this Point2F p, Vector2F v) {
      //    p.X += v.X; p.Y += v.Y;
      //    return p;
      //}
      //public static Vector2F Add(this Vector2F a, Vector2F b) {
      //    a.X += b.X; a.Y += b.Y;
      //    return a;
      //}
      //public static Point2F Subtract(this Point2F p, Vector2F v) {
      //    p.X -= v.X; p.Y -= v.Y;
      //    return p;
      //}
      //public static Vector2F Subtract(this Vector2F a, Vector2F b) {
      //    a.X -= b.X; a.Y -= b.Y;
      //    return a;
      //}
      //public static Vector2F Multiply(this Vector2F v, double factor) {
      //    v.X *= factor; v.Y *= factor;
      //    return v;
      //}
      //public static Vector2F Divide(this Vector2F v, double factor) {
      //    v.X /= factor; v.Y /= factor;
      //    return v;
      //}
      public static Point2F To2D(this Point3F p) {
        return new Point2F(p.X, p.Y);
      }
      public static Point3F To3D(this Point2F p) {
        return p.To3D(0);
      }
      public static Point3F To3D(this Point2F p, double zval) {
        return new Point3F(p.X, p.Y, zval);
      }
      public static Point3F ProjectTo(this Point2F p, Triangle3F t) {
        return p.To3D(t.ZAtPoint(p));
      }
      public static Vector2F To2D(this Vector3F v) {
        return new Vector2F(v.X, v.Y);
      }
      public static Vector3F To3D(this Vector2F v) {
        return v.To3D(0);
      }
      public static Vector3F To3D(this Vector2F p, double zval) {
        return new Vector3F(p.X, p.Y, zval);
      }
      public static Line2F To2D(this Line3F line) {
        return new Line2F(line.p1.To2D(), line.p2.To2D());
      }
      public static Line3F To3D(this Line2F line, double z1, double z2) {
        return new Line3F(line.p1.To3D(z1), line.p2.To3D(z2));
      }
      public static Line3F To3D(this Line2F line, double z) {
        return line.To3D(z, z);
      }
      public static Line3F To3D(this Line2F line) {
        return line.To3D(0);
      }
      public static bool Intersects(this Line2F a, Line2F b) {
        Line2F result = Line2F.Intersect(a, b);
        return !result.p1.IsUndefined && result.p2.IsUndefined;
      }
      public static bool Intersects(this Line2F line, Triangle2F t) {
        return line.Intersects(t.Edge(0))
            || line.Intersects(t.Edge(1))
            || line.Intersects(t.Edge(2));
      }
      public static bool Intersects(this Line2F line, Triangle3F t) {
        return line.Intersects(t.Edge(0).To2D())
            || line.Intersects(t.Edge(1).To2D())
            || line.Intersects(t.Edge(2).To2D());
      }
      public static bool IntersectsXY(this Line3F a, Line3F b) {
        return a.To2D().Intersects(b.To2D());
      }
      public static bool IntersectsXY(this Line3F line, Triangle3F t) {
        return line.To2D().Intersects(t.Edge(0).To2D())
            || line.To2D().Intersects(t.Edge(1).To2D())
            || line.To2D().Intersects(t.Edge(2).To2D());
      }
      public static Triangle2F To2D(this Triangle3F t) {
        return new Triangle2F(t.A.To2D(), t.B.To2D(), t.C.To2D());
      }
      public static Triangle3F To3D(this Triangle2F t,
                                    double z1, double z2, double z3) {
        return new Triangle3F(t.A.To3D(z1), t.B.To3D(z1), t.C.To3D(z3));
      }
      public static Triangle3F To3D(this Triangle2F t, double z) {
        return t.To3D(z, z, z);
      }
      public static Triangle3F To3D(this Triangle2F t) {
        return t.To3D(0);
      }
      public static Line2F Edge(this Triangle2F t, int i) {
        return new Line2F(t[i], t[(i + 1) % 3]);
      }
      public static Line3F Edge(this Triangle3F t, int i) {
        return new Line3F(t[i], t[(i + 1) % 3]);
      }
      public static Line2F Edge2D(this Triangle3F t, int i) {
        return new Line2F(t[i].To2D(), t[(i + 1) % 3].To2D());
      }
      // These avoid creating temp objects to eval (p-origin)x(defining-origin)
      // 0 on edge, +1 right, -1 left
      private static int Side(double xo, double yo,
                              double xd, double yd, double xp, double yp) {
        return Math.Sign((xp - xo)*(yd - yo) - (yp - yo)*(xd - xo));
      }
      public static int Side(this Point2F origin, Point2F defining, Point2F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(this Point2F origin, Point2F defining, Point3F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(this Point3F origin, Point3F defining, Point2F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(this Point3F origin, Point3F defining, Point3F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(this Line2F line, Point2F p) {
        return Side(line.p1, line.p2, p);
      }
      public static int Side(this Line2F line, Point3F p) {
        return Side(line.p1, line.p2, p);
      }
      public static int Side(this Line3F line, Point2F p) {
        return Side(line.p1, line.p2, p);
      }
      public static int Side(this Line3F line, Point3F p) {
        return Side(line.p1, line.p2, p);
      }
      // Inside methods exclude boundary
      public static bool Inside(this Triangle2F t, Point2F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;
      }
      public static bool Inside(this Triangle3F t, Point2F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;
      }
      public static bool InsideXY(this Triangle3F t, Point3F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;
      }
      // Contains methods include boundary
      public static bool Contains(this Triangle2F t, Point2F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3)
          == Math.Abs(side1) + Math.Abs(side2) + Math.Abs(side3);
      }
      public static bool Contains(this Triangle2F a, Triangle2F b) {
        return a.Contains(b.A) && a.Contains(b.B) && a.Contains(b.C);
      }
      public static bool Contains(this Triangle3F t, Point2F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3)
          == Math.Abs(side1) + Math.Abs(side2) + Math.Abs(side3);
      }
      public static bool Contains(this Triangle3F a, Triangle2F b) {
        return a.Contains(b.A) && a.Contains(b.B) && a.Contains(b.C);
      }
      public static bool ContainsXY(this Triangle3F t, Point3F p) {
        int side1 = t.A.Side(t.B, p);
        int side2 = t.B.Side(t.C, p);
        int side3 = t.C.Side(t.A, p);
        return Math.Abs(side1 + side2 + side3)
          == Math.Abs(side1) + Math.Abs(side2) + Math.Abs(side3);
      }
      public static bool ContainsXY(this Triangle3F a, Triangle3F b) {
        return a.ContainsXY(b.A) && a.ContainsXY(b.B) && a.ContainsXY(b.C);
      }
      // Compare methods < 0 B contains A
      //                 = 0 overlap
      //                 > 0 A contains B
      public static int Compare(this Triangle2F a, Triangle2F b) {
        int rv = 0;
        if (a.Contains(b.A)) {
          if (a.Contains(b.B) && a.Contains(b.C)) {
            rv = 1;
          }
        } else if (b.Contains(a.A) && b.Contains(a.B) && b.Contains(a.C)) {
            rv = -1;
        }
        return rv;
      }
      public static int CompareXY(this Triangle3F a, Triangle3F b) {
        int rv = 0;
        if (a.ContainsXY(b.A)) {
          if (a.ContainsXY(b.B) && a.ContainsXY(b.C)) {
            rv = 1;
          }
        } else if (b.ContainsXY(a.A) && b.ContainsXY(a.B) && b.ContainsXY(a.C)) {
            rv = -1;
        }
        return rv;
      }
      public static int CompareZ(this Point3F p, Triangle3F t) {
        try {
          return Math.Sign(p.Z - t.ZAtPoint(p.To2D()));
        } catch (Exception e) {
          throw e;
        }
      }
      public static bool Above(this Point3F p, Triangle3F t) {
        return p.CompareZ(t) > 0;
      }
      public static bool On(this Point3F p, Triangle3F t) {
        return p.CompareZ(t) == 0;
      }
      public static bool Below(this Point3F p, Triangle3F t) {
        return p.CompareZ(t) < 0;
      }
      public static bool Above(this Triangle3F t, Triangle3F t2) {
        if (t.MaxZ > t2.MinZ) {
          return true;
        }
        int a = t.A.CompareZ(t2);
        int b = t.B.CompareZ(t2);
        int c = t.C.CompareZ(t2);
        return a >= 0 && b >= 0 && c >= 0 && a + b + c > 0;
      }
      public static bool On(this Triangle3F t, Triangle3F t2, double tol) {
        double dzA = Math.Abs(t2.ZAtPoint(t.A.To2D()));
        double dzB = Math.Abs(t2.ZAtPoint(t.B.To2D()));
        double dzC = Math.Abs(t2.ZAtPoint(t.C.To2D()));
        return dzA + dzB + dzC < tol;
      }
      public static bool On(this Triangle3F t, Triangle3F t2) {
        return t.On(t2, TOL);
      }
      public static bool Below(this Triangle3F t, Triangle3F t2) {
        if (t.MaxZ < t2.MinZ) {
          return true;
        }
        int a = t.A.CompareZ(t2);
        int b = t.B.CompareZ(t2);
        int c = t.C.CompareZ(t2);
        return a <= 0 && b <= 0 && c <= 0 && a + b + c < 0;
      }
      public static void Slice(this Triangle3F t, Triangle3F t2,
         ref Triangle3FArray inside, ref Triangle3FArray outside) {
        if (t.ContainsXY(t2)) {
          inside.Add(t2);
          return;
        }
        var insideSubFacets = new Triangle3FArray();
        insideSubFacets.Add(t2);
        var sliced = false;
        for (int vertex = 0; vertex < 3; ++vertex) {
          var newInsideSubFacets = new Triangle3FArray();
          for (int fi = 0; fi < insideSubFacets.Count; ++fi) {
            var subFacet = insideSubFacets[fi];
            if (subFacet.InsideXY(t[vertex])
                || t.Edge(vertex).IntersectsXY(t2)) {
              // NB: This assumes that all facets are CCW and interior
              // is left of the edge.
              subFacet.Slice(t.Edge(vertex),
                  ref newInsideSubFacets, ref outside);
              sliced = true;
            }
          }
          insideSubFacets = newInsideSubFacets;
        }
        if (sliced) {
          foreach (Triangle3F insideSubFacet in insideSubFacets) {
            inside.Add(insideSubFacet);
          }
        } else {
          outside.Add(t2);
        }
      }
      public static void Slice(this Triangle3F t, Line3F line,
          ref Triangle3FArray left, ref Triangle3FArray right) {
        if (t.A.Equals(t.B) && t.B.Equals(t.C)) {
          return;   // degenerate case
        }
        Line2F projectedLine = line.To2D();
        var newv = new List<Point2F>();
        for (int i = 0; i < 3; ++i) {
          Line2F edge = t.Edge2D(i);
          var intersection = Line2F.ProjectionIntersect(edge, projectedLine);
          if (!intersection.IsUndefined && edge.PointWithinLine(intersection, TOL)) {
            newv.Add(intersection);
          }
        }
        if (newv.Count == 3) {  // slice through vertex
          if (Point2F.Distance(newv[0], newv[1]) < TOL) {
            newv[1] = newv[2];
          }
        }
        if (newv.Count >= 2) { // normal case
          // Make sure (newv1,newv2) is in the same direction as
          // projectedLine
          if (Vector2F.DotProduct(new Vector2F(projectedLine),
                                  new Vector2F(newv[0], newv[1])) < 0) {
            t.Slice(newv[1], newv[0], ref left, ref right);
          } else {
            t.Slice(newv[0], newv[1], ref left, ref right);
          }
        } else {               // no intersections
          if (projectedLine.Side(t[0]) > 0
              || projectedLine.Side(t[1]) > 0
              || projectedLine.Side(t[2]) > 0) {
            right.Add(t);
          } else if (projectedLine.Side(t[0]) < 0
              || projectedLine.Side(t[1]) < 0
              || projectedLine.Side(t[2]) < 0) {
            left.Add(t);
          } else {
            // TODO: unreachable?
          }
        }
      }
      public static void Slice(this Triangle3F t, Point2F p1_2d, Point2F p2_2d,
          ref Triangle3FArray left, ref Triangle3FArray right) {
        Point3F p1 = p1_2d.ProjectTo(t);
        Point3F p2 = p2_2d.ProjectTo(t);
        Vector2F sliceRay = new Vector2F(p1_2d, p2_2d);
        int nLeft = 0, nCenter = 0, nRight = 0;
        int leftIndex = 0, rightIndex = 0;  // making these simple values
                                            // eliminates array allocations
        for (int i = 0; i < 3; ++i) {
          int side = p1_2d.Side(p2_2d, t[i]);
          if (side < 0) {
            ++nLeft;
            leftIndex <<= 8;  // Is shifting an entire byte actually faster?
            leftIndex |= i;
          } else if (side > 0) {
            ++nRight;
            rightIndex <<= 8;
            rightIndex |= i;
          } else {  // == 0
            ++nCenter;
          }
        }
        if (nCenter == 3) {     // degenerate
          return;
        }
        if (nLeft == 0) {        // entirely on one side of line
          right.Add(t);
          return;
        }
        if (nRight == 0) {
          left.Add(t);
          return;
        }
        if (nLeft == 1) {
          left.Add(new Triangle3F(p1, p2, t[leftIndex])); // CCW
        } else {  // nLeft == 2
          int leftIndex1 = 0xFF & (leftIndex >> 8);
          int leftIndex2 = 0xFF & leftIndex;
          _AddFacets(ref left, p1, p2, t[leftIndex1], t[leftIndex2]);
        }
        if (nRight == 1) {
          right.Add(new Triangle3F(p1, t[rightIndex], p2)); // CCW
        } else {  // nRight == 2
          int rightIndex1 = 0xFF & (rightIndex >> 8);
          int rightIndex2 = 0xFF & rightIndex;
          _AddFacets(ref right, p1, p2, t[rightIndex1], t[rightIndex2]);
        }
      }

      // NB: this assumes (p1,p2) is a good side.
      private static void _AddFacets(ref Triangle3FArray facets, Point3F p1, Point3F p2, Point3F p3, Point3F p4) {
        if (Line2F.Intersect(new Line2F(p1.To2D(), p4.To2D()),
                             new Line2F(p2.To2D(), p3.To2D())).p1.IsUndefined) {
          Point3F tmp = p3;
          p3 = p4;
          p4 = tmp;
        }
        Triangle3F facet1 = new Triangle3F(p1, p2, p3);
        if (facet1.CalcNormal().Z >= 0) {   // TODO: verify true for CCW, use normal
          facets.Add(facet1);
          facets.Add(new Triangle3F(p2, p4, p3));
        } else {
          facets.Add(new Triangle3F(p3, p2, p1));
          facets.Add(new Triangle3F(p3, p4, p2));
        }
      }
    }
  }
}