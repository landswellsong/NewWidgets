﻿using NewWidgets.UI;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace NewWidgets.Widgets
{
    public class WidgetText : Widget
    {
        private static char[] s_separatorChars = { ' ', '\t' };

        private LabelObject[] m_labels;

        private string m_text;
        private Font m_font;
        private float m_fontSize;
        private WidgetAlign m_textAlign;
        private float m_lineSpacing;

        private float m_maxWidth;

        private bool m_needLayout;

        private bool m_richText;

        public Font Font
        {
            get { return m_font; }
            set { m_font = value; m_needLayout = true; }
        }

        public float FontSize
        {
            get { return m_fontSize; }
            set { m_fontSize = value; m_needLayout = true; }
        }

        public float LineSpacing
        {
            get { return m_lineSpacing; }
            set { m_lineSpacing = value; m_needLayout = true; }
        }

        public float MaxWidth
        {
            get { return m_maxWidth; }
            set { m_maxWidth = value; m_needLayout = true; }
        }

        public string Text
        {
            get { return m_text; }
            set
            {
                if (!string.IsNullOrEmpty(value) && value[0] == '@')
                    value = ResourceLoader.Instance.GetString(value);
                if (m_text == value)
                    return;
                m_text = value;
                m_needLayout = true;
            }
        }

        public bool RichText
        {
            get { return m_richText; }
            set { m_richText = value; m_needLayout = true; }
        }

        public WidgetAlign TextAlign
        {
            get { return m_textAlign; }
            set { m_textAlign = value; m_needLayout = true; }
        }

        public override int Color
        {
            get { return base.Color; }
            set
            {
                base.Color = value;

                if (m_labels != null)
                    foreach (LabelObject label in m_labels)
                        label.Color = value;
            }
        }

        public override float Alpha
        {
            get { return base.Alpha; }
            set
            {
                base.Alpha = value;

                if (m_labels != null)
                    foreach (LabelObject label in m_labels)
                        label.Alpha = value;
            }
        }

        public int LineCount
        {
            get { return m_labels == null ? 0 : m_labels.Length; }
        }

        public WidgetText(string text = "")
            : this(WidgetManager.DefaultLabelStyle, text)
        {
        }

        public WidgetText(WidgetStyleSheet style, string text = "")
            : base(style)
        {
            m_text = text;
            m_textAlign = WidgetAlign.Left | WidgetAlign.Top;
            m_needLayout = true;

            m_lineSpacing = 5;
            m_font = style.Font;
            m_fontSize = style.FontSize;
            m_richText = true;
            base.Color = style.Color;
        }

        protected override void Resize(Vector2 size)
        {
            base.Resize(size);

            m_needLayout = true;
        }

        public void Relayout()
        {
            string[] lines = m_text.Split(new string[] { "\r", "\n", "|n", "\\n" }, StringSplitOptions.None);

            float lineHeight = (m_font.Height + m_lineSpacing) * m_fontSize; // TODO: spacing

            Vector2 maxSize = Vector2.Zero;
            Vector2[] sizes = new Vector2[lines.Length];
            LabelObject.TextSpan[][] colors = null;


            if (m_richText)
                colors = new LabelObject.TextSpan[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (m_richText)
                    line = LabelObject.ParseRichText(line, Color, out colors[i], m_font.SpaceWidth + m_font.Spacing);

                Vector2 size = m_font.MeasureString(line);

                size = new Vector2(size.X * m_fontSize, lineHeight);
                maxSize = new Vector2(Math.Max(size.X, maxSize.X), size.Y + maxSize.Y);

                sizes[i] = size;
                lines[i] = line;
            }

            if (m_maxWidth == 0 || m_maxWidth > maxSize.X)
            {
                Size = new Vector2(Math.Max(maxSize.X, Size.X), Math.Max(Size.Y, maxSize.Y));
            }
            else
            {
                // Perform text wrapping

                List<string> newLines = new List<string>();
                List<Vector2> newSizes = new List<Vector2>();
                List<ArraySegment<LabelObject.TextSpan>> newColors = new List<ArraySegment<LabelObject.TextSpan>>();

                for (int i = 0; i < lines.Length; i++)
                {
                    if (sizes[i].X > m_maxWidth)
                    {
                        float[] charSizes = m_font.MeasureChars(lines[i]);

                        float width = 0;
                        int lastSeparator = -1;
                        int start = 0;

                        for (int j = 0; j < charSizes.Length; j++)
                        {
                            if (Array.IndexOf(s_separatorChars, lines[i][j]) != -1)
                                lastSeparator = j;

                            width += charSizes[j] * m_fontSize;

                            if (width > m_maxWidth)
                            {
                                if (lastSeparator == -1)
                                    lastSeparator = j;

                                string line = lines[i].Substring(start, lastSeparator - start);
                                newLines.Add(line);

                                width = 0;
                                for (int k = start; k < lastSeparator; k++)
                                    width += charSizes[k] * m_fontSize;

                                newSizes.Add(new Vector2(width, lineHeight));

                                if (m_richText)
                                    newColors.Add(new ArraySegment<LabelObject.TextSpan>(colors[i], start, lastSeparator - start));

                                if (lines[i][lastSeparator] == ' ') // skip line start space
                                    lastSeparator++;

                                start = lastSeparator;
                                width = 0;

                                for (int k = lastSeparator; k <= j; k++)
                                    width += charSizes[k] * m_fontSize;

                                lastSeparator = -1;
                            }
                        }

                        if (start != charSizes.Length - 1)
                        {
                            newLines.Add(lines[i].Substring(start));
                            newSizes.Add(new Vector2(width, lineHeight));

                            if (m_richText)
                                newColors.Add(new ArraySegment<LabelObject.TextSpan>(colors[i], start, colors[i].Length - start));
                        }

                    }
                    else
                    {
                        newLines.Add(lines[i]);
                        newSizes.Add(sizes[i]);
                        if (m_richText)
                            newColors.Add(new ArraySegment<LabelObject.TextSpan>(colors[i]));
                    }
                }
                lines = newLines.ToArray();
                sizes = newSizes.ToArray();

                if (m_richText)
                {
                    LabelObject.TextSpan[][] newColorsArray = new LabelObject.TextSpan[newColors.Count][];
                    for (int i = 0; i < newColors.Count; i++)
                    {
                        ArraySegment<LabelObject.TextSpan> segment = newColors[i];
                        newColorsArray[i] = new LabelObject.TextSpan[segment.Count];
                        Array.Copy(segment.Array, segment.Offset, newColorsArray[i], 0, segment.Count);
                    }

                    colors = newColorsArray;
                }

                float height = sizes.Length * lineHeight;
                Size = new Vector2(Math.Max(Size.X, m_maxWidth), Math.Max(Size.Y, height));
            }

            m_labels = new LabelObject[lines.Length];

            float y = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                LabelObject label = new LabelObject(this, m_font, string.Empty, LabelAlign.Start, LabelAlign.Start, false);
                label.Color = Color;
                label.Scale = m_fontSize;
                label.Alpha = Alpha;
                label.Text = lines[i];

                if (m_richText)
                    label.SetColors(colors[i]);

                float x = 0;

                if ((m_textAlign & WidgetAlign.HorizontalCenter) == WidgetAlign.HorizontalCenter)
                    x = (Size.X - sizes[i].X) / 2;
                else if ((m_textAlign & WidgetAlign.Right) == WidgetAlign.Right)
                    x = Size.X - sizes[i].X;

                label.Position = new Vector2(x, y);

                m_labels[i] = label;

                y += lineHeight;
            }

            m_needLayout = false;
        }

        public override bool Update()
        {
            if (!base.Update())
                return false;

            if (m_needLayout)
                Relayout();


            if (m_labels != null)
                foreach (LabelObject label in m_labels)
                    label.Update();

            return true;
        }

        protected override void DrawContents(object canvas)
        {
            base.DrawContents(canvas);

            if (m_labels != null)
                foreach (LabelObject label in m_labels)
                    label.Draw(canvas);
        }
    }
}

