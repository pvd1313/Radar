using System;
using UnityEngine;

using static Beast.Claw.Math.BMath;

namespace Beast.Radar.City
{
    // http://members.chello.at/easyfilter/Bresenham.pdf
    public struct BresenhamRasterizer<TPixelPlotter> 
        where TPixelPlotter : unmanaged, IPixelPlotter
    {
        public TPixelPlotter plotter;

        public void AddCubicBezier(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            /* plot any cubic Bezier curve */
            int n = 0; 
            long xc = x0 + x1 - x2 - x3;
            long xa = xc - 4 * (x1 - x2);
            long xb = x0 - x1 - x2 + x3;
            long xd = xb + 4 * (x1 + x2);
            long yc = y0 + y1 - y2 - y3;
            long ya = yc - 4 * (y1 - y2);
            long yb = y0 - y1 - y2 + y3;
            long yd = yb + 4 * (y1 + y2);

            float fx1, fx2, fx3, fy1, fy2, fy3;
            float fx0 = x0; 
            float fy0 = y0;
            float t1 = xb * xb - xa * xc, t2;

            Span<float> t = stackalloc float[5];

/* sub-divide curve at gradient sign changes */
            if (xa == 0)
            {
                /* horizontal */
                if (Abs(xc) < 2 * Abs(xb))
                {
                    /* one change */
                    t[n++] = xc / (2.0f * xb);
                } 
            }
            else if (t1 > 0f)
            {
                /* two changes */
                t2 = Sqrt(t1);
                t1 = (xb - t2) / xa;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
                t1 = (xb + t2) / xa;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
            }

            t1 = yb * yb - ya * yc;
            
            if (ya == 0)
            {
                /* vertical */
                if (Abs(yc) < 2 * Abs(yb))
                {
                    /* one change */
                    t[n++] = yc / (2.0f * yb);
                } 
            }
            else if (t1 > 0)
            {
                /* two changes */
                t2 = Sqrt(t1);
                t1 = (yb - t2) / ya;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
                t1 = (yb + t2) / ya;
                if (Abs(t1) < 1.0)
                {
                    t[n++] = t1;
                }
            }

            /* bubble sort of 4 points */
            for (int i = 1; i < n; i++)
            {
                if ((t1 = t[i - 1]) > t[i])
                {
                    t[i - 1] = t[i];
                    t[i] = t1;
                    i = 0;
                }
            }

            t1 = -1.0f;
            t[n] = 1.0f; /* begin / end point */
            for (int i = 0; i <= n; i++)
            {
                /* plot each segment separately */
                t2 = t[i]; /* sub-divide at t[i-1], t[i] */
                fx1 = (t1 * (t1 * xb - 2 * xc) - t2 * (t1 * (t1 * xa - 2 * xb) + xc) + xd) / 8 - fx0;
                fy1 = (t1 * (t1 * yb - 2 * yc) - t2 * (t1 * (t1 * ya - 2 * yb) + yc) + yd) / 8 - fy0;
                fx2 = (t2 * (t2 * xb - 2 * xc) - t1 * (t2 * (t2 * xa - 2 * xb) + xc) + xd) / 8 - fx0;
                fy2 = (t2 * (t2 * yb - 2 * yc) - t1 * (t2 * (t2 * ya - 2 * yb) + yc) + yd) / 8 - fy0;
                fx0 -= fx3 = (t2 * (t2 * (3 * xb - t2 * xa) - 3 * xc) + xd) / 8;
                fy0 -= fy3 = (t2 * (t2 * (3 * yb - t2 * ya) - 3 * yc) + yd) / 8;
                x3 = FloorToInt(fx3 + 0.5f);
                y3 = FloorToInt(fy3 + 0.5f); /* scale bounds to int */
                
                if (fx0 != 0)
                {
                    fx1 *= fx0 = (x0 - x3) / fx0;
                    fx2 *= fx0;
                }

                if (fy0 != 0)
                {
                    fy1 *= fy0 = (y0 - y3) / fy0;
                    fy2 *= fy0;
                }

                if (x0 != x3 || y0 != y3) /* segment t1 - t2 */
                {
                    AddCubicBezierSeg(x0, y0, x0 + fx1, y0 + fy1, x0 + fx2, y0 + fy2, x3, y3);
                }
                
                x0 = x3;
                y0 = y3;
                fx0 = fx3;
                fy0 = fy3;
                t1 = t2;
            }
        }

        public void AddCubicBezierSeg(int x0, int y0, float x1, float y1, float x2, float y2, int x3, int y3)
        {
            /* plot limited cubic Bezier segment */
            int f, fx, fy, leg = 1;
            int sx = x0 < x3 ? 1 : -1, sy = y0 < y3 ? 1 : -1; /* step direction */
            float xc = Abs(x0 + x1 - x2 - x3) * -1.0f;
            float xa = xc - 4 * sx * (x1 - x2);
            float xb = sx * (x0 - x1 - x2 + x3);
            float yc = Abs(y0 + y1 - y2 - y3) * -1.0f;
            float ya = yc - 4 * sy * (y1 - y2); 
            float yb = sy * (y0 - y1 - y2 + y3);

            float ab, ac, bc, cb, xx, xy, yy, dx, dy, ex, pxy; 
            float ep = 0.01f;
            
/* check for curve restrains */
/* slope P0-P1 == P2-P3 and (P0-P3 == P1-P2 or no slope change) */
            Debug.Assert((x1 - x0) * (x2 - x3) < ep && ((x3 - x0) * (x1 - x2) < ep || xb * xb < xa * xc + ep));
            Debug.Assert((y1 - y0) * (y2 - y3) < ep && ((y3 - y0) * (y1 - y2) < ep || yb * yb < ya * yc + ep));
            
            if (xa == 0 && ya == 0)
            {
                /* quadratic Bezier */
                sx = FloorToInt((3 * x1 - x0 + 1) / 2);
                sy = FloorToInt((3 * y1 - y0 + 1) / 2); /* new midpoint */
                AddQuadBezierSegment(x0, y0, sx, sy, x3, y3);
                
                return;
            }

            x1 = (x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0) + 1; /* line lengths */
            x2 = (x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3) + 1;
            
            do
            {
                /* loop over both ends */
                ab = xa * yb - xb * ya;
                ac = xa * yc - xc * ya;
                bc = xb * yc - xc * yb;
                ex = ab * (ab + ac - 3 * bc) + ac * ac; /* P0 part of self-intersection loop? */
                f = ex > 0 ? 1 : CeilToInt(1 + 1024 / x1); /* calculate resolution */
                ab *= f;
                ac *= f;
                bc *= f;
                ex *= f * f; 
                
                /* increase resolution */
                xy = 9 * (ab + ac + bc) / 8;
                cb = 8 * (xa - ya); 
                
                /* init differences of 1st degree */
                dx = 27 * (8 * ab * (yb * yb - ya * yc) + ex * (ya + 2 * yb + yc)) / 64 - ya * ya * (xy - ya);
                dy = 27 * (8 * ab * (xb * xb - xa * xc) - ex * (xa + 2 * xb + xc)) / 64 - xa * xa * (xy + xa);
                
                /* init differences of 2nd degree */
                xx = 3 * (3 * ab * (3 * yb * yb - ya * ya - 2 * ya * yc) - ya * (3 * ac * (ya + yb) + ya * cb)) / 4;
                yy = 3 * (3 * ab * (3 * xb * xb - xa * xa - 2 * xa * xc) - xa * (3 * ac * (xa + xb) + xa * cb)) / 4;
                xy = xa * ya * (6 * ab + 6 * ac - 3 * bc + cb);
                ac = ya * ya;
                cb = xa * xa;
                xy = 3 * (xy + 9 * f * (cb * yb * yc - xb * xc * ac) - 18 * xb * yb * ab) / 8;
                
                if (ex < 0)
                {
                    /* negate values if inside self-intersection loop */
                    dx = -dx;
                    dy = -dy;
                    xx = -xx;
                    yy = -yy;
                    xy = -xy;
                    ac = -ac;
                    cb = -cb;
                } /* init differences of 3rd degree */

                ab = 6 * ya * ac;
                ac = -6 * xa * ac;
                bc = 6 * ya * cb;
                cb = -6 * xa * cb;
                dx += xy;
                ex = dx + dy;
                dy += xy; /* error of 1st step */
                
                for (pxy = xy, fx = fy = f; x0 != x3 && y0 != y3;)
                {
                    /* plot curve */
                    plotter.AddPixel(x0, y0);
                    
                    do
                    {
                        /* move sub-steps of one pixel */
                        if (dx > pxy || dy < pxy)
                        {
                            /* confusing values */
                            goto exit;
                        } 
                        
                        y1 = 2.0f * ex - dy; /* save value for test of y step */
                        
                        if (2 * ex >= dx)
                        {
                            /* x sub-step */
                            fx--;
                            ex += dx += xx;
                            dy += xy += ac;
                            yy += bc;
                            xx += ab;
                        }

                        if (y1 <= 0)
                        {
                            /* y sub-step */
                            fy--;
                            ex += dy += yy;
                            dx += xy += bc;
                            xx += ac;
                            yy += cb;
                        }
                    } while (fx > 0 && fy > 0); /* pixel complete? */

                    if (2 * fx <= f)
                    {
                        x0 += sx;
                        fx += f;
                    } /* x step */

                    if (2 * fy <= f)
                    {
                        y0 += sy;
                        fy += f;
                    } /* y step */

                    if (Approximately(pxy, xy) && dx < 0 && dy > 0)
                    {
                        /* pixel ahead valid */
                        pxy = ep;
                    } 
                }

                exit:
                xx = x0;
                x0 = x3;
                x3 = FloorToInt(xx);
                sx = -sx;
                xb = -xb; /* swap legs */
                yy = y0;
                y0 = y3;
                y3 = FloorToInt(yy);
                sy = -sy;
                yb = -yb;
                x1 = x2;
                
            } while (leg-- > 0); /* try other end */

            AddLine(x0, y0, x3, y3); /* remaining part in case of cusp or crunode */
        }

        public void AddQuadBezier(int x0, int y0, int x1, int y1, int x2, int y2)
        { /* plot any quadratic Bezier curve */
            int x = x0 - x1;
            int y = y0 - y1;
            float t = x0 - 2 * x1 + x2;
            float r;
            
            if ((long)x * (x2 - x1) > 0)
            {
                /* horizontal cut at P4? */
                if ((long)y * (y2 - y1) > 0) /* vertical cut at P6 too? */
                {
                    if (Abs((y0 - 2.0f * y1 + y2) / t * x) > Abs(y))
                    {
                        /* which first? */
                        x0 = x2;
                        x2 = x + x1;
                        y0 = y2;
                        y2 = y + y1; /* swap points */
                    } /* now horizontal cut at P4 comes first */
                }

                t = (x0 - x1) / t;
                r = (1 - t) * ((1 - t) * y0 + 2.0f * t * y1) + t * t * y2; /* By(t=P4) */
                t = (x0 * x2 - x1 * x1) * t / (x0 - x1); /* gradient dP4/dx=0 */
                x = FloorToInt(t + 0.5f);
                y = FloorToInt(r + 0.5f);
                r = (y1 - y0) * (t - x0) / (x1 - x0) + y0; /* intersect P3 | P0 P1 */
                
                AddQuadBezierSegment(x0, y0, x, FloorToInt(r + 0.5f), x, y);
                
                r = (y1 - y2) * (t - x2) / (x1 - x2) + y2; /* intersect P4 | P1 P2 */
                x0 = x1 = x;
                y0 = y;
                y1 = FloorToInt(r + 0.5f); /* P0 = P4, P1 = P8 */
            }

            if ((long)(y0 - y1) * (y2 - y1) > 0)
            {
                /* vertical cut at P6? */
                t = y0 - 2 * y1 + y2;
                t = (y0 - y1) / t;
                r = (1 - t) * ((1 - t) * x0 + 2.0f * t * x1) + t * t * x2; /* Bx(t=P6) */
                t = (y0 * y2 - y1 * y1) * t / (y0 - y1); /* gradient dP6/dy=0 */
                x = FloorToInt(r + 0.5f);
                y = FloorToInt(t + 0.5f);
                r = (x1 - x0) * (t - y0) / (y1 - y0) + x0; /* intersect P6 | P0 P1 */
                
                AddQuadBezierSegment(x0, y0, FloorToInt(r + 0.5f), y, x, y);
                
                r = (x1 - x2) * (t - y2) / (y1 - y2) + x2; /* intersect P7 | P1 P2 */
                x0 = x;
                x1 = FloorToInt(r + 0.5f);
                y0 = y1 = y; /* P0 = P6, P1 = P7 */
            }

            AddQuadBezierSegment(x0, y0, x1, y1, x2, y2); /* remaining part */
        }

        public void AddQuadBezierSegment(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            /* plot a limited quadratic Bezier segment */
            int sx = x2 - x1, sy = y2 - y1;
            long xx = x0 - x1, yy = y0 - y1, xy; /* relative values for checks */
            double dx, dy, err, cur = xx * sy - yy * sx; /* curvature */
            
            Debug.Assert(xx * sx <= 0 && yy * sy <= 0); /* sign of gradient must not change */
            
            if (sx * (long)sx + sy * (long)sy > xx * xx + yy * yy)
            {
                /* begin with longer part */
                x2 = x0;
                x0 = sx + x1;
                y2 = y0;
                y0 = sy + y1;
                cur = -cur; /* swap P0 P2 */
            }

            if (cur != 0)
            {
                /* no straight line */
                xx += sx;
                xx *= sx = x0 < x2 ? 1 : -1; /* x step direction */
                yy += sy;
                yy *= sy = y0 < y2 ? 1 : -1; /* y step direction */
                xy = 2 * xx * yy;
                xx *= xx;
                yy *= yy; /* differences 2nd degree */
                
                if (cur * sx * sy < 0)
                {
                    /* negated curvature? */
                    xx = -xx;
                    yy = -yy;
                    xy = -xy;
                    cur = -cur;
                }

                dx = 4.0 * sy * cur * (x1 - x0) + xx - xy; /* differences 1st degree */
                dy = 4.0 * sx * cur * (y0 - y1) + yy - xy;
                xx += xx;
                yy += yy;
                err = dx + dy + xy; /* error 1st step */
                
                do
                {
                    /* plot curve */
                    plotter.AddPixel(x0, y0);
                    
                    if (x0 == x2 && y0 == y2)
                    {
                        return;
                    } /* last pixel -> curve finished */
                    
                    y1 = 2 * err < dx ? 1 : 0; /* save value for test of y step */
                    
                    if (2 * err > dy)
                    {
                        x0 += sx;
                        dx -= xy;
                        err += dy += yy;
                    } /* x step */

                    if (y1 > 0)
                    {
                        y0 += sy;
                        dy -= xy;
                        err += dx += xx;
                    } /* y step */
                } while (dy < 0 && dx > 0); /* gradient negates -> algorithm fails */
            }

            AddLine(x0, y0, x2, y2); /* plot remaining part to end */
        }

        public void AddLine(int x0, int y0, int x1, int y1)
        {
            int dx = Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = Abs(y1 - y0) * -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int e2; /* error value e_xy */
            
            for (;;)
            {
                plotter.AddPixel(x0, y0);
                
                e2 = 2 * err;
                
                if (e2 >= dy)
                {
                    /* e_xy+e_x > 0 */
                    if (x0 == x1)
                    {
                        break;
                    }
                    
                    err += dy;
                    x0 += sx;
                }

                if (e2 <= dx)
                {
                    /* e_xy+e_y < 0 */
                    if (y0 == y1)
                    {
                        break;
                    }
                    
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}