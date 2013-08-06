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
  public class SurfaceBuilderTests {
    [TestMethod, TestCategory("Small")]
    public void TestSurfaceBuilderAddFace3Points() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 0, 0);
      Point3F c = new Point3F(1, 1, 0);
      bob.AddFacet(a, b, c);
      Surface s = bob.Build();
      Assert.AreEqual(1, s.Faces.Length);
      Assert.AreEqual(3, s.Points.Count);
      GA.AssertAreApproxEqual(new Triangle3F(a, b, c),
                              new Triangle3F(s.Points[s.Faces[0].A],
                                             s.Points[s.Faces[0].B],
                                             s.Points[s.Faces[0].C]));
    }

    [TestMethod, TestCategory("Medium")]
    public void TestSurfaceBuilderAddFace3PointsBackwards() {
      SurfaceBuilder bob = new SurfaceBuilder();
      Vector3F front = new Vector3F(0, 0, 1);
      bob.Front = front;
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 1, 0);
      Point3F c = new Point3F(1, 0, 0);
      bob.AddFacet(a, b, c);
      Surface s = bob.Build();
      Assert.AreEqual(1, s.Faces.Length);
      Assert.AreEqual(3, s.Points.Count);
      GA.AssertAreApproxEqual(new Triangle3F(a, c, b),
                              new Triangle3F(s.Points[s.Faces[0].A],
                                             s.Points[s.Faces[0].B],
                                             s.Points[s.Faces[0].C]));
      Vector3F ab = new Vector3F(s.Points[s.Faces[0].A],
                                 s.Points[s.Faces[0].B]);
      Vector3F ac = new Vector3F(s.Points[s.Faces[0].A],
                                 s.Points[s.Faces[0].C]);
      Assert.IsTrue(Vector3F.DotProduct(front,
          Vector3F.CrossProduct(ab, ac)) >= 0);
    }

    [TestMethod, TestCategory("Small")]
    public void TestSurfaceBuilderAddFace4Points() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 0, 0);
      Point3F c = new Point3F(1, 1, 0);
      Point3F d = new Point3F(0, 1, 0);
      bob.AddFacets(a, b, c, d);
      Surface s = bob.Build();
      Assert.AreEqual(2, s.Faces.Length);
      Assert.AreEqual(4, s.Points.Count);
      Assert.AreEqual(a, s.Points[s.Faces[0].A]);
      Assert.AreEqual(b, s.Points[s.Faces[0].B]);
      Assert.AreEqual(c, s.Points[s.Faces[0].C]);
      Assert.AreEqual(a, s.Points[s.Faces[1].A]);
      Assert.AreEqual(c, s.Points[s.Faces[1].B]);
      Assert.AreEqual(d, s.Points[s.Faces[1].C]);
    }

    [TestMethod, TestCategory("Small")]
    public void TestSurfaceBuilderAddSimpleOverlap() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);

      Triangle3F top = new Triangle3F(new Point3F(1, 1, 1),
                                      new Point3F(1, 2, 1),
                                      new Point3F(2, 1, 1));
      bob.AddFacet(top);

      Triangle3F bottom = new Triangle3F(new Point3F(0, 0, 0),
                                         new Point3F(0, 4, 0),
                                         new Point3F(4, 0, 0));
      bob.AddFacet(bottom);

      Surface s = bob.Build();
      foreach (var face in s.Faces) {
        var a = s.Points[face.A];
        var b = s.Points[face.B];
        var c = s.Points[face.C];
        Assert.IsTrue(bottom.ContainsXY(a), "face " + face.ToString() + " a");
        Assert.IsTrue(bottom.ContainsXY(b), "face " + face.ToString() + " b");
        Assert.IsTrue(bottom.ContainsXY(c), "face " + face.ToString() + " c");
      // TODO: Restore when hidden surface removal works
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " a.Z", 0, a.Z);
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " b.Z", 0, b.Z);
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " c.Z", 0, c.Z);
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestSurfaceBuilderAddSimpleOverlap2() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);

      bob.AddFacets(new Point3F(1, 1, 1),
                    new Point3F(1, 2, 1),
                    new Point3F(2, 2, 1),
                    new Point3F(2, 1, 1));

      bob.AddFacets(new Point3F(0, 0, 0),
                    new Point3F(0, 4, 0),
                    new Point3F(4, 4, 0),
                    new Point3F(4, 0, 0));
      Rect2F bounds = new Rect2F(new Point2F(0, 0), new Point2F(4, 4));
      Surface s = bob.Build();
      foreach (var face in s.Faces) {
        var a = s.Points[face.A];
        var b = s.Points[face.B];
        var c = s.Points[face.C];
        Assert.IsTrue(bounds.PointInside(a, 1e-10),
                      "face " + face.ToString() + " a");
        Assert.IsTrue(bounds.PointInside(b, 1e-10),
                      "face " + face.ToString() + " b");
        Assert.IsTrue(bounds.PointInside(c, 1e-10),
                      "face " + face.ToString() + " c");
        // TODO: Restore when hidden surface removal works
        //GA.AssertAreApproxEqual("face " + face.ToString() + " a.Z", 0, a.Z);
        //GA.AssertAreApproxEqual("face " + face.ToString() + " b.Z", 0, b.Z);
        //GA.AssertAreApproxEqual("face " + face.ToString() + " c.Z", 0, c.Z);
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestSurfaceBuilderAddMultipleOverlap() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);

      bob.AddFacets(new Point3F(1, 1, 1),
                    new Point3F(1, 2, 1),
                    new Point3F(2, 2, 1),
                    new Point3F(2, 1, 1));

      bob.AddFacets(new Point3F(0.75, 1.5, 0.5),
                    new Point3F(0.75, 2.5, 0.5),
                    new Point3F(1.75, 2.5, 0.5),
                    new Point3F(1.75, 1.5, 0.5));

      bob.AddFacets(new Point3F(0, 0, 0),
                    new Point3F(0, 4, 0),
                    new Point3F(4, 4, 0),
                    new Point3F(4, 0, 0));
      Rect2F bounds = new Rect2F(new Point2F(0, 0), new Point2F(4, 4));
      Surface s = bob.Build();
      foreach (var face in s.Faces) {
        var a = s.Points[face.A];
        var b = s.Points[face.B];
        var c = s.Points[face.C];
        Assert.IsTrue(bounds.PointInside(a, 1e-10),
                      "face " + face.ToString() + " a");
        Assert.IsTrue(bounds.PointInside(b, 1e-10),
                      "face " + face.ToString() + " b");
        Assert.IsTrue(bounds.PointInside(c, 1e-10),
                      "face " + face.ToString() + " c");
      // TODO: Restore when hidden surface removal works.
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " a.Z", 0, a.Z);
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " b.Z", 0, b.Z);
      //  GA.AssertAreApproxEqual("face " + face.ToString() + " c.Z", 0, c.Z);
      }
    }
  }
}
