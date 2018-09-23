﻿using System.Numerics;
using NewWidgets.UI;

namespace NewWidgets.Widgets
{
    public class WidgetImage : Widget
    {
        public string Image
        {
            get { return BackgroundTexture; }
            set { BackgroundTexture = value; }
        }
        
        public float ImageRotation
        {
            get { return BackgroundRotation; }
            set { BackgroundRotation = value; }
        }
        
        public Vector2 ImagePivot
        {
            get { return BackgroundPivot; }
            set { BackgroundPivot = value; }
        }

		public WidgetBackgroundStyle ImageStyle
		{
			get { return BackgroundStyle; }
			set { BackgroundStyle = value; }
		}

        public Vector2 ImageSize
        {
			get { return ((ImageObject)m_background[0]).Sprite.Size; }
        }

		public ImageObject ImageObject
        {
            get { return ((ImageObject)m_background[0]); }
        }

        public WidgetImage(string image)
            : this(WidgetBackgroundStyle.ImageFit, image)
        {
        }

        public WidgetImage(WidgetBackgroundStyle style, string image)
			: base(WidgetManager.DefaultWidgetStyle)
        {
            BackgroundTexture = image;
            BackgroundStyle = style;
        }

		public WidgetImage(WidgetStyleSheet style, string image)
			: base(style)
		{
			BackgroundTexture = image;
		}
	}
}
