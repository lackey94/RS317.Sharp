﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Rs317.Sharp
{
	//Based on Bubletan's release here: https://www.rune-server.ee/runescape-development/rs2-client/snippets/547675-smooth-shading.html
	public sealed partial class Rasterizer : DrawingArea
	{
		public static void drawHDGouraudScanline(int[] dest, int offset, int x1, int x2, int r1, int g1, int b1, int r2, int g2, int b2)
		{
			int n = x2 - x1;
			if(n <= 0)
			{
				return;
			}
			r2 = (r2 - r1) / n;
			g2 = (g2 - g1) / n;
			b2 = (b2 - b1) / n;
			if(restrictEdges)
			{
				if(x2 > DrawingArea.centerX)
				{
					n -= x2 - DrawingArea.centerX;
					x2 = DrawingArea.centerX;
				}
				if(x1 < 0)
				{
					n = x2;
					r1 -= x1 * r2;
					g1 -= x1 * g2;
					b1 -= x1 * b2;
					x1 = 0;
				}
			}
			if(x1 < x2)
			{
				offset += x1;
				if(alpha == 0)
				{
					while(--n >= 0)
					{
						dest[offset] = (r1 & 0xff0000) | (g1 >> 8 & 0xff00) | (b1 >> 16 & 0xff);
						r1 += r2;
						g1 += g2;
						b1 += b2;
						offset++;
					}
				}
				else
				{
					int a1 = alpha;
					int a2 = 256 - alpha;
					int rgb;
					int dst;
					while(--n >= 0)
					{
						rgb = (r1 & 0xff0000) | (g1 >> 8 & 0xff00) | (b1 >> 16 & 0xff);
						rgb = ((rgb & 0xff00ff) * a2 >> 8 & 0xff00ff) + ((rgb & 0xff00) * a2 >> 8 & 0xff00);
						dst = dest[offset];
						dest[offset] = rgb + ((dst & 0xff00ff) * a1 >> 8 & 0xff00ff) + ((dst & 0xff00) * a1 >> 8 & 0xff00);
						r1 += r2;
						g1 += g2;
						b1 += b2;
						offset++;
					}
				}
			}
		}

		public static unsafe void drawHDTexturedTriangle(int y1, int y2, int y3, int x1, int x2, int x3, int l1, int l2,
						int l3, int tx1, int tx2, int tx3, int ty1, int ty2, int ty3,
						int tz1, int tz2, int tz3, int textureId)
		{
			EnsureTextRowIsIniitalized(textureId);

			fixed (int* texelCachePointer = texelCache)
			{
				int* texturePtr = textureId * HIGH_MEMORY_TEXEL_WIDTH + texelCachePointer;
				DrawHDGouraudTexturedTriangleWithSpan(texturePtr, y1, y2, y3, x1, x2, x3, l1, l2, l3, tx1, tx2, tx3, ty1, ty2, ty3, tz1, tz2, tz3, textureId);
			}
		}

		private unsafe static void DrawHDGouraudTexturedTriangleWithSpan(int* texturePtr, int y1, int y2, int y3, int x1, int x2, int x3, int l1, int l2, int l3, int tx1, int tx2, int tx3, int ty1, int ty2, int ty3, int tz1, int tz2, int tz3, int textureId)
		{
			l1 = 0x7f - l1 << 1;
			l2 = 0x7f - l2 << 1;
			l3 = 0x7f - l3 << 1;
			opaque = !transparent[textureId];
			tx2 = tx1 - tx2;
			ty2 = ty1 - ty2;
			tz2 = tz1 - tz2;
			tx3 -= tx1;
			ty3 -= ty1;
			tz3 -= tz1;
			int l4 = tx3 * ty1 - ty3 * tx1 << 14;
			int i5 = ty3 * tz1 - tz3 * ty1 << 8;
			int j5 = tz3 * tx1 - tx3 * tz1 << 5;
			int k5 = tx2 * ty1 - ty2 * tx1 << 14;
			int l5 = ty2 * tz1 - tz2 * ty1 << 8;
			int i6 = tz2 * tx1 - tx2 * tz1 << 5;
			int j6 = ty2 * tx3 - tx2 * ty3 << 14;
			int k6 = tz2 * ty3 - ty2 * tz3 << 8;
			int l6 = tx2 * tz3 - tz2 * tx3 << 5;
			int i7 = 0;
			int j7 = 0;
			if (y2 != y1)
			{
				i7 = (x2 - x1 << 16) / (y2 - y1);
				j7 = (l2 - l1 << 16) / (y2 - y1);
			}

			int k7 = 0;
			int l7 = 0;
			if (y3 != y2)
			{
				k7 = (x3 - x2 << 16) / (y3 - y2);
				l7 = (l3 - l2 << 16) / (y3 - y2);
			}

			int i8 = 0;
			int j8 = 0;
			if (y3 != y1)
			{
				i8 = (x1 - x3 << 16) / (y1 - y3);
				j8 = (l1 - l3 << 16) / (y1 - y3);
			}

			if (y1 <= y2 && y1 <= y3)
			{
				if (y1 >= DrawingArea.bottomY)
					return;
				if (y2 > DrawingArea.bottomY)
					y2 = DrawingArea.bottomY;
				if (y3 > DrawingArea.bottomY)
					y3 = DrawingArea.bottomY;
				if (y2 < y3)
				{
					x3 = x1 <<= 16;
					l3 = l1 <<= 16;
					if (y1 < 0)
					{
						x3 -= i8 * y1;
						x1 -= i7 * y1;
						l3 -= j8 * y1;
						l1 -= j7 * y1;
						y1 = 0;
					}

					x2 <<= 16;
					l2 <<= 16;
					if (y2 < 0)
					{
						x2 -= k7 * y2;
						l2 -= l7 * y2;
						y2 = 0;
					}

					int k8 = y1 - centreY;
					l4 += j5 * k8;
					k5 += i6 * k8;
					j6 += l6 * k8;
					if (y1 != y2 && i8 < i7 || y1 == y2 && i8 > k7)
					{
						y3 -= y2;
						y2 -= y1;
						y1 = lineOffsets[y1];
						while (--y2 >= 0)
						{
							drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x3 >> 16, x1 >> 16, l3, l1, l4, k5, j6, i5, l5, k6);
							x3 += i8;
							x1 += i7;
							l3 += j8;
							l1 += j7;
							y1 += DrawingArea.width;
							l4 += j5;
							k5 += i6;
							j6 += l6;
						}

						while (--y3 >= 0)
						{
							drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x3 >> 16, x2 >> 16, l3, l2, l4, k5, j6, i5, l5, k6);
							x3 += i8;
							x2 += k7;
							l3 += j8;
							l2 += l7;
							y1 += DrawingArea.width;
							l4 += j5;
							k5 += i6;
							j6 += l6;
						}

						return;
					}

					y3 -= y2;
					y2 -= y1;
					y1 = lineOffsets[y1];
					while (--y2 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x1 >> 16, x3 >> 16, l1, l3, l4, k5, j6, i5, l5, k6);
						x3 += i8;
						x1 += i7;
						l3 += j8;
						l1 += j7;
						y1 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					while (--y3 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x2 >> 16, x3 >> 16, l2, l3, l4, k5, j6, i5, l5, k6);
						x3 += i8;
						x2 += k7;
						l3 += j8;
						l2 += l7;
						y1 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					return;
				}

				x2 = x1 <<= 16;
				l2 = l1 <<= 16;
				if (y1 < 0)
				{
					x2 -= i8 * y1;
					x1 -= i7 * y1;
					l2 -= j8 * y1;
					l1 -= j7 * y1;
					y1 = 0;
				}

				x3 <<= 16;
				l3 <<= 16;
				if (y3 < 0)
				{
					x3 -= k7 * y3;
					l3 -= l7 * y3;
					y3 = 0;
				}

				int l8 = y1 - centreY;
				l4 += j5 * l8;
				k5 += i6 * l8;
				j6 += l6 * l8;
				if (y1 != y3 && i8 < i7 || y1 == y3 && k7 > i7)
				{
					y2 -= y3;
					y3 -= y1;
					y1 = lineOffsets[y1];
					while (--y3 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x2 >> 16, x1 >> 16, l2, l1, l4, k5, j6, i5, l5, k6);
						x2 += i8;
						x1 += i7;
						l2 += j8;
						l1 += j7;
						y1 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					while (--y2 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x3 >> 16, x1 >> 16, l3, l1, l4, k5, j6, i5, l5, k6);
						x3 += k7;
						x1 += i7;
						l3 += l7;
						l1 += j7;
						y1 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					return;
				}

				y2 -= y3;
				y3 -= y1;
				y1 = lineOffsets[y1];
				while (--y3 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x1 >> 16, x2 >> 16, l1, l2, l4, k5, j6, i5, l5, k6);
					x2 += i8;
					x1 += i7;
					l2 += j8;
					l1 += j7;
					y1 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				while (--y2 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y1, x1 >> 16, x3 >> 16, l1, l3, l4, k5, j6, i5, l5, k6);
					x3 += k7;
					x1 += i7;
					l3 += l7;
					l1 += j7;
					y1 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				return;
			}

			if (y2 <= y3)
			{
				if (y2 >= DrawingArea.bottomY)
					return;
				if (y3 > DrawingArea.bottomY)
					y3 = DrawingArea.bottomY;
				if (y1 > DrawingArea.bottomY)
					y1 = DrawingArea.bottomY;
				if (y3 < y1)
				{
					x1 = x2 <<= 16;
					l1 = l2 <<= 16;
					if (y2 < 0)
					{
						x1 -= i7 * y2;
						x2 -= k7 * y2;
						l1 -= j7 * y2;
						l2 -= l7 * y2;
						y2 = 0;
					}

					x3 <<= 16;
					l3 <<= 16;
					if (y3 < 0)
					{
						x3 -= i8 * y3;
						l3 -= j8 * y3;
						y3 = 0;
					}

					int i9 = y2 - centreY;
					l4 += j5 * i9;
					k5 += i6 * i9;
					j6 += l6 * i9;
					if (y2 != y3 && i7 < k7 || y2 == y3 && i7 > i8)
					{
						y1 -= y3;
						y3 -= y2;
						y2 = lineOffsets[y2];
						while (--y3 >= 0)
						{
							drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x1 >> 16, x2 >> 16, l1, l2, l4, k5, j6, i5, l5, k6);
							x1 += i7;
							x2 += k7;
							l1 += j7;
							l2 += l7;
							y2 += DrawingArea.width;
							l4 += j5;
							k5 += i6;
							j6 += l6;
						}

						while (--y1 >= 0)
						{
							drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x1 >> 16, x3 >> 16, l1, l3, l4, k5, j6, i5, l5, k6);
							x1 += i7;
							x3 += i8;
							l1 += j7;
							l3 += j8;
							y2 += DrawingArea.width;
							l4 += j5;
							k5 += i6;
							j6 += l6;
						}

						return;
					}

					y1 -= y3;
					y3 -= y2;
					y2 = lineOffsets[y2];
					while (--y3 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x2 >> 16, x1 >> 16, l2, l1, l4, k5, j6, i5, l5, k6);
						x1 += i7;
						x2 += k7;
						l1 += j7;
						l2 += l7;
						y2 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					while (--y1 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x3 >> 16, x1 >> 16, l3, l1, l4, k5, j6, i5, l5, k6);
						x1 += i7;
						x3 += i8;
						l1 += j7;
						l3 += j8;
						y2 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					return;
				}

				x3 = x2 <<= 16;
				l3 = l2 <<= 16;
				if (y2 < 0)
				{
					x3 -= i7 * y2;
					x2 -= k7 * y2;
					l3 -= j7 * y2;
					l2 -= l7 * y2;
					y2 = 0;
				}

				x1 <<= 16;
				l1 <<= 16;
				if (y1 < 0)
				{
					x1 -= i8 * y1;
					l1 -= j8 * y1;
					y1 = 0;
				}

				int j9 = y2 - centreY;
				l4 += j5 * j9;
				k5 += i6 * j9;
				j6 += l6 * j9;
				if (i7 < k7)
				{
					y3 -= y1;
					y1 -= y2;
					y2 = lineOffsets[y2];
					while (--y1 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x3 >> 16, x2 >> 16, l3, l2, l4, k5, j6, i5, l5, k6);
						x3 += i7;
						x2 += k7;
						l3 += j7;
						l2 += l7;
						y2 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					while (--y3 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x1 >> 16, x2 >> 16, l1, l2, l4, k5, j6, i5, l5, k6);
						x1 += i8;
						x2 += k7;
						l1 += j8;
						l2 += l7;
						y2 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					return;
				}

				y3 -= y1;
				y1 -= y2;
				y2 = lineOffsets[y2];
				while (--y1 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x2 >> 16, x3 >> 16, l2, l3, l4, k5, j6, i5, l5, k6);
					x3 += i7;
					x2 += k7;
					l3 += j7;
					l2 += l7;
					y2 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				while (--y3 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y2, x2 >> 16, x1 >> 16, l2, l1, l4, k5, j6, i5, l5, k6);
					x1 += i8;
					x2 += k7;
					l1 += j8;
					l2 += l7;
					y2 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				return;
			}

			if (y3 >= DrawingArea.bottomY)
				return;
			if (y1 > DrawingArea.bottomY)
				y1 = DrawingArea.bottomY;
			if (y2 > DrawingArea.bottomY)
				y2 = DrawingArea.bottomY;
			if (y1 < y2)
			{
				x2 = x3 <<= 16;
				l2 = l3 <<= 16;
				if (y3 < 0)
				{
					x2 -= k7 * y3;
					x3 -= i8 * y3;
					l2 -= l7 * y3;
					l3 -= j8 * y3;
					y3 = 0;
				}

				x1 <<= 16;
				l1 <<= 16;
				if (y1 < 0)
				{
					x1 -= i7 * y1;
					l1 -= j7 * y1;
					y1 = 0;
				}

				int k9 = y3 - centreY;
				l4 += j5 * k9;
				k5 += i6 * k9;
				j6 += l6 * k9;
				if (k7 < i8)
				{
					y2 -= y1;
					y1 -= y3;
					y3 = lineOffsets[y3];
					while (--y1 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x2 >> 16, x3 >> 16, l2, l3, l4, k5, j6, i5, l5, k6);
						x2 += k7;
						x3 += i8;
						l2 += l7;
						l3 += j8;
						y3 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					while (--y2 >= 0)
					{
						drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x2 >> 16, x1 >> 16, l2, l1, l4, k5, j6, i5, l5, k6);
						x2 += k7;
						x1 += i7;
						l2 += l7;
						l1 += j7;
						y3 += DrawingArea.width;
						l4 += j5;
						k5 += i6;
						j6 += l6;
					}

					return;
				}

				y2 -= y1;
				y1 -= y3;
				y3 = lineOffsets[y3];
				while (--y1 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x3 >> 16, x2 >> 16, l3, l2, l4, k5, j6, i5, l5, k6);
					x2 += k7;
					x3 += i8;
					l2 += l7;
					l3 += j8;
					y3 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				while (--y2 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x1 >> 16, x2 >> 16, l1, l2, l4, k5, j6, i5, l5, k6);
					x2 += k7;
					x1 += i7;
					l2 += l7;
					l1 += j7;
					y3 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				return;
			}

			x1 = x3 <<= 16;
			l1 = l3 <<= 16;
			if (y3 < 0)
			{
				x1 -= k7 * y3;
				x3 -= i8 * y3;
				l1 -= l7 * y3;
				l3 -= j8 * y3;
				y3 = 0;
			}

			x2 <<= 16;
			l2 <<= 16;
			if (y2 < 0)
			{
				x2 -= i7 * y2;
				l2 -= j7 * y2;
				y2 = 0;
			}

			int l9 = y3 - centreY;
			l4 += j5 * l9;
			k5 += i6 * l9;
			j6 += l6 * l9;
			if (k7 < i8)
			{
				y1 -= y2;
				y2 -= y3;
				y3 = lineOffsets[y3];
				while (--y2 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x1 >> 16, x3 >> 16, l1, l3, l4, k5, j6, i5, l5, k6);
					x1 += k7;
					x3 += i8;
					l1 += l7;
					l3 += j8;
					y3 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				while (--y1 >= 0)
				{
					drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x2 >> 16, x3 >> 16, l2, l3, l4, k5, j6, i5, l5, k6);
					x2 += i7;
					x3 += i8;
					l2 += j7;
					l3 += j8;
					y3 += DrawingArea.width;
					l4 += j5;
					k5 += i6;
					j6 += l6;
				}

				return;
			}

			y1 -= y2;
			y2 -= y3;
			y3 = lineOffsets[y3];
			while (--y2 >= 0)
			{
				drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x3 >> 16, x1 >> 16, l3, l1, l4, k5, j6, i5, l5, k6);
				x1 += k7;
				x3 += i8;
				l1 += l7;
				l3 += j8;
				y3 += DrawingArea.width;
				l4 += j5;
				k5 += i6;
				j6 += l6;
			}

			while (--y1 >= 0)
			{
				drawHDTexturedScanline(DrawingArea.pixels, texturePtr, y3, x3 >> 16, x2 >> 16, l3, l2, l4, k5, j6, i5, l5, k6);
				x2 += i7;
				x3 += i8;
				l2 += j7;
				l3 += j8;
				y3 += DrawingArea.width;
				l4 += j5;
				k5 += i6;
				j6 += l6;
			}
		}

		private static unsafe void drawHDTexturedScanline(int[] ai, int* texturePtr, int k, int x1, int x2, int l1, int l2, int a1, int i2, int j2, int k2, int a2, int i3)
		{
			//fixed (int* texturePtr = texture)
			{
				int i = 0; //was parameter
				int j = 0; //was parameter
				if (x1 >= x2)
					return;
				int dl = (l2 - l1) / (x2 - x1);
				int n;
				if (restrictEdges)
				{
					if (x2 > DrawingArea.centerX)
						x2 = DrawingArea.centerX;
					if (x1 < 0)
					{
						l1 -= x1 * dl;
						x1 = 0;
					}
				}

				if (x1 >= x2)
					return;
				n = x2 - x1 >> 3;
				k += x1;

				int j4 = 0;
				int l4 = 0;
				int l6 = x1 - centreX;
				a1 += (k2 >> 3) * l6;
				i2 += (a2 >> 3) * l6;
				j2 += (i3 >> 3) * l6;
				int l5 = j2 >> 14;
				if (l5 != 0)
				{
					i = a1 / l5;
					j = i2 / l5;
					if (i < 0)
						i = 0;
					else if (i > 16256)
						i = 16256;
				}

				a1 += k2;
				i2 += a2;
				j2 += i3;
				l5 = j2 >> 14;
				if (l5 != 0)
				{
					j4 = a1 / l5;
					l4 = i2 / l5;
					if (j4 < 7)
						j4 = 7;
					else if (j4 > 16256)
						j4 = 16256;
				}

				int j7 = j4 - i >> 3;
				int l7 = l4 - j >> 3;
				if (opaque)
				{
					while (n-- > 0)
					{
						int rgb;
						int l;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
						a1 += k2;
						i2 += a2;
						j2 += i3;
						int i6 = j2 >> 14;
						if (i6 != 0)
						{
							j4 = a1 / i6;
							l4 = i2 / i6;
							if (j4 < 7)
								j4 = 7;
							else if (j4 > 16256)
								j4 = 16256;
						}

						j7 = j4 - i >> 3;
						l7 = l4 - j >> 3;
						l1 += dl;
					}

					for (n = x2 - x1 & 7; n-- > 0;)
					{
						int rgb;
						int l;
						rgb = texturePtr[(j & 0x3f80) + (i >> 7)];
						l = l1 >> 16;
						ai[k++] = ((rgb & 0xff00ff) * l & ~0xff00ff) + ((rgb & 0xff00) * l & 0xff0000) >> 8;
						i += j7;
						j += l7;
						l1 += dl;
					}

					return;
				}

				while (n-- > 0)
				{
					int i9;
					int l;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					if ((i9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((i9 & 0xff00ff) * l & ~0xff00ff) + ((i9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
					a1 += k2;
					i2 += a2;
					j2 += i3;
					int j6 = j2 >> 14;
					if (j6 != 0)
					{
						j4 = a1 / j6;
						l4 = i2 / j6;
						if (j4 < 7)
							j4 = 7;
						else if (j4 > 16256)
							j4 = 16256;
					}

					j7 = j4 - i >> 3;
					l7 = l4 - j >> 3;
					l1 += dl;
				}

				for (int l3 = x2 - x1 & 7; l3-- > 0;)
				{
					int j9;
					int l;
					if ((j9 = texturePtr[(j & 0x3f80) + (i >> 7)]) != 0)
					{
						l = l1 >> 16;
						ai[k] = ((j9 & 0xff00ff) * l & ~0xff00ff) + ((j9 & 0xff00) * l & 0xff0000) >> 8;
						;
					}

					k++;
					i += j7;
					j += l7;
					l1 += dl;
				}
			}
		}

		public static void drawHDGouraudTriangle(int y1, int y2, int y3, int x1, int x2, int x3, int hsl1, int hsl2, int hsl3)
		{
			int rgb1 = HSL_TO_RGB[hsl1];
			int rgb2 = HSL_TO_RGB[hsl2];
			int rgb3 = HSL_TO_RGB[hsl3];
			int r1 = rgb1 >> 16 & 0xff;
			int g1 = rgb1 >> 8 & 0xff;
			int b1 = rgb1 & 0xff;
			int r2 = rgb2 >> 16 & 0xff;
			int g2 = rgb2 >> 8 & 0xff;
			int b2 = rgb2 & 0xff;
			int r3 = rgb3 >> 16 & 0xff;
			int g3 = rgb3 >> 8 & 0xff;
			int b3 = rgb3 & 0xff;
			int dx1 = 0;
			int dr1 = 0;
			int dg1 = 0;
			int db1 = 0;
			if(y2 != y1)
			{
				dx1 = (x2 - x1 << 16) / (y2 - y1);
				dr1 = (r2 - r1 << 16) / (y2 - y1);
				dg1 = (g2 - g1 << 16) / (y2 - y1);
				db1 = (b2 - b1 << 16) / (y2 - y1);
			}
			int dx2 = 0;
			int dr2 = 0;
			int dg2 = 0;
			int db2 = 0;
			if(y3 != y2)
			{
				dx2 = (x3 - x2 << 16) / (y3 - y2);
				dr2 = (r3 - r2 << 16) / (y3 - y2);
				dg2 = (g3 - g2 << 16) / (y3 - y2);
				db2 = (b3 - b2 << 16) / (y3 - y2);
			}
			int dx3 = 0;
			int dr3 = 0;
			int dg3 = 0;
			int db3 = 0;
			if(y3 != y1)
			{
				dx3 = (x1 - x3 << 16) / (y1 - y3);
				dr3 = (r1 - r3 << 16) / (y1 - y3);
				dg3 = (g1 - g3 << 16) / (y1 - y3);
				db3 = (b1 - b3 << 16) / (y1 - y3);
			}
			if(y1 <= y2 && y1 <= y3)
			{
				if(y1 >= DrawingArea.bottomY)
				{
					return;
				}
				if(y2 > DrawingArea.bottomY)
				{
					y2 = DrawingArea.bottomY;
				}
				if(y3 > DrawingArea.bottomY)
				{
					y3 = DrawingArea.bottomY;
				}
				if(y2 < y3)
				{
					x3 = x1 <<= 16;
					r3 = r1 <<= 16;
					g3 = g1 <<= 16;
					b3 = b1 <<= 16;
					if(y1 < 0)
					{
						x3 -= dx3 * y1;
						x1 -= dx1 * y1;
						r3 -= dr3 * y1;
						g3 -= dg3 * y1;
						b3 -= db3 * y1;
						r1 -= dr1 * y1;
						g1 -= dg1 * y1;
						b1 -= db1 * y1;
						y1 = 0;
					}
					x2 <<= 16;
					r2 <<= 16;
					g2 <<= 16;
					b2 <<= 16;
					if(y2 < 0)
					{
						x2 -= dx2 * y2;
						r2 -= dr2 * y2;
						g2 -= dg2 * y2;
						b2 -= db2 * y2;
						y2 = 0;
					}
					if(y1 != y2 && dx3 < dx1 || y1 == y2 && dx3 > dx2)
					{
						y3 -= y2;
						y2 -= y1;
						for(y1 = lineOffsets[y1]; --y2 >= 0; y1 += DrawingArea.width)
						{
							drawHDGouraudScanline(DrawingArea.pixels, y1, x3 >> 16, x1 >> 16, r3, g3, b3, r1, g1, b1);
							x3 += dx3;
							x1 += dx1;
							r3 += dr3;
							g3 += dg3;
							b3 += db3;
							r1 += dr1;
							g1 += dg1;
							b1 += db1;
						}
						while(--y3 >= 0)
						{
							drawHDGouraudScanline(DrawingArea.pixels, y1, x3 >> 16, x2 >> 16, r3, g3, b3, r2, g2, b2);
							x3 += dx3;
							x2 += dx2;
							r3 += dr3;
							g3 += dg3;
							b3 += db3;
							r2 += dr2;
							g2 += dg2;
							b2 += db2;
							y1 += DrawingArea.width;
						}
						return;
					}
					y3 -= y2;
					y2 -= y1;
					for(y1 = lineOffsets[y1]; --y2 >= 0; y1 += DrawingArea.width)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y1, x1 >> 16, x3 >> 16, r1, g1, b1, r3, g3, b3);
						x3 += dx3;
						x1 += dx1;
						r3 += dr3;
						g3 += dg3;
						b3 += db3;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
					}
					while(--y3 >= 0)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y1, x2 >> 16, x3 >> 16, r2, g2, b2, r3, g3, b3);
						x3 += dx3;
						x2 += dx2;
						r3 += dr3;
						g3 += dg3;
						b3 += db3;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
						y1 += DrawingArea.width;
					}
					return;
				}
				x2 = x1 <<= 16;
				r2 = r1 <<= 16;
				g2 = g1 <<= 16;
				b2 = b1 <<= 16;
				if(y1 < 0)
				{
					x2 -= dx3 * y1;
					x1 -= dx1 * y1;
					r2 -= dr3 * y1;
					g2 -= dg3 * y1;
					b2 -= db3 * y1;
					r1 -= dr1 * y1;
					g1 -= dg1 * y1;
					b1 -= db1 * y1;
					y1 = 0;
				}
				x3 <<= 16;
				r3 <<= 16;
				g3 <<= 16;
				b3 <<= 16;
				if(y3 < 0)
				{
					x3 -= dx2 * y3;
					r3 -= dr2 * y3;
					g3 -= dg2 * y3;
					b3 -= db2 * y3;
					y3 = 0;
				}
				if(y1 != y3 && dx3 < dx1 || y1 == y3 && dx2 > dx1)
				{
					y2 -= y3;
					y3 -= y1;
					for(y1 = lineOffsets[y1]; --y3 >= 0; y1 += DrawingArea.width)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y1, x2 >> 16, x1 >> 16, r2, g2, b2, r1, g1, b1);
						x2 += dx3;
						x1 += dx1;
						r2 += dr3;
						g2 += dg3;
						b2 += db3;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
					}
					while(--y2 >= 0)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y1, x3 >> 16, x1 >> 16, r3, g3, b3, r1, g1, b1);
						x3 += dx2;
						x1 += dx1;
						r3 += dr2;
						g3 += dg2;
						b3 += db2;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
						y1 += DrawingArea.width;
					}
					return;
				}
				y2 -= y3;
				y3 -= y1;
				for(y1 = lineOffsets[y1]; --y3 >= 0; y1 += DrawingArea.width)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y1, x1 >> 16, x2 >> 16, r1, g1, b1, r2, g2, b2);
					x2 += dx3;
					x1 += dx1;
					r2 += dr3;
					g2 += dg3;
					b2 += db3;
					r1 += dr1;
					g1 += dg1;
					b1 += db1;
				}
				while(--y2 >= 0)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y1, x1 >> 16, x3 >> 16, r1, g1, b1, r3, g3, b3);
					x3 += dx2;
					x1 += dx1;
					r3 += dr2;
					g3 += dg2;
					b3 += db2;
					r1 += dr1;
					g1 += dg1;
					b1 += db1;
					y1 += DrawingArea.width;
				}
				return;
			}
			if(y2 <= y3)
			{
				if(y2 >= DrawingArea.bottomY)
				{
					return;
				}
				if(y3 > DrawingArea.bottomY)
				{
					y3 = DrawingArea.bottomY;
				}
				if(y1 > DrawingArea.bottomY)
				{
					y1 = DrawingArea.bottomY;
				}
				if(y3 < y1)
				{
					x1 = x2 <<= 16;
					r1 = r2 <<= 16;
					g1 = g2 <<= 16;
					b1 = b2 <<= 16;
					if(y2 < 0)
					{
						x1 -= dx1 * y2;
						x2 -= dx2 * y2;
						r1 -= dr1 * y2;
						g1 -= dg1 * y2;
						b1 -= db1 * y2;
						r2 -= dr2 * y2;
						g2 -= dg2 * y2;
						b2 -= db2 * y2;
						y2 = 0;
					}
					x3 <<= 16;
					r3 <<= 16;
					g3 <<= 16;
					b3 <<= 16;
					if(y3 < 0)
					{
						x3 -= dx3 * y3;
						r3 -= dr3 * y3;
						g3 -= dg3 * y3;
						b3 -= db3 * y3;
						y3 = 0;
					}
					if(y2 != y3 && dx1 < dx2 || y2 == y3 && dx1 > dx3)
					{
						y1 -= y3;
						y3 -= y2;
						for(y2 = lineOffsets[y2]; --y3 >= 0; y2 += DrawingArea.width)
						{
							drawHDGouraudScanline(DrawingArea.pixels, y2, x1 >> 16, x2 >> 16, r1, g1, b1, r2, g2, b2);
							x1 += dx1;
							x2 += dx2;
							r1 += dr1;
							g1 += dg1;
							b1 += db1;
							r2 += dr2;
							g2 += dg2;
							b2 += db2;
						}
						while(--y1 >= 0)
						{
							drawHDGouraudScanline(DrawingArea.pixels, y2, x1 >> 16, x3 >> 16, r1, g1, b1, r3, g3, b3);
							x1 += dx1;
							x3 += dx3;
							r1 += dr1;
							g1 += dg1;
							b1 += db1;
							r3 += dr3;
							g3 += dg3;
							b3 += db3;
							y2 += DrawingArea.width;
						}
						return;
					}
					y1 -= y3;
					y3 -= y2;
					for(y2 = lineOffsets[y2]; --y3 >= 0; y2 += DrawingArea.width)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y2, x2 >> 16, x1 >> 16, r2, g2, b2, r1, g1, b1);
						x1 += dx1;
						x2 += dx2;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
					}
					while(--y1 >= 0)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y2, x3 >> 16, x1 >> 16, r3, g3, b3, r1, g1, b1);
						x1 += dx1;
						x3 += dx3;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
						r3 += dr3;
						g3 += dg3;
						b3 += db3;
						y2 += DrawingArea.width;
					}
					return;
				}
				x3 = x2 <<= 16;
				r3 = r2 <<= 16;
				g3 = g2 <<= 16;
				b3 = b2 <<= 16;
				if(y2 < 0)
				{
					x3 -= dx1 * y2;
					x2 -= dx2 * y2;
					r3 -= dr1 * y2;
					g3 -= dg1 * y2;
					b3 -= db1 * y2;
					r2 -= dr2 * y2;
					g2 -= dg2 * y2;
					b2 -= db2 * y2;
					y2 = 0;
				}
				x1 <<= 16;
				r1 <<= 16;
				g1 <<= 16;
				b1 <<= 16;
				if(y1 < 0)
				{
					x1 -= dx3 * y1;
					r1 -= dr3 * y1;
					g1 -= dg3 * y1;
					b1 -= db3 * y1;
					y1 = 0;
				}
				if(dx1 < dx2)
				{
					y3 -= y1;
					y1 -= y2;
					for(y2 = lineOffsets[y2]; --y1 >= 0; y2 += DrawingArea.width)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y2, x3 >> 16, x2 >> 16, r3, g3, b3, r2, g2, b2);
						x3 += dx1;
						x2 += dx2;
						r3 += dr1;
						g3 += dg1;
						b3 += db1;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
					}
					while(--y3 >= 0)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y2, x1 >> 16, x2 >> 16, r1, g1, b1, r2, g2, b2);
						x1 += dx3;
						x2 += dx2;
						r1 += dr3;
						g1 += dg3;
						b1 += db3;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
						y2 += DrawingArea.width;
					}
					return;
				}
				y3 -= y1;
				y1 -= y2;
				for(y2 = lineOffsets[y2]; --y1 >= 0; y2 += DrawingArea.width)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y2, x2 >> 16, x3 >> 16, r2, g2, b2, r3, g3, b3);
					x3 += dx1;
					x2 += dx2;
					r3 += dr1;
					g3 += dg1;
					b3 += db1;
					r2 += dr2;
					g2 += dg2;
					b2 += db2;
				}
				while(--y3 >= 0)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y2, x2 >> 16, x1 >> 16, r2, g2, b2, r1, g1, b1);
					x1 += dx3;
					x2 += dx2;
					r1 += dr3;
					g1 += dg3;
					b1 += db3;
					r2 += dr2;
					g2 += dg2;
					b2 += db2;
					y2 += DrawingArea.width;
				}
				return;
			}
			if(y3 >= DrawingArea.bottomY)
			{
				return;
			}
			if(y1 > DrawingArea.bottomY)
			{
				y1 = DrawingArea.bottomY;
			}
			if(y2 > DrawingArea.bottomY)
			{
				y2 = DrawingArea.bottomY;
			}
			if(y1 < y2)
			{
				x2 = x3 <<= 16;
				r2 = r3 <<= 16;
				g2 = g3 <<= 16;
				b2 = b3 <<= 16;
				if(y3 < 0)
				{
					x2 -= dx2 * y3;
					x3 -= dx3 * y3;
					r2 -= dr2 * y3;
					g2 -= dg2 * y3;
					b2 -= db2 * y3;
					r3 -= dr3 * y3;
					g3 -= dg3 * y3;
					b3 -= db3 * y3;
					y3 = 0;
				}
				x1 <<= 16;
				r1 <<= 16;
				g1 <<= 16;
				b1 <<= 16;
				if(y1 < 0)
				{
					x1 -= dx1 * y1;
					r1 -= dr1 * y1;
					g1 -= dg1 * y1;
					b1 -= db1 * y1;
					y1 = 0;
				}
				if(dx2 < dx3)
				{
					y2 -= y1;
					y1 -= y3;
					for(y3 = lineOffsets[y3]; --y1 >= 0; y3 += DrawingArea.width)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y3, x2 >> 16, x3 >> 16, r2, g2, b2, r3, g3, b3);
						x2 += dx2;
						x3 += dx3;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
						r3 += dr3;
						g3 += dg3;
						b3 += db3;
					}
					while(--y2 >= 0)
					{
						drawHDGouraudScanline(DrawingArea.pixels, y3, x2 >> 16, x1 >> 16, r2, g2, b2, r1, g1, b1);
						x2 += dx2;
						x1 += dx1;
						r2 += dr2;
						g2 += dg2;
						b2 += db2;
						r1 += dr1;
						g1 += dg1;
						b1 += db1;
						y3 += DrawingArea.width;
					}
					return;
				}
				y2 -= y1;
				y1 -= y3;
				for(y3 = lineOffsets[y3]; --y1 >= 0; y3 += DrawingArea.width)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y3, x3 >> 16, x2 >> 16, r3, g3, b3, r2, g2, b2);
					x2 += dx2;
					x3 += dx3;
					r2 += dr2;
					g2 += dg2;
					b2 += db2;
					r3 += dr3;
					g3 += dg3;
					b3 += db3;
				}
				while(--y2 >= 0)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y3, x1 >> 16, x2 >> 16, r1, g1, b1, r2, g2, b2);
					x2 += dx2;
					x1 += dx1;
					r2 += dr2;
					g2 += dg2;
					b2 += db2;
					r1 += dr1;
					g1 += dg1;
					b1 += db1;
					y3 += DrawingArea.width;
				}
				return;
			}
			x1 = x3 <<= 16;
			r1 = r3 <<= 16;
			g1 = g3 <<= 16;
			b1 = b3 <<= 16;
			if(y3 < 0)
			{
				x1 -= dx2 * y3;
				x3 -= dx3 * y3;
				r1 -= dr2 * y3;
				g1 -= dg2 * y3;
				b1 -= db2 * y3;
				r3 -= dr3 * y3;
				g3 -= dg3 * y3;
				b3 -= db3 * y3;
				y3 = 0;
			}
			x2 <<= 16;
			r2 <<= 16;
			g2 <<= 16;
			b2 <<= 16;
			if(y2 < 0)
			{
				x2 -= dx1 * y2;
				r2 -= dr1 * y2;
				g2 -= dg1 * y2;
				b2 -= db1 * y2;
				y2 = 0;
			}
			if(dx2 < dx3)
			{
				y1 -= y2;
				y2 -= y3;
				for(y3 = lineOffsets[y3]; --y2 >= 0; y3 += DrawingArea.width)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y3, x1 >> 16, x3 >> 16, r1, g1, b1, r3, g3, b3);
					x1 += dx2;
					x3 += dx3;
					r1 += dr2;
					g1 += dg2;
					b1 += db2;
					r3 += dr3;
					g3 += dg3;
					b3 += db3;
				}
				while(--y1 >= 0)
				{
					drawHDGouraudScanline(DrawingArea.pixels, y3, x2 >> 16, x3 >> 16, r2, g2, b2, r3, g3, b3);
					x2 += dx1;
					x3 += dx3;
					r2 += dr1;
					g2 += dg1;
					b2 += db1;
					r3 += dr3;
					g3 += dg3;
					b3 += db3;
					y3 += DrawingArea.width;
				}
				return;
			}
			y1 -= y2;
			y2 -= y3;
			for(y3 = lineOffsets[y3]; --y2 >= 0; y3 += DrawingArea.width)
			{
				drawHDGouraudScanline(DrawingArea.pixels, y3, x3 >> 16, x1 >> 16, r3, g3, b3, r1, g1, b1);
				x1 += dx2;
				x3 += dx3;
				r1 += dr2;
				g1 += dg2;
				b1 += db2;
				r3 += dr3;
				g3 += dg3;
				b3 += db3;
			}
			while(--y1 >= 0)
			{
				drawHDGouraudScanline(DrawingArea.pixels, y3, x3 >> 16, x2 >> 16, r3, g3, b3, r2, g2, b2);
				x2 += dx1;
				x3 += dx3;
				r2 += dr1;
				g2 += dg1;
				b2 += db1;
				r3 += dr3;
				g3 += dg3;
				b3 += db3;
				y3 += DrawingArea.width;
			}
		}
	}
}
