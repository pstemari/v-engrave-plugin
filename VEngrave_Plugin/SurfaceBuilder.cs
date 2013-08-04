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

  public class BSPTree : IEnumerable<Triangle3F> {
    private BSPNode _root;
    public BSPTree(Rect2F bounds) {
      _root = new BSPNode(bounds.P1, bounds.P2);
    }
    public BSPTree(Point2F lowerLeft, Point2F upperRight) {
      _root = new BSPNode(lowerLeft, upperRight);
    }
    public BSPTree(Point3F lowerLeft, Point3F upperRight) {
      _root = new BSPNode(lowerLeft, upperRight);
    }
    public void Add(Triangle3F face) {
      _root.AddItem(face);
    }
    public void Add(Triangle3FArray faces) {
      foreach (Triangle3F face in faces) {
        Add(face);
      }
    }
    public bool Remove(Triangle3F face) {
      BSPNode node = _root.FindEndNode(face);
      if (node == null) {
        return false;
      }
      return node.Items.Remove(face);
    }
    public void Clear() {
      BSPNode oldRoot = _root;
      _root = new BSPNode(oldRoot.Region.P1, oldRoot.Region.P2);
      ClearNode(oldRoot);
    }
    private void ClearNode(BSPNode node) {
      foreach (BSPNode child in node.Child) {
        ClearNode(child);
      }
      node.Items.Clear();
      node.Child.Clear();
    }
    private Triangle3F downcast(IBSPItem item) {
      return (Triangle3F) item;
    }
    public List<Triangle3F> FindNeighbors(Triangle3F face) {
      List<IBSPItem> nearby = _root.GetNearItems(face);
      return nearby.ConvertAll(downcast);
    }
    public IEnumerator<Triangle3F> GetEnumerator() {
      return new Enumerator(_root);
    }
    System.Collections.IEnumerator
      System.Collections.IEnumerable.GetEnumerator() {
        return ((IEnumerable<Triangle3F>) this).GetEnumerator();
    }
    class Enumerator : IEnumerator<Triangle3F> {
      private BSPNode _node;
      private IEnumerator<BSPNode> _childCollectionEnumerator = null;
      private Enumerator _childEnumerator = null;
      private IEnumerator<IBSPItem> _itemEnumerator = null;

      public Enumerator(BSPNode node) {
        _node = node;
        Reset();
      }

      object System.Collections.IEnumerator.Current {
        get { return ((IEnumerator<Triangle3F>) this).Current; }
      }
      public Triangle3F Current {
        get {
          if (_itemEnumerator != null) {
            return (Triangle3F) _itemEnumerator.Current;
          } else if (_childEnumerator != null) {
            return _childEnumerator.Current;
          } else {
            throw new InvalidOperationException();
          }
        }
      }
      public bool MoveNext() {
        if (_itemEnumerator != null) {
          if (_itemEnumerator.MoveNext()) {
            return true;
          } else {
            _itemEnumerator.Dispose();
            _itemEnumerator = null;
            if (_node.Child != null) {
              _childCollectionEnumerator = _node.Child.GetEnumerator();
            }
          }
        }
        if (_childEnumerator != null) {
          if (_childEnumerator.MoveNext()) {
            return true;
          } else {
            _childEnumerator.Dispose();
            _childEnumerator = null;
          }
        }
        if (_childCollectionEnumerator != null) {
          while (_childEnumerator == null) {
            if (!_childCollectionEnumerator.MoveNext()) {
              return false;
            }
            _childEnumerator
                = new Enumerator(_childCollectionEnumerator.Current);
            if (!_childEnumerator.MoveNext()) {
              _childEnumerator.Dispose();
              _childEnumerator = null;
            }
          }
        }
        return false;  
      }
      public void Reset() {
        DiscardEnumerators();
        _itemEnumerator = _node.Items.GetEnumerator();
      }
      public void Dispose() {
        DiscardEnumerators();
        _node = null;
      }
      private void DiscardEnumerators() {
        if (_childCollectionEnumerator != null) {
          _childCollectionEnumerator.Dispose();
          _childCollectionEnumerator = null;
        }
        if (_childEnumerator != null) {
          _childEnumerator.Dispose();
          _childEnumerator = null;
        }
        if (_itemEnumerator != null) {
          _itemEnumerator.Dispose();
          _itemEnumerator = null;
        }
      }
    }
  }

  public class SurfaceBuilder {
    private const double TOL = 1e-10;
    // Smallest number such that 1.0 + DBL_EPSILON != 1.0
    const double DBL_EPSILON = 2.2204460492503131e-016;
    private Point3FArray _points;
    private List<TriangleFace> _faces;
    private Dictionary<Point3F,int> _pointIndexes;
    private BSPTree _facetTree;
    private Logger _log = new NullLogger();
    // Log levels
    const int ERROR = 0, WARNING = 1, INFO = 2, DEBUG = 3, TRACE = 4,
      FINE = 5, FINER = 6, FINEST = 7;

    public Vector3F Front { set; get; }

    public SurfaceBuilder(Rect2F bounds) {
      _points = new Point3FArray();
      _pointIndexes = new Dictionary<Point3F, int>();
      _faces = new List<TriangleFace>();
      _facetTree = new BSPTree(bounds);
    }

    internal void setLogger(Logger log) {
      _log = log;
    }

    public void AddFacet(Triangle3F facetToAdd) {
      // check for degenerate facets
      if (Point3F.Distance(facetToAdd.A, facetToAdd.B) < DBL_EPSILON
          || Point3F.Distance(facetToAdd.B, facetToAdd.C) < DBL_EPSILON
          || Point3F.Distance(facetToAdd.C, facetToAdd.A) < DBL_EPSILON) {
        _log.Log(DEBUG, "Degenerate facet A:{0}, B:{1}, C:{2}",
                 facetToAdd.A, facetToAdd.B, facetToAdd.C);
        return;
      }
      // check for vertical facets
      if (Math.Abs(Vector3F.CrossProduct(
              new Vector3F(facetToAdd.A, facetToAdd.B),
              new Vector3F(facetToAdd.A, facetToAdd.C)).Z) < DBL_EPSILON) {
        _log.Log(DEBUG, "Vertical facet A:{0}, B:{1}, C:{2}",
                 facetToAdd.A, facetToAdd.B, facetToAdd.C);
        return;
      }
      // Correct facet order
      if (!CorrectPermutation(facetToAdd)) {
        facetToAdd = new Triangle3F(facetToAdd.A, facetToAdd.C, facetToAdd.B);
      }
      _facetTree.Add(facetToAdd);
      return;
      var internalFacets = new Triangle3FArray();
      var neighboringFacets = new Queue<Triangle3F>(
         _facetTree.FindNeighbors(facetToAdd));
      while (neighboringFacets.Count > 0) {
        var facetBeingChecked = neighboringFacets.Dequeue();
        _facetTree.Remove(facetBeingChecked);
        var externalFacets = new Triangle3FArray();
        facetToAdd.Slice(facetBeingChecked, ref internalFacets,
            ref externalFacets);
        _facetTree.Add(externalFacets);
      }
      // At this point no existing facets crosses an edge of the facet to add
      // and interior facets contains all the existing facets inside the facet
      // to add.
      // Cull any internal facets above the facet to add and aplit any that
      // cross it in Z.
      var lowerFacets = new Triangle3FArray();
      foreach (Triangle3F internalFacet in internalFacets) {
        if (internalFacet.Above(facetToAdd) || internalFacet.On(facetToAdd)) {
          // discard it
        } else if (internalFacet.Below(facetToAdd)) {
          lowerFacets.Add(internalFacet);
          _facetTree.Add(internalFacet);
        } else {  // it's a crossing facet--slice yet again
          var sp1 = new Point3F(double.NaN, double.NaN, double.NaN);
          var sp2 = new Point3F(double.NaN, double.NaN, double.NaN);
          for (int vi = 0; vi < 3; ++vi) {
            var v = internalFacet[vi];
            var vnext = internalFacet[(vi + 1) % 3];
            var crossingPoint = facetToAdd.LinePlaneIntersection(v, vnext);
            if (!crossingPoint.IsUndefined) {
              sp1 = sp2;
              sp2 = crossingPoint;
            }
          }
          // Cases where the line is very close to parallel will
          // give undefined points for the intersection, even though
          // Above/Below indicate a crossing.
          if (!sp1.IsUndefined) {
            var leftFacets = new Triangle3FArray();
            var rightFacets = new Triangle3FArray();
            internalFacet.Slice(sp1.To2D(), sp2.To2D(),
              ref leftFacets, ref rightFacets);
            foreach (Triangle3F leftFacet in leftFacets) {
              if (leftFacet.Below(facetToAdd)) {
                lowerFacets.Add(leftFacet);
                _facetTree.Add(leftFacet);
              }
            }
            foreach (Triangle3F rightFacet in rightFacets) {
              if (rightFacet.Below(facetToAdd)) {
                lowerFacets.Add(rightFacet);
                _facetTree.Add(rightFacet);
              }
            }
          }
        }
      }
      // Finally, break up facetToAdd removing all lowerFacets
      var remainingFacets = new Triangle3FArray();
      remainingFacets.Add(facetToAdd);
      foreach (Triangle3F existing in lowerFacets) {
        var toBeSliced = remainingFacets;
        remainingFacets = new Triangle3FArray();
        foreach (Triangle3F newFacet in toBeSliced) {
          var discard = new Triangle3FArray();
          existing.Slice(newFacet, inside: ref discard,
                         outside: ref remainingFacets);
        }
      }
      foreach (Triangle3F newFacet in remainingFacets) {
        _facetTree.Add(newFacet);
      }
    }

    public void AddFacet(Point3F a, Point3F b, Point3F c) {
      AddFacet(new Triangle3F(a, b, c));
    }

    public void AddFacets(Point3F a, Point3F b, Point3F c, Point3F d) {
      Line3F ad = new Line3F(a, d);
      Line3F bc = new Line3F(b, c);
      AddFacet(new Triangle3F(a, b, c));
      if (ad.IntersectsXY(bc)) {
        AddFacet(new Triangle3F(b, c, d));
      } else {
        AddFacet(new Triangle3F(a, c, d));
      }
    }

    public Surface Build() {
      Surface surface = new Surface();
      _faces.Clear();
      _points.Clear();
      _pointIndexes.Clear();
      foreach (Triangle3F t in _facetTree) {
        ConvertFacet(t);
      }
      surface.Faces = _faces.ToArray();
      surface.Points = new Point3FArray(_points);
      return surface;
    }

    public void Reset() {
      _facetTree.Clear();
      _faces.Clear();
      _points.Clear();
      _pointIndexes.Clear();
    }

    internal bool CorrectPermutation(Triangle3F t) {
      return CorrectPermutation(t.A, t.B, t.C);
    }

    // TODO: The surface removal algorithm only works if Front == +z
    internal bool CorrectPermutation(Point3F a, Point3F b, Point3F c) {
      return Vector3F.DotProduct(
        Vector3F.CrossProduct(new Vector3F(a, b), new Vector3F(a, c)), Front) >= 0;
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

    private void ConvertFacet(Triangle3F t) {
      int ia = AddPoint(t.A);
      int ib = AddPoint(t.B);
      int ic = AddPoint(t.C);
      _faces.Add(new TriangleFace(ia, ib, ic));
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
