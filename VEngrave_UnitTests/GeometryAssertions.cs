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

namespace VEngrave_UnitTests {
  public static class GeometryAssertions {
    const double TOL = 1e-8;
    // Smallest number such that 1.0 + DBL_EPSILON != 1.0, 2^-52
    public const double DBL_EPSILON = 2.2204460492503131e-016;
    public static void AssertAreApproxEqual(
      string msg, double expected, double actual) {
      Assert.AreEqual(expected, actual,
                      Math.Max(1e-12, 1e-8*Math.Max(Math.Abs(expected),
                                                    Math.Abs(actual))), msg);
    }

    public static void AssertAreApproxEqual(
        Point2F expected, Point2F actual) {
      AssertAreApproxEqual(expected, actual, TOL);
    }

    public static void AssertAreApproxEqual(
        Point2F expected, Point2F actual, double tol) {
      Assert.IsTrue(Point2F.Distance(expected, actual) <= tol,
          "Distance between expected point {0} and actual point {1} greater "
          + "than tolerance {2}", expected, actual, tol);
    }

    public static void AssertAreApproxEqual(
        Point3F expected, Point3F actual) {
      AssertAreApproxEqual(expected, actual, TOL);
    }

    public static void AssertAreApproxEqual(
        Point3F expected, Point3F actual, double tol) {
      Assert.IsTrue(Point3F.Distance(expected, actual) <= tol,
          "Distance between expected point {0} and actual point {1} greater "
          + "than tolerance {2}", expected, actual, tol);
    }

    public static void AssertAreApproxEqual(
        Triangle2F expected, Triangle2F actual) {
      AssertAreApproxEqual(expected, actual, TOL);
    }

    public static void AssertAreApproxEqual(
        Triangle2F expected, Triangle2F actual, double tol) {
      bool ok = false;
      for (int i = 0; !ok && i < 3; ++i) {
        ok = Point2F.Distance(expected.A, actual[i]) <= tol
          && Point2F.Distance(expected.B, actual[(i + 1) % 3]) <= tol
          && Point2F.Distance(expected.C, actual[(i + 2) % 3]) <= tol;
      }
      Assert.IsTrue(ok, "Difference between expected triangle ({0}, {1}, {2}) "
          + "and actual triangle ({3}, {4}, {5}) greater than tolerance {6}",
          expected.A, expected.B, expected.C,
          actual.A, actual.B, actual.C, tol);
    }

    public static void AssertAreApproxEqual(
        Triangle3F expected, Triangle3F actual) {
      AssertAreApproxEqual(expected, actual, TOL);
    }

    public static void AssertAreApproxEqual(
        Triangle3F expected, Triangle3F actual, double tol) {
      bool ok = false;
      for (int i = 0; !ok && i < 3; ++i) {
        ok = Point3F.Distance(expected.A, actual[i]) <= tol
          && Point3F.Distance(expected.B, actual[(i + 1) % 3]) <= tol
          && Point3F.Distance(expected.C, actual[(i + 2) % 3]) <= tol;
      }
      Assert.IsTrue(ok, "Difference between expected triangle ({0}, {1}, {2}) "
          + "and actual triangle ({3}, {4}, {5}) greater than tolerance {6}",
          expected.A, expected.B, expected.C,
          actual.A, actual.B, actual.C, tol);
    }
  }
}