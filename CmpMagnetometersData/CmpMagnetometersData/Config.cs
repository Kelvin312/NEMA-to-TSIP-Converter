﻿using System.Drawing;


namespace CmpMagnetometersData
{
    public static class Config
    {
        public static bool IsMagneticField = true;
        public static Color NormalColor = Color.DarkGreen;
        public static Color WarningColor = Color.DarkOrange;
        public static Color ErrorColor = Color.Red;

        public static double ZoomSpeed = 0.125;
        public static double XMinZoom = 1.0 / 24 / 60 / 60;
        public static double YMinZoom = 1.0;

        public static ChartRect GlobalBorder = new ChartRect();
        public static bool IsYAutoScroll = false;
        public static bool IsYSyncZoom = false;

        public static string ViewTimeText = "yy.MM.dd-HH:mm:ss";
        public static string ViewTimeDtp = "yy.MM.dd-HH:mm:ss";
        public static string ViewTimeChart = "yy.MM.dd-HH:mm:ss";
    }
}
