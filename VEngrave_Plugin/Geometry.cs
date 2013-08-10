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

[assembly: InternalsVisibleTo("VEngrave_UnitTests")]

namespace VEngraveForCamBam {

  using CamBamExtensions;

  public static class Geometry {
    // Smallest number such that 1.0 + DBL_EPSILON != 1.0, 2^-52
    public const double DBL_EPSILON = 2.2204460492503131e-016;
    // Radians per degree
    public const double DEGREES = Math.PI/180;

    public static Point2F Add(Point2F p, Vector2F v) {
      return new Point2F(p.X + v.X, p.Y + v.Y);
    }
    public static Vector2F Add(Vector2F a, Vector2F b) {
      return new Vector2F(a.X + b.X, a.Y + b.Y);
    }
    public static Point2F Subtract(Point2F p, Vector2F v) {
      return new Point2F(p.X - v.X, p.Y - v.Y);
    }
    public static Vector2F Subtract(Vector2F p, Vector2F v) {
      return new Vector2F(p.X - v.X, p.Y - v.Y);
    }
    public static Vector2F Multiply(double f, Vector2F v) {
      return new Vector2F(f*v.X, f*v.Y);
    }
    public static Vector2F Multiply(Vector2F v, double f) {
      return Multiply(f, v);
    }
    public static Vector2F Divide(Vector2F v, double d) {
      return new Vector2F(v.X/d, v.Y/d);
    }
    public static Point3F Add(Point3F p, Vector3F v) {
      return new Point3F(p.X + v.X, p.Y + v.Y, p.Z + v.Z);
    }
    public static Vector3F Add(Vector3F a, Vector3F b) {
      return new Vector3F(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }
    public static Point3F Subtract(Point3F p, Vector3F v) {
      return new Point3F(p.X - v.X, p.Y - v.Y, p.Z - v.Z);
    }
    public static Vector3F Subtract(Vector3F a, Vector3F b) {
      return new Vector3F(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }
    public static Vector3F Multiply(double f, Vector3F v) {
      return new Vector3F(f*v.X, f*v.Y, f*v.Z);
    }
    public static Vector3F Multiply(Vector3F v, double f) {
      return Multiply(f, v);
    }
    public static Vector3F Divide(Vector3F v, double d) {
      return new Vector3F(v.X/d, v.Y/d, v.Z/d);
    }

    public static double RadiusToLineSegment(
      Point2F position, Vector2F normal, Point2F start, Point2F end) {
      double radius;
      Vector2F dse = new Vector2F(start, end);
      double len = dse.Length;
      if (len < DBL_EPSILON) {
        return Math.Min(RadiusToEndPoint(position, normal, start),
                        RadiusToEndPoint(position, normal, end));
      }
      // Normal points to the right, i.e. outside for a CCW curve.
      Vector2F normal2 = dse.Normal().Unit();

      // Figure out which side of the (start,end) vector we're on from
      // the cross product of (start->position)x(start->end).
      // + is on the right.
      Vector2F dsp = new Vector2F(start, position);
      double crossProduct = Vector2F.Determinant(dsp, dse);

      // If the cross product is zero we're either touching
      // the line or the radius will be infinite.  Some
      // extra slop is needed in the cross product because
      // of the subtraction.
      if (Math.Abs(crossProduct) < 3*DBL_EPSILON) {
        if (Math.Min(start.X, end.X) - 2*DBL_EPSILON <= position.X
            && position.X <= Math.Max(start.X, end.X) + 2*DBL_EPSILON
            && Math.Min(start.Y, end.Y) - 2*DBL_EPSILON <= position.Y
            && position.Y <= Math.Max(start.Y, end.Y) + 2*DBL_EPSILON) {
          return 0;
        } else {
          return Double.MaxValue;
        }
      }

      // and adjust normal2 to point in that direction
      normal2 = Multiply(normal2, Math.Sign(crossProduct));

      // NB: Normal must be pointing in the right direction, neg radius
      // means line segment is on the other side and irrelevant
      // Calculate r such that current + r*(normal-normal2) is on
      // (end - start)*t + start
      Vector2F du = Subtract(normal2, normal);
      double denominator = Vector2F.Determinant(du, dse);
      // radius will be ~infinite
      if (Math.Abs(denominator) < DBL_EPSILON) {
        return Double.MaxValue;
      }
      radius = Vector2F.Determinant(dsp, dse)/denominator;
      // line = (end-start)*tline + start
      double tline = Vector2F.Determinant(du, dsp)/denominator;
      if (tline < 0.0) {
        radius = RadiusToEndPoint(position, normal, start);
      } else if (1.0 < tline) {
        radius = RadiusToEndPoint(position, normal, end);
      }
      return radius < 0 ? Double.MaxValue : radius;
    }

    public static double RadiusToEndPoint(Point2F position, Vector2F normal,
                                          Point2F endpoint) {
      Vector2F dpe = new Vector2F(position, endpoint);
      // Empirically determined from jitter testing.
      if (dpe.Length < 3*DBL_EPSILON) {
        return 0;
      }
      Vector2F unitToEP = dpe.Unit();
      // cos angle between unit and normal is the dot product
      double cosTheta = Vector2F.DotProduct(unitToEP, normal);
      if (Math.Abs(cosTheta) < DBL_EPSILON) {
        return Double.MaxValue;
      }
      // leg of triangle is 1/2 the distance, hypotenuse is the
      // leg/cos theta
      double radius = (dpe.Length/2) / cosTheta;
      // TODO: this comes up very slightly <0 for problem corner
      return radius < 0 ? Double.MaxValue : radius;
    }

    public static double ConvertBulgeToArc(Point2F start, Point2F end,
                                           double bulge, out Point2F center) {
      // Precondition: Math.Abs(bulge) >= DBL_EPSILON
      // NB: this does the correct thing for negative bulges
      Vector2F dse = new Vector2F(start, end);
      double b2m1 = bulge*bulge - 1;
      double arcRadius = Math.Abs((bulge*bulge + 1)*dse.Length/(4*bulge));
      center = new Point2F((start.X + end.X)/2 + (b2m1*dse.Y)/(4*bulge),
                           (start.Y + end.Y)/2 - (b2m1*dse.X)/(4*bulge));
      return arcRadius;
    }

    public static double RadiusToArc(Point2F position, Vector2F normal,
                                     Point2F start, Point2F end,
                                     double bulge) {
      // Figure out which side of the (start,end) vector we're on from
      // the cross product of (start->current)x(start->end).
      // + is on the right.
      var darc = new Vector2F(start, end);
      var dsp = new Vector2F(start, position);
      // Precondition: Math.Abs(bulge) >= DBL_EPSILON
      // Compute arc center and radius
      Point2F arcCenter;
      double arcRadius = ConvertBulgeToArc(start, end, bulge, out arcCenter);
      // and from that the cross products of the vectors from the center
      // to start and position, and to the position and end
      Vector2F dpc = new Vector2F(position, arcCenter);
      // Compute tangent circle
      double radius;

      // Common values
      double normalDotCenterMinusPos = Vector2F.DotProduct(normal, dpc);
      double distanceToCenter = dpc.Length;

      int directionToContactPoint;    // +1 to towards arcCenter, -1 if away
      Point2F contactPoint;
      if (Math.Abs(distanceToCenter - arcRadius) < DBL_EPSILON) {
        // then the point is on the circle defined by the arc.
        directionToContactPoint = 0;
        radius = 0.0; // Unconstrained if normal is perpendular to arc
        contactPoint = position;
      } else {
        double distanceToCenter2 = distanceToCenter*distanceToCenter;
        if (distanceToCenter2 < arcRadius*arcRadius) {
          // then we're inside the circle and have the concave case
          double denom = 2*(arcRadius - normalDotCenterMinusPos);
          // normal . (C-P) < |C-P| < Ra, ergo denom != 0
          radius = (arcRadius*arcRadius - distanceToCenter2)/denom;
          directionToContactPoint = -1;
        } else {
          // We're outside the circle of the arc and have the convex case.
          double denom = 2*(arcRadius + normalDotCenterMinusPos);
          // When we're on a line tangent to the arc there is no solution.
          if (Math.Abs(denom) < DBL_EPSILON) {
            return Double.MaxValue;
          }
          radius = (distanceToCenter2 - arcRadius*arcRadius)/denom;
          directionToContactPoint = +1;
        }

        // At this point, figure out the contact point with the arc's circle.
        // This is then used to determine if we actually contact the arc or
        // if we contact an endpoint.
        Point2F tangentCircleCenter = new Point2F(position.X + radius*normal.X,
                                                  position.Y + radius*normal.Y);
        Vector2F utcc2ac = new Vector2F(tangentCircleCenter, arcCenter).Unit();
        contactPoint = new Point2F(
          tangentCircleCenter.X + radius*directionToContactPoint*utcc2ac.X,
          tangentCircleCenter.Y + radius*directionToContactPoint*utcc2ac.Y);
      }
      // Sign((CP-S)x(E-S)) == Sign(bulge) is true if the contact point is on
      // the same side of the start->end vector as the bulge, but that
      // test is unreliable since CP-S can be very small and its direction
      // becomes indeterminate.
      var startToCP = new Vector2F(start, contactPoint);
      if (Math.Sign(Vector2F.Determinant(startToCP, darc))
          == Math.Sign(bulge)) {
      // Testing direction from the arc center doesn't
      // require such small differences, but the cross-products flip sign for
      // angles > 180.
      //var centerStart = new Vector2F(arcCenter, start);
      //var centerContactPoint = new Vector2F(arcCenter, contactPoint);
      //var centerEnd = new Vector2F(arcCenter, end);
      //var dirStartContactPoint = Math.Sign(
      //    Vector2F.Determinant(centerStart, centerContactPoint));
      //var dirStartEnd = Math.Sign(
      //    Vector2F.Determinant(centerStart, centerEnd));
      //var dirContactPointEnd = Math.Sign(
      //    Vector2F.Determinant(centerContactPoint, centerEnd));
      //if (dirStartContactPoint == dirStartEnd
      //    && dirContactPointEnd == dirStartEnd)  {
        // Contact point is between the arc's endpoint, check that
        // we're on the correct side of the position
        if (radius < 0) {
          radius = Double.MaxValue;
        }
      } else {
        // TODO: this is causing a problem in sharp corners
        double rstart = RadiusToEndPoint(position, normal, start);
        double rend   = RadiusToEndPoint(position, normal, end);
        // Take closest point on the correct side
        if (0 <= rstart && rstart <= rend) {
          radius = rstart;
        } else if (0 <= rend && rend <= rstart) {
          radius = rend;
        } else {    // Otherwise the arc is irrelevant
          radius = Double.MaxValue;
        }
      }
      return radius;
    }

    public enum CornerType { SharpInside, SmoothInside, Outside, NotACorner }

    public static CornerType GetCornerType(Polyline line, int i,
                                           double threshold, bool leftIsInside,
                                           out Vector2F nextNormal) {
      int previ = line.PrevSegment(i);
      int nexti = line.NextSegment(i);
      if (previ < 0 || nexti < 0) {
        nextNormal = Vector2F.Undefined;
        return CornerType.NotACorner;
      }
      return GetCornerType(line.Points[previ], line.Points[i],
                           line.Points[nexti], threshold,
                           leftIsInside, out nextNormal);
    }

    public static Vector2F GetDirection(PolylineItem startItem,
                                        PolylineItem endItem,
                                        Point2F point) {
      Point2F start = startItem.Point.To2D();
      double bulge = startItem.Bulge;
      Point2F end = endItem.Point.To2D();
      Vector2F direction;

      if (Math.Abs(bulge) < DBL_EPSILON) {
        direction = new Vector2F(start, end);
      } else {
        Point2F center;
        ConvertBulgeToArc(start, end, bulge, out center);
        // normal returns vector to the right
        direction = new Vector2F(center, point).Normal();
        if (bulge > 0) {
          direction.Invert();
        }
      }
      return direction;
    }

    public static CornerType GetCornerType(PolylineItem prevPoint,
                                           PolylineItem cornerPoint,
                                           PolylineItem nextPoint,
                                           double threshold,
                                           bool leftIsInside,
                                           out Vector2F nextNormal) {
      double limitingCos = -Math.Cos(threshold);
      Vector2F invector = GetDirection(prevPoint, cornerPoint,
                                       cornerPoint.Point.To2D());
      Vector2F outvector = GetDirection(cornerPoint, nextPoint,
                                        cornerPoint.Point.To2D());
      nextNormal = outvector.Normal().Unit();
      if (leftIsInside) {
        nextNormal.Invert();
      }
      if ((leftIsInside ? -1 : 1)
          *Vector2F.Determinant(invector, outvector) > 0) {
        return CornerType.Outside;
      } else if (Vector2F.DotProduct(invector.Unit(),
                                     outvector.Unit()) < limitingCos) {
        return CornerType.SharpInside;
      } else {
        return CornerType.SmoothInside;
      }
    }

    public static Vector2F Bisect(Vector2F a, Vector2F b) {
      Vector2F sum = Add(a.Unit(), b.Unit());
      if (Math.Abs(sum.Length) == 0) {
        // opposite directions, return orthogonal vector
        return a.Normal().Unit();
      }
      return sum.Unit();
    }

    public static Vector3F Bisect(Vector3F a, Vector3F b) {
      return Add(a.Unit(), b.Unit()).Unit();
    }
    // See http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
    public static double DistanceFromLineToPoint(Point3F origin,
                                                 Vector3F direction,
                                                 Point3F point) {
      Vector3F unitDirection = direction.Unit();
      Vector3F diagonal = new Vector3F(point, origin);    // a - p
      Vector3F projectionOnLine = Multiply(               // ((a-p).n)n
        Vector3F.DotProduct(diagonal, unitDirection), unitDirection);
      Vector3F orthogonal = Subtract(diagonal, projectionOnLine);
      return orthogonal.Length;
    }

    public static bool Colinear(Point3F a, Point3F b, Point3F c, double tol) {
      return DistanceFromLineToPoint(a, new Vector3F(a, b), c) < tol;
    }

    public static bool PointBetween(
        Point3F a, Point3F b, Point3F c, double tol) {
      Vector3F ac = new Vector3F(a, c);
      Vector3F unitAC = ac.Unit();
      double projectedDistance
          = Vector3F.DotProduct(unitAC, new Vector3F(a, b));
      if (projectedDistance < 0 || projectedDistance > ac.Length) {
        return false;
      }
      Point3F nearestPoint = Add(a, Multiply(projectedDistance, unitAC));
      return Point3F.Distance(nearestPoint, b) < tol;
    }

    public static bool PointOnRay(Point3F point, Point3F origin,
                                  Point3F pointOnRay, double tol) {
      return PointOnRay(point, origin, new Vector3F(origin, pointOnRay), tol);
    }

    public static bool PointOnRay(Point3F point, Point3F origin,
                                  Vector3F direction, double tol) {
      Vector3F unitDirection = direction.Unit();
      double projectedDistance = Vector3F.DotProduct(
        unitDirection, new Vector3F(origin, point));
      if (projectedDistance < 0) {
        return false;
      }
      Point3F nearestPoint = Add(origin, Multiply(projectedDistance,
                                                  unitDirection));
      return Point3F.Distance(nearestPoint, point) < tol;
    }

    public static double NormalizeAngularInterval(
        int direction, double startTheta, double endTheta) {
      // Adjust angle to be continuous & < 360
      while (direction*endTheta < direction*startTheta) {
        endTheta += direction*360*DEGREES;
      }
      while (direction*(endTheta - startTheta) > 360*DEGREES) {
        endTheta -= direction*360*DEGREES;
      }
      return endTheta;
    }
  }
}