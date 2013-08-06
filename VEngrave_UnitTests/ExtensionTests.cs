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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CamBam.CAD;
using CamBam.Geom;

using VEngraveForCamBam;
using VEngraveForCamBam.CamBamExtensions;

using GA = VEngrave_UnitTests.GeometryAssertions;

namespace VEngrave_UnitTests {
  [TestClass]
  public class ExtensionTests {

    [TestMethod, TestCategory("Small")]
    public void TestPointTo2D() {
      for (double x = -0.8; x < 2 ; x += .33) {
        for (double y = -7.9; y < 8; y += 1.56) {
          for (double z = -1.6; z < 2.7; z += .714) {
            Assert.AreEqual(new Point2F(x, y), new Point3F(x, y, z).To2D(),
              "x = {0}, y = {1}, z = {2}", x, y, z);
          }
        }
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestPointTo3D() {
      for (double x = -0.83; x < 2; x += .32) {
        for (double y = -7.59; y < 8.13; y += 1.7) {
          for (double z = -1.34; z < 2.78; z += .691) {
            Assert.AreEqual(new Point3F(x, y, 0), new Point2F(x, y).To3D(),
                "x = {0}, y = {1}, z = {2}", x, y, z);
            Assert.AreEqual(new Point3F(x, y, z), new Point2F(x, y).To3D(z),
                "x = {0}, y = {1}, z = {2}", x, y, z);
          }
        }
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestProjectTo() {
      GA.AssertAreApproxEqual(new Point3F(0.5, 0.5, 0.5),
          new Point2F(0.5, 0.5).ProjectTo(
              new Triangle3F(new Point3F(0, 0, 0), new Point3F(5, 2, 2),
                             new Point3F(0, 1, 1))));
    }

    [TestMethod, TestCategory("Small")]
    public void TestVectorTo2D() {
      for (double x = -0.8; x < 2 ; x += .33) {
        for (double y = -7.9; y < 8; y += 1.56) {
          for (double z = -1.6; z < 2.7; z += .714) {
            Assert.AreEqual(new Vector2F(x, y), new Vector3F(x, y, z).To2D(),
              "x = {0}, y = {1}, z = {2}", x, y, z);
          }
        }
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestVectorTo3D() {
      const double x = 2.521, y = 13.65, z = 13.5;
      Assert.AreEqual(new Vector3F(x, y, 0), new Vector2F(x, y).To3D());
      Assert.AreEqual(new Vector3F(x, y, z), new Vector2F(x, y).To3D(z));
    }

    [TestMethod, TestCategory("Small")]
    public void TestLineTo2D() {
      var a = new Point3F(23.0, 4.5, 6.87);
      var b = new Point3F(5.0, 7.5, 9.3);
      Assert.AreEqual(new Line2F(a.To2D(), b.To2D()), new Line3F(a, b).To2D());
    }

    [TestMethod, TestCategory("Small")]
    public void TestLineTo3D() {
      var a = new Point2F(3.0, 4.67);
      var b = new Point2F(8.0, 75.6);
      Assert.AreEqual(new Line3F(a.To3D(), b.To3D()), new Line2F(a, b).To3D());
      Assert.AreEqual(new Line3F(a.To3D(1.3), b.To3D(1.3)), new Line2F(a, b).To3D(1.3));
      Assert.AreEqual(new Line3F(a.To3D(1.1), b.To3D(2.7)), new Line2F(a, b).To3D(1.1, 2.7));
    }

    [TestMethod, TestCategory("Small")]
    public void TestIntersectsLineLine() {
      Assert.IsTrue(new Line2F(new Point2F(0, 0), new Point2F(1, 1))
          .Intersects(new Line2F(new Point2F(0, 1), new Point2F(1, 0))));
      Assert.IsFalse(new Line2F(new Point2F(0, 0), new Point2F(1, 0))
          .Intersects(new Line2F(new Point2F(0, 1), new Point2F(1, 1))));
      Assert.IsFalse(new Line2F(new Point2F(0, 0), new Point2F(1, 1))
          .Intersects(new Line2F(new Point2F(0.5, 0.5), new Point2F(1.5, 1.5))));
    }

    [TestMethod, TestCategory("Small")]
    public void TestIntersectsLineTriangle2F() {
      Assert.IsTrue(new Line2F(new Point2F(0, 0), new Point2F(1, 1))
          .Intersects(new Triangle2F(
            new Point2F(0, 1), new Point2F(1, 0), new Point2F(3, 3))));
      Assert.IsTrue(new Line2F(new Point2F(0, 0), new Point2F(1, 1))
          .Intersects(new Triangle2F(
            new Point2F(-5, -5), new Point2F(1, 0), new Point2F(0, 1))));
      Assert.IsTrue(new Line2F(new Point2F(0, 0), new Point2F(1, 1))
          .Intersects(new Triangle2F(
            new Point2F(0, 2), new Point2F(-3, -2), new Point2F(0.5, 0))));
      Assert.IsFalse(new Line2F(new Point2F(2, 0), new Point2F(3, 1))
          .Intersects(new Triangle2F(
            new Point2F(0, 2), new Point2F(-3, -2), new Point2F(0.5, 0))));
    }

    [TestMethod, TestCategory("Small")]
    public void TestSliceLineBasic1R2L() {
      Point3F p000 = new Point3F(0, 0, 0);
      Point3F p100 = new Point3F(1, 0, 0);
      Point3F p111 = new Point3F(1, 1, 1);
      Triangle3F t = new Triangle3F(p000, p100, p111);
      Triangle3FArray left = new Triangle3FArray();
      Triangle3FArray right = new Triangle3FArray();
      Line3F line = new Line3F(new Point3F(0, -0.5, 1), new Point3F(2, 1.5, 2));

      t.Slice(line, ref left, ref right);

      Assert.AreEqual(1, right.Count);
      Assert.IsTrue(right[0].CalcNormal().Z > 0, "right[0] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(new Point3F(0.5, 0, 0), p100,
                                             new Point3F(1, 0.5, 0.5)),
                           right[0], 1e-10);

      Assert.AreEqual(2, left.Count);
      Assert.IsTrue(left[0].CalcNormal().Z > 0, "left[0] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(new Point3F(0.5, 0, 0),
                                             new Point3F(1, 0.5, 0.5), p000),
                           left[0], 1e-10);

      Assert.IsTrue(left[1].CalcNormal().Z > 0, "left[1] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(new Point3F(1, 0.5, 0.5), p111, p000),
                             left[1], 1e-10);      
    }

    [TestMethod, TestCategory("Small")]
    public void TestSliceLineBasic2R1L() {
      Point3F p000 = new Point3F(0, 0, 0);
      Point3F p100 = new Point3F(1, 0, 0);
      Point3F p111 = new Point3F(1, 1, 1);
      Triangle3F t = new Triangle3F(p000, p100, p111);
      Triangle3FArray left = new Triangle3FArray();
      Triangle3FArray right = new Triangle3FArray();
      Line3F line = new Line3F(new Point3F(0.5, -0.5, 1), new Point3F(0.5, 1.5, 2));

      t.Slice(line, ref left, ref right);

      Assert.AreEqual(2, right.Count);
      Assert.IsTrue(right[0].CalcNormal().Z > 0, "right[0] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(p100, new Point3F(0.5, 0.5, 0.5),
                                             new Point3F(0.5, 0, 0)),
                           right[0], 1e-10);

      Assert.IsTrue(right[1].CalcNormal().Z > 0, "right[1] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(p100, p111,
                                             new Point3F(0.5, 0.5, 0.5)),
                           right[1], 1e-10);

      Assert.AreEqual(1, left.Count);
      Assert.IsTrue(left[0].CalcNormal().Z > 0, "left[0] is CCW");
      GA.AssertAreApproxEqual(new Triangle3F(new Point3F(0.5, 0, 0),
                                             new Point3F(0.5, 0.5, 0.5), p000),
                           left[0], 1e-10);
    }

    [TestMethod, TestCategory("Small")]
    public void TestSliceLineVertex() {
      Point3F p000 = new Point3F(0, 0, 0);
      Point3F p100 = new Point3F(1, 0, 0);
      Point3F p011 = new Point3F(0, 1, 1);
      Triangle3F t = new Triangle3F(p000, p100, p011);
      Triangle3FArray left = new Triangle3FArray();
      Triangle3FArray right = new Triangle3FArray();
      Line3F line = new Line3F(new Point3F(-0.5, -0.5, 1), new Point3F(1.5, 1.5, 2));

      t.Slice(line, ref left, ref right);

      Assert.AreEqual(1, right.Count);
      Assert.IsTrue(right[0].CalcNormal().Z > 0, "right[0] is CCW");
      GA.AssertAreApproxEqual(p000, right[0].A, 1e-10);
      GA.AssertAreApproxEqual(p100, right[0].B, 1e-10);
      GA.AssertAreApproxEqual(new Point3F(0.5, 0.5, 0.5), right[0].C, 1e-10);

      Assert.AreEqual(1, left.Count);
      Assert.IsTrue(left[0].CalcNormal().Z > 0, "left[0] is CCW");
      GA.AssertAreApproxEqual(p000, left[0].A, 1e-10);
      GA.AssertAreApproxEqual(new Point3F(0.5, 0.5, 0.5), left[0].B, 1e-10);
      GA.AssertAreApproxEqual(p011, left[0].C, 1e-10);
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestSliceProjectedPoints() {
      Point3F tA = new Point3F(1.10455260341352, 1.10864904182517, 0);
      Point3F tB = new Point3F(1.10366597719706, 1.10753169851535,
                               -0.00101254294727068);
      Point3F tC = new Point3F(1.10445890833639, 1.10690201333341, 0);
      Triangle3F t = new Triangle3F(tA, tB, tC);
      Point3F v1 = new Point3F(1.10366597719706, 1.10753169851535,
                               -0.00101254294727068);
      Point3F v2 = new Point3F(1.10455260341339, 1.10864904182497,
                               -1.27897692436818E-13);

      double v1z = v1.Z;
      double tv1z = t.ZAtPoint(v1.To2D());
      double v2z = v2.Z;
      double tv2z = t.ZAtPoint(v2.To2D());

      Assert.IsTrue(v1.Below(t));
      Assert.IsTrue(v2.Above(t));
      // This gives an undefined point! 
      //Assert.IsFalse(t.LinePlaneIntersection(v1, v2).IsUndefined);
    }
  }
  //private void doTestPointAdd(Point2F p, Vector2F v) {
  //    Point2F p1 = p;
  //    p1.Add(v);
  //    Assert.AreEqual(p.X + v.X, p1.X, 1e-8);
  //    Assert.AreEqual(p.Y + v.Y, p1.Y, 1e-8);
  //}
  //[TestMethod, TestCategory("Small")]
  //public void TestPointAdd() {
  //    doTestPointAdd(new Point2F(1.3, 2.4), new Vector2F(0.2, 0.1));
  //    doTestPointAdd(new Point2F(1.0, 2.4), new Vector2F(0.2, 0.1));
  //    doTestPointAdd(new Point2F(1.3, 2.0), new Vector2F(0.2, 0.1));
  //    doTestPointAdd(new Point2F(1.3, 2.4), new Vector2F(1.2, 0.1));
  //    doTestPointAdd(new Point2F(1.3, 2.4), new Vector2F(0.2, 1.1));

  //    doTestPointAdd(new Point2F(0.0, 0.0), new Vector2F(0.0, 0.0));
  //    doTestPointAdd(new Point2F(-1.3, -2.4), new Vector2F(-0.2, -0.1));
  //    doTestPointAdd(new Point2F(Double.MaxValue, Double.MaxValue),
  //                   new Vector2F(Double.MaxValue, Double.MaxValue));
  //    doTestPointAdd(new Point2F(Double.MinValue, Double.MinValue),
  //                   new Vector2F(Double.MinValue, Double.MinValue));
  //}
  //private void doTestVectorAdd(Vector2F p, Vector2F v) {
  //    Vector2F p1 = p;
  //    p1.Add(v);
  //    Assert.AreEqual(p.X + v.X, p1.X, 1e-8);
  //    Assert.AreEqual(p.Y + v.Y, p1.Y, 1e-8);
  //}
  //[TestMethod, TestCategory("Small")]
  //public void TestVectorAdd() {
  //    doTestVectorAdd(new Vector2F(1.3, 2.4), new Vector2F(0.2, 0.1));
  //    doTestVectorAdd(new Vector2F(1.0, 2.4), new Vector2F(0.2, 0.1));
  //    doTestVectorAdd(new Vector2F(1.3, 2.0), new Vector2F(0.2, 0.1));
  //    doTestVectorAdd(new Vector2F(1.3, 2.4), new Vector2F(1.2, 0.1));
  //    doTestVectorAdd(new Vector2F(1.3, 2.4), new Vector2F(0.2, 1.1));

  //    doTestVectorAdd(new Vector2F(0.0, 0.0), new Vector2F(0.0, 0.0));
  //    doTestVectorAdd(new Vector2F(-1.3, -2.4), new Vector2F(-0.2, -0.1));
  //    doTestVectorAdd(new Vector2F(Double.MaxValue, Double.MaxValue),
  //                    new Vector2F(Double.MaxValue, Double.MaxValue));
  //    doTestVectorAdd(new Vector2F(Double.MinValue, Double.MinValue),
  //                    new Vector2F(Double.MinValue, Double.MinValue));
  //}

}
