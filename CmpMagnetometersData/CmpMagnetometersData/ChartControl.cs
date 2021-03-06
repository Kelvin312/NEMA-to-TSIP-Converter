﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CmpMagnetometersData.Properties;

namespace CmpMagnetometersData
{
    public partial class ChartControl : UserControl
    {
        public ChartControl()
        {
            InitializeComponent();
            _ptrSeries = ch.Series[0];
            _ptrChartArea = ch.ChartAreas[0];
            _ptrAxisX = _ptrChartArea.AxisX;
            _ptrAxisY = _ptrChartArea.AxisY;
            ChartControlInit();

            _ptrSeries.XValueType = ChartValueType.DateTime;
            _ptrChartArea.CursorX.IntervalType = DateTimeIntervalType.Seconds;
            _ptrSeries.YValueType = ChartValueType.Int32;
            _ptrAxisY.LabelStyle.Format = "#";
            _ptrAxisX.LabelStyle.Format = Settings.Default.ViewTimeChart;
        }

        public readonly Series _ptrSeries;
        public readonly ChartArea _ptrChartArea;
        public readonly Axis _ptrAxisX;
        public readonly Axis _ptrAxisY;

        public ChartRect Border { get; protected set; }
        public ChartRect GlobalBorder { get; set; }
        protected double XMinZoom = 1e-6, YMinZoom = 1e-6;


        public event EventHandler<ChartArea> ViewChanged;

        protected void ChartControlInit()
        {
            //X
            _ptrAxisX.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisX.IntervalAutoMode = IntervalAutoMode.VariableCount;
            _ptrChartArea.CursorX.IsUserSelectionEnabled = true;
            _ptrAxisX.ScrollBar.Enabled = false;
            //Y
            _ptrAxisY.LabelStyle.IsEndLabelVisible = false;
            _ptrAxisY.IntervalAutoMode = IntervalAutoMode.VariableCount;
            _ptrChartArea.CursorY.IsUserSelectionEnabled = true;
            _ptrAxisY.ScrollBar.Enabled = false;
            _ptrAxisY.IsStartedFromZero = false;
            //Mouse
            ch.MouseEnter += ChartControl_MouseEnter;
            ch.MouseWheel += ChartControl_MouseWheel;
            ch.MouseDown += ChartControl_MouseDown;
            ch.MouseMove += ChartControl_MouseMove;
            ch.AxisViewChanged += ChartControl_AxisViewChanged;
           
        }

        private void ChartControl_MouseWheel(object sender, MouseEventArgs e)
        {
            ChartRect newZoom = new ChartRect(_ptrChartArea);
            ScaleViewZoom(e.Delta, ref newZoom.X);
            ScaleViewZoom(e.Delta, ref newZoom.Y);
            UpdateAxis(newZoom, true);
            ViewChanged?.Invoke(this, _ptrChartArea);
        }

        private void ScaleViewZoom(int delta, ref AxisSize axis)
        {
            var deltaPos = axis.Size * Settings.Default.ZoomSpeed;
            if (delta > 0) deltaPos = -deltaPos;
            axis.Size = axis.Size + deltaPos;
        }

        private void ChartControl_MouseEnter(object sender, EventArgs e)
        {
           // ch.Focus();
        }

        private bool _mouseDowned;
        private double _xStart, _yStart;

        private void ChartControl_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Middle:
                    _mouseDowned = true;
                    _xStart = _yStart = 0;
                    try
                    {
                        _xStart = _ptrAxisX.PixelPositionToValue(e.Location.X);
                        _yStart = _ptrAxisY.PixelPositionToValue(e.Location.Y);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                    break;
                case MouseButtons.Right:
                    UpdateAxis(null, false, true);
                    ViewChanged?.Invoke(this, null);
                    break;
            }
        }

        private void ChartControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(_mouseDowned && e.Button == MouseButtons.Middle))
            {
                _mouseDowned = false;
                return;
            }

            double selX, selY;
            try
            {
                selX = _ptrAxisX.PixelPositionToValue(e.Location.X);
                selY = _ptrAxisY.PixelPositionToValue(e.Location.Y);
            }
            catch (Exception)
            {
                return;
            }
            if (_ptrAxisX.ScaleView.IsZoomed || _ptrAxisY.ScaleView.IsZoomed)
            {
                double dx = -selX + _xStart;
                double dy = -selY + _yStart;
                double newX = _ptrAxisX.ScaleView.Position + dx;
                double newY = _ptrAxisY.ScaleView.Position + dy;

                _ptrAxisX.ScaleView.Scroll(newX);
                _ptrAxisY.ScaleView.Scroll(newY);
                ViewChanged?.Invoke(this, _ptrChartArea);
            }
        }

        private void ChartControl_AxisViewChanged(object sender, ViewEventArgs e)
        {
            ViewChanged?.Invoke(this, _ptrChartArea);
        }


        public void UpdateAxis(ChartRect newView = null, bool isUpdateY = false, bool isResetZoom = false, bool isUpdateBorder = false)
        {
            var curView = new ChartRect(_ptrChartArea);
            var globalBorder = new ChartRect(_ptrChartArea, true);

            if (isUpdateBorder)
            {
                globalBorder = new ChartRect(GlobalBorder);
                globalBorder.Y.Min -= 2;
                globalBorder.Y.Max += YMinZoom * 10.0;
                _ptrAxisX.Minimum = globalBorder.X.Min;
                _ptrAxisX.Maximum = globalBorder.X.Max;
                _ptrAxisY.Minimum = globalBorder.Y.Min;
                _ptrAxisY.Maximum = globalBorder.Y.Max;
            }
            if (isResetZoom)
            {
                curView.X = globalBorder.X;
                curView.Y = Border.Y;
            }
            else
            {
                if (newView != null)
                {
                    //X
                    curView.X = newView.X;
                    //Y
                    if (isUpdateY)
                    {
                        curView.Y = newView.Y;
                    }
                }
            }
            var oldView = new ChartRect(_ptrChartArea);
            if (curView.X.Check(oldView.X, XMinZoom, globalBorder.X))
            {
                _ptrAxisX.ScaleView.Zoom(curView.X.Min, curView.X.Max);
            }
            if (curView.Y.Check(oldView.Y, YMinZoom, globalBorder.Y))
            {
                _ptrAxisY.ScaleView.Zoom(curView.Y.Min, curView.Y.Max);
            }
        }

    }
}
