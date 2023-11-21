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
using System.Runtime.CompilerServices;

using CamBam.CAD;
using CamBam.Geom;
using CamBam.Geom.BSP;

[assembly: InternalsVisibleTo("VEngrave_UnitTests")]

namespace VEngraveForCamBam {
  using CamBamExtensions;

  public class SurfaceBuilder {
    private const double TOL = 1e-10;
    // Smallest number such that 1.0 + DBL_EPSILON != 1.0
    const double DBL_EPSILON = 2.2204460492503131e-016;
    private Point3FArray _points;
    private List<TriangleFace> _faces;
    private Dictionary<Point3F,int> _pointIndexes;
    private Logger _log = new NullLogger();
    // Log levels
    const int ERROR = 0, WARNING = 1, INFO = 2, DEBUG = 3, TRACE = 4,
      FINE = 5, FINER = 6, FINEST = 7;

    public Vector3F Front { set; get; }

    public SurfaceBuilder() {
      _points = new Point3FArray();
      _pointIndexes = new Dictionary<Point3F, int>();
      _faces = new List<TriangleFace>();
    }

    internal void setLogger(Logger log) {
      _log = log;
    }

    public void AddFacet(Triangle3F facetToAdd) {
      AddFacet(facetToAdd.A, facetToAdd.B, facetToAdd.C);
    }

    public void AddFacets(Point3F a, Point3F b, Point3F c, Point3F d) {
      Line3F ad = new Line3F(a, d);
      Line3F bc = new Line3F(b, c);
      AddFacet(a, b, c);
      if (ad.IntersectsXY(bc)) {
        AddFacet(b, c, d);
      } else {
        AddFacet(a, c, d);
      }
    }

    public void AddFacet(Point3F a, Point3F b, Point3F c) {
      int ia = AddPoint(a);
      int ib = AddPoint(b);
      int ic = AddPoint(c);
      if (CorrectPermutation(a, b, c)) {
        _faces.Add(new TriangleFace(ia, ib, ic));
      } else {
        _faces.Add(new TriangleFace(ia, ic, ib));
      }
    }

    public Surface Build() {
      Surface surface = new Surface();
      surface.Faces = _faces.ToArray();
      surface.Points = new Point3FArray(_points);
      Reset();
      return surface;
    }

    public void Reset() {
      _faces.Clear();
      _points.Clear();
      _pointIndexes.Clear();
    }

    internal int AddPoint(Point3F point) {
      int index;
      if (!_pointIndexes.TryGetValue(point, out index)) {
        index = _points.Count;
        _points.Add(point);
        _pointIndexes.Add(point, index);
      }
      return index;
    }

    internal bool CorrectPermutation(Triangle3F t) {
      return CorrectPermutation(t.A, t.B, t.C);
    }

    internal bool CorrectPermutation(Point3F a, Point3F b, Point3F c) {
      return Vector3F.DotProduct(Vector3F.CrossProduct(new Vector3F(a, b),
                                                       new Vector3F(a, c)),
                                 Front) >= 0;
    }
  }

  // This is for some more efficient O(n log n) algorithms from ch 3 of
  // _Computatonal Geometry_ by de Berg et al.

  namespace Polygon {
    public class Vertex {
      public Point3F Position { get; set; }
      public HalfEdge IncidentEdge { get; set; }
    }

    public class Face {
      public HalfEdge OuterComponent { get; set; }
      public LinkedList<HalfEdge> InnerComponents;
    }
    public class HalfEdge {
      public Vertex Origin { get; set; }
      public HalfEdge Twin { get; set; }
      public HalfEdge Next { get; set; }
      public HalfEdge Prev { get; set; }
    }

    public class PointYComparator : IComparer<Point3F>, IComparer<Point2F> {
      public int Compare(Point3F lhs, Point3F rhs) {
        if (lhs.Y > rhs.Y) {
          return -1;
        } else if (lhs.Y < rhs.Y) {
          return +1;
        } else if (lhs.X > rhs.X) {
          return -1;
        } else if (lhs.X < rhs.X) {
          return +1;
        } else {
          return 0;
        }
      }
      public int Compare(Point2F lhs, Point2F rhs) {
        if (lhs.Y > rhs.Y) {
          return -1;
        } else if (lhs.Y < rhs.Y) {
          return +1;
        } else if (lhs.X > rhs.X) {
          return -1;
        } else if (lhs.X < rhs.X) {
          return +1;
        } else {
          return 0;
        }
      }
    }

    struct TriangleVertexIndex {
      public int triangleIndex;
      public int vertexIndex;
    }
  }
}
