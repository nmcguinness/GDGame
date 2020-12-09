using GDLibrary.Actors;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary.Managers
{
    /// <summary>
    /// Provide mouse input functions
    /// </summary>
    /// <see cref="GDLibrary.Controllers.FlightCameraController"/>
    public class MouseManager : GameComponent
    {
        #region Fields
        private PhysicsManager physicsManager;
        private MouseState newState, oldState;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Returns a 1x1 pixel bounding box representing the mouse pointer position
        /// </summary>
        public Microsoft.Xna.Framework.Rectangle Bounds
        {
            get
            {
                return new Microsoft.Xna.Framework.Rectangle(newState.X, newState.Y, 1, 1);
            }
        }

        /// <summary>
        /// Returns the current position of the mouse pointer
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return new Vector2(newState.X, newState.Y);
            }
        }

        /// <summary>
        /// Gets/sets mouse visibility
        /// </summary>
        public bool MouseVisible
        {
            get
            {
                return Game.IsMouseVisible;
            }
            set
            {
                Game.IsMouseVisible = value;
            }
        }

        #endregion Properties

        #region Constructors & Core

        public MouseManager(Game game, bool bMouseVisible, PhysicsManager physicsManager, Vector2 screenCentre)
            : base(game)
        {
            MouseVisible = bMouseVisible;
            this.physicsManager = physicsManager;
            this.SetPosition(screenCentre);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //store the old state
            oldState = newState;

            //get the new state
            newState = Mouse.GetState();

            base.Update(gameTime);
        }

        #region Button State

        /// <summary>
        /// Checks if left button is currently clicked
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsLeftButtonClicked()
        {
            return (newState.LeftButton.Equals(ButtonState.Pressed));
        }

        /// <summary>
        /// Checks if left button is currently clicked and was NOT clicked in the last update
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsLeftButtonClickedOnce()
        {
            return ((newState.LeftButton.Equals(ButtonState.Pressed)) && (!oldState.LeftButton.Equals(ButtonState.Pressed)));
        }

        /// <summary>
        /// Checks if middle button is currently clicked
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsMiddleButtonClicked()
        {
            return (newState.MiddleButton.Equals(ButtonState.Pressed));
        }

        /// <summary>
        /// Checks if middle button is currently clicked and was NOT clicked in the last update
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsMiddleButtonClickedOnce()
        {
            return ((newState.MiddleButton.Equals(ButtonState.Pressed)) && (!oldState.MiddleButton.Equals(ButtonState.Pressed)));
        }

        /// <summary>
        /// Checks if right button is currently clicked
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsRightButtonClickedOnce()
        {
            return ((newState.RightButton.Equals(ButtonState.Pressed)) && (!oldState.RightButton.Equals(ButtonState.Pressed)));
        }

        /// <summary>
        /// Checks if right button is currently clicked and was NOT clicked in the last update
        /// </summary>
        /// <returns>True if clicked, otherwise false</returns>
        public bool IsRightButtonClicked()
        {
            return (newState.RightButton.Equals(ButtonState.Pressed));
        }

        /// <summary>
        /// Checks if any button, scrollwheel, or mouse movement has taken place since last update
        /// </summary>
        /// <returns>True if state changed, otherwise false</returns>
        public bool IsStateChanged()
        {
            return (newState.Equals(oldState)) ? false : true;
        }

        #endregion Button State

        #region Scroll Wheel State

        /// <summary>
        /// Gets the current -ve/+ve scroll wheel value
        /// </summary>
        /// <returns>A positive or negative integer</returns>
        public int GetScrollWheelValue()
        {
            return newState.ScrollWheelValue;
        }

        /// <summary>
        /// Checks if the scroll wheel been moved since the last update
        /// </summary>
        /// <returns>True if the scroll wheel has been moved, otherwise false</returns>
        public int GetDeltaFromScrollWheel()
        {
            if (IsStateChanged()) //if state changed then return difference
            {
                return newState.ScrollWheelValue - oldState.ScrollWheelValue;
            }

            return 0;
        }

        #endregion Scroll Wheel State

        #region Position State

        /// <summary>
        /// Sets the mouse position
        /// </summary>
        /// <param name="position">User-defined Vector2 position on screen (i.e. between (0,0) and (width,height)</param>
        public void SetPosition(Vector2 position)
        {
            Mouse.SetPosition((int)position.X, (int)position.Y);
        }

        /// <summary>
        /// Checks if the mouse has moved moved more than <paramref name="mouseSensitivity"/> since the last update
        /// </summary>
        /// <param name="mouseSensitivity">An floating-point radius value of mouse sensitivity </param>
        /// <returns>True if the mouse has moved outside the <paramref name="mouseSensitivity"/> radius since last update, otherwise false</returns>
        public bool HasMoved(float mouseSensitivity)
        {
            float deltaPositionLength = new Vector2(newState.X - oldState.X,
                newState.Y - oldState.Y).Length();

            return (deltaPositionLength > mouseSensitivity) ? true : false;
        }

        ////did the mouse move above the limits of precision from centre position
        //public bool IsStateChangedOutsidePrecision(float mousePrecision)
        //{
        //    return ((Math.Abs(newState.X - oldState.X) > mousePrecision) || (Math.Abs(newState.Y - oldState.Y) > mousePrecision));
        //}

        /// <summary>
        /// Calculates the mouse pointer distance (in X and Y) from a user-defined target position
        /// </summary>
        /// <param name="target">Delta from this target position</param>
        /// <param name="activeCamera">Currently active camera</param>
        /// <returns>Vector2</returns>
        public Vector2 GetDeltaFromPosition(Vector2 target, Camera3D activeCamera)
        {
            Vector2 delta;
            //remember Position is the Property
            if (Position != target) //e.g. not the centre
            {
                if (activeCamera.View.Up.Y == -1)
                {
                    delta.X = 0;
                    delta.Y = 0;
                }
                else
                {
                    delta.X = Position.X - target.X;
                    delta.Y = Position.Y - target.Y;
                }
                SetPosition(target);
                return delta;
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Calculates the mouse pointer distance (in X and Y) from the screen centre (e.g. width/2, height/2)
        /// </summary>
        /// <param name="screenCentre">Delta from this screen centre position</param>
        /// <returns>Vector2</returns>
        public Vector2 GetDeltaFromCentre(Vector2 screenCentre)
        {
            return new Vector2(newState.X - screenCentre.X, newState.Y - screenCentre.Y);
        }

        #endregion Position State

        #endregion Constructors & Core

        #region Ray Picking
        private float frac;
        private CollisionSkin skin;

        /// <summary>
        /// Used when in 1st person collidable camera mode start distance allows us to start the ray outside the
        /// collidable skin of the 1st person colliable camera object, otherwise the only thing we would ever collide
        /// with would be ourselves!
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="screenPosition"></param>
        /// <param name="startDistance"></param>
        /// <param name="endDistance"></param>
        /// <param name="pos"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public Actor GetPickedObject(Camera3D camera, Vector2 screenPosition,
            float startDistance, float endDistance,
            out Vector3 pos, out Vector3 normal)
        {
            Vector3 ray = GetMouseRayDirection(camera, screenPosition);
            ImmovableSkinPredicate pred = new ImmovableSkinPredicate();

            physicsManager.PhysicsSystem.CollisionSystem.SegmentIntersect(out frac, out skin, out pos, out normal,
                new Segment(camera.Transform3D.Translation + startDistance * Vector3.Normalize(ray), ray * endDistance), pred);

            if (skin != null && skin.Owner != null)
            {
                return skin.Owner.ExternalData as Actor;
            }

            return null;
        }

        public Actor GetPickedObject(CameraManager<Camera3D> cameraManager, float distance, out Vector3 pos, out Vector3 normal)
        {
            return GetPickedObject(cameraManager.ActiveCamera, new Vector2(newState.X, newState.Y), 0, distance, out pos, out normal);
        }

        public Actor GetPickedObject(CameraManager<Camera3D> cameraManager, float startDistance, float distance, out Vector3 pos, out Vector3 normal)
        {
            return GetPickedObject(cameraManager.ActiveCamera, new Vector2(newState.X, newState.Y), startDistance, distance, out pos, out normal);
        }

        public Vector3 GetMouseRayDirection(Camera3D camera)
        {
            return GetMouseRayDirection(camera, new Vector2(newState.X, newState.Y));
        }

        /// <summary>
        /// Inner class used for ray picking
        /// </summary>
        internal class ImmovableSkinPredicate : CollisionSkinPredicate1
        {
            public override bool ConsiderSkin(CollisionSkin skin0)
            {
                if (skin0.Owner != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Get a ray positioned at the mouse's location on the screen - used for picking
        /// </summary>
        /// <param name="camera"></param>
        /// <returns></returns>
        public Microsoft.Xna.Framework.Ray GetMouseRay(Camera3D camera)
        {
            //get the positions of the mouse in screen space
            Vector3 near = new Vector3(newState.X, Position.Y, 0);

            //convert from screen space to world space
            near = camera.Viewport.Unproject(near, camera.Projection, camera.View, Matrix.Identity);

            return GetMouseRayFromNearPosition(camera, near);
        }

        /// <summary>
        /// Get a ray from a user-defined near position in world space and the mouse pointer
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="near"></param>
        /// <returns></returns>
        public Microsoft.Xna.Framework.Ray GetMouseRayFromNearPosition(Camera3D camera, Vector3 near)
        {
            //get the positions of the mouse in screen space
            Vector3 far = new Vector3(newState.X, Position.Y, 1);

            //convert from screen space to world space
            far = camera.Viewport.Unproject(far, camera.Projection, camera.View, Matrix.Identity);

            //generate a ray to use for intersection tests
            return new Microsoft.Xna.Framework.Ray(near, Vector3.Normalize(far - near));
        }

        /// <summary>
        /// Get a ray positioned at the screen position - used for picking when we have a centred reticule
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public Vector3 GetMouseRayDirection(Camera3D camera, Vector2 screenPosition)
        {
            //get the positions of the mouse in screen space
            Vector3 near = new Vector3(screenPosition.X, screenPosition.Y, 0);
            Vector3 far = new Vector3(Position, 1);

            //convert from screen space to world space
            near = camera.Viewport.Unproject(near, camera.Projection, camera.View, Matrix.Identity);
            far = camera.Viewport.Unproject(far, camera.Projection, camera.View, Matrix.Identity);

            //generate a ray to use for intersection tests
            return Vector3.Normalize(far - near);
        }

        #endregion Ray Picking
    }
}