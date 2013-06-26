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

using CamBam.CAD;
using CamBam.Geom;

using VEngraveForCamBam;

namespace VEngrave_UnitTests {
  [TestClass]
  public class ExtensionTests {
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

    [TestMethod, TestCategory("Small")]
    public void testSurfaceBuilderAddFace3Points() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 0, 0);
      Point3F c = new Point3F(1, 1, 0);
      bob.AddFace(a, b, c);
      Surface s = bob.Build();
      Assert.AreEqual(1, s.Faces.Length);
      Assert.AreEqual(3, s.Points.Count);
      Assert.AreEqual(a, s.Points[s.Faces[0].A]);
      Assert.AreEqual(b, s.Points[s.Faces[0].B]);
      Assert.AreEqual(c, s.Points[s.Faces[0].C]);
    }

    [TestMethod, TestCategory("Small")]
    public void testSurfaceBuilderAddFace3PointsBackwards() {
      SurfaceBuilder bob = new SurfaceBuilder();
      Vector3F front = new Vector3F(0, 0, -1);
      bob.Front = front;
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 0, 0);
      Point3F c = new Point3F(1, 1, 0);
      bob.AddFace(a, b, c);
      Surface s = bob.Build();
      Assert.AreEqual(1, s.Faces.Length);
      Assert.AreEqual(3, s.Points.Count);
      Assert.AreEqual(a, s.Points[0]);
      Assert.AreEqual(b, s.Points[1]);
      Assert.AreEqual(c, s.Points[2]);
      Vector3F ab = new Vector3F(s.Points[s.Faces[0].A],
                                 s.Points[s.Faces[0].B]);
      Vector3F ac = new Vector3F(s.Points[s.Faces[0].A],
                                 s.Points[s.Faces[0].C]);
      Assert.IsTrue(Vector3F.DotProduct(front,
          Vector3F.CrossProduct(ab, ac)) >= 0);
    }

    [TestMethod, TestCategory("Small")]
    public void testSurfaceBuilderAddFace4Points() {
      SurfaceBuilder bob = new SurfaceBuilder();
      bob.Front = new Vector3F(0, 0, 1);
      Point3F a = new Point3F(0, 0, 0);
      Point3F b = new Point3F(1, 0, 0);
      Point3F c = new Point3F(1, 1, 0);
      Point3F d = new Point3F(0, 1, 0);
      bob.AddFace(a, b, c, d);
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
  }
}
