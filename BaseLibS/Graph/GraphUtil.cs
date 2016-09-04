﻿using System;
using System.Collections.Generic;
using System.Text;
using BaseLibS.Util;

namespace BaseLibS.Graph{
	public static class GraphUtil{
		public const bool isJavaScript = false;
		public const int scrollBarWidth = 18;
		public const int zoomButtonSize = 14;
		public const float zoomStep = 1.2f;
		public const int maxOverviewSize = 100;
		public static readonly Color2 zoomColor = Color2.CornflowerBlue;
		public static readonly Brush2 zoomBrush = new Brush2(zoomColor);
		public static readonly Brush2 zoomBrushHighlight = new Brush2(Color2.Lighter(zoomColor, 30));
		public static readonly Brush2 zoomBrushPress = new Brush2(Color2.Darker(zoomColor, 30));

		public static void PaintZoomButtons(IGraphics g, int width, int height, int bsize, ZoomButtonState state){
			g.SmoothingMode = SmoothingMode2.AntiAlias;
			Brush2 b = zoomBrush;
			switch (state){
				case ZoomButtonState.HighlightPlus:
					b = zoomBrushHighlight;
					break;
				case ZoomButtonState.PressPlus:
					b = zoomBrushPress;
					break;
			}
			PaintPlusZoomButton(g, b, width - bsize - 4, height - 2*bsize - 8, bsize);
			b = zoomBrush;
			switch (state){
				case ZoomButtonState.HighlightMinus:
					b = zoomBrushHighlight;
					break;
				case ZoomButtonState.PressMinus:
					b = zoomBrushPress;
					break;
			}
			PaintMinusZoomButton(g, b, width - bsize - 4, height - bsize - 4, bsize);
			g.SmoothingMode = SmoothingMode2.Default;
		}

		public static void PaintPlusZoomButton(IGraphics g, Brush2 b, int x, int y, int bsize){
			Pen2 w = new Pen2(Color2.White, 2);
			PaintRoundButton(g, b, w, x, y, bsize);
			g.DrawLine(w, x + 4, y + bsize/2, x + bsize - 4, y + bsize/2);
			g.DrawLine(w, x + bsize - bsize/2, y + 4, x + bsize/2, y + bsize - 4);
		}

		public static void PaintMinusZoomButton(IGraphics g, Brush2 b, int x, int y, int bsize){
			Pen2 w = new Pen2(Color2.White, 2);
			PaintRoundButton(g, b, w, x, y, bsize);
			g.DrawLine(w, x + 4, y + bsize/2, x + bsize - 4, y + bsize/2);
		}

		public static void PaintRoundButton(IGraphics g, Brush2 b, Pen2 w, int x, int y, int size){
			g.FillEllipse(b, x, y, size, size);
			g.DrawEllipse(w, x + 2, y + 2, size - 4, size - 4);
		}

		public static Rectangle2 CalcWin(Size2 overview, int totalWidth, int totalHeight, int visibleX, int visibleY,
			int visibleWidth, int visibleHeight, float s){
			float winX = visibleX*overview.Width/totalWidth;
			float winWidth = visibleWidth*overview.Width/totalWidth/s;
			if (winWidth > overview.Width - winX){
				winWidth = overview.Width - winX;
			}
			float winY = visibleY*overview.Height/totalHeight;
			float winHeight = visibleHeight*overview.Height/totalHeight/s;
			if (winHeight > overview.Height - winY){
				winHeight = overview.Height - winY;
			}
			return new Rectangle2(winX, winY, winWidth, winHeight);
		}

		public static void PaintOverview(IGraphics g, int width, int height, int totalWidth, int totalHeight, int visibleX,
			int visibleY, int visibleWidth, int visibleHeight, Func<int, int, Bitmap2> getOverviewBitmap, float s){
			Size2 overview = CalcOverviewSize(width, height, totalWidth, totalHeight);
			Rectangle2 win = CalcWin(overview, totalWidth, totalHeight, visibleX, visibleY, visibleWidth, visibleHeight, s);
			g.FillRectangle(Brushes2.White, 0, height - overview.Height, overview.Width, overview.Height);
			g.DrawImageUnscaled(getOverviewBitmap((int) overview.Width, (int) overview.Height), 0, height - overview.Height);
			Brush2 b = new Brush2(Color2.FromArgb(30, 0, 0, 255));
			if (win.X > 0){
				g.FillRectangle(b, 0, height - overview.Height, win.X, overview.Height);
			}
			if (overview.Width - win.X - win.Width > 0){
				g.FillRectangle(b, win.X + win.Width, height - overview.Height, overview.Width - win.X - win.Width, overview.Height);
			}
			if (win.Y > 0){
				g.FillRectangle(b, win.X, height - overview.Height, win.Width, win.Y);
			}
			if (overview.Height - win.Y - win.Height > 0){
				g.FillRectangle(b, win.X, height - overview.Height + win.Y + win.Height, win.Width,
					overview.Height - win.Y - win.Height);
			}
			g.DrawRectangle(Pens2.Black, 0, height - overview.Height - 1, overview.Width, overview.Height);
			g.DrawRectangle(Pens2.Blue, win.X, height - overview.Height - 1 + win.Y, win.Width, win.Height);
		}

		public static Size2 CalcOverviewSize(int width, int height, int totalWidth, int totalHeight){
			int maxSize = Math.Min(Math.Min(maxOverviewSize, height), width - 20);
			if (totalWidth > totalHeight){
				return new Size2(maxSize, (int) Math.Round(totalHeight/(float) totalWidth*maxSize));
			}
			return new Size2((int) Math.Round(totalWidth/(float) totalHeight*maxSize), maxSize);
		}

		public static bool HitsPlusButton(int x, int y, int width, int height){
			if (x < width - zoomButtonSize - 4){
				return false;
			}
			if (x > width - 4){
				return false;
			}
			if (y < height - 2*zoomButtonSize - 8){
				return false;
			}
			return y <= height - zoomButtonSize - 8;
		}

		public static bool HitsMinusButton(int x, int y, int width, int height){
			if (x < width - zoomButtonSize - 4){
				return false;
			}
			if (x > width - 4){
				return false;
			}
			if (y < height - zoomButtonSize - 4){
				return false;
			}
			return y <= height - 4;
		}

		private static readonly Color2[] predefinedColors ={
			Color2.Blue, Color2.FromArgb(255, 144, 144),
			Color2.FromArgb(255, 0, 255), Color2.FromArgb(168, 156, 82), Color2.LightBlue, Color2.Orange, Color2.Cyan,
			Color2.Pink, Color2.Turquoise, Color2.LightGreen, Color2.Brown, Color2.DarkGoldenrod, Color2.DeepPink,
			Color2.LightSkyBlue, Color2.BlueViolet, Color2.Crimson
		};

		public static Font2 defaultFont = new Font2("Lucida Sans Unicode", 8F, FontStyle2.Regular);

		public static Color2 GetPredefinedColor(int index){
			return predefinedColors[Math.Abs(index%predefinedColors.Length)];
		}

		public static void FillShadedRectangle(Bitmap2 b, int w, int h){
			b.FillRectangle(Color2.White, 0, 0, w - 1, h - 1);
			b.SetPixel(1, 1, Color2.FromArgb(230, 238, 252));
			b.SetPixel(1, h - 3, Color2.FromArgb(219, 227, 248));
			b.SetPixel(w - 3, 1, Color2.FromArgb(220, 230, 249));
			b.SetPixel(w - 3, h - 3, Color2.FromArgb(217, 227, 246));
			b.SetPixel(w - 1, h - 3, Color2.FromArgb(174, 192, 214));
			b.SetPixel(w - 2, h - 2, Color2.FromArgb(174, 196, 219));
			b.SetPixel(0, h - 2, Color2.FromArgb(195, 212, 231));
			b.SetPixel(0, h - 1, Color2.FromArgb(237, 241, 243));
			b.SetPixel(w - 2, h - 1, Color2.FromArgb(236, 242, 247));
			int wi = w - 5;
			int he = h - 5;
			int[][] upper = InterpolateRgb(225, 234, 254, 188, 206, 250, wi);
			int[][] lower = InterpolateRgb(183, 203, 249, 174, 200, 247, wi);
			for (int i = 0; i < wi; i++){
				int[][] pix = InterpolateRgb(upper[0][i], upper[1][i], upper[2][i], lower[0][i], lower[1][i], lower[2][i], he);
				for (int j = 0; j < he; j++){
					b.SetPixel(i + 2, j + 2, Color2.FromArgb(pix[0][j], pix[1][j], pix[2][j]));
				}
			}
			int[][] pix2 = InterpolateRgb(208, 223, 252, 170, 192, 243, he);
			for (int j = 0; j < he; j++){
				b.SetPixel(1, j + 2, Color2.FromArgb(pix2[0][j], pix2[1][j], pix2[2][j]));
			}
			pix2 = InterpolateRgb(185, 202, 243, 176, 197, 242, he);
			for (int j = 0; j < he; j++){
				b.SetPixel(w - 3, j + 2, Color2.FromArgb(pix2[0][j], pix2[1][j], pix2[2][j]));
			}
			pix2 = InterpolateRgb(208, 223, 252, 175, 197, 244, wi);
			for (int i = 0; i < wi; i++){
				b.SetPixel(i + 2, 1, Color2.FromArgb(pix2[0][i], pix2[1][i], pix2[2][i]));
			}
			pix2 = InterpolateRgb(183, 198, 241, 176, 196, 242, wi);
			for (int i = 0; i < wi; i++){
				b.SetPixel(i + 2, h - 3, Color2.FromArgb(pix2[0][i], pix2[1][i], pix2[2][i]));
			}
			pix2 = InterpolateRgb(238, 237, 229, 160, 181, 211, he + 2);
			for (int i = 0; i < he + 2; i++){
				b.SetPixel(w - 1, i, Color2.FromArgb(pix2[0][i], pix2[1][i], pix2[2][i]));
			}
			pix2 = InterpolateRgb(170, 192, 225, 126, 159, 211, w/2);
			for (int i = 1; i <= w/2; i++){
				b.SetPixel(i, h - 1, Color2.FromArgb(pix2[0][i - 1], pix2[1][i - 1], pix2[2][i - 1]));
			}
			pix2 = InterpolateRgb(126, 159, 211, 148, 176, 221, w - 3 - w/2);
			for (int i = w/2 + 1; i <= w - 3; i++){
				b.SetPixel(i, h - 1, Color2.FromArgb(pix2[0][i - w/2 - 1], pix2[1][i - w/2 - 1], pix2[2][i - w/2 - 1]));
			}
		}

		public static int[][] InterpolateRgb(int start0, int start1, int start2, int end0, int end1, int end2, int n){
			if (n == 0){
				return new[]{new int[0], new int[0], new int[0]};
			}
			if (n == 1){
				int r1 = (start0 + end0)/2;
				int g1 = (start1 + end1)/2;
				int b1 = (start2 + end2)/2;
				return new[]{new[]{r1}, new[]{g1}, new[]{b1}};
			}
			int[] r = new int[n];
			int[] g = new int[n];
			int[] b = new int[n];
			double rstep = (end0 - start0)/(n - 1.0);
			double gstep = (end1 - start1)/(n - 1.0);
			double bstep = (end2 - start2)/(n - 1.0);
			for (int i = 0; i < n; i++){
				r[i] = (int) Math.Round(start0 + i*rstep);
				g[i] = (int) Math.Round(start1 + i*gstep);
				b[i] = (int) Math.Round(start2 + i*bstep);
			}
			return new[]{r, g, b};
		}

		public static string[] WrapString(IGraphics g, string s, int width, Font2 font){
			if (width < 20){
				return new[]{s};
			}
			if (g.MeasureString(s, font).Width < width - 7){
				return new[]{s};
			}
			s = StringUtils.ReduceWhitespace(s);
			string[] q = s.Split(' ');
			List<string> result = new List<string>();
			string current = q[0];
			for (int i = 1; i < q.Length; i++){
				string next = current + " " + q[i];
				if (g.MeasureString(next, font).Width > width - 7){
					result.Add(current);
					current = q[i];
				} else{
					current += " " + q[i];
				}
			}
			result.Add(current);
			return result.ToArray();
		}

		public static string GetStringValue(IGraphics g, string s, int width, Font2 font){
			if (width < 20){
				return "";
			}
			if (g.MeasureString(s, font).Width < width - 7){
				return s;
			}
			StringBuilder sb = new StringBuilder();
			foreach (char t in s){
				if (g.MeasureString(sb.ToString(), font).Width < width - 21){
					sb.Append(t);
				} else{
					break;
				}
			}
			return sb + "...";
		}
	}
}