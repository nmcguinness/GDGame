using GDGame.Actors;
using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.GameComponents;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using System;

namespace GDLibrary.Managers
{
    public class PickingManager : PausableGameComponent
    {
        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private GamePadManager gamePadManager;
        private CameraManager<Camera3D> cameraManager;

        private float pickStartDistance;
        private float pickEndDistance;
        private Predicate<CollidableObject> collisionPredicate;

        public PickingManager(Game game, StatusType statusType,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            GamePadManager gamePadManager,
            CameraManager<Camera3D> cameraManager,
           float pickStartDistance, float pickEndDistance, Predicate<CollidableObject> collisionPredicate)
           : base(game, statusType)
        {
            this.keyboardManager = keyboardManager;
            this.mouseManager = mouseManager;
            this.gamePadManager = gamePadManager;
            this.cameraManager = cameraManager;

            this.pickStartDistance = pickStartDistance;
            this.pickEndDistance = pickEndDistance;
            this.collisionPredicate = collisionPredicate;
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            HandleMouse(gameTime);
            // base.ApplyUpdate(gameTime);
        }

        protected override void HandleMouse(GameTime gameTime)
        {
            if (mouseManager.IsLeftButtonClicked())
            {
                DoRemove();
            }
            else if (mouseManager.IsRightButtonClicked())
            {
                DoShowInfo();
            }
            else if (mouseManager.IsMiddleButtonClicked())
            {
                //to do...test...
                DoLift();
            }
            //base.HandleMouse(gameTime);
        }

        private Camera3D camera;
        private float cameraPickDistance;
        private bool bCurrentlyPicking;
        private CollidableObject currentPickedObject;
        private Vector3 pos, normal;
        private ConstraintWorldPoint objectController = new ConstraintWorldPoint();
        private ConstraintVelocity damperController = new ConstraintVelocity();

        private void DoLift()
        {
            if (!bCurrentlyPicking)
            {
                camera = cameraManager.ActiveCamera;
                currentPickedObject = mouseManager.GetPickedObject(cameraManager,
                    pickStartDistance, pickEndDistance, out pos, out normal) as CollidableObject;
                if (currentPickedObject != null)
                {
                    Vector3 vectorDeltaFromCentreOfMass = pos - currentPickedObject.Collision.Owner.Position;
                    vectorDeltaFromCentreOfMass = Vector3.Transform(vectorDeltaFromCentreOfMass, Matrix.Transpose(currentPickedObject.Collision.Owner.Orientation));
                    cameraPickDistance = (camera.Transform3D.Translation - pos).Length();

                    //remove any controller from any previous pick-release
                    objectController.Destroy();
                    damperController.Destroy();

                    currentPickedObject.Collision.Owner.SetActive();
                    //move object by pos (i.e. point of collision and not centre of mass)
                    objectController.Initialise(currentPickedObject.Collision.Owner, vectorDeltaFromCentreOfMass, pos);
                    //dampen velocity (linear and angular) on object to Zero
                    damperController.Initialise(currentPickedObject.Collision.Owner, ConstraintVelocity.ReferenceFrame.Body, Vector3.Zero, Vector3.Zero);
                    objectController.EnableConstraint();
                    damperController.EnableConstraint();
                    //we're picking a valid object for the first time
                    bCurrentlyPicking = true;
                }

                //if we have an object picked from the last update then move it according to the mouse pointer
                if (objectController.IsConstraintEnabled && (objectController.Body != null))
                {
                    // Vector3 delta = objectController.Body.Position - this.camera.Transform.Translation;
                    Vector3 direction = mouseManager.GetMouseRay(camera).Direction;
                    cameraPickDistance += mouseManager.GetDeltaFromScrollWheel() * 0.1f;

                    //set the desired world position
                    objectController.WorldPosition = camera.Transform3D.Translation + cameraPickDistance * direction;
                    objectController.Body.SetActive();
                }
            }
            else //releasing object
            {
                //release object from constraints and allow to behave as defined by gravity etc
                objectController.DisableConstraint();
                damperController.DisableConstraint();
            }
        }

        //private void DoLift()
        //{
        //    //       if (!this.bCurrentlyPicking)
        //    {
        //        this.camera = this.cameraManager.ActiveCamera;
        //        this.pickedObject = this.mouseManager.GetPickedObject(camera,
        //            new Vector2(512, 384),  //REFACTOR - NMCG
        //            pickStartDistance, pickEndDistance, out pos, out normal) as CollidableObject;

        //        this.distanceToObject = (float)Math.Round(Vector3.Distance(camera.Transform3D.Translation, pos),
        //            DefaultDistanceToTargetPrecision);

        //        //does this object return true in the predicate
        //        if (collisionPredicate(pickedObject))
        //        {
        //            Vector3 vectorDeltaFromCentreOfMass = pos - this.pickedObject.Collision.Owner.Position;
        //            vectorDeltaFromCentreOfMass = Vector3.Transform(vectorDeltaFromCentreOfMass, Matrix.Transpose(this.pickedObject.Collision.Owner.Orientation));
        //            cameraPickDistance = (this.cameraManager.ActiveCamera.Transform3D.Translation - pos).Length();

        //            //remove any controller from any previous pick-release
        //            objectController.Destroy();
        //            damperController.Destroy();

        //            this.pickedObject.Collision.Owner.SetActive();
        //            //move object by pos (i.e. point of collision and not centre of mass)
        //            this.objectController.Initialise(this.pickedObject.Collision.Owner, vectorDeltaFromCentreOfMass, pos);
        //            //dampen velocity (linear and angular) on object to Zero
        //            this.damperController.Initialise(this.pickedObject.Collision.Owner, ConstraintVelocity.ReferenceFrame.Body, Vector3.Zero, Vector3.Zero);
        //            this.objectController.EnableConstraint();
        //            this.damperController.EnableConstraint();

        //            //we're picking a valid object for the first time
        //            this.bCurrentlyPicking = true;

        //            //to do...send an event to say that we're picking an object
        //        }

        //        //if we have an object picked from the last update then move it according to the mouse pointer
        //        if (objectController.IsConstraintEnabled && (objectController.Body != null))
        //        {
        //            // Vector3 delta = objectController.Body.Position - this.cameraManager.ActiveCamera.Transform3D.Translation;
        //            Vector3 direction = this.mouseManager.GetMouseRay(this.cameraManager.ActiveCamera).Direction;
        //            cameraPickDistance += this.mouseManager.GetDeltaFromScrollWheel() * 0.1f;
        //            Vector3 result = this.cameraManager.ActiveCamera.Transform3D.Translation + cameraPickDistance * direction;
        //            //set the desired world position
        //            objectController.WorldPosition = this.cameraManager.ActiveCamera.Transform3D.Translation + cameraPickDistance * direction;
        //            objectController.Body.SetActive();
        //        }
        //    }
        //    //    else //releasing object
        //    {
        //        //release object from constraints and allow to behave as defined by gravity etc
        //        objectController.DisableConstraint();
        //        damperController.DisableConstraint();

        //        //we're not currently picking a valid object
        //        this.bCurrentlyPicking = false;

        //        //to do...send an event to say that we're not currently picking an object
        //    }
        //}

        private void DoShowInfo()
        {
            Vector3 pos, normal;
            CollidableObject pickedObject = mouseManager.GetPickedObject(cameraManager, 10, 1000, out pos, out normal) as CollidableObject;

            //does this object return true in the predicate
            if (collisionPredicate(pickedObject))
            {
                object[] parameters = { pickedObject };
                EventDispatcher.Publish(new EventData(EventCategoryType.UIPicking, EventActionType.OnObjectPicked, parameters));
            }
            else
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.UIPicking, EventActionType.OnNoObjectPicked, null));
            }
        }

        private void DoRemove()
        {
            Vector3 pos, normal;
            CollidableObject pickedObject = mouseManager.GetPickedObject(cameraManager, 10, 1000, out pos, out normal) as CollidableObject;

            //does this object return true in the predicate
            if (collisionPredicate(pickedObject))
            {
                object[] parameters = { pickedObject };
                EventDispatcher.Publish(new EventData(EventCategoryType.Object, EventActionType.OnRemoveActor, parameters));

                //publish other events...
                //play sound, increment player health, start countdown, increment UI
            }
        }
    }
}