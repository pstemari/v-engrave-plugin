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
  public class CutWidthTests {
    private const double DEGREES = Math.PI/180;

    [TestMethod, TestCategory("Repro")]
    public void TestBigE() {
      var mop = CreateMOP();
      var outline = CreateBigE();
      var tseq = CreateToolpathSequence(mop, outline);
      List<Surface> cutwidths = mop.ComputeCutSurfaces(tseq.Toolpaths);
      foreach (Surface s in cutwidths) {
        foreach (Point3F p in s.Points) {
          //Assert.IsTrue(outline.PointInPolyline2(p.To2D(), 1e-6),
          //              "Bad point {0}", p);
        }
      }
    }

    [TestMethod, TestCategory("Repro")]
    public void TestCatullLittleL() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestCatullLittleD() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestCatullBigS() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestCatullLittleT() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestCatullLittleA() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsBigH() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsLittleL() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsComma() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsLittleA() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsLittleM() {
    }

    [TestMethod, TestCategory("Repro")]
    public void TestBrusselsBang() {
    }

    private ToolpathSequence CreateToolpathSequence(MOPVEngrave mop,
                                                    Entity shape) {
      mop.ClearToolpaths();
      ShapeList shapes = new ShapeList();
      shapes.AddEntity(shape);
      ToolpathSequence tseq = mop.CreateToolpathSequence(shapes);
//      mop.SetToolpathSequence(tseq);
//      mop.DetermineCuttingOrder(tseq);
      return tseq;
    }

    private MOPVEngrave CreateMOP() {
      var mop = new MOPVEngrave();
      mop.SetLogger(new NullLogger());
      return mop;
    }

    private Polyline CreateBigE() {
      Polyline outline = new Polyline();
      outline.ID = 17;
      outline.Add(0.79423504047766, 0.791196232424028, 0);
      outline.Add(0.793023097456662, 0.862700870662932, 0);
      outline.Add(0.666981023272832, 0.861488927641933, 0);
      outline.Add(0.666981023272832, 0.947536882132817, 0);
      outline.Add(0.721518459217758, 0.949960768174814, 0);
      outline.Add(0.721518459217758, 0.992378773909757, 0);
      outline.Add(0.666981023272832, 0.996014602972752, 0);
      outline.Add(0.662330445336019, 1.09462699512203, 0);
      outline.Add(0.779441226620903, 1.09285258934498, 0);
      outline.Add(0.767020386181597, 1.15673119731856, 0);
      outline.Add(0.580707779592008, 1.14963357421038, 0);
      outline.Add(0.586030996923139, 0.796526824578683, 0);
      outline.Closed = true;

      return outline;
    }

    private Polyline CreateCatullLittleL() {
      Polyline outline = new Polyline();
      outline.ID = 28;
      outline.Add(1.35267000940759, 0.887481926122061, 0, 0.22816739532194919);
      outline.Add(1.35289500940759, 0.890706926122061, 0);
      outline.Add(1.35289500940759, 0.931506926122061, 0);
      outline.Add(1.35319500940755, 0.965781926122067, 0,
                  -0.0019371076676125486);
      outline.Add(1.35394500940759, 1.00485692612206, 0, -0.034059023978827783);
      outline.Add(1.35469500940759, 1.02533192612206, 0);
      outline.Add(1.35544500940759, 1.04505692612206, 0);
      outline.Add(1.33594500940759, 1.04505692612206, 0);
      outline.Add(1.35469500940759, 1.05405692612206, 0);
      outline.Add(1.39039500940759, 1.05405692612206, 0);
      outline.Add(1.38552000940759, 1.05105692612206, 0);
      outline.Add(1.38229500940759, 1.04835692612206, 0, 0.068142826572446955);
      outline.Add(1.38019500940759, 1.04468192612206, 0);
      outline.Add(1.37914500940759, 1.03935692612206, 0, 0.06666502683089176);
      outline.Add(1.37727000940759, 1.00283192612206, 0, 0.046104210648730611);
      outline.Add(1.37709435197821, 0.990567898851503, 0);
      outline.Add(1.37689500940759, 0.954006926122062, 0);
      outline.Add(1.37689500940759, 0.896256926122061, 0, 0.19090797807917417);
      outline.Add(1.37877000940759, 0.886881926122061, 0, 0.15054731531770713);
      outline.Add(1.38694500940759, 0.884256926122062, 0, 0.023596029213905911);
      outline.Add(1.39324500940759, 0.883656926122061, 0);
      outline.Add(1.39969500940759, 0.883056926122062, 0);
      outline.Add(1.39039500940759, 0.877506926122062, 0);
      outline.Add(1.34614500940759, 0.877506926122062, 0);
      outline.Add(1.34967000940759, 0.882006926122062, 0);
      outline.Add(1.35169500940759, 0.885006926122061, 0, 0.23673051230036826);
      outline.Closed = true;

      return outline;
    }

    private Region CreateCatullLittleD() {
      Region region = new Region();
      region.ID = 37;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(1.79314500940759, 0.890556926122062, 0,
                            0.31302924619776523);
      region.OuterCurve.Add(1.79562000940759, 0.886431926122062, 0,
                            0.069960102016347614);
      region.OuterCurve.Add(1.80364500940759, 0.885006926122061, 0,
                            0.16118080880541227);
      region.OuterCurve.Add(1.80927000940759, 0.884631926122061, 0,
                            -0.17587595603245892);
      region.OuterCurve.Add(1.81489500940759, 0.883956926122062, 0);
      region.OuterCurve.Add(1.80364500940759, 0.877506926122062, 0);
      region.OuterCurve.Add(1.74859500940759, 0.877506926122062, 0,
                            -0.003301184143064398);
      region.OuterCurve.Add(1.73517000940759, 0.878031926122062, 0,
                            -0.033496676823529206);
      region.OuterCurve.Add(1.72092000940759, 0.880506926122062, 0);
      region.OuterCurve.Add(1.70704500940759, 0.886731926122061, 0,
                            -0.042880236040647571);
      region.OuterCurve.Add(1.69504500940759, 0.898506926122061, 0,
                            -0.068860959413119011);
      region.OuterCurve.Add(1.68784500940759, 0.912081926122061, 0,
                            -0.12999176592073491);
      region.OuterCurve.Add(1.68499500940759, 0.930456926122062, 0,
                            -0.098232324470845456);
      region.OuterCurve.Add(1.68934500940759, 0.952956926122062, 0,
                            -0.12032034959818255);
      region.OuterCurve.Add(1.70059500940759, 0.969831926122062, 0,
                            -0.064764945024913478);
      region.OuterCurve.Add(1.71604500940759, 0.981456926122062, 0,
                            -0.0460231440382177);
      region.OuterCurve.Add(1.73284500940759, 0.988206926122062, 0,
                            -0.030673587488576289);
      region.OuterCurve.Add(1.75182000940759, 0.991131926122061, 0);
      region.OuterCurve.Add(1.77064500940759, 0.991506926122061, 0,
                            0.012206978018225911);
      region.OuterCurve.Add(1.77169500940759, 1.02083192612206, 0);
      region.OuterCurve.Add(1.77169500940759, 1.04535692612206, 0);
      region.OuterCurve.Add(1.75189500940759, 1.04535692612206, 0);
      region.OuterCurve.Add(1.77109500940759, 1.05405692612206, 0);
      region.OuterCurve.Add(1.80514500940759, 1.05405692612206, 0,
                            0.04743112196722573);
      region.OuterCurve.Add(1.79922000940759, 1.04993192612206, 0,
                            0.014285714283818);
      region.OuterCurve.Add(1.79607000940759, 1.04573192612206, 0,
                            0.14230859351962752);
      region.OuterCurve.Add(1.79487000940759, 1.03860692612206, 0,
                            0.041409485517202034);
      region.OuterCurve.Add(1.79464500940759, 1.02600692612206, 0);
      region.OuterCurve.Add(1.79359500940759, 0.959556926122062, 0);
      region.OuterCurve.Add(1.79284500940759, 0.925731926122062, 0,
                            0.041472657228840915);
      region.OuterCurve.Add(1.79254500940759, 0.902706926122061, 0);
      region.OuterCurve.Add(1.79269500940759, 0.893331926122062, 0);
      region.OuterCurve.Closed = true;

      Polyline hole = new Polyline();
      hole.Add(1.76877000940759, 0.890256926122062, 0);
      hole.Add(1.76944500940759, 0.892956926122062, 0);
      hole.Add(1.76982000940759, 0.899481926122061, 0, -0.07362561860797652);
      hole.Add(1.77004500940759, 0.910506926122061, 0);
      hole.Add(1.77019500940759, 0.921831926122062, 0);
      hole.Add(1.77019500940759, 0.983706926122061, 0);
      hole.Add(1.76389500940759, 0.984081926122061, 0, 0.12317215479496037);
      hole.Add(1.75759500940759, 0.984156926122062, 0);
      hole.Add(1.75039500940759, 0.983931926122062, 0);
      hole.Add(1.74184500940759, 0.982581926122061, 0, 0.13930386541013487);
      hole.Add(1.73269500940759, 0.979431926122062, 0, 0.098611536501855085);
      hole.Add(1.72354500940759, 0.973956926122061, 0, 0.12072521643463076);
      hole.Add(1.71102000940759, 0.957081926122062, 0, 0.0895372828529142);
      hole.Add(1.70824500940759, 0.939906926122062, 0, 0.083251124092031115);
      hole.Add(1.71087000940759, 0.921456926122062, 0, 0.063869162051259629);
      hole.Add(1.71912000940759, 0.903606926122062, 0, 0.11496820113938015);
      hole.Add(1.73367000940759, 0.890106926122062, 0, 0.10759735984916108);
      hole.Add(1.75519500940759, 0.884706926122062, 0);
      hole.Add(1.76277000940759, 0.885456926122062, 0, 0.1171309252584508);
      hole.Add(1.76704500940759, 0.887481926122061, 0);
      hole.Closed = true;
      region.HoleCurves = new Polyline[] {hole};

      return region;
    }

    private Polyline CreateCatullBigS() {
      Polyline outline = new Polyline();
      outline.ID = 23;
      outline.Add(1.12062000940759, 0.599256926122065, 0, 0.23777766521583379);
      outline.Add(1.12182000940759, 0.596556926122065, 0,
                  0.095022222917084079);
      outline.Add(1.12744500940759, 0.592356926122065, 0, 0.11446589194406354);
      outline.Add(1.13712000940759, 0.587706926122065, 0);
      outline.Add(1.14904500940759, 0.583956926122065, 0,
                  0.062860024548844137);
      outline.Add(1.16157000940759, 0.582456926122065, 0,
                  0.056486044026981611);
      outline.Add(1.17672000940759, 0.585081926122065, 0, 0.12101357951806566);
      outline.Add(1.18879500940759, 0.592206926122065, 0);
      outline.Add(1.19682000940759, 0.602706926122065, 0, 0.11251172580517918);
      outline.Add(1.19967000940759, 0.615756926122065, 0,
                  0.077689688932032042);
      outline.Add(1.19869500940759, 0.623181926122065, 0, 0.11758306953408153);
      outline.Add(1.19637000940759, 0.629556926122065, 0, 0.13844889227451082);
      outline.Add(1.18392000940759, 0.644631926122065, 0);
      outline.Add(1.16742000940759, 0.654306926122065, 0);
      outline.Add(1.15017000940759, 0.662556926122065, 0,
                  -0.037272129495485876);
      outline.Add(1.13419500940759, 0.671331926122065, 0,
                  -0.12546791475411021);
      outline.Add(1.12107000940759, 0.684456926122062, 0,
                  -0.12247684474140957);
      outline.Add(1.11612000940759, 0.695856926122062, 0);
      outline.Add(1.11462000940759, 0.706806926122061, 0,
                  -0.091603497684611471);
      outline.Add(1.11784500940759, 0.723306926122062, 0,
                  -0.092564645409094912);
      outline.Add(1.12827000940759, 0.739956926122062, 0,
                  -0.12289304280359367);
      outline.Add(1.14642000940759, 0.752706926122062, 0,
                  -0.12151474587630626);
      outline.Add(1.17312000940759, 0.757806926122062, 0);
      outline.Add(1.19487000940759, 0.755406926122061, 0);
      outline.Add(1.20765656641442, 0.751617595067374, 0);
      outline.Add(1.21977000940759, 0.745806926122062, 0);
      outline.Add(1.20072000940759, 0.725556926122062, 0);
      outline.Add(1.19412000940759, 0.724056926122062, 0);
      outline.Add(1.19817000940759, 0.731556926122062, 0);
      outline.Add(1.19937000940759, 0.735006926122061, 0, 0.17221210609389936);
      outline.Add(1.19734500940759, 0.738831926122061, 0, 0.16419823758286586);
      outline.Add(1.19157000940759, 0.743706926122062, 0);
      outline.Add(1.18234500940759, 0.747981926122061, 0);
      outline.Add(1.16982000940759, 0.749856926122062, 0, 0.14279913664995123);
      outline.Add(1.15669500940759, 0.747606926122061, 0);
      outline.Add(1.14597000940759, 0.741231926122062, 0, 0.10700679519088531);
      outline.Add(1.13869500940759, 0.731481926122062, 0);
      outline.Add(1.13607000940759, 0.719106926122062, 0, 0.0957806095972174);
      outline.Add(1.13929500940759, 0.705231926122062, 0);
      outline.Add(1.14747000940759, 0.695031926122061, 0);
      outline.Add(1.15827000940759, 0.687681926122062, 0);
      outline.Add(1.16907000940759, 0.682056926122065, 0);
      outline.Add(1.19097000940759, 0.671556926122065, 0,
                  -0.11827148811464787);
      outline.Add(1.20094500940759, 0.665556926122065, 0);
      outline.Add(1.21122000940759, 0.656931926122065, 0, -0.1286573366793255);
      outline.Add(1.21924500940759, 0.644631926122065, 0,
                  -0.11828224241525713);
      outline.Add(1.22247000940759, 0.627606926122065, 0,
                  -0.11780285729218144);
      outline.Add(1.21774500940759, 0.606531926122065, 0,
                  -0.13192074695148551);
      outline.Add(1.20424500940759, 0.589356926122065, 0,
                  -0.081631619641193351);
      outline.Add(1.18317000940759, 0.577881926122065, 0,
                  -0.10216738178590425);
      outline.Add(1.15587000940759, 0.573756926122065, 0, -0.0889390226522771);
      outline.Add(1.12699500940759, 0.577131926122065, 0);
      outline.Add(1.09932000940759, 0.586206926122065, 0);
      outline.Add(1.11882000940759, 0.608256926122065, 0);
      outline.Add(1.12557000940759, 0.609756926122065, 0);
      outline.Add(1.12137000940759, 0.601206926122065, 0, 0.30369460544867921);
      outline.Closed = true;

      return outline;
    }

    private Polyline CreateCatullLittleT() {
      Polyline outline = new Polyline();
      outline.ID = 18;
      outline.Add(1.29537000940759, 0.577506926122065, 0);
      outline.Add(1.29267000940759, 0.576756926122065, 0);
      outline.Add(1.28022000940759, 0.575256926122065, 0, -0.1683509363292508);
      outline.Add(1.26507000940759, 0.578406926122065, 0,
                  -0.10338285723478563);
      outline.Add(1.25719500940759, 0.585906926122065, 0,
                  -0.058428641433753092);
      outline.Add(1.25434500940759, 0.594756926122065, 0);
      outline.Add(1.25397000940759, 0.601956926122065, 0);
      outline.Add(1.25547000940759, 0.682206926122065, 0);
      outline.Add(1.24047000940759, 0.682206926122065, 0);
      outline.Add(1.25547000940759, 0.690756926122062, 0);
      outline.Add(1.25547000940759, 0.708306926122062, 0);
      outline.Add(1.26222000940759, 0.710781926122062, 0);
      outline.Add(1.26732000940758, 0.712731926122065, 0,
                  -0.00018052883161057736);
      outline.Add(1.27249500940759, 0.714831926122061, 0);
      outline.Add(1.27947000940759, 0.717756926122062, 0);
      outline.Add(1.27947000940759, 0.690756926122062, 0);
      outline.Add(1.31502000940759, 0.690756926122062, 0);
      outline.Add(1.30197000940759, 0.682206926122065, 0);
      outline.Add(1.27947000940759, 0.682206926122065, 0);
      outline.Add(1.27842000940759, 0.636156926122065, 0);
      outline.Add(1.27842000940759, 0.628281926122065, 0);
      outline.Add(1.27857000940759, 0.617856926122065, 0);
      outline.Add(1.27879500940759, 0.607581926122065, 0);
      outline.Add(1.27917000940759, 0.600006926122065, 0);
      outline.Add(1.28044500940759, 0.593856926122065, 0, 0.21375138636253674);
      outline.Add(1.28329500940759, 0.588906926122065, 0, 0.17166710989969072);
      outline.Add(1.28839500940759, 0.585531926122065, 0);
      outline.Add(1.29687000940759, 0.584256926122065, 0,
                  0.098261296941821841);
      outline.Add(1.30279500940759, 0.584631926122065, 0,
                  0.0053442100767081765);
      outline.Add(1.30872000940759, 0.585456926122065, 0);
      outline.Add(1.29822000940759, 0.578256926122065, 0);
      outline.Closed = true;

      return outline;
    }

    private Region CreateCatullLittleA() {
      Region region = new Region();
      region.ID = 38;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(1.65109500940759, 0.575031926122065, 0);
      region.OuterCurve.Add(1.64757000940759, 0.574806926122065, 0,
                            -0.075308496175476816);
      region.OuterCurve.Add(1.63962000940759, 0.576081926122065, 0,
                            -0.0062528210149970414);
      region.OuterCurve.Add(1.63212000940759, 0.579306926122065, 0,
                            -0.12911047545243193);
      region.OuterCurve.Add(1.62282000940759, 0.587106926122065, 0,
                            -0.10769228448123187);
      region.OuterCurve.Add(1.61802000940759, 0.595056926122065, 0);
      region.OuterCurve.Add(1.60632000940759, 0.583506926122065, 0);
      region.OuterCurve.Add(1.59522000940759, 0.578031926122065, 0,
                            -0.12996923987811782);
      region.OuterCurve.Add(1.57557000940759, 0.574506926122065, 0);
      region.OuterCurve.Add(1.56192000940759, 0.576606926122065, 0,
                            -0.15945182309352018);
      region.OuterCurve.Add(1.55217000940759, 0.582381926122065, 0,
                            -0.1500821644503724);
      region.OuterCurve.Add(1.54639500940759, 0.590631926122065, 0,
                            -0.098923058311046835);
      region.OuterCurve.Add(1.54452000940759, 0.600456926122065, 0,
                            -0.17139775548217434);
      region.OuterCurve.Add(1.55104500940759, 0.619506926122065, 0,
                            -0.078960484742557707);
      region.OuterCurve.Add(1.56672000940759, 0.633606926122065, 0);
      region.OuterCurve.Add(1.58547000940759, 0.643206926122065, 0);
      region.OuterCurve.Add(1.60137000940759, 0.649206926122065, 0);
      region.OuterCurve.Add(1.59762000940759, 0.660306926122065, 0,
                            0.1228621929002182);
      region.OuterCurve.Add(1.59117000940759, 0.673881926122065, 0,
                            0.15208888478670352);
      region.OuterCurve.Add(1.58389500940759, 0.681156926122065, 0,
                            0.11357505551310769);
      region.OuterCurve.Add(1.57407000940759, 0.683931926122062, 0);
      region.OuterCurve.Add(1.55982000940759, 0.684006926122062, 0);
      region.OuterCurve.Add(1.57812000940759, 0.691806926122061, 0,
                            -0.0430411445265834);
      region.OuterCurve.Add(1.59432000940759, 0.691206926122061, 0,
                            -0.080934881809928846);
      region.OuterCurve.Add(1.60827000940759, 0.686556926122062, 0,
                            -0.10463701596225672);
      region.OuterCurve.Add(1.61397000940759, 0.681156926122065, 0,
                            -0.083595312576193126);
      region.OuterCurve.Add(1.61824500940759, 0.673656926122065, 0);
      region.OuterCurve.Add(1.62199500940759, 0.663306926122065, 0);
      region.OuterCurve.Add(1.62612000940759, 0.649356926122065, 0);
      region.OuterCurve.Add(1.63107000940759, 0.631506926122065, 0,
                            0.017652609317298753);
      region.OuterCurve.Add(1.63669500940759, 0.612906926122065, 0);
      region.OuterCurve.Add(1.64262000940759, 0.596856926122065, 0,
                            0.12436141442447588);
      region.OuterCurve.Add(1.64892000940759, 0.583431926122065, 0);
      region.OuterCurve.Add(1.65477000940759, 0.575556926122065, 0);
      region.OuterCurve.Closed = true;

      Polyline hole = new Polyline();
      hole.Add(1.61262000940759, 0.609681926122065, 0, -0.001525886799584416);
      hole.Add(1.61097000940759, 0.615531926122065, 0);
      hole.Add(1.60857000940759, 0.624006926122065, 0);
      hole.Add(1.60632000940759, 0.632556926122065, 0, 0.0034998082409088481);
      hole.Add(1.60407000940759, 0.639531926122065, 0, 0.24027664196827997);
      hole.Add(1.60062000940759, 0.641556926122065, 0);
      hole.Add(1.59342000940759, 0.639306926122065, 0);
      hole.Add(1.58284500940759, 0.632931926122065, 0, 0.018063459333063286);
      hole.Add(1.57309500940759, 0.622806926122065, 0, 0.15911663357588213);
      hole.Add(1.56882000940759, 0.609006926122065, 0, 0.059793165884617555);
      hole.Add(1.57107000940759, 0.599556926122065, 0, 0.18872345507766961);
      hole.Add(1.57684500940759, 0.593181926122065, 0, 0.054425464494866758);
      hole.Add(1.58457000940759, 0.589581926122065, 0, 0.095757792189697113);
      hole.Add(1.59282000940759, 0.588456926122065, 0, 0.16971241154351624);
      hole.Add(1.60819500940759, 0.592206926122065, 0);
      hole.Add(1.61637000940759, 0.598506926122065, 0);
      hole.Add(1.61419500940759, 0.604656926122065, 0);
      hole.Closed = true;
      region.HoleCurves = new Polyline[] {hole};

      return region;
    }

    private Region CreateBrusselsBigH() {
      Region region = new Region();
      region.ID = 2;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(0.527955470698132, 0.279429734273086, 0);
      region.OuterCurve.Add(0.601881251948132, 0.279429734273086, 0);
      region.OuterCurve.Add(0.601881251948132, 0.313609421773086, 0,
                            0.044705542545749308);
      region.OuterCurve.Add(0.60141955433583, 0.319425786581944, 0,
                            0.11944213995382062);
      region.OuterCurve.Add(0.599244533198132, 0.324839890523086, 0,
                            0.13179961139040317);
      region.OuterCurve.Add(0.594978661298453, 0.327996002862565, 0,
                            0.058370657533932356);
      region.OuterCurve.Add(0.589869533198132, 0.329429734273086, 0);
      region.OuterCurve.Add(0.583131251948132, 0.330601609273086, 0);
      region.OuterCurve.Add(0.583131251948132, 0.337437546773086, 0);
      region.OuterCurve.Add(0.640748439448132, 0.337437546773086, 0);
      region.OuterCurve.Add(0.640748439448132, 0.330601609273086, 0);
      region.OuterCurve.Add(0.633912501948132, 0.329429734273086, 0,
                            0.058028447629619707);
      region.OuterCurve.Add(0.628763393622735, 0.327986712680241, 0,
                            0.12763135392063227);
      region.OuterCurve.Add(0.624439845698132, 0.324839890523086, 0,
                            0.11625773141920573);
      region.OuterCurve.Add(0.622342891775879, 0.3194117286168, 0,
                            0.04291611480474572);
      region.OuterCurve.Add(0.621900783198132, 0.313609421773086, 0);
      region.OuterCurve.Add(0.621900783198132, 0.225230515523086, 0,
                            0.042972217459807852);
      region.OuterCurve.Add(0.622348013176885, 0.21938213951878, 0,
                            0.11308190703381325);
      region.OuterCurve.Add(0.624439845698132, 0.213902390523086, 0,
                            0.12585589532236943);
      region.OuterCurve.Add(0.62878273406565, 0.210826662878074, 0,
                            0.055858949898684646);
      region.OuterCurve.Add(0.633912501948132, 0.209410203023086, 0);
      region.OuterCurve.Add(0.640748439448132, 0.208238328023086, 0);
      region.OuterCurve.Add(0.640748439448132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.583131251948132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.583131251948132, 0.208238328023086, 0);
      region.OuterCurve.Add(0.589869533198132, 0.209410203023086, 0,
                            0.056212050605122153);
      region.OuterCurve.Add(0.594959010854089, 0.210817389979457, 0,
                            0.13001917635814297);
      region.OuterCurve.Add(0.599244533198132, 0.213902390523086, 0,
                            0.11619700870894111);
      region.OuterCurve.Add(0.60141427184693, 0.219368286772016, 0,
                            0.044781524088163688);
      region.OuterCurve.Add(0.601881251948132, 0.225230515523086, 0);
      region.OuterCurve.Add(0.601881251948132, 0.270250046773086, 0);
      region.OuterCurve.Add(0.527955470698132, 0.270250046773086, 0);
      region.OuterCurve.Add(0.527955470698132, 0.225230515523086, 0,
                            0.042972217459807852);
      region.OuterCurve.Add(0.528402700676885, 0.21938213951878, 0,
                            0.11308190703381774);
      region.OuterCurve.Add(0.530494533198132, 0.213902390523086, 0,
                            0.1258558953223457);
      region.OuterCurve.Add(0.53483742156565, 0.210826662878074, 0,
                            0.055858949898684646);
      region.OuterCurve.Add(0.539967189448132, 0.209410203023086, 0);
      region.OuterCurve.Add(0.546705470698132, 0.208238328023086, 0);
      region.OuterCurve.Add(0.546705470698132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.489088283198132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.489088283198132, 0.208238328023086, 0);
      region.OuterCurve.Add(0.495924220698132, 0.209410203023086, 0,
                            0.056212050605087646);
      region.OuterCurve.Add(0.501013698354089, 0.210817389979457, 0,
                            0.13001917635816451);
      region.OuterCurve.Add(0.505299220698132, 0.213902390523086, 0,
                            0.11619700870894641);
      region.OuterCurve.Add(0.50746895934693, 0.219368286772016, 0,
                            0.044781524088153835);
      region.OuterCurve.Add(0.507935939448132, 0.225230515523086, 0);
      region.OuterCurve.Add(0.507935939448132, 0.313609421773086, 0,
                            0.044705542545746484);
      region.OuterCurve.Add(0.50747424183583, 0.319425786581944, 0,
                            0.11944213995382061);
      region.OuterCurve.Add(0.505299220698132, 0.324839890523086, 0,
                            0.13179961139040311);
      region.OuterCurve.Add(0.501033348798453, 0.327996002862565, 0,
                            0.058370657533932363);
      region.OuterCurve.Add(0.495924220698132, 0.329429734273086, 0);
      region.OuterCurve.Add(0.489088283198132, 0.330601609273086, 0);
      region.OuterCurve.Add(0.489088283198132, 0.337437546773086, 0);
      region.OuterCurve.Add(0.546705470698132, 0.337437546773086, 0);
      region.OuterCurve.Add(0.546705470698132, 0.330601609273086, 0);
      region.OuterCurve.Add(0.539967189448132, 0.329429734273086, 0,
                            0.058028447629688971);
      region.OuterCurve.Add(0.534818081122735, 0.327986712680241, 0,
                            0.1276313539206323);
      region.OuterCurve.Add(0.530494533198132, 0.324839890523086, 0,
                            0.11625773141922123);
      region.OuterCurve.Add(0.528397579275879, 0.3194117286168, 0,
                            0.042916114804733937);
      region.OuterCurve.Add(0.527955470698132, 0.313609421773086, 0);
      region.OuterCurve.Closed = true;

      return region;
    }

    private Region CreateBrusselsLittleL() {
      Region region = new Region();
      region.ID = 4;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(0.759498439448132, 0.209410203023086, 0,
                            0.065835414985014884);
      region.OuterCurve.Add(0.763750948118824, 0.210783960739988, 0,
                            0.12347510202080689);
      region.OuterCurve.Add(0.767213283198132, 0.213609421773086, 0,
                            0.10737259161622637);
      region.OuterCurve.Add(0.769043447054616, 0.218279127093406, 0,
                            0.046422630714093971);
      region.OuterCurve.Add(0.769459376948132, 0.223277390523086, 0);
      region.OuterCurve.Add(0.769459376948132, 0.319859421773086, 0,
                            0.070947027034597654);
      region.OuterCurve.Add(0.768700196907018, 0.325774955713297, 0,
                            0.20670255625206302);
      region.OuterCurve.Add(0.763266873181957, 0.332756488354985, 0,
                            0.12337635703214202);
      region.OuterCurve.Add(0.754615626948132, 0.334605515523086, 0);
      region.OuterCurve.Add(0.752662501948132, 0.334605515523086, 0);
      region.OuterCurve.Add(0.752662501948132, 0.341441453023086, 0);
      region.OuterCurve.Add(0.780103908198132, 0.345445359273086, 0);
      region.OuterCurve.Add(0.789478908198132, 0.345445359273086, 0);
      region.OuterCurve.Add(0.789478908198132, 0.223277390523086, 0,
                            0.04635840367076409);
      region.OuterCurve.Add(0.789894838091648, 0.218279127093406, 0,
                            0.10739275245094745);
      region.OuterCurve.Add(0.791725001948132, 0.213609421773086, 0,
                            0.123448881319954);
      region.OuterCurve.Add(0.79518733702744, 0.210783960739988, 0,
                            0.065856551408462249);
      region.OuterCurve.Add(0.799439845698132, 0.209410203023086, 0);
      region.OuterCurve.Add(0.806275783198132, 0.208238328023086, 0);
      region.OuterCurve.Add(0.806275783198132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.752662501948132, 0.201402390523086, 0);
      region.OuterCurve.Add(0.752662501948132, 0.208238328023086, 0);
      region.OuterCurve.Closed = true;

      return region;
    }

    private Region CreateBrusselsComma() {
      Region region = new Region();
      region.ID = 7;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(1.00178359569813, 0.178355515523086, 0,
                            0.09459807983554834);
      region.OuterCurve.Add(1.00949843944813, 0.186851609273086, 0,
                            0.041995119592303419);
      region.OuterCurve.Add(1.01180039371256, 0.191066904904552, 0,
                            0.099232225192698142);
      region.OuterCurve.Add(1.01291640819813, 0.195738328023086, 0,
                            0.37366675721438314);
      region.OuterCurve.Add(1.00940078319813, 0.199351609273086, 0,
                            -0.16972186464262759);
      region.OuterCurve.Add(1.00139297069813, 0.204234421773086, 0,
                            -0.17448042746141329);
      region.OuterCurve.Add(0.998365626948132, 0.213023484273086, 0,
                            -0.19975586176092416);
      region.OuterCurve.Add(1.00236953319813, 0.223472703023086, 0,
                            -0.21393765295700315);
      region.OuterCurve.Add(1.01233047069813, 0.227769578023086, 0,
                            -0.23466853443210917);
      region.OuterCurve.Add(1.02356093944813, 0.222593796773086, 0,
                            -0.10590224530095881);
      region.OuterCurve.Add(1.02698431183138, 0.21630707690806, 0,
                            -0.0738403738653717);
      region.OuterCurve.Add(1.02795547069813, 0.209214890523086, 0,
                            -0.13618714103877846);
      region.OuterCurve.Add(1.02248672069813, 0.189878953023086, 0,
                            -0.10894032352057116);
      region.OuterCurve.Add(1.00715468944813, 0.173570359273086, 0);
      region.OuterCurve.Closed = true;

      return region;
    }

    private Region CreateBrusselsLittleM() {
      Region region = new Region();
      region.ID = 10;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(1.37402933088521, 0.209410203023086, 0,
                            0.065835414985014884);
      region.OuterCurve.Add(1.3782818395559, 0.210783960739988, 0,
                            0.12347510202080689);
      region.OuterCurve.Add(1.38174417463521, 0.213609421773086, 0,
                            0.10685075090331521);
      region.OuterCurve.Add(1.38357086170176, 0.218229438014121, 0,
                            0.047136914475485452);
      region.OuterCurve.Add(1.38399026838521, 0.223179734273086, 0);
      region.OuterCurve.Add(1.38399026838521, 0.271812546773086, 0,
                            0.070947027034650958);
      region.OuterCurve.Add(1.3832310883441, 0.277728080713297, 0,
                            0.20670255625205219);
      region.OuterCurve.Add(1.37779776461904, 0.284709613354985, 0,
                            0.12337635703214202);
      region.OuterCurve.Add(1.36914651838521, 0.286558640523086, 0);
      region.OuterCurve.Add(1.36719339338521, 0.286558640523086, 0);
      region.OuterCurve.Add(1.36719339338521, 0.293394578023086, 0);
      region.OuterCurve.Add(1.39463479963521, 0.297398484273086, 0);
      region.OuterCurve.Add(1.40400979963521, 0.297398484273086, 0);
      region.OuterCurve.Add(1.40400979963521, 0.283726609273086, 0,
                            -0.063634457786895526);
      region.OuterCurve.Add(1.42034133885081, 0.294964165753961, 0,
                            -0.11644387288552989);
      region.OuterCurve.Add(1.43955667463521, 0.299839890523086, 0,
                            -0.050707913203247891);
      region.OuterCurve.Add(1.448180269944, 0.299028202523919, 0,
                            -0.13330124076039621);
      region.OuterCurve.Add(1.45990937788357, 0.293435160731098, 0,
                            -0.13842679768823243);
      region.OuterCurve.Add(1.46670511213521, 0.282359421773086, 0,
                            -0.081193755074784407);
      region.OuterCurve.Add(1.48438089338521, 0.295640671773086, 0,
                            -0.11560810777539125);
      region.OuterCurve.Add(1.50234964338521, 0.299839890523086, 0,
                            -0.079623657747206061);
      region.OuterCurve.Add(1.51497162648215, 0.298006383122877, 0,
                            -0.12616008837259732);
      region.OuterCurve.Add(1.52578714338521, 0.291246140523086, 0,
                            -0.12152279209725195);
      region.OuterCurve.Add(1.53228048959659, 0.279790720696882, 0,
                            -0.0719069607693457);
      region.OuterCurve.Add(1.53399026838521, 0.266734421773086, 0);
      region.OuterCurve.Add(1.53399026838521, 0.223179734273086, 0,
                            0.047195307503079022);
      region.OuterCurve.Add(1.53440967506867, 0.218229438014121, 0,
                            0.10683840469982697);
      region.OuterCurve.Add(1.53623636213521, 0.213609421773086, 0,
                            0.12344888131996724);
      region.OuterCurve.Add(1.53969869721452, 0.210783960739988, 0,
                            0.065856551408496874);
      region.OuterCurve.Add(1.54395120588521, 0.209410203023086, 0);
      region.OuterCurve.Add(1.55078714338521, 0.208238328023086, 0);
      region.OuterCurve.Add(1.55078714338521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.49717386213521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.49717386213521, 0.208238328023086, 0);
      region.OuterCurve.Add(1.50400979963521, 0.209410203023086, 0,
                            0.06583541498497851);
      region.OuterCurve.Add(1.5082623083059, 0.210783960739988, 0,
                            0.12347510202079107);
      region.OuterCurve.Add(1.51172464338521, 0.213609421773086, 0,
                            0.10685075090332101);
      region.OuterCurve.Add(1.51355133045176, 0.218229438014121, 0,
                            0.047136914475487381);
      region.OuterCurve.Add(1.51397073713521, 0.223179734273086, 0);
      region.OuterCurve.Add(1.51397073713521, 0.266539109273086, 0,
                            0.042023112062124536);
      region.OuterCurve.Add(1.51339483282376, 0.274048747677985, 0,
                            0.057108856321478725);
      region.OuterCurve.Add(1.51182883267567, 0.279613071963353, 0,
                            0.15909422285644678);
      region.OuterCurve.Add(1.50634876865151, 0.286310036917567, 0,
                            0.1391267020452571);
      region.OuterCurve.Add(1.49795511213521, 0.288414109273086, 0,
                            0.095319088551457667);
      region.OuterCurve.Add(1.48796089568831, 0.286427437480404, 0,
                            0.065175390949939543);
      region.OuterCurve.Add(1.47910745588521, 0.281382859273086, 0,
                            0.083337139652807646);
      region.OuterCurve.Add(1.47202842457219, 0.274306162700719, 0,
                            0.15343922705848725);
      region.OuterCurve.Add(1.46895120588521, 0.264781296773086, 0);
      region.OuterCurve.Add(1.46895120588521, 0.223179734273086, 0,
                            0.047195307503079022);
      region.OuterCurve.Add(1.46937061256867, 0.218229438014121, 0,
                            0.10683840469982697);
      region.OuterCurve.Add(1.47119729963521, 0.213609421773086, 0,
                            0.12344888131996724);
      region.OuterCurve.Add(1.47465963471452, 0.210783960739988, 0,
                            0.065856551408496874);
      region.OuterCurve.Add(1.47891214338521, 0.209410203023086, 0);
      region.OuterCurve.Add(1.48574808088521, 0.208238328023086, 0);
      region.OuterCurve.Add(1.48574808088521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.43213479963521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.43213479963521, 0.208238328023086, 0);
      region.OuterCurve.Add(1.43897073713521, 0.209410203023086, 0,
                            0.06583541498497851);
      region.OuterCurve.Add(1.4432232458059, 0.210783960739988, 0,
                            0.12347510202082539);
      region.OuterCurve.Add(1.44668558088521, 0.213609421773086, 0,
                            0.10685075090332101);
      region.OuterCurve.Add(1.44851226795176, 0.218229438014121, 0,
                            0.047136914475476244);
      region.OuterCurve.Add(1.44893167463521, 0.223179734273086, 0);
      region.OuterCurve.Add(1.44893167463521, 0.266539109273086, 0,
                            0.043607568435983096);
      region.OuterCurve.Add(1.4482643742379, 0.275105480253775, 0,
                            0.10890109716114511);
      region.OuterCurve.Add(1.44522073713521, 0.283140671773086, 0,
                            0.1493580455889586);
      region.OuterCurve.Add(1.44009043386414, 0.287276435129223, 0,
                            0.097150364341478015);
      region.OuterCurve.Add(1.43359964338521, 0.288414109273086, 0,
                            0.087718827564664814);
      region.OuterCurve.Add(1.42451761213521, 0.286460984273086, 0,
                            0.068416466910947007);
      region.OuterCurve.Add(1.41494729963521, 0.281382859273086, 0,
                            0.077520065234924967);
      region.OuterCurve.Add(1.40746897352237, 0.274406034559036, 0,
                            0.16501229122573);
      region.OuterCurve.Add(1.40400979963521, 0.264781296773086, 0);
      region.OuterCurve.Add(1.40400979963521, 0.223179734273086, 0,
                            0.047195307503079022);
      region.OuterCurve.Add(1.40442920631867, 0.218229438014121, 0,
                            0.10683840469982692);
      region.OuterCurve.Add(1.40625589338521, 0.213609421773086, 0,
                            0.12344888131995398);
      region.OuterCurve.Add(1.40971822846452, 0.210783960739988, 0,
                            0.065856551408462249);
      region.OuterCurve.Add(1.41397073713521, 0.209410203023086, 0);
      region.OuterCurve.Add(1.42080667463521, 0.208238328023086, 0);
      region.OuterCurve.Add(1.42080667463521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.36719339338521, 0.201402390523086, 0);
      region.OuterCurve.Add(1.36719339338521, 0.208238328023086, 0);
      region.OuterCurve.Closed = true;

      return region;
    }

    private Region CreateBrusselsLittleA() {
      Region region = new Region();
      region.ID = 12;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(1.79766214338521, 0.219761765523086, 0,
                            0.10553018286017851);
      region.OuterCurve.Add(1.79866530924581, 0.214028380431597, 0,
                            0.31675436540121882);
      region.OuterCurve.Add(1.80361917463521, 0.210972703023086, 0,
                            0.17655587918127663);
      region.OuterCurve.Add(1.81182229963521, 0.214390671773086, 0);
      region.OuterCurve.Add(1.81690042463521, 0.208921921773086, 0,
                            -0.092691010343726482);
      region.OuterCurve.Add(1.80654886213521, 0.201597703023086, 0,
                            -0.11003545762692474);
      region.OuterCurve.Add(1.79541604963521, 0.198960984273086, 0,
                            -0.17174081007383726);
      region.OuterCurve.Add(1.78350198713521, 0.202671921773086, 0,
                            -0.1938416262144024);
      region.OuterCurve.Add(1.77783792463521, 0.213121140523086, 0,
                            -0.070688822234169016);
      region.OuterCurve.Add(1.76049012202116, 0.202878181115447, 0,
                            -0.096656974464093326);
      region.OuterCurve.Add(1.74072854963521, 0.198960984273086, 0,
                            -0.073176188743321019);
      region.OuterCurve.Add(1.72775015608304, 0.200745044624192, 0,
                            -0.095539725147092247);
      region.OuterCurve.Add(1.71611917463521, 0.206773484273086, 0,
                            -0.2384060978089774);
      region.OuterCurve.Add(1.70635354963521, 0.226988328023086, 0,
                            -0.1562043340518087);
      region.OuterCurve.Add(1.7103436797351, 0.240400619659149, 0,
                            -0.12229280501726784);
      region.OuterCurve.Add(1.72109964338521, 0.249351609273086, 0,
                            -0.048893501098937191);
      region.OuterCurve.Add(1.73470282938963, 0.254553534589306, 0,
                            -0.028702267805605631);
      region.OuterCurve.Add(1.75601913327675, 0.259180339294372, 0,
                            -0.014308068367864663);
      region.OuterCurve.Add(1.77764261213521, 0.262046921773086, 0);
      region.OuterCurve.Add(1.77764261213521, 0.268882859273086, 0,
                            0.060478458779493371);
      region.OuterCurve.Add(1.77673304723046, 0.277287655002162, 0,
                            0.13094773573175902);
      region.OuterCurve.Add(1.77285745588521, 0.284800828023086, 0,
                            0.1389306656572934);
      region.OuterCurve.Add(1.76586048617007, 0.288887480567806, 0,
                            0.068810701120236);
      region.OuterCurve.Add(1.75781839338521, 0.289878953023086, 0,
                            0.081650872156172158);
      region.OuterCurve.Add(1.74415980503801, 0.287980683782676, 0,
                            0.10449105673361903);
      region.OuterCurve.Add(1.73799127653709, 0.2844057284296, 0,
                            0.14407560373234568);
      region.OuterCurve.Add(1.73438089338521, 0.278257859273086, 0,
                            -0.12673566360404748);
      region.OuterCurve.Add(1.73046065812828, 0.270470081869107, 0,
                            -0.21253405981499196);
      region.OuterCurve.Add(1.72236917463521, 0.267222703023086, 0,
                            -0.20021032813643672);
      region.OuterCurve.Add(1.71465433088521, 0.270054734273086, 0,
                            -0.20591742810900754);
      region.OuterCurve.Add(1.71172464338521, 0.277671921773086, 0,
                            -0.20181081146960825);
      region.OuterCurve.Add(1.71591678443403, 0.286810252571622, 0,
                            -0.075131856833904775);
      region.OuterCurve.Add(1.72412698713521, 0.292613328023086, 0,
                            -0.074879838267087454);
      region.OuterCurve.Add(1.74172122996978, 0.298337549537194, 0,
                            -0.04408846313191777);
      region.OuterCurve.Add(1.76016214338521, 0.299839890523086, 0,
                            -0.050935381969477209);
      region.OuterCurve.Add(1.77376676292782, 0.298579899962819, 0,
                            -0.069154214842990336);
      region.OuterCurve.Add(1.78363849278685, 0.29516810087277, 0,
                            -0.16374704081461905);
      region.OuterCurve.Add(1.79456704601804, 0.284115124491156, 0,
                            -0.11351790040869852);
      region.OuterCurve.Add(1.79766214338521, 0.268882859273086, 0);
      region.OuterCurve.Closed = true;
      Polyline hole = new Polyline();
      hole.Add(1.77764261213521, 0.251793015523086, 0, 0.017021367509672126);
      hole.Add(1.75714365052504, 0.248705445596318, 0, 0.018059627655326479);
      hole.Add(1.74701586848318, 0.246422482247757, 0, 0.043139500532947732);
      hole.Add(1.73721292463521, 0.243003953023086, 0, 0.10652463131885551);
      hole.Add(1.73054505918075, 0.238157522714013, 0, 0.18205522094057161);
      hole.Add(1.72774026838521, 0.230406296773086, 0, 0.20374108864104279);
      hole.Add(1.73281839338521, 0.217320359273086, 0, 0.2069795952503381);
      hole.Add(1.74561136213521, 0.212144578023086, 0, 0.13606556570884895);
      hole.Add(1.76787698713521, 0.217906296773086, 0, 0.088204274892966075);
      hole.Add(1.7746208080754, 0.223529320943047, 0, 0.17449033089043875);
      hole.Add(1.77764261213521, 0.231773484273086, 0);
      hole.Closed = true;
      region.HoleCurves = new Polyline[] {hole};

      return region;
    }

    private Region CreateBrusselsBangBottom() {
      Region region = new Region();
      region.ID = 14;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(2.04844339338521, 0.213023484273086, 0,
                            -0.20710001664725541);
      region.OuterCurve.Add(2.04434183088521, 0.203062546773086, 0,
                            -0.20709754188542096);
      region.OuterCurve.Add(2.03438089338521, 0.198960984273086, 0,
                            -0.20710001664725536);
      region.OuterCurve.Add(2.02441995588521, 0.203062546773086, 0,
                            -0.20354907785080906);
      region.OuterCurve.Add(2.02041604963521, 0.213023484273086, 0,
                            -0.20701781907565472);
      region.OuterCurve.Add(2.02441995588521, 0.222886765523086, 0,
                            -0.20709754188542098);
      region.OuterCurve.Add(2.03438089338521, 0.226988328023086, 0,
                            -0.20710001664725536);
      region.OuterCurve.Add(2.04434183088521, 0.222886765523086, 0,
                            -0.21060736615621781);
      region.OuterCurve.Closed = true;

      return region;
    }

    private Region CreateBrusselsBangTop() {
      Region region = new Region();
      region.ID = 15;
      region.OuterCurve = new Polyline();
      region.OuterCurve.Add(2.02940042463521, 0.237339890523086, 0,
                            0.025616807852200954);
      region.OuterCurve.Add(2.02520120588521, 0.289781296773086, 0,
                            0.0103835630853921);
      region.OuterCurve.Add(2.02227151838521, 0.309214890523086, 0,
                            -0.0067753983293559472);
      region.OuterCurve.Add(2.02113549701837, 0.316218654480137, 0,
                            -0.029228782631419654);
      region.OuterCurve.Add(2.02041604963521, 0.323277390523086, 0,
                            -0.064114670278621458);
      region.OuterCurve.Add(2.02116139617489, 0.329648754744346, 0,
                            -0.11140906630820317);
      region.OuterCurve.Add(2.02402933088521, 0.335386765523086, 0,
                            -0.23031626983188069);
      region.OuterCurve.Add(2.03438089338521, 0.339781296773086, 0,
                            -0.0921539934042168);
      region.OuterCurve.Add(2.04005035986703, 0.338822198421695, 0,
                            -0.1285730355317242);
      region.OuterCurve.Add(2.04473245588521, 0.335484421773086, 0,
                            -0.11375295495395875);
      region.OuterCurve.Add(2.04767915878055, 0.329712641510043, 0,
                            -0.065171493326904376);
      region.OuterCurve.Add(2.04844339338521, 0.323277390523086, 0,
                            -0.029996464512886224);
      region.OuterCurve.Add(2.04678323713521, 0.309214890523086, 0,
                            0.0028872796637611524);
      region.OuterCurve.Add(2.04356058088521, 0.289781296773086, 0,
                            0.033259789573563542);
      region.OuterCurve.Add(2.03936136213521, 0.237339890523086, 0);
      region.OuterCurve.Closed = true;

      return region;
    }
  }
}
