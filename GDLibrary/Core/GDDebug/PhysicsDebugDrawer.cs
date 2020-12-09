using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.GameComponents;
using GDLibrary.Managers;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GDLibrary.Debug
{
    /// <summary>
    /// Renders the collision skins of any collidable objects within the scene. We can disable this component for the release.
    /// </summary>
    public class PhysicsDebugDrawer : PausableDrawableGameComponent
    {
        #region Fields
        private CameraManager<Camera3D> cameraManager;
        #endregion Fields

        #region Temp Vars Used In Methods
        private ObjectManager objectManager;
        private BasicEffect basicEffect;
        private List<VertexPositionColor> vertexData;
        private VertexPositionColor[] wf;
        private DrawnActor3D actor;
        #endregion Temp Vars Used In Methods

        public PhysicsDebugDrawer(Game game, StatusType statusType, CameraManager<Camera3D> cameraManager, ObjectManager objectManager)
            : base(game, statusType)
        {
            this.cameraManager = cameraManager;
            this.objectManager = objectManager;
            this.vertexData = new List<VertexPositionColor>();
            this.basicEffect = new BasicEffect(game.GraphicsDevice);
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            //add the vertices for each and every drawn object (opaque or transparent) to the vertexData array for drawing
            ProcessAllDrawnObjects();

            //no vertices to draw - would happen if we forget to call DrawCollisionSkins() above or there were no drawn objects to see!
            if (vertexData.Count == 0) return;

            this.basicEffect.AmbientLightColor = Vector3.One;
            this.basicEffect.VertexColorEnabled = true;

            DrawCollisionSkin(this.cameraManager.ActiveCamera);

            vertexData.Clear();
        }

        private void DrawCollisionSkin(Camera3D activeCamera)
        {
            this.Game.GraphicsDevice.Viewport = activeCamera.Viewport;
            this.basicEffect.View = activeCamera.View;
            this.basicEffect.Projection = activeCamera.Projection;
            this.basicEffect.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vertexData.ToArray(), 0, vertexData.Count - 1);
        }

        //debug method to draw collision skins for collidable objects and zone objects
        private void ProcessAllDrawnObjects()
        {
            for (int i = 0; i < objectManager.OpaqueList.Count; i++)
            {
                actor = objectManager.OpaqueList[i];
                if (actor is CollidableObject)
                    AddCollisionSkinVertexData(actor as CollidableObject);
            }

            for (int i = 0; i < objectManager.TransparentList.Count; i++)
            {
                actor = objectManager.TransparentList[i];
                if (actor is CollidableObject)
                    AddCollisionSkinVertexData(actor as CollidableObject);
            }
        }

        public void AddVertexDataForShape(List<Vector3> shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0], color));
            }

            foreach (Vector3 p in shape)
            {
                vertexData.Add(new VertexPositionColor(p, color));
            }
        }

        public void AddVertexDataForShape(List<Vector3> shape, Color color, bool closed)
        {
            AddVertexDataForShape(shape, color);

            Vector3 v = shape[0];
            vertexData.Add(new VertexPositionColor(v, color));
        }

        public void AddVertexDataForShape(List<VertexPositionColor> shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0].Position, color));
            }

            foreach (VertexPositionColor vps in shape)
            {
                vertexData.Add(vps);
            }
        }

        public void AddVertexDataForShape(VertexPositionColor[] shape, Color color)
        {
            if (vertexData.Count > 0)
            {
                Vector3 v = vertexData[vertexData.Count - 1].Position;
                vertexData.Add(new VertexPositionColor(v, color));
                vertexData.Add(new VertexPositionColor(shape[0].Position, color));
            }

            foreach (VertexPositionColor vps in shape)
            {
                vertexData.Add(vps);
            }
        }

        public void AddVertexDataForShape(List<VertexPositionColor> shape, Color color, bool closed)
        {
            AddVertexDataForShape(shape, color);

            VertexPositionColor v = shape[0];
            vertexData.Add(v);
        }

        public void AddCollisionSkinVertexData(CollidableObject collidableObject)
        {
            if (!collidableObject.Body.CollisionSkin.GetType().Equals(typeof(JigLibX.Geometry.Plane)))
            {
                wf = collidableObject.Collision.GetLocalSkinWireframe();

                // if the collision skin was also added to the body
                // we have to transform the skin wireframe to the body space
                if (collidableObject.Body.CollisionSkin != null)
                {
                    collidableObject.Body.TransformWireframe(wf);
                }

                AddVertexDataForShape(wf, collidableObject.EffectParameters.DiffuseColor);
            }
        }
    }
}