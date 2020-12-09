using GDLibrary.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary.Parameters
{
    /// <summary>
    /// Encapsulates the vertex array, primitive type and primitive count for a drawn primitive
    /// </summary>
    /// <see cref="GDLibrary.Actors.PrimitiveObject"/>
    public class VertexData<T> : IVertexData, ICloneable where T : struct, IVertexType
    {
        #region Fields

        private T[] vertices;
        private PrimitiveType primitiveType;
        private int primitiveCount;

        #endregion Fields

        #region Properties

        public int GetPrimitiveCount()
        {
            return primitiveCount;
        }

        public PrimitiveType GetPrimitiveType()
        {
            return primitiveType;
        }

        public void SetPrimitiveCount(int primitiveCount)
        {
            this.primitiveCount = primitiveCount;
        }

        public void SetPrimitiveType(PrimitiveType primitiveType)
        {
            this.primitiveType = primitiveType;
        }

        #endregion Properties

        #region Constructors & Core

        public VertexData(T[] vertices,
           PrimitiveType primitiveType, int primitiveCount)
        {
            this.vertices = vertices;
            this.primitiveType = primitiveType;
            this.primitiveCount = primitiveCount;
        }

        public void Draw(GameTime gameTime, BasicEffect effect, GraphicsDevice graphicsDevice)
        {
            //serialising the vertices from RAM to VRAM
            //constrained by the bandwidth of the bus and
            //by the lower of the two clock speeds (cpu, gpu)
            graphicsDevice.DrawUserPrimitives<T>(primitiveType,
                vertices, 0, primitiveCount);
        }

        public object Clone()
        {
            //shallow (no use of the "new" keyword)
            return this;
        }

        public override bool Equals(object obj)
        {
            return obj is VertexData<T> data &&
                   EqualityComparer<T[]>.Default.Equals(vertices, data.vertices) &&
                   primitiveType == data.primitiveType &&
                   primitiveCount == data.primitiveCount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(vertices, primitiveType, primitiveCount);
        }

        #endregion Constructors & Core
    }
}