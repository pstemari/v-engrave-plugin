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
    public static class GeometryExtensions {
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
      public static int Side(Point2F origin, Point2F defining, Point2F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(Point2F origin, Point2F defining, Point3F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(Point3F origin, Point3F defining, Point2F p) {
        return Side(origin.X, origin.Y, defining.X, defining.Y, p.X, p.Y);
      }
      public static int Side(Point3F origin, Point3F defining, Point3F p) {
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
      public static bool Contains(this Triangle2F t, Point2F p) {
        int side1 = Side(t.A, t.B, p);
        int side2 = Side(t.B, t.C, p);
        int side3 = Side(t.C, t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;  // exclude boundary
      }
      public static bool Contains(this Triangle3F t, Point2F p) {
        int side1 = Side(t.A, t.B, p);
        int side2 = Side(t.B, t.C, p);
        int side3 = Side(t.C, t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;  // exclude boundary
      }
      public static bool Contains2D(this Triangle3F t, Point3F p) {
        int side1 = Side(t.A, t.B, p);
        int side2 = Side(t.B, t.C, p);
        int side3 = Side(t.C, t.A, p);
        return Math.Abs(side1 + side2 + side3) == 3;  // exclude boundary
      }
      public static void Slice(this Triangle3F t, Point2F p1_2d, Point2F p2_2d,
          ref Triangle3FArray left, ref Triangle3FArray right) {
        Point3F p1 = p1_2d.ProjectTo(t);
        Point3F p2 = p2_2d.ProjectTo(t);
        Vector2F sliceRay = new Vector2F(p1_2d, p2_2d);
        int nLeft = 0, nCenter = 0, nRight = 0;
        int leftIndex = 0, rightIndex = 0;  // making these simple values eliminates
                                            // array allocations
        for (int i = 0; i < 3; ++i) {
          int side = Math.Sign(Vector2F.Determinant(
              new Vector2F(p1_2d, t[i].To2D()), sliceRay));
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
          int leftIndex1 = 0xFF & leftIndex;
          int leftIndex2 = 0xFF & (leftIndex >> 8);
          addFacets(ref left, p1, p2, t[leftIndex1], t[leftIndex2]);
        }
        if (nRight == 1) {
          right.Add(new Triangle3F(p1, t[rightIndex], p2)); // CCW
        } else {  // nRight == 2
          int rightIndex1 = 0xFF & rightIndex;
          int rightIndex2 = 0xFF & (rightIndex >> 8);
          addFacets(ref right, p1, p2, t[rightIndex1], t[rightIndex2]);
        }
      }
      // NB: this assumes (p1,p2) is a good side.
      private static void addFacets(ref Triangle3FArray facets, Point3F p1, Point3F p2, Point3F p3, Point3F p4) {
        if (Line2F.Intersect(new Line2F(p1.To2D(), p4.To2D()),
                             new Line2F(p2.To2D(), p3.To2D())).p1.IsUndefined) {
          Point3F tmp = p3;
          p3 = p4;
          p4 = tmp;
        }
        Triangle3F facet1 = new Triangle3F(p1, p2, p3);
        if (facet1.CalcNormal().Z >= 0) {   // TODO: verify true for CCW
          facets.Add(facet1);
          facets.Add(new Triangle3F(p3, p4, p1));
        } else {
          facets.Add(new Triangle3F(p3, p2, p1));
          facets.Add(new Triangle3F(p1, p4, p3));
        }
      }

      public static void Slice(this Triangle3F t, Line3F line,
          ref Triangle3FArray left, ref Triangle3FArray right) {
        Line2F projectedLine = line.To2D();
        int edge1, edge2;
        Point2F newv1, newv2;
        for (int i = 0; i < 3; ++i) {
          Line2F edge = t.Edge2D(i);
          var intersection = Line2F.ProjectionIntersect(edge, projectedLine);
          if (edge.PointWithinLine(intersection, 1e-10)) {

          }
        }
      }
    }
  }

  public class BSPTree {
    private BSPNode root;
    public BSPTree(Rect2F bounds) {
      root = new BSPNode(bounds.P1, bounds.P2);
    }
    public BSPTree(Point2F lowerLeft, Point2F upperRight) {
      root = new BSPNode(lowerLeft, upperRight);
    }
    public BSPTree(Point3F lowerLeft, Point3F upperRight) {
      root = new BSPNode(lowerLeft, upperRight);
    }
    public void Add(Triangle3F face) {
      root.AddItem(face);
    }
    public bool Remove(Triangle3F face) {
      BSPNode node = root.FindEndNode(face);
      if (node == null) {
        return false;
      }
      return node.Items.Remove(face);
    }
    private Triangle3F downcast(IBSPItem item) {
      return (Triangle3F) item;
    }
    public List<Triangle3F> FindNearby(Triangle3F face) {
      List<IBSPItem> nearby = root.GetNearItems(face);
      return nearby.ConvertAll(downcast);
    }
  }
  public class SurfaceBuilder {
    private const double TOL = 1e-10;
    private Point3FArray points;
    private List<TriangleFace> faces;
    private Dictionary<Point3F,int> pointIndexes;
    private BSPTree facetTree;

    public Vector3F Front { set; get; }

    public SurfaceBuilder() {
      points = new Point3FArray();
      pointIndexes = new Dictionary<Point3F, int>();
      faces = new List<TriangleFace>();
    }

    public SurfaceBuilder(Rect2F bounds) {
      points = new Point3FArray();
      pointIndexes = new Dictionary<Point3F, int>();
      faces = new List<TriangleFace>();
      facetTree = new BSPTree(bounds);
    }

    private int[][] slicingEdges = {                     // 2 1 0
                                     new int[]{},        // 0 0 0
                                     new int[]{0, 2},    // 0 0 1
                                     new int[]{0, 1},    // 0 1 0
                                     new int[]{1, 2},    // 0 1 1
                                     new int[]{1, 2},    // 1 0 0
                                     new int[]{0, 1},    // 1 0 1
                                     new int[]{0, 2},    // 1 1 0
                                     new int[]{0, 1, 2}, // 1 1 1
                                   };

//    public void AddFacet(Triangle3F facetToAdd) {
//      var facetToAdd2D = facetToAdd.To2D();
//      var facetsToCheck = new Queue<Triangle3F>(
//          facetTree.FindNearby(facetToAdd));
//      while (facetsToCheck.Count > 0) {
//        var facetBeingChecked = facetsToCheck.Dequeue();
////        var facetBeingChecked2D = facetBeingChecked.To2D();
//        int insideVertexesBeingChecked = 0;
//        int insideVertexesToAdd = 0;
//        for (int i = 0; i < 3; ++i) {
//          if (facetToAdd.Contains2D(facetBeingChecked[i])) {
//            insideVertexesBeingChecked |= (1 << i);
//          }
//          if (facetBeingChecked.Contains2D(facetToAdd[i])) {
//            insideVertexesToAdd |= (1 << i);
//          }
//        }
//        // No overlap
//        if (insideVertexesBeingChecked == 0 && insideVertexesToAdd == 0) {
//          continue; // nothing to do
//        } else if (insideVertexesBeingChecked == 3) {
//          // factor being checked lies completely within facet to added
//        } else if (insideVertexesToAdd == 3) {
//          // factor to add lies completely within facet being checked
//        }

        
        
//        var replacementFacets = new List<Triangle3F>();
//        Point2F vertex = facetToAdd[0].To2D();
//        for (int vertexi = 0; vertexi < 3; ++vertexi) {
//          int nextVertexi = (vertexi + 1) % 3;
//          Point2F nextVertex = facetToAdd[nextVertexi].To2D();
//          var edge = new Line2F(vertex, nextVertex);
//          var otherEdge = facetBeingChecked.Edge2D(0);
//          Point2F break0 = Line2F.ProjectionIntersect(edge, otherEdge);
//          if (edge.PointWithinLine(break0, TOL)
//              && otherEdge.PointWithinLine(break0, TOL)) {
//            // sides cross, split facet being checked
//          }
//          Point2F break1 = Line2F.ProjectionIntersect(edge,
//              facetBeingChecked.Edge2D(1));
//          Point2F break2 = Line2F.ProjectionIntersect(edge,
//              facetBeingChecked.Edge2D(2));
//          bool cornerInside = facetBeingChecked.Contains(vertex);

//          // split facet being checked along edge

//          vertex = nextVertex;
//        }
//        Point2F[] intersects 
//      }
//    }

    public int AddPoint(Point3F point) {
      int index;
      if (!pointIndexes.TryGetValue(point, out index)) {
        index = points.Count;
        points.Add(point);
        pointIndexes.Add(point, index);
      }
      return index;
    }

    public void AddFace(int ia, int ib, int ic) {
      Point3F a = points[ia];
      Point3F b = points[ib];
      Point3F c = points[ic];
      if (_CorrectPermutation(a, b, c)) {
        faces.Add(new TriangleFace(ia, ib, ic));
      } else {
        faces.Add(new TriangleFace(ia, ic, ib));
      }
    }

    public void AddFace(Point3F a, Point3F b, Point3F c) {
      int ia = AddPoint(a);
      int ib = AddPoint(b);
      int ic = AddPoint(c);
      if (_CorrectPermutation(a, b, c)) {
        faces.Add(new TriangleFace(ia, ib, ic));
      } else {
        faces.Add(new TriangleFace(ia, ic, ib));
      }
    }

    public void AddFace(Point3F a, Point3F b, Point3F c, Point3F d) {
      int ia = AddPoint(a);
      int ib = AddPoint(b);
      int ic = AddPoint(c);
      int id = AddPoint(d);
      if (_CorrectPermutation(a, b, c)) {
        faces.Add(new TriangleFace(ia, ib, ic));
      } else {
        faces.Add(new TriangleFace(ia, ic, ib));
      }
      if (_CorrectPermutation(a, c, d)) {
        faces.Add(new TriangleFace(ia, ic, id));
      } else {
        faces.Add(new TriangleFace(ia, id, ic));
      }
    }

    public Surface Build() {
      Surface surface = new Surface();
      surface.Faces = faces.ToArray();
      surface.Points = new Point3FArray(points);
      return surface;
    }

    public void reset() {
      faces.Clear();
      points.Clear();
      pointIndexes.Clear();
    }

    private bool _CorrectPermutation(Point3F a, Point3F b, Point3F c) {
      return Vector3F.DotProduct(
        Vector3F.CrossProduct(new Vector3F(a, b), new Vector3F(a, c)), Front) >= 0;
    }
  }
}