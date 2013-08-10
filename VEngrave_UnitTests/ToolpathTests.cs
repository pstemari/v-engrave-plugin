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
using Microsoft.VisualStudio.TestTools.UnitTesting;

using CamBam.CAD;
using CamBam.CAM;
using CamBam.Geom;

using VEngraveForCamBam;
using VEngraveForCamBam.CamBamExtensions;

using GA = VEngrave_UnitTests.GeometryAssertions;

namespace VEngrave_UnitTests {
  [TestClass]
  public class ToolpathTests {
    private const double DEGREES = Math.PI/180;

    private MOPVEngrave CreateMOP() {
      var mop = new MOPVEngrave();
      mop.SetLogger(new NullLogger());
      return mop;
    }

    private Point3F GetNearestPoint(Polyline polyline, Point3F point) {
      Point3F nearest = new Point3F();
      double closest_approach = Double.MaxValue;
      foreach (PolylineItem item in polyline.Points) {
        double distance = Point3F.Distance(item.Point, point);
        if (distance < closest_approach) {
          closest_approach = distance;
          nearest = item.Point;
        }
      }
      return nearest;
    }

    private static void TestCornerType(Polyline outline, int corner,
                                       Geometry.CornerType expected,
                                       string msg) {
      Vector2F nextNormal;
      var leftIsInside = outline.Direction == RotationDirection.CCW;
      Assert.AreEqual(expected,
                      Geometry.GetCornerType(outline, corner,
                                             135*DEGREES, leftIsInside,
                                             out nextNormal),
                      msg);
    }

    private void TestCorner(Polyline outline, int corner,
                            double expected, string msg) {
      bool leftIsInside = outline.Direction == RotationDirection.CCW;
      var outlines = new List<Polyline>(new Polyline[] { outline });
      TestCorner(outline, corner, outlines, leftIsInside, expected, msg);
    }

    private void TestCorner(Polyline outline, int corner,
                            List<Polyline> outlines, bool leftIsInside,
                            double expected, string msg) {
      var mop = CreateMOP();
      var toolpath = new Polyline();
      Vector2F normal, normal2;
      int previ = outline.PrevSegment(corner);
      int nexti = outline.NextSegment(corner);
      var cornerPoint = outline.Points[corner].Point.To2D();
      Geometry.CornerType startCornerType
        =  Geometry.GetCornerType(outline, previ, 135*DEGREES,
                                  leftIsInside, out normal);
      Geometry.CornerType endCornerType
        =  Geometry.GetCornerType(outline, corner, 135*DEGREES,
                                  leftIsInside, out normal2);
      GA.AssertAreApproxEqual(msg + " incoming", expected,
                              mop.AnalyzePoint(outline, previ, cornerPoint,
                                               normal2, Double.MaxValue,
                                               outlines, startCornerType,
                                               endCornerType, toolpath,
                                               leftIsInside));
      Assert.AreEqual(1, toolpath.Points.Count,
                      msg + " incoming point not added");
      startCornerType = endCornerType;
      normal = Geometry.GetDirection(outline.Points[corner],
                                     outline.Points[nexti],
                                     cornerPoint).Normal().Unit();
      if (leftIsInside) {
        normal.Invert();
      }
      endCornerType = Geometry.GetCornerType(outline, corner, 135*DEGREES,
                                             leftIsInside, out normal2);
      toolpath = new Polyline();
      GA.AssertAreApproxEqual(msg + " outgoing", expected,
                              mop.AnalyzePoint(outline, corner, cornerPoint,
                                               normal, Double.MaxValue,
                                               outlines, startCornerType,
                                               endCornerType, toolpath,
                                               leftIsInside));
      Assert.AreEqual(1, toolpath.Points.Count,
                      msg + " outgoing point not added");
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestCornerOvershoot() {
      TestCorner(MBottom1(), 7, 0.0, "CornerOvershoot");
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestCommaBottom() {
      Polyline comma = Comma();
      TestCorner(comma, COMMA_RIGHT_CORNER, 0.0, "Right");
      TestCorner(comma, COMMA_LEFT_CORNER, 0.0, "Left");
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestLTopCorner() {
      Polyline ltop = LTop();
      TestCorner(ltop, LTOP_LEFT_SERIF_BOTTOM, 0.0,
                 "bottom corner of left serif");
      TestCorner(ltop, LTOP_LEFT_SERIF_TOP, 0.0, "top corner of left serif");
      TestCorner(ltop, LTOP_TOP_RIGHT, 0.0, "top right");
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestMBottomCorners() {
      Polyline mbottom = MBottom();
      TestCorner(mbottom, MBOTTOM_LEFT_SERIF_TOP, 0.0, "Left serif top");
      TestCorner(mbottom, MBOTTOM_LEFT_SERIF_BOTTOM, 0.0,
                 "Left serif bottom");
      TestCorner(mbottom, MBOTTOM_RIGHT_SERIF_TOP, 0.0, "Right serif bottom");
      TestCorner(mbottom, MBOTTOM_RIGHT_SERIF_BOTTOM, 0.0, "Right serif top");
    }

    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestMBottomCornerTypes() {
      Polyline mbottom = MBottom();
      TestCornerType(mbottom, MBOTTOM_LEFT_SERIF_TOP,
                     Geometry.CornerType.SharpInside, "Left serif top");
      TestCornerType(mbottom, MBOTTOM_LEFT_SERIF_BOTTOM,
                     Geometry.CornerType.SharpInside, "Left serif bottom");
      TestCornerType(mbottom, MBOTTOM_RIGHT_SERIF_TOP,
                     Geometry.CornerType.SharpInside, "Right serif bottom");
      TestCornerType(mbottom, MBOTTOM_RIGHT_SERIF_BOTTOM,
                     Geometry.CornerType.SharpInside, "Right serif top");
    }
    [TestMethod, TestCategory("Small"), TestCategory("Repro")]
    public void TestLTopOutlineCorners() {
      MOPVEngrave mop = CreateMOP();
      ToolpathSequence tseq = new ToolpathSequence(mop);
      var outline = LTop();
      var outlines = new List<Polyline>(new Polyline[] {outline});
      mop.FollowOutline(tseq, outline, outlines,
                        outlineID: new EntityIdentifier(1), parentID: -1,
                        offsetIndex: 0, depthIndex: 0, traceInside: true);
      Assert.AreEqual(1, tseq.Toolpaths.Count);
      Assert.IsNotNull(tseq.Toolpaths[0]);
      Assert.IsNotNull(tseq.Toolpaths[0].Toolpath);
      Point3F npc1 = GetNearestPoint(
        tseq.Toolpaths[0].Toolpath,
        outline.Points[LTOP_LEFT_SERIF_BOTTOM].Point);
      Assert.AreEqual(0.0,
                      Point3F.Distance(
                        npc1, outline.Points[LTOP_LEFT_SERIF_BOTTOM].Point),
                      1e-5, "L top left serif bottom");
      Point3F npc2 = GetNearestPoint(tseq.Toolpaths[0].Toolpath,
                                     outline.Points[LTOP_LEFT_SERIF_TOP].Point);
      Assert.AreEqual(0.0, Point3F.Distance(
                        npc2, outline.Points[LTOP_LEFT_SERIF_TOP].Point), 1e-5,
                      "L top left serif top");
      Point3F npc3 = GetNearestPoint(tseq.Toolpaths[0].Toolpath,
                                     outline.Points[LTOP_TOP_RIGHT].Point);
      Assert.AreEqual(0.0, Point3F.Distance(
                        npc3, outline.Points[LTOP_TOP_RIGHT].Point), 1e-5,
                      "L top right corner");
    }
    [TestMethod, TestCategory("Medium"), TestCategory("Repro")]
    public void TestCommaOutlineCorners() {
      MOPVEngrave mop = CreateMOP();
      ToolpathSequence tseq = new ToolpathSequence(mop);
      var outline = Comma();
      var outlines = new List<Polyline>(new Polyline[] { outline });
      mop.FollowOutline(tseq, outline, outlines,
                        outlineID: new EntityIdentifier(1), parentID: -1,
                        offsetIndex: 0, depthIndex: 0, traceInside: true);
      Assert.AreEqual(1, tseq.Toolpaths.Count);
      Assert.IsNotNull(tseq.Toolpaths[0]);
      Assert.IsNotNull(tseq.Toolpaths[0].Toolpath);
      Point3F npc1 = GetNearestPoint(tseq.Toolpaths[0].Toolpath,
                                     outline.Points[COMMA_LEFT_CORNER].Point);
      Assert.AreEqual(0.0, Point3F.Distance(
                        npc1, outline.Points[COMMA_LEFT_CORNER].Point), 1e-5,
                      "Comma left corner");
      Point3F npc2 = GetNearestPoint(tseq.Toolpaths[0].Toolpath,
                                     outline.Points[COMMA_RIGHT_CORNER].Point);
      Assert.AreEqual(0.0, Point3F.Distance(
                        npc2, outline.Points[COMMA_RIGHT_CORNER].Point), 1e-5,
                      "Comma right corner");
    }
    [TestMethod, TestCategory("Medium"), TestCategory("Repro")]
    public void TestTestVEngrave3DotCBBase1() {
      MOPVEngrave mop = CreateMOP();
      ToolpathSequence tseq = new ToolpathSequence(mop);
      var outline = TestVEngrave3DotCBOutlineBase1();
      var outlines = new List<Polyline>(new Polyline[] { outline });
      var errorBox = TestVEngrave3DotCBErrorBoxBase1();
      var boundary = TestVEngrave3DotCBToolpathOuterBoundaryBase1();
      mop.FollowOutline(tseq, outline, outlines,
                        outlineID: new EntityIdentifier(1), parentID: -1,
                        offsetIndex: 0, depthIndex: 0, traceInside: true);
      Assert.AreEqual(1, tseq.Toolpaths.Count);
      Assert.IsNotNull(tseq.Toolpaths[0]);
      Assert.IsNotNull(tseq.Toolpaths[0].Toolpath);
      foreach (PolylineItem point in tseq.Toolpaths[0].Toolpath.Points) {
        Assert.IsFalse(errorBox.PointInPolyline2(point.Point.To2D(), 0),
                       "ErrorBox {0}", point.Point.To2D());
        Assert.IsTrue(boundary.PointInPolyline2(point.Point.To2D(), 0),
                      "Boundary {0}", point.Point.To2D());
      }
    }

    [TestMethod, TestCategory("Medium"), TestCategory("Repro")]
    public void TestTestVEngrave3DotCBBang() {
      MOPVEngrave mop = CreateMOP();
      ToolpathSequence tseq = new ToolpathSequence(mop);
      var outline = TestVEngrave3DotCBOutlineBang();
      var outlines = new List<Polyline>(new Polyline[] { outline });
      var errorBox1 = TestVEngrave3DotCBErrorBox1Bang();
      var errorBox2 = TestVEngrave3DotCBErrorBox2Bang();
      var boundary = TestVEngrave3DotCBToolpathOuterBoundaryBang();
      mop.FollowOutline(tseq, outline, outlines,
                        outlineID: new EntityIdentifier(1), parentID: -1,
                        offsetIndex: 0, depthIndex: 0, traceInside: true);
      Assert.AreEqual(1, tseq.Toolpaths.Count);
      Assert.IsNotNull(tseq.Toolpaths[0]);
      Assert.IsNotNull(tseq.Toolpaths[0].Toolpath);
      foreach (PolylineItem point in tseq.Toolpaths[0].Toolpath.Points) {
        Assert.IsFalse(errorBox1.PointInPolyline2(point.Point.To2D(), 0),
                       "ErrorBox1 {0}", point.Point.To2D());
        Assert.IsFalse(errorBox2.PointInPolyline2(point.Point.To2D(), 0),
                       "ErrorBox2 {0}", point.Point.To2D());
        Assert.IsTrue(boundary.PointInPolyline2(point.Point.To2D(), 0),
                      "Boundary {0}", point.Point.To2D());
      }
    }
    [TestMethod, TestCategory("Medium"), TestCategory("Repro")]
    public void TestTestVEngrave4DotCB() {
      MOPVEngrave mop = CreateMOP();
      ToolpathSequence tseq = new ToolpathSequence(mop);
      var outline = TestVEngrave4DotCBOutline();
      var outlines = new List<Polyline>(new Polyline[] { outline });
      var errorBox = TestVEngrave4DotCBErrorBox();
      var boundary = TestVEngrave4DotCBToolpathOuterBoundary();
      mop.FollowOutline(tseq, outline, outlines,
                        outlineID: new EntityIdentifier(1), parentID: -1,
                        offsetIndex: 0, depthIndex: 0, traceInside: true);
      Assert.AreEqual(1, tseq.Toolpaths.Count);
      Assert.IsNotNull(tseq.Toolpaths[0]);
      Assert.IsNotNull(tseq.Toolpaths[0].Toolpath);
      foreach (PolylineItem point in tseq.Toolpaths[0].Toolpath.Points) {
        Assert.IsFalse(errorBox.PointInPolyline2(point.Point.To2D(), 0),
                       "ErrorBox {0}", point.Point.To2D());
        Assert.IsTrue(boundary.PointInPolyline2(point.Point.To2D(), 0),
                      "Boundary {0}", point.Point.To2D());
      }
    }
    // These produce samples to repro bugs
    private const int COMMA_LEFT_CORNER = 0;
    private const int COMMA_RIGHT_CORNER = 13;
    private Polyline Comma() {
      Polyline outline = new Polyline();
      outline.Add(0.142719886391866, 0.0085397992979962, 0,
                  0.09459807983554834);                      // 0
      outline.Add(0.152363441079366, 0.0191599164854962, 0,
                  0.041995119592303419);                     // 1
      outline.Add(0.155240883909898, 0.0244290360248285, 0,
                  0.099232225192698142);                     // 2
      outline.Add(0.156635902016866, 0.0302683149229962, 0,
                  0.37366675721438314);                      // 3
      outline.Add(0.152241370766866, 0.0347849164854962, 0,
                  -0.16972186464262759);                     // 4
      outline.Add(0.142231605141866, 0.0408884321104962, 0,
                  -0.17448042746141329);                     // 5
      outline.Add(0.138447425454366, 0.0518747602354962, 0,
                  -0.19975586176092416);                     // 6
      outline.Add(0.143452308266866, 0.0649362836729962, 0,
                  -0.21393765295700315);                     // 7
      outline.Add(0.155903480141866, 0.0703073774229962, 0,
                  -0.23466853443210917);                     // 8
      outline.Add(0.169941566079366, 0.0638376508604962, 0,
                  -0.10590224530095881);                     // 9
      outline.Add(0.17422078155843, 0.0559792510292142, 0,
                  -0.0738403738653717);                      // 10
      outline.Add(0.175434730141866, 0.0471140180479962, 0,
                  -0.13618714103877846);                     // 11
      outline.Add(0.168598792641866, 0.0229440961729962, 0,
                  -0.10894032352057116);                     // 12
      outline.Add(0.149433753579366, 0.0025583539854962, 0); // 13
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private const int LTOP_LEFT_SERIF_BOTTOM = 4;
    private const int LTOP_LEFT_SERIF_TOP    = 5;
    private const int LTOP_TOP_RIGHT         = 7;
    private Polyline LTop() {
      Polyline outline = new Polyline();
      outline.Add(0.0889506275924134, 0.0226224760678276, 0, // 0
                  0.063906280299910426);
      outline.Add(0.0880016525410208, 0.0292767253246591, 0, // 1
                  0.20670255625206302);
      outline.Add(0.0812099978846946, 0.0380036411267686, 0, // 2
                  0.12337635703214202);
      outline.Add(0.0703959400924134, 0.0403149250868947, 0); // 3
      outline.Add(0.0679545338424134, 0.0403149250868947, 0); // 4 top left
      outline.Add(0.0679545338424134, 0.0488598469618947, 0); // 5  serif
      outline.Add(0.102256291654913, 0.0538647297743947, 0); // 6
      outline.Add(0.113975041654913, 0.0538647297743947, 0); // 7 top right
      outline.Add(0.113975041654913, 0.0253045546475792, 0); // 8
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private const int MBOTTOM_LEFT_SERIF_TOP     = 5;
    private const int MBOTTOM_LEFT_SERIF_BOTTOM  = 6;
    private const int MBOTTOM_RIGHT_SERIF_BOTTOM = 7;
    private const int MBOTTOM_RIGHT_SERIF_TOP    = 8;
    private Polyline MBottom() {
      Polyline outline = new Polyline();
      outline.Add(0.237768816671673, 0.0569632941802821, 0,
                  0.042972217459807852);                      // 0
      outline.Add(0.238216046650426, 0.0511149181759761, 0,
                  0.11308190703381774);                       // 1
      outline.Add(0.240307879171673, 0.0456351691802821, 0,
                  0.1258558953223457);                        // 2
      outline.Add(0.244650767539191, 0.0425594415352701, 0,
                  0.055858949898684646);                      // 3
      outline.Add(0.249780535421673, 0.0411429816802821, 0);  // 4
      outline.Add(0.256518816671673, 0.0399711066802821, 0);  // 5 left
      outline.Add(0.256518816671673, 0.0331351691802821, 0);  // 6 serif
      outline.Add(0.198901629171673, 0.0331351691802821, 0);  // 7 right
      outline.Add(0.198901629171673, 0.0399711066802821, 0);  // 8 serif
      outline.Add(0.205737566671673, 0.0411429816802821, 0,
                  0.056212050605087646);                      // 9
      outline.Add(0.21082704432763, 0.0425501686366531, 0,
                  0.13001917635816451);                       // 10
      outline.Add(0.215112566671673, 0.0456351691802821, 0,
                  0.11619700870894641);                       // 11
      outline.Add(0.217282305320471, 0.0511010654292121, 0,
                  0.044781524088153835);                      // 12
      outline.Add(0.217749285421673, 0.0569632941802821, 0);  // 13
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private Polyline MBottom1() {
      Polyline outline = new Polyline();
      outline.Add(0.527955470698132, 0.225230515523086, 0,
                  0.042972217459807852);
      outline.Add(0.528402700676885, 0.21938213951878, 0,
                  0.11308190703381774);
      outline.Add(0.530494533198132, 0.213902390523086, 0,
                  0.1258558953223457);
      outline.Add(0.53483742156565, 0.210826662878074, 0,
                  0.055858949898684646);
      outline.Add(0.539967189448132, 0.209410203023086, 0, 0);
      outline.Add(0.546705470698132, 0.208238328023086, 0, 0);
      outline.Add(0.546705470698132, 0.201402390523086, 0, 0);
      outline.Add(0.489088283198132, 0.201402390523086, 0, 0);
      outline.Add(0.489088283198132, 0.208238328023086, 0, 0);
      outline.Add(0.495924220698132, 0.209410203023086, 0,
                  0.056212050605087646);
      outline.Add(0.501013698354089, 0.210817389979457, 0,
                  0.13001917635816451);
      outline.Add(0.505299220698132, 0.213902390523086, 0,
                  0.11619700870894641);
      outline.Add(0.50746895934693, 0.219368286772016, 0,
                  0.044781524088153835);
      outline.Add(0.507935939448132, 0.225230515523086, 0, 0);
      outline.Closed = true;
      outline.ID = 1;
      return outline;
    }
    private Polyline TestVEngrave3DotCBOutlineBase1() {
      Polyline outline = new Polyline();
      outline.Add(0.202290038477607,0.0532429979306508,0);
      outline.Add(0.202290038477607,0.0597054864805459,0);
      outline.Add(0.227314452540107,0.0597054864805459,0);
      outline.Add(0.227314452540107,0.0532429979306508,0);
      outline.Add(0.227314452540107,0.0525906484414016,0,0.0415356585900027);
      outline.Add(0.227834364907002,0.0469951686435513,0,0.10739275245094745);
      outline.Add(0.230122069727607,0.0411580369931508,0,0.123448881319954);
      outline.Add(0.234449988576742,0.0376262107017789,0,0.065856551408462249);
      outline.Add(0.239765624415107,0.0359090135556508,0);
      outline.Add(0.248310546290107,0.0344441698056508,0);
      outline.Add(0.248310546290107,0.0258992479306508,0);
      outline.Add(0.181293944727607,0.0258992479306508,0);
      outline.Add(0.181293944727607,0.0344441698056508,0);
      outline.Add(0.189838866602607,0.0359090135556508,0,0.065835414985014884);
      outline.Add(0.195154502440971,0.0376262107017789,0,0.12347510202080689);
      outline.Add(0.199482421290107,0.0411580369931508,0,0.10737259161622637);
      outline.Add(0.201770126110711,0.0469951686435513,0,0.04153565859000341);
      outline.Add(0.202290038477607,0.052582873565007,0);
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private Polyline TestVEngrave3DotCBErrorBoxBase1() {
      Polyline errorBox = new Polyline();
      errorBox.Add(0.179122810949594,0.0279291739626575,0);
      errorBox.Add(0.183065589871961,0.0279291739626575,0);
      errorBox.Add(0.183065589871961,0.0311260217375495,0);
      errorBox.Add(0.179122810949594,0.0311260217375495,0);
      errorBox.ID = 2;
      errorBox.Closed = true;
      return errorBox;
    }
    private Polyline TestVEngrave3DotCBToolpathOuterBoundaryBase1() {
      Polyline boundary = new Polyline();
      boundary.Add(0.21698962756279,0.0399035597209444,0);
      boundary.Add(0.215802218272168,0.041137144206855,0);
      boundary.Add(0.215802227456089,0.0428506539101674,0);
      boundary.Add(0.215802812107743,0.0446816792320331,0);
      boundary.Add(0.215802643007603,0.044919880631123,0);
      boundary.Add(0.21580237562751,0.0459830757025587,0);
      boundary.Add(0.21662349142745,0.0473374888775767,0);
      boundary.Add(0.216644185405998,0.0472324967106073,0);
      boundary.Add(0.221287518565043,0.0522640268860162,0);
      boundary.Add(0.228021559321294,0.0589983796993594,0,0.99999999999999989);
      boundary.Add(0.22660734575892,0.0604125932617324,0);
      boundary.Add(0.219870003511324,0.0536752510141365,0);
      boundary.Add(0.215016236973274,0.0485419881549579,0);
      boundary.Add(0.214802316629069,0.0483005401574341,0);
      boundary.Add(0.214588316590671,0.0485421240080665,0);
      boundary.Add(0.21011025088572,0.0533704351993526,0);
      boundary.Add(0.21014443377089,0.0532652620821325,0);
      boundary.Add(0.209731029725076,0.0536787087954513,0);
      boundary.Add(0.209459633808689,0.0539501047118373,0);
      boundary.Add(0.202997145258794,0.0604125932617324,0,0.99999999999999989);
      boundary.Add(0.20158293169642,0.0589983796993594,0);
      boundary.Add(0.207437872058074,0.053143439337706,0,0.084899445551142858);
      boundary.Add(0.207632466505743,0.0528680942290934,0);
      boundary.Add(0.2097996919566,0.0506974961525178,0,0.067316854879475765);
      boundary.Add(0.21001280102321,0.0505349022168047,0);
      boundary.Add(0.212134891437448,0.0482663542141363,0);
      boundary.Add(0.212985192502462,0.0473330629547548,0);
      boundary.Add(0.213802212315766,0.045980682152605,0);
      boundary.Add(0.213801622761451,0.0448972276619335,0);
      boundary.Add(0.213801687074373,0.0446795594814957,0);
      boundary.Add(0.213802227507081,0.0428509786014969,0);
      boundary.Add(0.213802218271382,0.0411370035861251,0);
      boundary.Add(0.212614844974329,0.0399035930082819,0);
      boundary.Add(0.208922260221954,0.0378770899454234,0);
      boundary.Add(0.204937550154664,0.0358476102838221,0);
      boundary.Add(0.199715352488836,0.0337191316143378,0);
      boundary.Add(0.193305166728412,0.032333145503976,0);
      boundary.Add(0.190074999520026,0.0318917336505803,0);
      boundary.Add(0.186784024513245,0.031611557069398,0);
      boundary.Add(0.186219561094659,0.0315991127232463,0);
      boundary.Add(0.181938551656242,0.0352086839700424,0,0.99999999999999967);
      boundary.Add(0.180649337798972,0.0336796556412592,0);
      boundary.Add(0.184410028505795,0.0304560495286145,0);
      boundary.Add(0.184332974534524,0.0303524912999409,0);
      boundary.Add(0.18058683794642,0.0266063547118373,0,0.99999999999999989);
      boundary.Add(0.182001051508794,0.0251921411494643,0);
      boundary.Add(0.185798794564349,0.0289898842050199,0);
      boundary.Add(0.185888179503799,0.0290792691444689,0);
      boundary.Add(0.186584969281198,0.029587390869987,0);
      boundary.Add(0.186922318968018,0.0296160971810544,0);
      boundary.Add(0.190248576271573,0.0298991405054766,0);
      boundary.Add(0.197141378283878,0.0307986608817896,0);
      boundary.Add(0.202784321733209,0.0321846469921515,0);
      boundary.Add(0.209763751788959,0.0353773649963779,0);
      boundary.Add(0.213877544638376,0.0383441684565704,0);
      boundary.Add(0.214395456235699,0.0386985671672859,0);
      boundary.Add(0.214802226256067,0.0389872441500096,0);
      boundary.Add(0.215223256538453,0.0386886119266963,0);
      boundary.Add(0.215726900332203,0.0383441538954424,0);
      boundary.Add(0.219960651197212,0.0353031150458653,0);
      boundary.Add(0.226395586709607,0.0323578945613464,0);
      boundary.Add(0.233424516269299,0.0305016631635403,0);
      boundary.Add(0.238151718895712,0.0299324188682132,0);
      boundary.Add(0.238829069854513,0.02994397165231,0);
      boundary.Add(0.23935591528184,0.0298991404598922,0);
      boundary.Add(0.242682212411928,0.0296160937464856,0);
      boundary.Add(0.243019521736515,0.0295873908699861,0);
      boundary.Add(0.243716311513918,0.0290792691444704,0);
      boundary.Add(0.243805696453365,0.0289898842050198,0);
      boundary.Add(0.24760343950892,0.0251921411494643,0,0.99999999999999989);
      boundary.Add(0.249017653071294,0.0266063547118373,0);
      boundary.Add(0.245271516483189,0.0303524912999423,0);
      boundary.Add(0.245170345854367,0.0304884615937725,0);
      boundary.Add(0.248955153218742,0.0336796556412592,0,1.0000000000000004);
      boundary.Add(0.247665939361472,0.0352086839700424,0);
      boundary.Add(0.243384929923055,0.0315991127232463,0);
      boundary.Add(0.242820466504462,0.0316115570693985,0);
      boundary.Add(0.239525489894894,0.0318919466296366,0);
      boundary.Add(0.238953850116501,0.0319428927209702,0);
      boundary.Add(0.238621561259932,0.0319974189261181,0);
      boundary.Add(0.237550557050637,0.0321731629935912,0,0.11272245375866488);
      boundary.Add(0.237106175954916,0.0321456411653474,0);
      boundary.Add(0.236614172788539,0.0322099392895355,0);
      boundary.Add(0.231692033631346,0.0332241358803226,0);
      boundary.Add(0.226024340430045,0.0348823692623626,0);
      boundary.Add(0.221643634331223,0.0368623494200224,0);
      boundary.Add(0.217089679968606,0.0398570694084828,0);
      boundary.ID = 3;
      boundary.Closed = true;
      return boundary;
    }
    private Polyline TestVEngrave3DotCBOutlineBang() {
      Polyline outline = new Polyline();
      outline.Add(0.517719725977607,0.0708211229306508,0,0.025616807852200954);
      outline.Add(0.512470702540107,0.136372880743151,0,0.0103835630853921);
      outline.Add(0.508808593165107,0.160664872930651,0,-0.0067753983293559472);
      outline.Add(0.507388566456548,0.169419577876964,0,-0.029228782631419654);
      outline.Add(0.506489257227607,0.178242997930651,0,-0.064114670278621458);
      outline.Add(0.507420940402198,0.186207203207226,0,-0.11140906630820317);
      outline.Add(0.511005858790107,0.193379716680651,0,-0.23031626983188069);
      outline.Add(0.523945311915107,0.198872880743151,0,-0.0921539934042168);
      outline.Add(0.531032145017381,0.197674007803912,0,-0.1285730355317242);
      outline.Add(0.536884765040107,0.193501786993151,0,-0.11375295495395875);
      outline.Add(0.540568143659275,0.186287061664347,0,-0.065171493326904376);
      outline.Add(0.541523436915107,0.178242997930651,0,-0.029996464512886224);
      outline.Add(0.539448241602607,0.160664872930651,0,0.0028872796637611524);
      outline.Add(0.535419921290107,0.136372880743151,0,0.033259789573563542);
      outline.Add(0.530170897852607,0.0708211229306508,0);
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private Polyline TestVEngrave3DotCBErrorBox1Bang() {
      Polyline errorBox = new Polyline();
      errorBox.Add(0.514763633137015,0.0778787738391276,0);
      errorBox.Add(0.520525685937581,0.0778787738391276,0);
      errorBox.Add(0.520525685937581,0.0856353833783506,0);
      errorBox.Add(0.514763633137015,0.0856353833783506,0);
      errorBox.ID = 2;
      errorBox.Closed = true;
      return errorBox;
    }
    private Polyline TestVEngrave3DotCBErrorBox2Bang() {
      Polyline errorBox = new Polyline();
      errorBox.Add(0.522275662532366,0.0666142253986517,0);
      errorBox.Add(0.527882440390258,0.0666142253986517,0);
      errorBox.Add(0.527882440390258,0.0728439785740869,0);
      errorBox.Add(0.522275662532366,0.0728439785740869,0);
      errorBox.ID = 3;
      errorBox.Closed = true;
      return errorBox;
    }
    private Polyline TestVEngrave3DotCBToolpathOuterBoundaryBang() {
      Polyline boundary = new Polyline();
      boundary.Add(0.524896766650421,0.0781652857193691,0);
      boundary.Add(0.524996750841234,0.0775248329135693,0);
      boundary.Add(0.53088344835769,0.0715227437525385,0,-1);
      boundary.Add(0.529458387523945,0.0701194613088202,0);
      boundary.Add(0.523968564340957,0.0757146866283008,0);
      boundary.Add(0.523855451298592,0.075724406758336,0);
      boundary.Add(0.51843741925013,0.0701247635646796,0,-1);
      boundary.Add(0.517002002380629,0.0715174510417741,0);
      boundary.Add(0.522851212943118,0.0775646731717754,0);
      boundary.Add(0.522896574806617,0.0781904838397493,0);
      boundary.Add(0.522878734633925,0.181268396536719,0);
      boundary.Add(0.52109021747301,0.18419957743933,0,-1);
      boundary.Add(0.52277937256943,0.185665167890636,0);
      boundary.Add(0.52414560095624,0.183603405052358,0);
      boundary.Add(0.52563603192367,0.18569000840676,0,-1);
      boundary.Add(0.527374868052337,0.184348620536073,0);
      boundary.Add(0.525000045180959,0.181121356379847,0);
      boundary.ID = 4;
      boundary.Closed = true;
      return boundary;
    }
    private Polyline TestVEngrave4DotCBOutline() {
      Polyline outline = new Polyline();
      outline.Add(1.10455260341352,1.10864904182517,0,-0.11696123699846589);
      outline.Add(1.09112760341352,0.989699041825174,0);
      outline.Add(1.03080079394755,1.02025708670977,0);
      outline.Add(1.06118970040889,1.02618603176742,0,0.068304120596087356);
      outline.Add(1.05767760341352,1.09417404182517,0);
      outline.Add(1.04387760341352,1.08824904182517,0);
      outline.Add(1.05722760341352,1.10069904182517,0,-0.028464211010324765);
      outline.ID = 1;
      outline.Closed = true;
      return outline;
    }
    private Polyline TestVEngrave4DotCBErrorBox() {
      Polyline errorBox = new Polyline();
      errorBox.Add(1.02087443105378,1.08766539803767,0);
      errorBox.Add(1.04189960116236,1.08766539803767,0);
      errorBox.Add(1.04189960116236,1.12124290850957,0);
      errorBox.Add(1.02087443105378,1.12124290850957,0);
      errorBox.ID = 2;
      errorBox.Closed = true;
      return errorBox;
    }
    private Polyline TestVEngrave4DotCBToolpathOuterBoundary() {
      Polyline boundary = new Polyline();
      boundary.Add(1.10435891967534, 1.10518909436695, 0);
      boundary.Add(1.10611928905752, 1.10740586199389, 0, 0.99999999999999989);
      boundary.Add(1.10298591776952, 1.10989222165645, 0,
                   -0.0019139909220198094);
      boundary.Add(1.09436232244575, 1.09909904737785, 0);
      boundary.Add(1.0910035875836, 1.09494144381054, 0,
                   -0.0022243616177246325);
      boundary.Add(1.08278981545249, 1.08488846356531, 0, 0.011343561062565784);
      boundary.Add(1.06563761276892, 1.09687761601782, 0,
                   -0.041421217622887731);
      boundary.Add(1.06293421930482, 1.09842645034544, 0, 0.26808905177332448);
      boundary.Add(1.05503673080965, 1.09791685517165, 0);
      boundary.Add(1.0427848398383, 1.08992411429186, 0, 0.99999999999992306);
      boundary.Add(1.04497036698875, 1.08657396935848, 0);
      boundary.Add(1.05722225795579, 1.09456671023547, 0, -0.27045668524959565);
      boundary.Add(1.06104746495647, 1.09488460187183, 0,
                   -0.052016397011189076);
      boundary.Add(1.06442175487811, 1.09280509761779, 0);
      boundary.Add(1.06433408010304, 1.09293851257174, 0,
                   -0.0024539768851573572);
      boundary.Add(1.06803141559474, 1.09043777801538, 0,
                   -0.0024697758083316846);
      boundary.Add(1.07175271933142, 1.08786704345151, 0,
                   -0.0024898039928623974);
      boundary.Add(1.07550281903011, 1.08522106637612, 0,
                   -0.0025105037501734736);
      boundary.Add(1.07928106495225, 1.08249821144857, 0);
      boundary.Add(1.08117679758615, 1.08110934813917, 0,
                   -0.057068187916927055);
      boundary.Add(1.08127797334171, 1.07993790867347, 0);
      boundary.Add(1.08141048490235, 1.0777708817905, 0,
                   -0.0032517111286867561);
      boundary.Add(1.08157509445258, 1.07449845584283, 0,
                   -0.003240598704167724);
      boundary.Add(1.08169680187432, 1.07123484092584, 0,
                   -0.0032380372102421445);
      boundary.Add(1.08177616231923, 1.06797198512418, 0,
                   -0.0032354402985227669);
      boundary.Add(1.08181324916151, 1.06471047190162, 0,
                   -0.0032328086960549916);
      boundary.Add(1.08180814460878, 1.0614508821585, 0,
                   -0.0032301431331855277);
      boundary.Add(1.08176093962006, 1.05819379408071, 0,
                   -0.0032274443469039221);
      boundary.Add(1.08167173382099, 1.05493978299128, 0,
                   -0.0032247130747860963);
      boundary.Add(1.08154063541614, 1.05168942120462, 0,
                   -0.003221950060515216);
      boundary.Add(1.08136776109863, 1.04844327788335, 0,
                   -0.0032191560486948172);
      boundary.Add(1.08115323595712, 1.04520191889795, 0,
                   -0.0032163317877792518);
      boundary.Add(1.08089719338028, 1.04196590668918, 0,
                   -0.003213478029176512);
      boundary.Add(1.08059977495868, 1.03873580013326, 0, -0.00321059552468785);
      boundary.Add(1.08026113038449, 1.03551215441003, 0, -0.00320768502791398);
      boundary.Add(1.07988141734871, 1.032295520874, 0, -0.0032047472957535531);
      boundary.Add(1.07946080143635, 1.02908644692829, 0,
                   -0.0032017830829601185);
      boundary.Add(1.07899945601943, 1.02588547590164, 0,
                   -0.041023922373922488);
      boundary.Add(1.07779314058493, 1.02022299751542, 0);
      boundary.Add(1.07748664227487, 1.01948969018086, 0,
                   -0.083581288287817079);
      boundary.Add(1.06531812078096, 1.01756315741524, 0);
      boundary.Add(1.06334448125162, 1.01775347633303, 0);
      boundary.Add(1.03107610244716, 1.02223804739339, 0, 0.99999999999994482);
      boundary.Add(1.03052548544794, 1.01827612602615, 0);
      boundary.Add(1.0629503353682, 1.01376980903794, 0, 0.086238510937727345);
      boundary.Add(1.07799387918755, 1.01535226932107, 0);
      boundary.Add(1.07785123190105, 1.01539508795018, 0, -0.89412462478886368);
      boundary.Add(1.07814936259581, 1.01527389593695, 0,
                   -0.010760944791897797);
      boundary.Add(1.08926976142181, 0.988958485862497, 0, 1.0000000000000464);
      boundary.Add(1.09298545284987, 0.99043957911111, 0,
                   0.0014450068050039951);
      boundary.Add(1.09150521440214, 0.994132510577131, 0);
      boundary.Add(1.09050824048704, 0.996585355621557, 0,
                   0.0093522119401559713);
      boundary.Add(1.08131792629092, 1.01797877582013, 0, 0.030202834884394448);
      boundary.Add(1.08339711163067, 1.02835025903998, 0, 0.052787918509483243);
      boundary.Add(1.0851820039019, 1.08148134930864, 0, -0.018534744060450688);
      boundary.Add(1.08616905018124, 1.08269950451524, 0);
      boundary.Add(1.08931517038803, 1.08653281456924, 0,
                   0.0009674884514208668);
      boundary.Add(1.09334012340828, 1.09147225046795, 0);
      boundary.Add(1.09664187539159, 1.09555308003942, 0,
                   0.0016806836413417929);
      boundary.ID = 3;
      boundary.Closed = true;
      return boundary;
    }
  }
}