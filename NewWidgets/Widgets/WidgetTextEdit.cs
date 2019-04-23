﻿using System;
using System.Numerics;

using NewWidgets.UI;
using NewWidgets.Utility;

namespace NewWidgets.Widgets
{
    public class WidgetTextEdit : WidgetBackground, IFocusableWidget
    {
        public static readonly new WidgetStyleSheet DefaultStyle = WidgetManager.GetStyle("default_textedit", true);

        private int m_cursorPosition;

        private LabelObject m_label;
        private ISprite m_cursor;

        private string m_preffix;
        private string m_text;

        public event Action<WidgetTextEdit, string> OnFocusLost;
        public event Action<WidgetTextEdit, string> OnTextEntered;
        public event Action<WidgetTextEdit, string> OnTextChanged;

        private bool m_needLayout;

        public Font Font
        {
            get { return GetProperty(WidgetParameterIndex.Font, WidgetManager.MainFont); }
            set { SetProperty(WidgetParameterIndex.Font, value); m_needLayout = true; }
        }

        public float FontSize
        {
            get { return GetProperty(WidgetParameterIndex.FontSize, 1.0f); }
            set { SetProperty(WidgetParameterIndex.FontSize, value); m_needLayout = true; }
        }

        public WidgetAlign TextAlign
        {
            get { return GetProperty(WidgetParameterIndex.TextAlign, WidgetAlign.Left | WidgetAlign.Top); }
            set { SetProperty(WidgetParameterIndex.TextAlign, value); m_needLayout = true; }
        }

        public Margin TextPadding
        {
            get { return GetProperty(WidgetParameterIndex.TextPadding, new Margin(0)); }
            set { SetProperty(WidgetParameterIndex.TextPadding, value); m_needLayout = true; }
        }

        public int TextColor
        {
            get { return GetProperty(WidgetParameterIndex.TextColor, 0xffffff); }
            set
            {
                SetProperty(WidgetParameterIndex.TextColor, value);

                if (m_label != null) // try to avoid settings m_needLayout
                    m_label.Color = value;
            }
        }

        public int CursorColor
        {
            get { return GetProperty(WidgetParameterIndex.CursorColor, 0xffffff); }
            set
            {
                SetProperty(WidgetParameterIndex.CursorColor, value);

                if (m_cursor != null) // try to avoid settings m_needLayout
                    m_cursor.Color = value;
            }
        }

        public string CursorChar
        {
            get { return GetProperty(WidgetParameterIndex.CursorChar, "|"); }
            set
            {
                SetProperty(WidgetParameterIndex.CursorChar, value);
                m_needLayout = true;
            }
        }

        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text == value)
                    return;
                m_text = value;
                m_needLayout = true;
                m_cursorPosition = value.Length;
            }
        }

        public string MaskChar
        {
            get { return GetProperty(WidgetParameterIndex.MaskChar, ""); }
            set
            {
                SetProperty(WidgetParameterIndex.MaskChar, value);
                m_needLayout = true;
            }
        }

        public bool IsFocused
        {
            get { return Selected; }
            private set { Selected = value; }
        }

        public bool IsFocusable
        {
            get { return true; }
        }
        
        public string Preffix
        {
            get { return m_preffix; }
            set
            {
                string text = m_text;
                int shift = value.Length;
                if (text.StartsWith(m_preffix, StringComparison.CurrentCulture))
                {
                    text = text.Substring(m_preffix.Length);
                    shift -= m_preffix.Length;
                }
                
                m_preffix = value;
                m_cursorPosition += shift;
                m_text = m_preffix + text;
                m_needLayout = true;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NewWidgets.Widgets.WidgetTextEdit"/> class.
        /// </summary>
        /// <param name="style">Style.</param>
        public WidgetTextEdit(WidgetStyleSheet style = default(WidgetStyleSheet))
            : base(style.IsEmpty ? DefaultStyle : style)
        {
            m_text = string.Empty;
            m_preffix = string.Empty;
            m_needLayout = true;
        }

        private void Relayout()
        {
            if (m_label == null)
            {
                m_label = new LabelObject(this, Font, string.Empty,
                                          LabelAlign.Start, LabelAlign.Start);
                m_label.Position = TextPadding.TopLeft;
            }

            m_label.Scale = FontSize;

            if (m_cursor == null)
            {
                m_cursor = Font.GetSprites(CursorChar, Vector2.Zero)[0];
                m_cursor.Color = CursorColor;
                m_cursor.Scale = FontSize;
                //m_cursor.Blink(2000);
                m_cursor.Transform.Parent = Transform;
            }

            Vector2 minSize = m_label.Size * FontSize + TextPadding.Size;
            if (minSize.X < Size.X)
                minSize.X = Size.X;
            if (minSize.Y < Size.Y)
                minSize.Y = Size.Y;

            Size = minSize;

            m_needLayout = false;
            
            UpdateCursor(0);
        }

        protected override void Resize(Vector2 size)
        {
            base.Resize(size);
        }

        public override bool Update()
        {
            if (!base.Update())
                return false;

            if (m_needLayout)
                Relayout();

            if (m_label != null)
                m_label.Update();

            if (m_cursor != null)
            {
                m_cursor.Alpha = (int)(Math.Sin(WindowController.Instance.GetTime() / 1000.0f) * 255 + 128.5f); // blinks every 2 seconds
                m_cursor.Update();
            }

            return true;
        }

        protected override void DrawContents(object canvas)
        {
            base.DrawContents(canvas);

            if (m_label != null)
                m_label.Draw(canvas);

            if (IsFocused)
                m_cursor.Draw(canvas);
        }
        
        public override bool Key(SpecialKey key, bool up, char character)
        {
            if (!IsFocused)
                return false;

            if (up && key == SpecialKey.Back)
            {
                //SetFocused(false);
                if (OnTextEntered != null)
                    OnTextEntered(this, string.Empty);
                return true;
            }

            if (up && (key == SpecialKey.Enter))
            {
                //SetFocused(false);
                if (OnTextEntered != null)
                    OnTextEntered(this, m_text);
                return true;
            }

            if (up && key == SpecialKey.Tab)
            {
                if (WidgetManager.FocusNext(this) && OnFocusLost != null)
                    OnFocusLost(this, m_text);
                
                return true;
            }

            if ((key == SpecialKey.Letter || key == SpecialKey.Paste) && Font.HaveSymbol(character))
            {
                m_text = m_text.Insert(m_cursorPosition, character.ToString());
                
                if (OnTextChanged != null)
                    OnTextChanged(this, m_text);

                UpdateCursor(1);
                return true;
            }

            if (!up)
            {
                switch (key)
                {
                    case SpecialKey.Backspace:
                        if (m_cursorPosition > m_preffix.Length)
                        {
                            m_text = m_text.Substring(0, m_cursorPosition - 1) + m_text.Substring(m_cursorPosition);
                    
                            if (OnTextChanged != null)
                                OnTextChanged(this, m_text);

                            UpdateCursor(-1);
                        }
                        break;
                    case SpecialKey.Delete:
                        if (m_cursorPosition <= m_text.Length - 1)
                        {
                            m_text = m_text.Substring(0, m_cursorPosition) + m_text.Substring(m_cursorPosition + 1);
                    
                            if (OnTextChanged != null)
                                OnTextChanged(this, m_text);

                            UpdateCursor(0);
                        }
                        break;
                    case SpecialKey.Left:
                        if (m_cursorPosition > m_preffix.Length)
                            UpdateCursor(-1);
                        break;
                    case SpecialKey.Right:
                        if (m_cursorPosition <= m_text.Length - 1)
                            UpdateCursor(1);
                        break;
                }
            }
            
            switch(key)
            {
                case SpecialKey.Up:
                case SpecialKey.Down:
                    return false;
            }
            


            return true;
        }

        private void UpdateCursor(int change)
        {
            if (m_needLayout)
                return;
            
            if (IsFocused)
            {
                m_cursorPosition += change;

                if (m_cursorPosition < m_preffix.Length)
                    m_cursorPosition = m_preffix.Length;

                if (m_cursorPosition > m_text.Length)
                    m_cursorPosition = m_text.Length;

                m_label.Text = MaskText(m_text.Substring(0, m_cursorPosition) + '\0' + m_text.Substring(m_cursorPosition));

                var frame = m_label.GetCharFrame(m_cursorPosition);
                float nsize = Size.X / FontSize;

                if (m_label.Size.X > nsize)
                {
                    float from = -m_label.Position.X / FontSize;
                    float to = from + nsize;

                    if (frame.X > from && frame.X < to)
                    {
                    } else
                    {
                        if (frame.X > from)
                        {
                            float nx = (nsize - frame.X) * FontSize - TextPadding.Width;
                            if (nx > TextPadding.Left)
                                nx = TextPadding.Left;

                            m_label.Position = new Vector2(nx, TextPadding.Top);
                        } else
                            if (frame.X < to)
                        {
                            float nx = -frame.X * FontSize;
                            if (nx > TextPadding.Left)
                                nx = TextPadding.Left;

                            m_label.Position = new Vector2(nx, TextPadding.Top);
                        }
                    }
                } else
                    m_label.Position = TextPadding.TopLeft;

                m_cursor.Position = m_label.Position + new Vector2(frame.X * FontSize - 2, 0);

            } else
            {
                m_label.Text = MaskText(m_text);
                m_label.Position = TextPadding.TopLeft;
            }
        }

        private string MaskText(string text)
        {
            if (string.IsNullOrEmpty(MaskChar))
                return text;

            char [] result = text.ToCharArray();
            for (int i = 0; i < result.Length; i++)
                if (!IsFocused || i != m_cursorPosition)
                    result[i] = MaskChar[0];

            return new string(result);
        }

        public void SetFocused(bool value)
        {
            if (IsFocused == value)
                return;

            IsFocused = value;

            //UpdateColor();
            m_cursorPosition = m_text.Length;
            UpdateCursor(0);
            
            WidgetManager.UpdateFocus(this, value);
            //WidgetManager.UpdateFocus(this, true);
        }

        public override bool Touch(float x, float y, bool press, bool unpress, int pointer)
        {
            if (press)
            {
                bool wasFocused = IsFocused;

                SetFocused(true);

                Vector2 local = (this.Transform.GetClientPoint(new Vector2(x, y)) - m_label.Position) / FontSize;
                ISprite[] sprites = m_label.InternalGetSprites();

                if (sprites.Length > 0)
                {
                    bool found = false;

                    if (!found && wasFocused)
                        for (int i = 0; i < sprites.Length; i++)
                        {
                            if (local.X < sprites[i].Position.X + sprites[i].Size.X / 2)
                            {
                                if (i < m_cursorPosition || wasFocused)
                                    UpdateCursor(i - m_cursorPosition);
                                else
                                    if (i >= m_cursorPosition)
                                    UpdateCursor(i - m_cursorPosition - 1);
                                found = true;
                                break;
                            }
                        }

                    if (!found)
                    {
                        int last = m_text.Length - 1;
                        UpdateCursor(last - m_cursorPosition + 1);
                    }
                }
            }

            if (!press && !unpress && !Hovered)
            {
                Hovered = true;
                WindowController.Instance.OnTouch += UnHoverTouch;
            }
            return true;
        }

        private bool UnHoverTouch(float x, float y, bool press, bool unpress, int pointer)
        {
            if (Hovered && !HitTest(x, y))
            {
                Hovered = false;
                WindowController.Instance.OnTouch -= UnHoverTouch;
            }
            return false;
        }


        public override void Remove()
        {
            WidgetManager.UpdateFocus(this, false);
            base.Remove();
        }
    }
}

