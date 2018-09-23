﻿using System;

using NewWidgets.Widgets;
using NewWidgets.WinForms;

namespace NewWidgets.WinFormsSample

        private WinFormsController m_windowController;
            this.zoomTrackBar.Visible = false;

            m_windowController.OnInit += HandleOnInit;

            updateTimer.Start();
        }

        private void HandleOnInit()
        {
        }

        protected override void OnClosing(CancelEventArgs e)
        #region Events

        private void zoomTrackBar_Scroll(object sender, EventArgs e)

        private void perspectiveViewPictureBox_MouseEnter(object sender, EventArgs e)
        {
            perspectiveViewPictureBox.Focus();
        }

        private void perspectiveViewPictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
           /* int zoom = zoomTrackBar.Value + e.Delta / 40;
            if (zoom < zoomTrackBar.Minimum)
                zoom = zoomTrackBar.Minimum;
            if (zoom > zoomTrackBar.Maximum)
                zoom = zoomTrackBar.Maximum;
            zoomTrackBar.Value = zoom;*/
            if (m_windowController != null)
                m_windowController.Zoom(e.X, e.Y, e.Delta);
        }


            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;

            if (m_windowController != null)
                m_windowController.Draw(g);
            if (m_windowController != null)

            if (m_windowController != null)

            if (m_windowController != null)

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            if (m_windowController != null)
            {
                m_windowController.Update();
                perspectiveViewPictureBox.Invalidate();
            }
        }

        #endregion
    }