using System;
using System.Numerics;
using NewWidgets.Utility;

namespace NewWidgets.UI
{
    public enum SpecialKey
    {
        None = 0,
        Menu = 1,
        Back = 2,
        Left = 3,
        Right = 4,
        Up = 5,
        Down = 6,
        Select = 7,
        Enter = 8,
        Tab = 9,

        Slash,
        BackSlash,
        Semicolon,
        Quote,
        Comma,
        Period,
        Minus,
        Plus,
        BracketLeft,
        BracketRight,
        Tilde,
        Grave,
        Backspace,
        Delete,

        Letter,

        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,

        Shift,
        Paste,
        Control
    }

    [Flags]
	public enum WindowObjectFlags
    {
        None = 0x00,
        Removing = 0x01,
        Visible = 0x02,
        Enabled = 0x04,
        Changed = 0x08,

        Default = Visible | Enabled | Changed
	}

    public class WindowObject
    {
		private readonly Transform m_transform;

		private Vector2 m_size;
		private object m_tag;
        private int m_zIndex;
        private int m_tempZIndex;

        private WindowObject m_parent;

        private Animator m_animator;
		private WindowObjectFlags m_flags;

        private WindowObjectArray m_lastList;

        public event TouchDelegate OnTouch;

        internal WindowObjectArray LastList
        {
            get { return m_lastList; }
            set { m_lastList = value; }
        }

        public Animator Animator
        {
            get
            {
                // Lazy init. Not thread safe!!! However, all this class is not thread safe
                if (m_animator == null)
                    m_animator = new Animator();
                return m_animator;
            }
        }

		public object Tag
        {
            get { return m_tag; }
			set { m_tag = value; }
        }
        
        public int ZIndex
        {
			get { return m_zIndex == 0 ? m_tempZIndex : m_zIndex; }
            set { m_zIndex = value; }
        }

        internal int TempZIndex
        {
            get { return m_tempZIndex; }
            set { m_tempZIndex = value; }
        }

        public WindowObject Parent
        {
            get { return m_parent; }
			set { m_parent = value; m_transform.Parent = value == null ? null : value.Transform; }
        }

        public Transform Transform
		{
			get { return m_transform; }
		}

        public Vector2 Position
        {
			get { return new Vector2(m_transform.Position.X, m_transform.Position.Y); }
			set { m_transform.Position = new Vector3(value.X, value.Y, 0); }
        }

        /// <summary>
        /// Gets or sets the uniform scale. Setting this value resets non-uniform scale
        /// </summary>
        /// <value>The scale.</value>
        public float Scale
        {
			get { return m_transform.Scale.X; }
			set { m_transform.Scale = new Vector3(value, value, value); }
        }

        public float Rotation
        {
			get { return m_transform.Rotation.Z; }
			set { m_transform.Rotation = new Vector3(0, 0, value); }
        }
            
        public virtual bool Visible
        {
			get { return (m_flags & WindowObjectFlags.Visible) == WindowObjectFlags.Visible; }
			set
			{
				if (value)
                    m_flags |= WindowObjectFlags.Visible;
                else
					m_flags &= ~WindowObjectFlags.Visible;
			}
        }

        public virtual bool Enabled
        {
			get { return (m_flags & WindowObjectFlags.Enabled) == WindowObjectFlags.Enabled; }
			set
			{
				if (value)
					m_flags |= WindowObjectFlags.Enabled;
				else
					m_flags &= ~WindowObjectFlags.Enabled;
			}
        }

		public bool Removing
        {
			get { return (m_flags & WindowObjectFlags.Removing) == WindowObjectFlags.Removing; }
        }
        
        internal bool HasChanges
        {
			get
			{
				bool value = (m_flags & WindowObjectFlags.Changed) == WindowObjectFlags.Changed;
				m_flags &= ~WindowObjectFlags.Changed;
				return value;
			}
			set
            {
                if (value)
                    m_flags |= WindowObjectFlags.Changed;
                else
					m_flags &= ~WindowObjectFlags.Changed;
            }
        }

        public Vector2 Size
		{
			get { return m_size; }
			set { Resize(value); }
		}

        /// <summary>
        /// Unwinds hierarchy to find top-level window
        /// </summary>
        /// <value>The window</value>
        public IWindowContainer Window
        {
            get
            {
                if (m_parent == null)
                    return this as IWindowContainer;

                return m_parent.Window;
            }
        }

        protected WindowObject(WindowObject parent, Transform transform = null)
        {
            m_parent = parent;
			m_transform = transform == null ? new Transform() : transform;
			if (parent != null)
				m_transform.Parent = parent.Transform;

			m_flags = WindowObjectFlags.Default;
        }

        public virtual void Reset()
        {
			m_flags = WindowObjectFlags.Default;

            if (m_animator != null)
                m_animator.Reset();
        }

		protected virtual void Resize(Vector2 size)
        {
			m_size = size;
        }

        public virtual bool HitTest(float x, float y)
        {
            Vector2 coord = m_transform.GetClientPoint(new Vector2(x, y));

            return coord.X >= 0 && coord.Y >= 0 && coord.X < Size.X && coord.Y < Size.Y;
		}

        public virtual void Remove()
        {
			m_flags |= WindowObjectFlags.Removing;
        }

        public virtual bool Update()
        {
			if ((m_flags & WindowObjectFlags.Removing) != 0)
                return false;

            if (m_animator != null)
                m_animator.Update();

            return true;
        }

		public virtual void Draw(object canvas)
		{
			
		}

        public virtual bool Touch(float x, float y, bool press, bool unpress, int pointer)
        {
            if (Enabled && OnTouch != null)
                return OnTouch(x, y, press, unpress, pointer);

            return false;
        }

        public virtual bool Zoom(float x, float y, float value)
        {
            return false;
        }

        public virtual bool Key(SpecialKey key, bool up, char character)
        {
            return false;
        }

        public virtual void Move(Vector2 point, int time, Action callback)
        {
            if ((point - Position).LengthSquared() <= float.Epsilon)
            {
                if (callback != null)
                    callback();
                return;
            }

            Animator.StartAnimation(AnimationKind.Position, (point - Position), time, (Vector2 value) => Position += value, callback);
        }

        public virtual void Rotate(float angle, int time, Action callback, bool normalize)
        {
            float delta = (angle - Rotation) % 360;

            if (normalize)
            {
                if (delta > 180)
                    delta -= 360;
                else if (delta < -180)
                    delta += 360;
            }

            Animator.StartAnimation(AnimationKind.Rotation, delta, time, (float value) => Rotation += value, callback);
        }

        public virtual void Rotate(float angle, int time, Action callback)
        {
            Rotate(angle, time, callback, true);
        }

        public virtual void ScaleTo(float target, int time, Action callback)
        {
            ScaleTo(new Vector2(target, target), time, callback);
        }

        public virtual void ScaleTo(Vector2 target, int time, Action callback)
        {
			if ((target - Transform.FlatScale).LengthSquared() <= float.Epsilon)
            {
                if (callback != null)
                    callback();
                return;
            }

			Animator.StartAnimation(AnimationKind.Scale, target - Transform.FlatScale, time, (Vector2 value) => Transform.FlatScale += value, callback);
        }
    }
}