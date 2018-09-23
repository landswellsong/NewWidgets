﻿using System;
using System.Numerics;

namespace NewWidgets.Utility
{
    /// <summary>
    /// Helper class that is capable of baking 2d transform into 4x4 matrix with support for inheritance
    /// </summary>
    public class Transform
    {
#if STEP_UPDATES
        private static readonly float PositionEpsilon = 0.1f;
        private static readonly float AngleEpsilon = 0.0001f;
        private static readonly float ScaleEpsilon = 0.001f;
#else
        private static readonly float PositionEpsilon = float.Epsilon; // Position vector is three-component, so that should be eps^3
        private static readonly float AngleEpsilon = float.Epsilon;
        private static readonly float ScaleEpsilon = float.Epsilon; // Scale vector is three-component, so that should be eps^3
#endif

        private Matrix4x4 m_matrix;
        private Matrix4x4 m_imatrix;

		private Vector3 m_rotation;
        private Vector3 m_position;
        private Vector3 m_scale;
        
		private bool m_changed;
        private bool m_iMatrixChanged;
        private int m_version;
        private int m_parentVersion;

        private Transform m_parent;
        
        internal bool IsChanged
        {
            get
            {
                return m_changed || (m_parent != null && (m_parent.IsChanged || m_parent.m_version != m_parentVersion));
            }
        }

        internal int Version
        {
            get { return m_version; }
        }
        
        /// <summary>
        /// Non-uniform scale value
        /// </summary>
        /// <value>The scale vector.</value>
        public Vector3 Scale
        {
            get { return m_scale; }
            set
            {
                if (!m_changed && (m_scale - value).LengthSquared() >= ScaleEpsilon)
                    m_changed = true;

                m_scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position vector.</value>
        public Vector3 Position
        {
            get { return m_position; }
            set
            {
                if (!m_changed && (value - m_position).LengthSquared() > PositionEpsilon)
                    m_changed = true;

                m_position = value;
            }
        }

		/// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position vector.</value>
        public Vector2 FlatPosition
        {
			get { return new Vector2(m_position.X, m_position.Y); }
            set
            {
				Vector3 newVector = new Vector3(value.X, value.Y, 0);

				if (!m_changed && (newVector - m_position).LengthSquared() > PositionEpsilon)
                    m_changed = true;

				m_position = newVector;
            }
        }
        
        
		/// <summary>
        /// Gets or sets the position.
        /// </summary>
        /// <value>The position vector.</value>
        public Vector3 Rotation
        {
            get { return m_rotation; }
            set
            {
				if (!m_changed && (value - m_rotation).LengthSquared() > AngleEpsilon)
                    m_changed = true;

				m_rotation = value;
            }
        }
        
        /// <summary>
        /// Gets or sets the rotation in degrees.
        /// </summary>
        /// <value>The rotation.</value>
        public float RotationZ
        {
            get { return m_rotation.Z; }
            set
            {
                if (!m_changed && (Math.Abs(m_rotation.Z - value) >= AngleEpsilon))
                    m_changed = true;

                m_rotation.Z = value;
            }
        }
        
		/// <summary>
        /// Gets or sets the rotation in degrees.
        /// </summary>
        /// <value>The rotation.</value>
        public float UniformScale
        {
            get { return m_scale.X; }
            set
            {
				if (!m_changed && (Math.Abs(m_scale.X - value) >= ScaleEpsilon))
                    m_changed = true;

				m_scale = new Vector3(value, value, value);
            }
        }
        
		/// <summary>
        /// Gets or sets the rotation in degrees.
        /// </summary>
        /// <value>The rotation.</value>
        public Vector2 FlatScale
        {
			get { return new Vector2(m_scale.X, m_scale.Y); }
            set
            {
				Vector3 newScale = new Vector3(value.X, value.Y, value.X);

				if (!m_changed && (newScale - m_scale).LengthSquared() >= ScaleEpsilon)
                    m_changed = true;

				m_scale = newScale;
            }
        }
        
        /// <summary>
        /// Gets or sets the parent transform.
        /// </summary>
        /// <value>The parent reference.</value>
        public Transform Parent
        {
            get { return m_parent; }
            set
            {
                System.Diagnostics.Debug.Assert(m_parent != this);
                
                if (!m_changed && m_parent != value)
                    m_changed = true;
                
                m_parent = value;
            }
        }
        
        /// <summary>
        /// Result transformation 4x4 matrix
        /// </summary>
        /// <value>The matrix.</value>
        public Matrix4x4 Matrix
        {
            get
            {
                if (IsChanged)
                {
                    UpdateMatrix();
                }
                
                return m_matrix;
            }
        }

        public Matrix4x4 IMatrix
        {
            get
            {
                if (m_iMatrixChanged || IsChanged)
                {
                    MatrixHelper.Invert(Matrix, ref m_imatrix);
                    m_iMatrixChanged = false;
                }

                return m_imatrix;
            }    
        }
        
        // 2d actual values


        public Vector2 ActualPosition
        {
			get
			{
				// Alternative is GetScreenPoint(new Vector3(0,0,0));
				Matrix4x4 transform = Matrix;
                Vector3 position = transform.Translation;
                
                return new Vector2(position.X, position.Y);  // or new Vector3(transform[12], transform[13], transform[14]); 
			}
		}
        
        public Vector2 ActualScale
        {
            get
            {
				return (m_parent == null ? Vector2.One : m_parent.ActualScale) * new Vector2(m_scale.X, m_scale.Y);
            }
        }
        
        public float ActualRotation
        {
            get
            {
                return (m_parent == null ? 0 : m_parent.ActualRotation) + m_rotation.Z;
            }
        }

        public Transform()
			: this(Vector3.Zero, Vector3.Zero, Vector3.One)
        {
            
        }
        
        public Transform(Vector2 position, float rotation, float uniformScale)
			: this(new Vector3(position.X, position.Y, 0), new Vector3(0, 0, rotation), new Vector3(uniformScale, uniformScale, uniformScale))
        {
        }
        
        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            m_changed = true;
            m_iMatrixChanged = true;
            m_position = position;
            m_rotation = rotation;
            m_scale = scale;
			m_version = 1;
        }

        public Vector2 GetScreenPoint(Vector2 source)
        {            
            Vector3 result = Vector3.Transform(new Vector3(source.X, source.Y, 0), Matrix);
            return new Vector2(result.X, result.Y);
        }

        public Vector2 GetClientPoint(Vector2 source)
        {
            Vector3 result = Vector3.Transform(new Vector3(source.X, source.Y, 0), IMatrix);
            return new Vector2(result.X, result.Y);
        }

        /// <summary>
        /// This method bakes transform values to 4x4 matrix
        /// </summary>
        private void UpdateMatrix()
        {
            MatrixHelper.Init(ref m_matrix);

			MatrixHelper.GetMatrix3d(m_position, m_rotation * (float)MathHelper.Deg2Rad, m_scale, ref m_matrix);

            if (m_parent != null) // if there is parent transform, baked value contains also parent transforms
                MatrixHelper.Mul(m_parent.Matrix, ref m_matrix); // this one is most expensive thing in whole engine

            m_iMatrixChanged = true;

            m_changed = false;
            m_version++;
            m_parentVersion = m_parent != null ? m_parent.m_version : 0;
        }
    }
}
