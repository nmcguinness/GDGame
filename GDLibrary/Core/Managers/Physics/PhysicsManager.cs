using GDLibrary.Actors;
using GDLibrary.Controllers;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.GameComponents;
using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using System;

//Physics - Step 2
namespace GDLibrary.Managers
{
    /// <summary>
    /// Enables game physics and CD/cR by integrating forces applied to each collidable object within the scene
    /// </summary>
    public class PhysicsManager : PausableGameComponent
    {
        #region Fields
        private PhysicsSystem physicSystem;
        private PhysicsController physCont;
        private float timeStep = 0;
        #endregion Fields

        #region Properties

        public PhysicsSystem PhysicsSystem
        {
            get
            {
                return physicSystem;
            }
        }

        public PhysicsController PhysicsController
        {
            get
            {
                return physCont;
            }
        }

        #endregion Properties

        //gravity pre-defined
        public PhysicsManager(Game game, StatusType statusType)
            : this(game, statusType, -10 * Vector3.UnitY)
        {
        }

        protected override void SubscribeToEvents()
        {
            //remove
            EventDispatcher.Subscribe(EventCategoryType.Object, HandleEvent);

            base.SubscribeToEvents();
        }

        protected override void HandleEvent(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.Object)
            {
                HandleObjectCategoryEvent(eventData);
            }

            base.HandleEvent(eventData);
        }

        private void HandleObjectCategoryEvent(EventData eventData)
        {
            if (eventData.EventActionType == EventActionType.OnRemoveActor)
            {
                CollidableObject collidableObject = eventData.Parameters[0] as CollidableObject;

                this.PhysicsSystem.RemoveBody(collidableObject.Body);
            }
        }

        //user-defined gravity
        public PhysicsManager(Game game, StatusType statusType, Vector3 gravity)
            : base(game, statusType)
        {
            physicSystem = new PhysicsSystem();

            //add cd/cr system
            physicSystem.CollisionSystem = new CollisionSystemSAP();

            //allows us to define the direction and magnitude of gravity - default is (0, -9.8f, 0)
            physicSystem.Gravity = gravity;

            //prevents bug where objects would show correct CDCR response when velocity == Vector3.Zero
            physicSystem.EnableFreezing = false;

            physicSystem.SolverType = PhysicsSystem.Solver.Normal;
            physicSystem.CollisionSystem.UseSweepTests = true;

            //affect accuracy and the overhead == time required
            physicSystem.NumCollisionIterations = 8;
            physicSystem.NumContactIterations = 8;
            physicSystem.NumPenetrationRelaxtionTimesteps = 12;

            #region SETTING_COLLISION_ACCURACY
            //affect accuracy of the collision detection
            physicSystem.AllowedPenetration = 0.00025f;
            physicSystem.CollisionTollerance = 0.0005f;
            #endregion SETTING_COLLISION_ACCURACY

            physCont = new PhysicsController();
            physicSystem.AddController(physCont);
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //if the time between updates indicates a FPS of close to 60 fps or less then update CD/CR engine
            if (timeStep < 1.0f / 60.0f)
            {
                physicSystem.Integrate(timeStep);
            }
            else
            {
                //else fix at 60 updates per second
                physicSystem.Integrate(1.0f / 60.0f);
            }
        }
    }
}