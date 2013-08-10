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

using GA = VEngrave_UnitTests.GeometryAssertions;

namespace VEngrave_UnitTests {
  [TestClass]
  public class GeometryTests {
    private const double DEGREES = Math.PI/180;
    private const double DBL_EPSILON = 2.2204460492503131e-016;

    private Point2F Transform(Point2F point, double dx, double dy,
                              double theta) {
      double x1 = point.X + dx;
      double y1 = point.Y + dy;
      return new Point2F(Math.Cos(theta)*x1 - Math.Sin(theta)*y1,
                         Math.Sin(theta)*x1 + Math.Cos(theta)*y1);
    }

    private Point3F Transform(Point3F point, double dx, double dy,
                              double theta) {
      return Transform(point, dx, dy, 0, theta);
    }

    private Point3F Transform(Point3F point,
                              double dx, double dy, double dz,
                              double theta) {
      double x1 = point.X + dx;
      double y1 = point.Y + dy;
      double z1 = point.Z + dz;
      return new Point3F(Math.Cos(theta)*x1 - Math.Sin(theta)*y1,
                         Math.Sin(theta)*x1 + Math.Cos(theta)*y1,
                         z1);
    }

    private PolylineItem Transform(PolylineItem item,
                                   double dx, double dy, double theta) {
      return new PolylineItem(Transform(item.Point, dx, dy, theta),
                              item.Bulge);
    }

    private Vector2F Transform(Vector2F vector, double theta) {
      return new Vector2F(Math.Cos(theta)*vector.X - Math.Sin(theta)*vector.Y,
                          Math.Sin(theta)*vector.X + Math.Cos(theta)*vector.Y);
    }

    private void JitterRadiusToLineSegment(string msg, double expectedRadius,
                                           Point2F position, Vector2F normal,
                                           Point2F start, Point2F end) {
      for (double dx = -0.5; dx <= 0.5; dx += 0.1) {
        for (double dy = -0.5; dy <= 0.5; dy += 0.1) {
          for (double dtheta = 0; dtheta <= 360*DEGREES; dtheta += 15*DEGREES) {
            var txfpos = Transform(position, dx, dy, dtheta);
            var txfnorm = Transform(normal, dtheta);
            var txfstart = Transform(start, dx, dy, dtheta);
            var txfend = Transform(end, dx, dy, dtheta);
            GA.AssertAreApproxEqual(msg + " dx: " + dx + ", "
                                    + "dy: " + dy + ", "
                                    + "dtheta: " + dtheta,
                                    expectedRadius,
                                    Geometry.RadiusToLineSegment(txfpos,
                                                                 txfnorm,
                                                                 txfstart,
                                                                 txfend));
          }
        }
      }
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentParallel() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(0, 1);
      Point2F end = new Point2F(1, 1);
      JitterRadiusToLineSegment("TestRadiusToLineSegmentHorizontalParallel",
                                0.5, position, unitNormal, start, end);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentPerpendictular() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(1, 1);
      JitterRadiusToLineSegment(
        "TestRadiusToLineSegmentHorizontalPerpendictular",
        0.5, position, unitNormal, start, end);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentAcute() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(0, 1);
      JitterRadiusToLineSegment("TestRadiusToLineSegmentHorizontalAcute",
                                0.5*Math.Tan(22.5*DEGREES),
                                position, unitNormal, start, end);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentAtCorner() {
      Point2F position = new Point2F(0, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(0, 0);
      Point2F end = new Point2F(1, 0);
      JitterRadiusToLineSegment("TestRadiusToLineSegmentAtCorner",
                                0, position, unitNormal, start, end);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentColinear() {
      Point2F position = new Point2F(0, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(2, 0);
      JitterRadiusToLineSegment("TestRadiusToLineSegmentColinear",
                                Double.MaxValue, position, unitNormal,
                                start, end);
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToLineSegmentColinear02010785() {
      Point2F position = new Point2F(0, 0);
      Vector2F normal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(2, 0);
      double dx = 0.2;
      double dy = -0.1;
      double dtheta = 45*DEGREES;

      var txfpos = Transform(position, dx, dy, dtheta);
      var txfnorm = Transform(normal, dtheta);
      var txfstart = Transform(start, dx, dy, dtheta);
      var txfend = Transform(end, dx, dy, dtheta);

      //var txfpos = new Point2F(0.212132034355964, 0.0707106781186547);
      //var txfnorm = new Vector2F(-0.707106781186547, 0.707106781186548);
      //var txfstart = new Point2F(0.919238815542512, 0.777817459305202);
      //var txfend = new Point2F(1.62634559672906, 1.48492424049175);

      Assert.AreEqual(-1.0, Vector2F.Determinant(txfnorm, new Vector2F(txfstart, txfend)), 4*DBL_EPSILON);
      Assert.AreEqual(0.0, Vector2F.DotProduct(txfnorm, new Vector2F(txfstart, txfend)), 4*DBL_EPSILON);

      GA.AssertAreApproxEqual("TestRadiusToLineSegmentColinear02010785",
                              Double.MaxValue,
                              Geometry.RadiusToLineSegment(
                                txfpos, txfnorm, txfstart, txfend));
    }
    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToLineSegmentObtuse() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(2, 1);
      JitterRadiusToLineSegment("TestRadiusToLineSegmentHorizontalObtuse",
                                0.5*Math.Tan(67.5*DEGREES),
                                position, unitNormal, start, end);
    }

    private void JitterRadiusToEndPoint(
      string msg, double expectedRadius,
      Point2F position, Vector2F normal, Point2F endPoint) {
      for (double dx = -0.5; dx <= 0.5; dx += 0.1) {
        for (double dy = -0.5; dy <= 0.5; dy += 0.1) {
          for (double dtheta = 0; dtheta <= 2*Math.PI; dtheta += Math.PI/12) {
            GA.AssertAreApproxEqual(msg + " dx: " + dx +", " + "dy: " + dy +", "
                                    + "dtheta: " + dtheta,
                                    expectedRadius,
                                    Geometry.RadiusToEndPoint(
                                      Transform(position, dx, dy, dtheta),
                                      Transform(normal, dtheta),
                                      Transform(endPoint, dx, dy, dtheta)));
          }
        }
      }
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToEndPointDirect() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F endPoint = new Point2F(0.5, 1);
      JitterRadiusToEndPoint("TestRadiusToEndPointDirect",
                             0.5, position, unitNormal, endPoint);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToEndPointRight() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F endPoint = new Point2F(1, 0.5);
      JitterRadiusToEndPoint("TestRadiusToEndPointRight",
                             0.5, position, unitNormal, endPoint);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToEndPointLeft() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F endPoint = new Point2F(0, 0.5);
      JitterRadiusToEndPoint("TestRadiusToEndPointLeft",
                             0.5, position, unitNormal, endPoint);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToEndPointObtuse() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F endPoint = new Point2F(0.75, 0.5-0.25*Math.Sqrt(3));
      JitterRadiusToEndPoint("TestRadiusToEndPointObtuse",
                             0.5, position, unitNormal, endPoint);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToEndPointAcute() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F endPoint = new Point2F(0.75, 0.5+0.25*Math.Sqrt(3));
      JitterRadiusToEndPoint("TestRadiusToEndPointObtuse",
                             0.5, position, unitNormal, endPoint);
    }

    private void JitterConvertBulgeToArc(
      string msg,
      double expectedRadius, Point2F expectedCenter,
      Point2F start, Point2F end, double bulge) {
      for (double dx = -0.5; dx <= 0.5; dx += 0.25) {
        for (double dy = -0.5; dy <= 0.5; dy += 0.25) {
          for (double dtheta = 0; dtheta <= 2*Math.PI;
               dtheta += Math.PI/12) {
            string iter = " dx: " + dx +", dy: " + dy +", dtheta: " + dtheta;
            Point2F center;
            double radius = Geometry.ConvertBulgeToArc(
              Transform(start, dx, dy, dtheta),
              Transform(end, dx, dy, dtheta),
              bulge, out center);
            Point2F translatedCenter = Transform(
              expectedCenter, dx, dy, dtheta);
            GA.AssertAreApproxEqual(msg + iter + " radius",
                                    expectedRadius, radius);
            GA.AssertAreApproxEqual(msg + iter + " center.X",
                                    translatedCenter.X, center.X);
            GA.AssertAreApproxEqual(msg + iter + " center.Y",
                                    translatedCenter.Y, center.Y);
          }
        }
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestConvertBulgeToArc180() {
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(-1, 0);
      double bulge = 1;
      double expectedRadius = 1;
      Point2F expectedCenter = new Point2F(0, 0);
      JitterConvertBulgeToArc("TestConvertBulgeToArc180", expectedRadius,
                              expectedCenter, start, end, bulge);
    }

    [TestMethod, TestCategory("Small")]
    public void TestConvertBulgeToArc90() {
      Point2F start = new Point2F(1, 0);
      Point2F end = new Point2F(0, 1);
      double bulge = Math.Tan(90*DEGREES/4);
      double expectedRadius = 1;
      Point2F expectedCenter = new Point2F(0, 0);
      JitterConvertBulgeToArc("TestConvertBulgeToArc90", expectedRadius,
                              expectedCenter, start, end, bulge);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestConvertBulgeToArcMany() {
      Point2F start = new Point2F(1, 0);
      double expectedRadius = 1;
      for (double sweep = 1*DEGREES; sweep < 359*DEGREES;
           sweep += 13*DEGREES) {
        Point2F end = new Point2F(Math.Cos(sweep), Math.Sin(sweep));
        double bulge = Math.Tan(sweep/4);
        Point2F expectedCenter = new Point2F(0, 0);
        JitterConvertBulgeToArc(
          "TestConvertBulgeToArcMany sweep: " + sweep,
          expectedRadius, expectedCenter, start, end, bulge);
      }
    }

    private void JitterRadiusToArc(
      string msg, double expectedRadius,
      Point2F position, Vector2F normal,
      Point2F start, Point2F end, double bulge) {
      for (double dx = -0.5; dx <= 0.5; dx += 0.25) {
        for (double dy = -0.5; dy <= 0.5; dy += 0.25) {
          for (double dtheta = 0; dtheta <= 360*DEGREES;
               dtheta += 15*DEGREES) {
            string itermsg = msg + " dx: " + dx +", "
              + "dy: " + dy + ", " + "dtheta: " + dtheta;
            GA.AssertAreApproxEqual(itermsg, expectedRadius,
                                    Geometry.RadiusToArc(
                                      Transform(position, dx, dy, dtheta),
                                      Transform(normal, dtheta),
                                      Transform(start, dx, dy, dtheta),
                                      Transform(end, dx, dy, dtheta),
                                      bulge));
            GA.AssertAreApproxEqual(itermsg + " inverted", expectedRadius,
                                    Geometry.RadiusToArc(
                                      Transform(position, dx, dy, dtheta),
                                      Transform(normal, dtheta),
                                      Transform(end, dx, dy, dtheta),
                                      Transform(start, dx, dy, dtheta),
                                      -bulge));
          }
        }
      }
    }

    [TestMethod, TestCategory("Small")]
    public void TestRadiusToArcDirectConcaveMinRadius() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 0.49999);
      Point2F end = new Point2F(0, 0.49999);
      double bulge = 1.0;
      JitterRadiusToArc("TestRadiusToArcDirectConcaveMinRadius",
                        0.499995, position, unitNormal, start, end, bulge);
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcDirectConcaveLargeInsideBow() {
      Point2F position = new Point2F(0, 0);
      Vector2F unitNormal = new Vector2F(-1, 0);
      Point2F start = new Point2F(0.25, 1);
      Point2F end = new Point2F(0.25, -1);
      double bulge = 1.0;
      GA.AssertAreApproxEqual("TestRadiusToArcDirectConcaveLargeInsideBow",
                              0.375, Geometry.RadiusToArc(
                                position, unitNormal, start, end, bulge));
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcDirectConcaveLargeRadius000() {
      Point2F position = new Point2F(0, 0);
      Vector2F unitNormal = new Vector2F(-1, 0);
      Point2F start = new Point2F(0, 1);
      Point2F end = new Point2F(0, -1);
      double bulge = 1.0;
      GA.AssertAreApproxEqual("TestRadiusToArcDirectConcaveLargeRadius",
                              0.5, Geometry.RadiusToArc(
                                position, unitNormal, start, end, bulge));
    }

    [TestMethod, TestCategory("Small")]
    public void TestRadiusToArcDirectConcaveLargeRadius() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1.5, 0);
      Point2F end = new Point2F(-0.5, 0);
      double bulge = 1.0;
      JitterRadiusToArc("TestRadiusToArcDirectConcaveLargeRadius",
                        0.5, position, unitNormal, start, end, bulge);
    }

    [TestMethod, TestCategory("Small")]
    public void TestRadiusToArcDirectConvexLargeRadius() {
      Point2F position = new Point2F(0.5, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1.5, 2);
      Point2F end = new Point2F(-0.5, 2);
      double bulge = -1.0;
      JitterRadiusToArc("TestRadiusToArcDirectConvexLargeRadius",
                        0.5, position, unitNormal, start, end, bulge);
    }

    [TestMethod, TestCategory("Huge")]
    public void TestRadiusToArcSuperConvex() {
      Vector2F unitNormal = new Vector2F(0, 1);
      for (double tangentRadius = 0.1; tangentRadius <= 10;
           tangentRadius *= 5) {
        TestRadiusToArcSuperConvexOverRadiusAndPositionAngle(unitNormal,
                                                             tangentRadius);
      }
    }

    private void TestRadiusToArcSuperConvexOverRadiusAndPositionAngle(
      Vector2F unitNormal, double tangentRadius) {
      Point2F position = new Point2F(0, -tangentRadius);

      for (double arcRadius = 0.1; arcRadius <= 10; arcRadius *= 7) {
        // tangent circle is centered on origin
        for (double positionAngle = -89*DEGREES; positionAngle <= 269*DEGREES;
             positionAngle += 27*DEGREES) {
          Point2F arcCenter = new Point2F(
            (arcRadius + tangentRadius)*Math.Cos(positionAngle),
            (arcRadius + tangentRadius)*Math.Sin(positionAngle));
          TestRadiusToArcSuperConvexOverArcLengths(position, unitNormal,
                                                   tangentRadius, positionAngle,
                                                   arcCenter, arcRadius);
        }
      }
    }

    private void TestRadiusToArcSuperConvexOverArcLengths(
      Point2F position, Vector2F unitNormal, double tangentRadius,
      double positionAngle, Point2F arcCenter, double arcRadius) {
      // start just before the radius vector and work clockwise to
      // the radius vector
      for (double arcStart = positionAngle + 179*DEGREES;
           arcStart > positionAngle - 179*DEGREES;
           arcStart -= 13*DEGREES) {
        Point2F start = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcStart),
                                    arcCenter.Y + arcRadius*Math.Sin(arcStart));
        // start just after the radius vector and work counter-clockwise
        // to start
        for (double arcEnd = positionAngle + 181*DEGREES;
             arcEnd < arcStart + 359*DEGREES;
             arcEnd  += 17*DEGREES) {
          Point2F end = new Point2F(
            arcCenter.X + arcRadius*Math.Cos(arcEnd),
            arcCenter.Y + arcRadius*Math.Sin(arcEnd));
          double bulge=Math.Tan((arcEnd - arcStart)/4);
          JitterRadiusToArc("TestRadiusToArcSuperConvex, "
                            + "tangentRadius: " + tangentRadius           + ", "
                            + "positionAngle: " + (positionAngle/DEGREES) + ", "
                            + "arcRadius: "     + arcRadius               + ", "
                            + "arcStart: "      + (arcStart/DEGREES)      + ", "
                            + "arcEnd: "        + (arcEnd/DEGREES),
                            tangentRadius, position, unitNormal,
                            start, end, bulge);
        }
      }
    }
    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcSuperConvexPAm85() {
      double tangentRadius = 0.1;
      // This becomes indeterminate as the position angle approaches 270 deg and
      // the arc aligns with the position.
      double positionAngle = -85*DEGREES;
      double arcRadius = 0.1;
      double arcStart = 80*DEGREES;
      double arcEnd = 420*DEGREES;
      double dx = -0.5;
      double dy = -0.5;
      Vector2F unitNormal = new Vector2F(0, 1);

      Point2F position = new Point2F(0 + dx, -tangentRadius + dy);
      Point2F arcCenter = new Point2F(
        (arcRadius + tangentRadius)*Math.Cos(positionAngle) + dx,
        (arcRadius + tangentRadius)*Math.Sin(positionAngle) + dy);
      Point2F start = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcStart),
                                  arcCenter.Y + arcRadius*Math.Sin(arcStart));
      Point2F end = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcEnd),
                                arcCenter.Y + arcRadius*Math.Sin(arcEnd));
      double bulge=Math.Tan((arcEnd - arcStart)/4);
      GA.AssertAreApproxEqual("TestRadiusToArcSuperConvexPAm85",
                              tangentRadius, Geometry.RadiusToArc(
                                position, unitNormal, start, end, bulge));
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcSuperConvexPA270() {
      double tangentRadius = 0.1;
      // This becomes indeterminate as the position angle approaches 270 deg and
      // the arc aligns with the position.
      double positionAngle = 270.1*DEGREES;
      double arcRadius = 0.1;
      double arcStart = 40*DEGREES;
      double arcEnd = 140*DEGREES;
      double dx = -0.5;
      double dy = -0.5;
      Vector2F unitNormal = new Vector2F(0, 1);

      Point2F position = new Point2F(0 + dx, -tangentRadius + dy);
      Point2F arcCenter = new Point2F(
        (arcRadius + tangentRadius)*Math.Cos(positionAngle) + dx,
        (arcRadius + tangentRadius)*Math.Sin(positionAngle) + dy);
      Point2F start = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcStart),
                                  arcCenter.Y + arcRadius*Math.Sin(arcStart));
      Point2F end = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcEnd),
                                arcCenter.Y + arcRadius*Math.Sin(arcEnd));
      double bulge = Math.Tan((arcEnd - arcStart)/4);
      GA.AssertAreApproxEqual("TestRadiusToArcSuperConvexPA270",
                              tangentRadius,
                              Geometry.RadiusToArc(position, unitNormal,
                                                   start, end, bulge));
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcSuperConvexPA100AS227() {
      const double tangentRadius = 0.1;
      const double positionAngle = 100*DEGREES;
      const double arcRadius = 0.1;
      const double arcStart = 227*DEGREES;
      // 587*DEGREES blows up since it's a full circle
      const double arcEnd = 586.999*DEGREES;
      const double dx = -0.5;
      const double dy = -0.5;
      Vector2F unitNormal = new Vector2F(0, 1);

      Point2F position = new Point2F(0 + dx, -tangentRadius + dy);
      Point2F arcCenter = new Point2F(
        (arcRadius + tangentRadius)*Math.Cos(positionAngle) + dx,
        (arcRadius + tangentRadius)*Math.Sin(positionAngle) + dy);
      Point2F start = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcStart),
                                  arcCenter.Y + arcRadius*Math.Sin(arcStart));
      Point2F end = new Point2F(arcCenter.X + arcRadius*Math.Cos(arcEnd),
                                arcCenter.Y + arcRadius*Math.Sin(arcEnd));
      double bulge = Math.Tan((arcEnd - arcStart)/4);
      GA.AssertAreApproxEqual("TestRadiusToArcSuperConvexPA100AS227",
                              tangentRadius, Geometry.RadiusToArc(
                                position, unitNormal, start, end, bulge));
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcInnerCorner() {
      Point2F  position = new Point2F(0.54557606689115667,
                                      0.013894560011917596);
      Vector2F normal   = new Vector2F(-0.22903933372553, 0.97341716833358);
      Point2F  start    = new Point2F(0.545576066891157, 0.0138945600119176);
      Point2F  end      = new Point2F(0.535981127214644, 0.0304597381094125);
      double   bulge    = 0.0436580137706728;
      Assert.AreEqual(0.0, Geometry.RadiusToArc(position, normal,
                                                start, end, bulge),
                      1e-10, "TestRadiusToArcInnerCorner");
      JitterRadiusToArc("TestRadiusToArcInnerCorner",
                        0.0, position, normal, start, end, bulge);
    }

    [TestMethod, TestCategory("Medium")]
    public void TestRadiusToArcEndpointDirect() {
      Point2F position = new Point2F(0, 0);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(1, 1);
      Point2F end = new Point2F(0, 1);
      for (double bulge = 0.1; bulge < 2; bulge *= 2) {
        JitterRadiusToArc("TestRadiusToArcEndpointDirectConcave bulge: "
                          + bulge,
                          0.5, position, unitNormal, start, end, bulge);
      }
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestRadiusToArcEndpointDirectBulge08() {
      Point2F position = new Point2F(-0.5, -0.5);
      Vector2F unitNormal = new Vector2F(0, 1);
      Point2F start = new Point2F(0.5, 0.5);
      Point2F end = new Point2F(-0.5, 0.5);
      double bulge = 0.8;
      GA.AssertAreApproxEqual("TestRadiusToArcEndpointDirectBulge08", 0.5,
                              Geometry.RadiusToArc(
                                position, unitNormal, start, end, bulge));
    }

    private void JitterGetCornerType(string msg,
                                     Geometry.CornerType expectedType,
                                     Vector2F expectedNormal,
                                     PolylineItem prev, PolylineItem curr,
                                     PolylineItem next, bool leftIsInside) {
      for (double dx = -0.5; dx <= 0.5; dx += 0.1) {
        for (double dy = -0.5; dy <= 0.5; dy += 0.1) {
          for (double theta = 0; theta < 360*DEGREES; theta += 5*DEGREES) {
            string itermsg = msg + " dx: " + dx +", "
              + "dy: " + dy + ", " + "theta: " + theta;
            Vector2F nextNormal;
            Assert.AreEqual(expectedType,
                            Geometry.GetCornerType(
                              Transform(prev, dx, dy, theta),
                              Transform(curr, dx, dy, theta),
                              Transform(next, dx, dy, theta),
                              threshold: 135*DEGREES,
                              leftIsInside: leftIsInside,
                              nextNormal: out nextNormal),
                            itermsg);
            if (!expectedNormal.IsUndefined) {
              Vector2F xfxnorm = Transform(expectedNormal, theta);
              GA.AssertAreApproxEqual(itermsg, xfxnorm.X, nextNormal.X);
              GA.AssertAreApproxEqual(itermsg, xfxnorm.Y, nextNormal.Y);
            }
          }
        }
      }
    }

    [TestMethod, TestCategory("Large")]
    public void TestGetCornerTypeLineSegLineSegSharp() {
      JitterGetCornerType("right angle",
                          Geometry.CornerType.SharpInside,
                          new Vector2F(-1, 0),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("slightly open angle",
                          Geometry.CornerType.SharpInside,
                          new Vector2F(-1.1, -1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(0, 2.1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("very sharp angle",
                          Geometry.CornerType.SharpInside,
                          new Vector2F(-1, -0.8).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 0, 0, 0),
                          new PolylineItem(.2, 1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("closed angle",
                          Geometry.CornerType.SharpInside,
                          new Vector2F(1, -1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(0, 0, 0, 0),
                          leftIsInside: true);
    }

    [TestMethod, TestCategory("Large")]
    public void TestGetCornerTypeLineSegLineSegSmooth() {
      JitterGetCornerType("slope .25",
                          Geometry.CornerType.SmoothInside,
                          new Vector2F(-0.25, 1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 0, 0, 0),
                          new PolylineItem(2, 0.25, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("slanted",
                          Geometry.CornerType.SmoothInside,
                          new Vector2F(-1.1, 1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(2, 2.1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("vertical left",
                          Geometry.CornerType.SmoothInside,
                          new Vector2F(-1, -0.2).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(-0.2, 2, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("vertical right",
                          Geometry.CornerType.SmoothInside,
                          new Vector2F(1, -0.2).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(0.2, 2, 0, 0),
                          leftIsInside: false);
    }

    [TestMethod, TestCategory("Large")]
    public void TestGetCornerTypeLineSegLineSegOutside() {
      JitterGetCornerType("right angle",
                          Geometry.CornerType.Outside,
                          new Vector2F(0, 1),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("obtuse",
                          Geometry.CornerType.Outside,
                          new Vector2F(1, 1.1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(2.1, 0, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("short right",
                          Geometry.CornerType.Outside,
                          new Vector2F(0, 1),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(.2, 1, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("acute",
                          Geometry.CornerType.Outside,
                          new Vector2F(1, 0),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(1, 0, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("high obtuse",
                          Geometry.CornerType.Outside,
                          new Vector2F(2, 1).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 2, 0, 0),
                          new PolylineItem(2, 0, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("extreme acute",
                          Geometry.CornerType.Outside,
                          new Vector2F(0.75, -0.5).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(1, 1, 0, 0),
                          new PolylineItem(0.5, 0.25, 0, 0),
                          leftIsInside: true);
      JitterGetCornerType("obtuse right is inside",
                          Geometry.CornerType.Outside,
                          new Vector2F(1, 2.5).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(-2.5, 2, 0, 0),
                          leftIsInside: false);
      JitterGetCornerType("acute right is inside",
                          Geometry.CornerType.Outside,
                          new Vector2F(-1, 0.2).Unit(),
                          new PolylineItem(0, 0, 0, 0),
                          new PolylineItem(0, 1, 0, 0),
                          new PolylineItem(-0.2, 0, 0, 0),
                          leftIsInside: false);
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestGetCornerTypeArcLineSegOutside23() {
      Vector2F nextNormal;
      Assert.AreEqual(Geometry.CornerType.Outside,
                      Geometry.GetCornerType(
                        new PolylineItem(1.1259, 1.0467, 0, 0.0693),
                        new PolylineItem(1.1187, 1.0679, 0, 0),
                        new PolylineItem(1.1068, 1.0837, 0, 0.0868),
                        threshold: 135*DEGREES,
                        leftIsInside: false,
                        nextNormal: out nextNormal));
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestGetCornerTypeArcArc9() {
      Vector2F nextNormal;
      Assert.AreEqual(Geometry.CornerType.SmoothInside,
                      Geometry.GetCornerType(
                        new PolylineItem(1.0887, 1.0000, 0, 0.0995),
                        new PolylineItem(1.0982, 0.9979, 0, 0.1432),
                        new PolylineItem(1.1070, 0.9969, 0, 0.1324),
                        threshold: 135*DEGREES,
                        leftIsInside: false,
                        nextNormal: out nextNormal));
    }

    [TestMethod, TestCategory("Small")]
    public void TestPointOnRay() {
      Assert.IsTrue(Geometry.PointOnRay(new Point3F(2, 4, 6),
                                        new Point3F(0, 0, 0),
                                        new Point3F(1, 2, 3), 1e-6));
      Assert.IsTrue(Geometry.PointOnRay(new Point3F(3, 5, 7),
                                        new Point3F(1, 1, 1),
                                        new Point3F(2, 3, 4), 1e-6));
      Assert.IsTrue(Geometry.PointOnRay(new Point3F(2, 4, 6),
                                        new Point3F(0, 0, 0),
                                        new Point3F(1, 2, 3 + 1e-8), 1e-6));
      Assert.IsFalse(Geometry.PointOnRay(new Point3F(2, 4, 6),
                                         new Point3F(0, 0, 0),
                                         new Point3F(1, 2, 3 + 1.1e-6), 1e-6));
      Assert.IsFalse(Geometry.PointOnRay(new Point3F(-2, -4, -6),
                                         new Point3F(0, 0, 0),
                                         new Point3F(1, 2, 3), 1e-6));
    }
    [TestMethod, TestCategory("Small")]
    public void TestModulus() {
      Assert.AreEqual(-1, -4 % 3);
      Assert.AreEqual(-1, -1 % 3);
      Assert.AreEqual(2, 2 % 3);
      Assert.AreEqual(2, 5 % 3);
    }
  }
}
