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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;

using CamBam.CAD;
using CamBam.CAM;
using CamBam.Geom;
using CamBam.Geom.BSP;
using CamBam.Values;

using VEngraveForCamBam.CamBamExtensions;
using GA = VEngrave_UnitTests.GeometryAssertions;

namespace VEngrave_UnitTests {
  [TestClass]
  public class CamBamTests {
    public TestContext TestContext {get; set;}
    [TestMethod, TestCategory("Small")]
    public void TestCBValue() {
      CBValue<double> cbValue = new CBValue<double>(5.0);
      cbValue.SetState(CBValueStates.Default);
      Assert.IsTrue(cbValue.IsDefault);
      Assert.IsFalse(cbValue.IsValue);
      Assert.AreEqual(5.0, cbValue.Value);
      cbValue.Value = 0.0;
      Assert.IsTrue(cbValue.IsDefault);
      Assert.IsFalse(cbValue.IsValue);
      Assert.AreEqual(0.0, cbValue.Value);
      Assert.AreEqual(5.0, cbValue.Cached);
      cbValue.SetValue(7.5);
      Assert.IsTrue(cbValue.IsValue);
      Assert.AreEqual(7.5, cbValue.Value);
      Assert.AreEqual(7.5, cbValue.Cached);
      cbValue.Value = 0.0;    // this compiles but has no effect
      Assert.IsFalse(cbValue.IsDefault);
      Assert.IsTrue(cbValue.IsValue);
      Assert.AreEqual(0.0, cbValue.Value);
      Assert.AreEqual(7.5, cbValue.GetCache());
      cbValue.SetCache(3.1);
      Assert.IsTrue(cbValue.IsCacheSet);
      Assert.AreEqual(0.0, cbValue.Value);
      Assert.AreEqual(3.1, cbValue.GetCache());
      CBValue<double> defaultCBValue = new CBValue<double>();
      Assert.IsTrue(defaultCBValue.IsDefault);
    }
    [TestMethod, TestCategory("Small")]
    public void TestPolylineClosed() {
      Polyline pl = new Polyline();
      pl.Add(0, 0, 0);
      pl.Add(1, 0, 0);
      pl.Add(1, 1, 0);
      pl.Add(0, 0, 0);
      Assert.IsFalse(pl.Closed);
      Assert.AreEqual(3, pl.NumSegments);
      Assert.AreEqual(4, pl.Points.Count);
      Assert.AreEqual(-1, pl.PrevSegment(0));
      Assert.AreEqual(3, pl.NextSegment(2));
      Assert.AreEqual(-1, pl.NextSegment(3));

      Assert.IsTrue(pl.CheckForClosed(1e-3));
      Assert.IsTrue(pl.Closed);
      Assert.AreEqual(3, pl.NumSegments);
      Assert.AreEqual(3, pl.Points.Count);
      Assert.AreEqual(2, pl.PrevSegment(0));
      Assert.AreEqual(0, pl.NextSegment(2));
    }
    [TestMethod, TestCategory("Small")]
    public void TestPoint2F() {
      Assert.AreEqual(new Point2F(0, 0), new Point2F());
      Assert.IsTrue(new Point2F(1, double.NaN).IsUndefined);
      Assert.IsTrue(new Point2F(double.NaN, 2).IsUndefined);
      Assert.IsFalse(new Point2F(-1, -1).IsUndefined);
      Assert.IsFalse(new Point2F(-1, 0).IsUndefined);
      Assert.IsFalse(new Point2F(-1, 1).IsUndefined);
      Assert.IsFalse(new Point2F(0, -1).IsUndefined);
      Assert.IsFalse(new Point2F(0, 0).IsUndefined);
      Assert.IsFalse(new Point2F(0, 1).IsUndefined);
      Assert.IsFalse(new Point2F(1, -1).IsUndefined);
      Assert.IsFalse(new Point2F(1, 0).IsUndefined);
      Assert.IsFalse(new Point2F(1, 1).IsUndefined);
    }
    [TestMethod, TestCategory("Small")]
    public void TestPoint3F() {
      Assert.AreEqual(new Point3F(0, 0, 0), new Point3F());
      Assert.IsTrue(new Point3F(double.NaN, 0, 1).IsUndefined);
      Assert.IsTrue(new Point3F(2, double.NaN, -3).IsUndefined);
      Assert.IsTrue(new Point3F(-4, 5, double.NaN).IsUndefined);
      for (double x = -1; x < 1.1; ++x) {
        for (double y = -1; y < 1.1; ++y) {
          for (double z = -1; z < 1.1; ++z) {
            Assert.IsFalse(new Point3F(x, y, z).IsUndefined);
          }
        }
      }
      Assert.IsTrue(new Point3F(0, 0, 0).IsZero);
      Assert.IsFalse(new Point3F(-double.Epsilon, 0, 0).IsZero);
      Assert.IsFalse(new Point3F(double.Epsilon, 0, 0).IsZero);
      Assert.IsFalse(new Point3F(0, -double.Epsilon, 0).IsZero);
      Assert.IsFalse(new Point3F(0, double.Epsilon, 0).IsZero);
      Assert.IsFalse(new Point3F(0, 0, -double.Epsilon).IsZero);
      Assert.IsFalse(new Point3F(0, 0, double.Epsilon).IsZero);
    }
    [TestMethod, TestCategory("Small")]
    public void TestPointInPolyline() {
      Polyline pl = new Polyline();
      pl.Add(0, 0, 0);
      pl.Add(1, 0, 0);
      pl.Add(1, 1, 0);
      pl.Add(0, 1, 0);
      pl.Closed = true;
      Assert.IsTrue(pl.PointInPolyline(new Point2F(0.5, 0.5), 0),
                    "PointInPolyline .5,.5");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(1.5, 1.5), 0),
                     "PointInPolyline 1.5, 1.5");

      Assert.IsFalse(pl.PointInPolyline(new Point2F(0, 0), 0),
                     "PointInPolyline 0,0");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(0, 0), 1e-15),
                     "PointInPolyline 0,0 fuzz 1e-15");

      Assert.IsTrue(pl.PointInPolyline(new Point2F(1.1e-5, 1.1e-5), 0),
                    "PointInPolyline 1.1e-5, 1.1e-5");

      Assert.IsTrue(pl.PointInPolyline(new Point2F(1.1e-6, 1.1e-6), 0),
                    "PointInPolyline(new Point2F(1.1e-6, 1.1e-6), 0)");
      Assert.IsTrue(pl.PointInPolyline(new Point2F(1.1e-6, 1.1e-6), 1e-6),
                    "PointInPolyline(new Point2F(1.1e-6, 1.1e-6), 1e-6)");
      Assert.IsTrue(pl.PointInPolyline(new Point2F(1.1e-6, 1.1e-6), .5),
                    "PointInPolyline(new Point2F(1.1e-6, 1.1e-6), .5)");

      Assert.IsFalse(pl.PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 0),
                     "PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 0)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 0.5e-6),
                     "PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 0.5e-6)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 1.0e-6),
                     "PointInPolyline(new Point2F(0.9e-6, 0.9e-6), 1.0e-6)");

      Assert.IsFalse(pl.PointInPolyline(new Point2F(-1e-10, -1e-10), -2e-10),
                     "PointInPolyline(-1e-10, -1e-10), -2e-10)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(-1e-10, -1e-10), -0.5e-10),
                     "PointInPolyline(-1e-10, -1e-10), -0.5e-10)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(-1e-10, -1e-10), 0),
                     "PointInPolyline(-1e-10, -1e-10), 0)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(-1e-10, -1e-10), 0.5e-10),
                     "PointInPolyline(new Point2F(-1e-10, -1e-10), 0.5e-10)");
      Assert.IsFalse(pl.PointInPolyline(new Point2F(-1e-10, -1e-10), 2e-10),
                     "PointInPolyline(new Point2F(-1e-10, -1e-10), 2e-10)");
    }
    [TestMethod, TestCategory("Small")]
    public void TestPointInPolyline2() {
      Polyline pl = new Polyline();
      pl.Add(0, 0, 0);
      pl.Add(1, 0, 0);
      pl.Add(1, 1, 0);
      pl.Add(0, 1, 0);
      pl.Closed = true;
      Assert.IsTrue(pl.PointInPolyline2(new Point2F(0.5, 0.5), 0),
                    "PointInPolyline2 .5,.5");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(1.5, 1.5), 0),
                     "PointInPolyline2 1.5, 1.5");

      Assert.IsFalse(pl.PointInPolyline2(new Point2F(0, 0), 0),
                     "PointInPolyline2 0,0");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(0, 0), 1e-15),
                     "PointInPolyline2 0,0 fuzz 1e-15");

      Assert.IsTrue(pl.PointInPolyline2(new Point2F(1.1e-5, 1.1e-5), 0),
                    "PointInPolyline2 1.1e-5, 1.1e-5");

      Assert.IsTrue(pl.PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), 0),
                    "PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), 0)");
      Assert.IsTrue(pl.PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), 1e-6),
                    "PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), 1e-6)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), .5),
                     "PointInPolyline2(new Point2F(1.1e-6, 1.1e-6), .5)");

      Assert.IsTrue(pl.PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 0),
                    "PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 0)");
      Assert.IsTrue(pl.PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 0.5e-6),
                    "PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 0.5e-6)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 1.0e-6),
                     "PointInPolyline2(new Point2F(0.9e-6, 0.9e-6), 1.0e-6)");

      Assert.IsFalse(pl.PointInPolyline2(new Point2F(-1e-10, -1e-10), -2e-10),
                     "PointInPolyline2(-1e-10, -1e-10), -2e-10)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(-1e-10, -1e-10), -0.5e-10),
                     "PointInPolyline2(-1e-10, -1e-10), -0.5e-10)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(-1e-10, -1e-10), 0),
                     "PointInPolyline2(-1e-10, -1e-10), 0)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(-1e-10, -1e-10), 0.5e-10),
                     "PointInPolyline2(-1e-10, -1e-10), 0.5e-10)");
      Assert.IsFalse(pl.PointInPolyline2(new Point2F(-1e-10, -1e-10), 2e-10),
                     "PointInPolyline2(new Point2F(-1e-10, -1e-10), 2e-10)");
    }
    [TestMethod, TestCategory("Small")]
    public void TestGetNearestPoint() {
      Polyline pl = new Polyline();
      pl.Add(0, 0, 0);
      pl.Add(1, 0, 0);
      pl.Add(1, 1, 0);
      pl.Add(0, 1, 0);
      pl.Closed = true;
      Vector2F nearestNormal = new Vector2F();
      int nearestSegment = -1;
      Point3F nearestPoint = pl.GetNearestPoint(
          new Point2F(.5, .25), ref nearestNormal, ref nearestSegment);
      GA.AssertAreApproxEqual(new Point3F(0.5, 0, 0), nearestPoint, 1e-10);
      Assert.AreEqual(0, nearestSegment, "nearestSegment");
      // normal goes from point to polyline
      GA.AssertAreApproxEqual(new Point2F(0, -1), new Point2F(nearestNormal.X,
                                                              nearestNormal.Y),
                              1e-10);
    }
    [TestMethod, TestCategory("Small")]
    public void TestLine2FIntersect() {
      var p00 = new Point2F(0, 0);
      var p01 = new Point2F(0, 1);
      var p10 = new Point2F(1, 0);
      var p11 = new Point2F(1, 1);
      var a = new Line2F(p00, p11);
      var b = new Line2F(p10, p01);
      Line2F result;
      // A simple crossing gives one point
      result = Line2F.Intersect(a, b);
      Assert.IsTrue(result.IsUndefined);
      GA.AssertAreApproxEqual(new Point2F(0.5, 0.5), result.p1);
      Assert.IsTrue(result.p2.IsUndefined);

      // Self-intersect gives undefined
      result = Line2F.Intersect(a, a);
      Assert.IsTrue(result.IsUndefined);
      Assert.IsTrue(result.p1.IsUndefined);
      Assert.IsTrue(result.p2.IsUndefined);

      // Stopping short of intersect gives undefined
      var c = new Line2F(p00, new Point2F(0.25, 0.25));
      result = Line2F.Intersect(c, b);
      Assert.IsTrue(result.IsUndefined);
      Assert.IsTrue(result.p1.IsUndefined);
      Assert.IsTrue(result.p2.IsUndefined);

      // T-intersect gives single point
      var d = new Line2F(p00, new Point2F(0.5, 0.5));
      result = Line2F.Intersect(d, b);
      Assert.IsTrue(result.IsUndefined);
      Assert.AreEqual(new Point2F(0.5, 0.5), result.p1);
      Assert.IsTrue(result.p2.IsUndefined);

      // Parallel lines give undefined
      var e = new Line2F(p00, p01);
      var f = new Line2F(p10, p11);
      result = Line2F.Intersect(e, f);
      Assert.IsTrue(result.IsUndefined);
      Assert.IsTrue(result.p1.IsUndefined);
      Assert.IsTrue(result.p2.IsUndefined);

      // Overlapping (but distinct) lines give overlapping section
      var quarter_point = new Point2F(0.25, 0.25);
      var three_quarter_point = new Point2F(0.75, 0.75);
      var g = new Line2F(p00, three_quarter_point);
      var h = new Line2F(quarter_point, p11);
      result = Line2F.Intersect(g, h);
      Assert.IsFalse(result.IsUndefined);
      Assert.AreEqual(quarter_point, result.p1);
      Assert.AreEqual(three_quarter_point, result.p2);
    }
    [TestMethod, TestCategory("Small")]
    public void TestLine2FOverlaps() {
      var p00 = new Point2F(0, 0);
      var p01 = new Point2F(0, 1);
      var p10 = new Point2F(1, 0);
      var p11 = new Point2F(1, 1);
      var p22 = new Point2F(2, 2);
      var p33 = new Point2F(3, 3);

      var a = new Line2F(p00, p11);
      var b = new Line2F(p10, p01);
      Point2F[] result;
      result = Line2F.Overlaps(a, b, 1e-10);
      Assert.IsNull(result);
      result = Line2F.Overlaps(a, a, 1e-10);
      Assert.IsNull(result);

      var c = new Line2F(p00, new Point2F(0.25, 0.25));
      result = Line2F.Overlaps(c, b, 1e-10);
      Assert.IsNull(result);
      var d = new Line2F(p00, new Point2F(0.5, 0.5));
      result = Line2F.Overlaps(d, b, 1e-10);
      Assert.IsNull(result);
      var e = new Line2F(p00, p01);
      var f = new Line2F(p10, p11);
      result = Line2F.Overlaps(e, f, 1e-10);
      Assert.IsNull(result);

      var g = new Line2F(p00, p22);
      var h = new Line2F(p11, p33);
      result = Line2F.Overlaps(g, h, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[0]);
      Assert.AreEqual(p22, result[1]);
      var i = new Line2F(p22, p00);
      result = Line2F.Overlaps(i, h, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[0]);
      Assert.AreEqual(p22, result[1]);
      var j = new Line2F(p33, p11);
      result = Line2F.Overlaps(i, j, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[0]);
      Assert.AreEqual(p22, result[1]);

      result = Line2F.Overlaps(h, g, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[1]);
      Assert.AreEqual(p22, result[0]);
      result = Line2F.Overlaps(h, i, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[1]);
      Assert.AreEqual(p22, result[0]);
      result = Line2F.Overlaps(j, i, 1e-10);
      Assert.AreEqual(2, result.Length);
      Assert.AreEqual(p11, result[1]);
      Assert.AreEqual(p22, result[0]);
    }
    [TestMethod, TestCategory("Small")]
    public void TestLine2FProjectionIntersects() {
      var intersection = Line2F.ProjectionIntersect(
        new Line2F(new Point2F(0, 0), new Point2F(0.25, 0.25)),
        new Line2F(new Point2F(1, 0), new Point2F(0, 1)));
      Assert.IsNotNull(intersection);
      Assert.IsFalse(intersection.IsUndefined);
      Assert.AreEqual(0.5, intersection.X, 1e-14);
      Assert.AreEqual(0.5, intersection.Y, 1e-14);
      var intersection2 = Line2F.ProjectionIntersect(
        new Line2F(new Point2F(1, 0), new Point2F(0, 1)),
        new Line2F(new Point2F(0, 0), new Point2F(0.25, 0.25)));
      Assert.IsNotNull(intersection2);
      Assert.IsFalse(intersection2.IsUndefined);
      Assert.AreEqual(0.5, intersection2.X, 1e-14);
      Assert.AreEqual(0.5, intersection2.Y, 1e-14);
      var notintersection = Line2F.ProjectionIntersect(
        new Line2F(new Point2F(0, 0), new Point2F(1, 1)),
        new Line2F(new Point2F(0, 1), new Point2F(1, 2)));
      Assert.IsNotNull(notintersection);
      Assert.IsTrue(notintersection.IsUndefined);
    }
    [TestMethod, TestCategory("Small")]
    public void TestTriangle2F() {
      var a = new Point2F(0.0, 0.0);
      var b = new Point2F(1.0, 0.0);
      var c = new Point2F(1.0, 1.0);
      var triangle = new Triangle2F(a, b, c);
      Assert.AreEqual(a, triangle.A);
      Assert.AreEqual(b, triangle.B);
      Assert.AreEqual(c, triangle.C);
      Assert.IsTrue(triangle.IsInside(new Rect2F(new Point2F(-1.0, -1.0),
                                                 new Point2F(2.0, 2.0))));
      Assert.IsFalse(triangle.IsInside(new Rect2F(new Point2F(2.0, 2.0),
                                                  new Point2F(3.0, 3.0))));
      // IsInside checks for overlap
      Assert.IsTrue(triangle.IsInside(new Rect2F(new Point2F(0.5, 0.25),
                                                 new Point2F(2.0, 2.0))));
      Assert.IsTrue(triangle.IsInside(new Rect2F(a, c)));

      // Outer box not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(new Point2F(-1.0, -1.0),
                                                      new Point2F(2.0, 2.0))));
      // Overlap is not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.25),
                                                      new Point2F(2.0, 2.0))));
      // Bounding box not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(a, c)));
      // Nested box is inside
      Assert.IsTrue(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.0),
                                                     new Point2F(1.0, 0.5))));
      Assert.IsTrue(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.1),
                                                     new Point2F(0.5, 0.1))));
      double dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, 0.25), true,
                                             1e-10, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, 0.25), false,
                                             1e-10, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsFalse(triangle.PointInTriangle(new Point2F(0.5, 0.0), true,
                                              1e-10, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, 0.0), false,
                                             1e-10, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsFalse(triangle.PointInTriangle(new Point2F(0.5, -1e-3), true,
                                              1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsFalse(triangle.PointInTriangle(new Point2F(0.5, -1e-3), false,
                                              1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsFalse(triangle.PointInTriangle(new Point2F(0.5, +1e-3), true,
                                              1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, +1e-3), false,
                                             1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, +1e-1), true,
                                             1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);
      dval = 0.0;
      Assert.IsTrue(triangle.PointInTriangle(new Point2F(0.5, +1e-1), false,
                                             1e-2, ref dval));
      Assert.AreEqual(1.0, dval, 1e-10);

      Assert.IsTrue(triangle.PointInTriangleNew(new Point2F(0.5, 0.25)));
      Assert.IsTrue(triangle.PointInTriangleNew(new Point2F(0.5, 0.0)));
      Assert.IsFalse(triangle.PointInTriangleNew(new Point2F(0.5, -1e-3)));
      Assert.IsTrue(triangle.PointInTriangleNew(new Point2F(0.5, +1e-3)));
      Assert.IsTrue(triangle.PointInTriangleNew(new Point2F(0.5, +1e-1)));
    }
    [TestMethod, TestCategory("Medium")]
    public void TestTriangle3F() {
      var a = new Point3F(0.0, 0.0, 0.0);
      var b = new Point3F(1.0, 0.0, 0.0);
      var c = new Point3F(1.0, 1.0, 1.0);
      var triangle = new Triangle3F(a, b, c);
      Assert.AreEqual(a, triangle.GetVertex(0));
      Assert.AreEqual(b, triangle.GetVertex(1));
      Assert.AreEqual(c, triangle.GetVertex(2));
      Assert.IsTrue(triangle.IsInside(new Rect2F(new Point2F(-1.0, -1.0),
                                                 new Point2F(2.0, 2.0))));
      Assert.IsFalse(triangle.IsInside(new Rect2F(new Point2F(2.0, 2.0),
                                                  new Point2F(3.0, 3.0))));
      // IsInside checks for overlap
      Assert.IsTrue(triangle.IsInside(new Rect2F(new Point2F(0.5, 0.25),
                                                 new Point2F(2.0, 2.0))));
      Assert.IsTrue(triangle.IsInside(new Rect2F(a.To2D(), c.To2D())));

      // Outer box not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(new Point2F(-1.0, -1.0),
                                                      new Point2F(2.0, 2.0))));
      // Overlap is not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.25),
                                                      new Point2F(2.0, 2.0))));
      // Bounding box not inside triangle
      Assert.IsFalse(triangle.RegionInside(new Rect2F(a.To2D(), c.To2D())));
      // Nested box is inside
      Assert.IsTrue(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.0),
                                                     new Point2F(1.0, 0.5))));
      Assert.IsTrue(triangle.RegionInside(new Rect2F(new Point2F(0.5, 0.1),
                                                     new Point2F(0.5, 0.1))));

      Assert.AreEqual(0.5, triangle.ZAtPoint(new Point2F(1.0, 0.5)), 1e-10);
      Assert.AreEqual(0.5, triangle.ZAtPoint(new Point2F(10.0, 0.5)), 1e-10);
      Assert.AreEqual(10.0, triangle.ZAtPoint(new Point2F(1.0, 10.0)), 1e-10);
    }
    [TestMethod, TestCategory("Small")]
    public void TestTriangle3FLinePlaneIntersection() {
      var a = new Point3F(0.0, 0.0, 0.0);
      var b = new Point3F(1.0, 0.0, 0.0);
      var c = new Point3F(1.0, 1.0, 1.0);
      var triangle = new Triangle3F(a, b, c);

      Point3F intersect = triangle.LinePlaneIntersection(
        new Point3F(0.5, 0.0, 0.25), new Point3F(0.5, 1.0, 0.25));
      Assert.AreEqual(0.5, intersect.X, 1e-10);
      Assert.AreEqual(0.25, intersect.Y, 1e-10);
      Assert.AreEqual(0.25, intersect.Z, 1e-10);
      Point3F outsideintersect = triangle.LinePlaneIntersection(
        new Point3F(0.5, 0.0, 0.75), new Point3F(0.5, 1.0, 0.75));
      Assert.AreEqual(0.5, outsideintersect.X, 1e-10);
      Assert.AreEqual(0.75, outsideintersect.Y, 1e-10);
      Assert.AreEqual(0.75, outsideintersect.Z, 1e-10);
      Point3F notintersect = triangle.LinePlaneIntersection(
        new Point3F(0.5, 0.0, 0.75), new Point3F(0.5, 1.0, 1.75));
      Assert.IsTrue(notintersect.IsUndefined);

      var e2d = new Point2F(0.2, 0.3);
      var ez0 = triangle.ZAtPoint(e2d);
      var f2d = new Point2F(0.3, 0.2);
      var fz0 = triangle.ZAtPoint(f2d);
      for (double offset = 0.1; offset > 5e-8; offset /= 2) {
        var e = e2d.To3D(ez0 + offset);
        var f = f2d.To3D(fz0 - offset);
        var offsetInterect = triangle.LinePlaneIntersection(e, f);
        GA.AssertAreApproxEqual(new Point3F(0.25, 0.25, 0.25), offsetInterect);
      }
      triangle = new Triangle3F(
          new Point3F(1.10455260341352, 1.10864904182517, 0),
          new Point3F(1.10366597719706, 1.10753169851535,
                      -0.00101254294727068),
          new Point3F(1.10445890833639, 1.10690201333341, 0));
      var v1 = new Point3F(1.10366597719706, 1.10753169851535, 
                           -0.00101254294727068);
      var v2 = new Point3F(1.10455260341339, 1.10864904182497,
                           -1.27897692436818E-13);
      Point3F problemIntersection = triangle.LinePlaneIntersection(v1, v2);
      Assert.IsTrue(problemIntersection.IsUndefined);
    }
    [TestMethod, TestCategory("Medium")]
    public void TestBSPNode() {
      BSPNode.MinNodes = 3;
      BSPNode.MaxNodes = 100;
      BSPNode root = new BSPNode();
      root.Region = new Rect2F(new Point2F(0, 0), new Point2F(10, 10));
      double dx = 0.5, dy = 0.5;
      List<Triangle3F> facets = new List<Triangle3F>();
      for (double x = 0; x < 10; x += dx) {
        for (double y = 0; y < 10; y += dy) {
          Point3F botleft = new Point3F(x, y, 0);
          Point3F botright = new Point3F(x + dx, y, 0);
          Point3F topleft = new Point3F(x, y + dy, 0);
          Point3F topright = new Point3F(x + dx, y + dy, 0);
          Point3F center = new Point3F(x + dx/2, y + dy/2, -1);
          Triangle3F facet1 = new Triangle3F(botleft, botright, center);
          Triangle3F facet2 = new Triangle3F(botright, topright, center);
          Triangle3F facet3 = new Triangle3F(topright, topleft, center);
          Triangle3F facet4 = new Triangle3F(topleft, botleft, center);
          root.AddItem(facet1); facets.Add(facet1);
          root.AddItem(facet2); facets.Add(facet2);
          root.AddItem(facet3); facets.Add(facet3);
          root.AddItem(facet4); facets.Add(facet4);
        }
      }
      Triangle3F wildfacet = new Triangle3F(new Point3F(5, 7, 0),
                                            new Point3F(15, 7, 1),
                                            new Point3F(14.5, 21, -0.6));
      root.AddItem(wildfacet); facets.Add(wildfacet);

      var triangle = new Triangle3F(new Point3F(0.75, 0.75, -0.25),
                                    new Point3F(1.25, 0.75, -0.3),
                                    new Point3F(1.05, 1.2, -0.35));
      var nearby = root.GetNearItems(triangle);
      foreach (IBSPItem item in nearby) {
        BSPNode node = root.FindEndNode(item);
        Assert.IsTrue(triangle.IsInside(node.Region));
        Assert.IsTrue(node.Items.Contains(item));
        Assert.IsTrue(item.IsInside(node.Region));
        Assert.IsTrue(facets.Remove((Triangle3F) item));
      }
      Point3F triangleMin = new Point3F(), triangleMax = new Point3F();
      triangle.Extrema(ref triangleMin, ref triangleMax);
      foreach (Triangle3F remainingFacet in facets) {
        Point3F remainingFacetMin = new Point3F(),
                remainingFacetMax = new Point3F();
        remainingFacet.Extrema(ref remainingFacetMin, ref remainingFacetMax);
        Assert.IsTrue(triangleMax.X < remainingFacetMin.X
                      || remainingFacetMax.X < triangleMin.X
                      || triangleMax.Y < remainingFacetMin.Y
                      || remainingFacetMax.Y < triangleMin.Y);
      }
    }

    //    [TestMethod, TestCategory("Medium"), TestCategory("Repro")]
    public void TestBuildCutOrderEx_Inside() {
      var mop = new MOP3DSurface();
      var tseq = new ToolpathSequence(mop);
      tseq.Add(depthIndex: 0, offsetIndex: 0,
        entityID: new EntityIdentifier(3), parentEntityID: -1,
        toolpath: ProblemToolpath0(), direction: RotationDirection.CW,
        sourcepoint: Point3F.Undefined, zoffset: 0);
      tseq.Add(depthIndex: 0, offsetIndex: 0,
        entityID: new EntityIdentifier(33), parentEntityID: -1,
        toolpath: ProblemToolpath1(), direction: RotationDirection.CW,
        sourcepoint: Point3F.Undefined, zoffset: 0);
      tseq.UseSplitPoint3D = true;
      tseq.ToolDiameter = 0.0;
      mop.StartPoint.SetState(CBValueStates.Default);
      mop.StartPoint.SetCache(new Point3F());
      mop.SetToolpathSequence(tseq);
      mop.Toolpaths2.BuildCutOrderEx_Inside(
            CutOrderingOption.LevelFirst,
            MillingDirectionOptions.Mixed,
            SpindleDirectionOptions.CW,
            0.0000039370078740157482, mop.StartPoint,
            InsideOutsideOptions.Inside);
      Polyline updatedToolpath = tseq.Toolpaths[1].Toolpath;
      for (int i = 0; i < updatedToolpath.NumSegments; ++i) {
        Point3F from = updatedToolpath.Points[i].Point;
        Point3F to = updatedToolpath.Points[
          updatedToolpath.NextSegment(i)].Point;
        Assert.AreNotEqual(0.0, Point3F.Distance(from, to),
            "Segment {0} of updated toolpath is zero length", i);
        Assert.AreNotEqual(0.0, Point3F.Distance(from, to), 1e-20,
            "Segment {0} of updated toolpath is too short", i);
      }
    }
    private Polyline ProblemToolpath0() {
      var tp = new Polyline();
      tp.Add(0.142719886391866, 0.0085397992979962, 0);
      tp.Add(0.144074053508784, 0.00849285755992748, -0.000865750637465541);
      tp.Add(0.145475635276701, 0.00846317347745519, -0.00177592738568379);
      tp.Add(0.146927394859981, 0.00845199349255871, -0.00273329898645853);
      tp.Add(0.148432305571657, 0.00846068320417482, -0.00374086294347253);
      tp.Add(0.149874074703292, 0.0086116159437111, -0.00463189676593337);
      tp.Add(0.149844959963968, 0.00858606069790171, -0.00462961920196747);
      tp.Add(0.149681683516002, 0.00724843077515844, -0.00366681649237556);
      tp.Add(0.149613419979863, 0.00572952098269176, -0.00248729822815723);
      tp.Add(0.149531045504278, 0.00416679838330295, -0.00126568001821064);
      tp.Add(0.149433753579366, 0.00255835398549615, 0);
      tp.Add(0.149531208381944, 0.00416967743878708, -0.00126793804053512);
      tp.Add(0.1496151912192, 0.00576587929834833, -0.0025156237615344);
      tp.Add(0.149685702091133, 0.00734695956417993, -0.00374305716299783);
      tp.Add(0.149353373165093, 0.00847587270983809, -0.00436490406577042);
      tp.Add(0.147997666290601, 0.00845609632151992, -0.00344831342647512);
      tp.Add(0.146656464176033, 0.00845260078617664, -0.00255352770025043);
      tp.Add(0.145329766821387, 0.00846538610380827, -0.00168054688709636);
      tp.Add(0.144017574226665, 0.00849445227441479, -0.000829370987012883);
      tp.Add(0.142719886391866, 0.0085397992979962, 0);
      tp.CheckForClosed(2e-3);
      return tp;
    }
    private Polyline ProblemToolpath1() {
      var tp = new Polyline();
      tp.CheckForClosed(1e-3);
      tp.Add(0.237768816671673, 0.0569632941802821, 0);
      tp.Add(0.236573832970935, 0.0557590040509649, -0.00120429012931724);
      tp.Add(0.235336004477813, 0.0544679091880046, -0.00249538499227753);
      tp.Add(0.234052213468418, 0.0530819760977503, -0.00388131808253179);
      tp.Add(0.232719049900603, 0.0515921995034656, -0.00537109467681646);
      tp.Add(0.22959502066437, 0.047887246129341, -0.00907604805094114);
      tp.Add(0.227740239161952, 0.0451688223819917, -0.0116009949845273);
      tp.Add(0.22961575550246, 0.0435574538206944, -0.0104222846404123);
      tp.Add(0.231678150019275, 0.0422691988775098, -0.00913402969722765);
      tp.Add(0.236320400177519, 0.0399456134667754, -0.00681044428649331);
      tp.Add(0.238044698830246, 0.039299680962757, -0.00616451178247491);
      tp.Add(0.239710203659639, 0.0387692741389012, -0.0056341049586191);
      tp.Add(0.241327842432335, 0.0383420675500423, -0.00520689836976023);
      tp.Add(0.244112778053527, 0.037790561223477, -0.00465539204319486);
      tp.Add(0.245382556884815, 0.0375894914564898, -0.00445432227620772);
      tp.Add(0.246644034140566, 0.0374185056807134, -0.00428333650043133);
      tp.Add(0.247898536672287, 0.0372769029902726, -0.00414173380999047);
      tp.Add(0.250066451633866, 0.0370845724450368, -0.00394940326475473);
      tp.Add(0.252020572872136, 0.0369159146543711, -0.00378074547408905);
      tp.Add(0.253033866371938, 0.0370399248228781, -0.00348495029973492);
      tp.Add(0.256518816671673, 0.0399711066802821, 0);
      tp.Add(0.25303564304822, 0.0370414191802821, -0.00348317362345332);
      tp.Add(0.253589129171673, 0.0360648566802821, -0.0029296875);
      tp.Add(0.256518816671673, 0.0331351691802821, 0);
      tp.Add(0.253538617318225, 0.0361153685337304, -0.00298019935344829);
      tp.Add(0.252545217533742, 0.0368706332215828, -0.00373546404130066);
      tp.Add(0.249565018180294, 0.0371278505471265, -0.00399268136684441);
      tp.Add(0.248571618395811, 0.0372126180343108, -0.00407744885402873);
      tp.Add(0.247578218611328, 0.0373103630124231, -0.004175193832141);
      tp.Add(0.246584818826845, 0.0374258905364763, -0.00429072135619422);
      tp.Add(0.245591419042363, 0.0375592006064705, -0.00442403142618839);
      tp.Add(0.24459801925788, 0.0377102932224056, -0.00457512404212351);
      tp.Add(0.243604619473397, 0.0378791683842817, -0.00474399920399958);
      tp.Add(0.242611219688914, 0.0380647966394301, -0.00492962745914804);
      tp.Add(0.241617819904432, 0.038274647806613, -0.00513947862633092);
      tp.Add(0.240624420119949, 0.0385171859482507, -0.00538201676796861);
      tp.Add(0.239631020335466, 0.0387924110643432, -0.00565724188406113);
      tp.Add(0.238637620550983, 0.0391003231548906, -0.00596515397460846);
      tp.Add(0.2376442207665, 0.0394409222198927, -0.00630575303961061);
      tp.Add(0.236650820982018, 0.0398142082593497, -0.00667903907906759);
      tp.Add(0.235657421197535, 0.0402201812732615, -0.00708501209297938);
      tp.Add(0.234664021413052, 0.0406588412616281, -0.00752367208134599);
      tp.Add(0.233670621628569, 0.0411472966880215, -0.00801212750773941);
      tp.Add(0.232677221844087, 0.041691567724464, -0.00855639854418189);
      tp.Add(0.231683822059604, 0.0422658339540091, -0.00913066477372705);
      tp.Add(0.230690422275121, 0.0428700604511848, -0.00973489127090267);
      tp.Add(0.229697022490638, 0.0435042472159908, -0.0103690780357087);
      tp.Add(0.228703622706156, 0.0441683942484274, -0.0110332250681453);
      tp.Add(0.227710222921673, 0.0448211578264212, -0.0116859886461391);
      tp.Add(0.22671682313719, 0.0441332557109177, -0.0109980865306356);
      tp.Add(0.225723423352707, 0.0434755677378844, -0.0103403985576023);
      tp.Add(0.224730023568225, 0.0428480939073216, -0.00971292472703948);
      tp.Add(0.223736623783742, 0.0422508342192291, -0.00911566503894696);
      tp.Add(0.222743223999259, 0.0416837886736069, -0.00854861949332481);
      tp.Add(0.221749824214776, 0.0411469572704551, -0.00801178809017301);
      tp.Add(0.220756424430294, 0.0406577386964616, -0.00752256951617953);
      tp.Add(0.219763024645811, 0.0402141225540057, -0.00707895337372357);
      tp.Add(0.218769624861328, 0.0398040917120924, -0.00666892253181025);
      tp.Add(0.217776225076845, 0.0394276461707217, -0.00629247699043958);
      tp.Add(0.216782825292362, 0.0390847859298937, -0.00594961674961156);
      tp.Add(0.21578942550788, 0.0387755109896083, -0.00564034180932619);
      tp.Add(0.214796025723397, 0.0384998213498656, -0.00536465216958347);
      tp.Add(0.213802625938914, 0.0382577170106655, -0.0051225478303834);
      tp.Add(0.212809226154431, 0.0380491979720081, -0.00491402879172597);
      tp.Add(0.211815826369949, 0.0378641448128817, -0.00472897563259958);
      tp.Add(0.210822426585466, 0.0376961055576228, -0.00456093637734065);
      tp.Add(0.209829026800983, 0.037546063324508, -0.00441089414422591);
      tp.Add(0.2088356270165, 0.0374140181135374, -0.00427884893325534);
      tp.Add(0.207842227232018, 0.0372999699247111, -0.00416480074442896);
      tp.Add(0.206848827447535, 0.0372039187580289, -0.00406874957774675);
      tp.Add(0.202875228309604, 0.0368665166632278, -0.00373134748294574);
      tp.Add(0.201881828525121, 0.0361153685337301, -0.00298019935344804);
      tp.Add(0.198901629171673, 0.0331351691802821, 0);
      tp.Add(0.201831316671673, 0.0360648566802821, -0.0029296875);
      tp.Add(0.202376285573107, 0.0370414191802821, -0.00347465640143382);
      tp.Add(0.198901629171673, 0.0399711066802821, 0);
      tp.Add(0.202426972054776, 0.0369986824217054, -0.00352534288310294);
      tp.Add(0.203446556285958, 0.0369151330374301, -0.003779963857148);
      tp.Add(0.205428172458342, 0.0370837559604255, -0.00394858678014345);
      tp.Add(0.207610967958463, 0.0372760024676443, -0.00414083328736216);
      tp.Add(0.208858347989234, 0.0374168371366904, -0.00428166795640831);
      tp.Add(0.210112712675287, 0.0375870751135049, -0.00445190593322281);
      tp.Add(0.211375398270672, 0.0377874231269149, -0.00465225394663284);
      tp.Add(0.214154377756121, 0.0383396028188854, -0.00520443363860329);
      tp.Add(0.215782200466819, 0.038773384641116, -0.00563821546083386);
      tp.Add(0.217460366205584, 0.0393149895369933, -0.00617982035671119);
      tp.Add(0.219200694581653, 0.0399778930769997, -0.00684272389671761);
      tp.Add(0.223604038988779, 0.0421734059179236, -0.00903823673764154);
      tp.Add(0.225684692103434, 0.0434505373839013, -0.0103153682036192);
      tp.Add(0.227739774211287, 0.0450044007953803, -0.0116535898298355);
      tp.Add(0.225999818277196, 0.0477566344232414, -0.00920665975704071);
      tp.Add(0.222828992683864, 0.0515458864979843, -0.00541740768229778);
      tp.Add(0.221484670819408, 0.0530551467119393, -0.00390814746834283);
      tp.Add(0.220192387377262, 0.054454709569451, -0.0025085846108311);
      tp.Add(0.218948396123266, 0.0557544913602368, -0.00120880282004526);
      tp.Add(0.217749285421673, 0.0569632941802821, 0);
      tp.Add(0.218702596433578, 0.0560058040295208, -0.000957490150761298);
      tp.Add(0.219655907445483, 0.0550209614909386, -0.00194233268934349);
      tp.Add(0.220609218457387, 0.0540087665645355, -0.00295452761574656);
      tp.Add(0.221562529469292, 0.0529692192503116, -0.00399407492997055);
      tp.Add(0.222515840481197, 0.0519023195482667, -0.00506097463201543);
      tp.Add(0.223469151493102, 0.0508080674584009, -0.00615522672188121);
      tp.Add(0.224422462505006, 0.0496906235034767, -0.00727267067680543);
      tp.Add(0.225375773516911, 0.0485394644526019, -0.0084238297276802);
      tp.Add(0.226329084528816, 0.047334253331065, -0.00962904084921707);
      tp.Add(0.227282395540721, 0.046074990138866, -0.0108883040414161);
      tp.Add(0.228235706552625, 0.0461198499957718, -0.0108434441845103);
      tp.Add(0.22918901756453, 0.0473706364679302, -0.00959265771235193);
      tp.Add(0.230142328576435, 0.0485684496565146, -0.00839484452376752);
      tp.Add(0.23109563958834, 0.049713289561525, -0.00725000461875706);
      tp.Add(0.232048950600244, 0.0508239333132948, -0.00613936086698732);
      tp.Add(0.233002261612149, 0.0519129892740771, -0.00505030490620498);
      tp.Add(0.233955572624054, 0.0529757135750124, -0.00398758060526974);
      tp.Add(0.234908883635959, 0.0540121062161005, -0.00295118796418162);
      tp.Add(0.235862194647864, 0.0550221671973415, -0.00194112698294061);
      tp.Add(0.236815505659768, 0.0560058965187354, -0.000957397661546699);
      tp.Add(0.237768816671673, 0.0569632941802821, 0);
//      tp.CheckForClosed(2e-3);
      tp.Closed = true;
      return tp;
    }
  }
}