using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using JigLibX.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary.Controllers
{
    /// <summary>
    /// First person COLLIDABLE camera controller.
    /// A collidable camera has a body and collision skin from a player object but it has no modeldata or texture
    /// </summary>
    public class CollidableFirstPersonCameraController : UserInputController
    {
        #region Fields
        private PlayerObject playerObject;
        private float radius, height;
        private float accelerationRate, decelerationRate, mass, jumpHeight;
        private Vector3 translationOffset;
        #endregion Fields

        #region Properties
        public PlayerObject PlayerObject { get => playerObject; set => playerObject = value; }
        public float Radius { get => radius; set => radius = value; }
        public float Height { get => height; set => height = value; }
        public float AccelerationRate { get => accelerationRate; set => accelerationRate = value; }
        public float DecelerationRate { get => decelerationRate; set => decelerationRate = value; }
        public float Mass { get => mass; set => mass = value; }
        public float JumpHeight { get => jumpHeight; set => jumpHeight = value; }
        public Vector3 TranslationOffset { get => translationOffset; set => translationOffset = value; }
        #endregion Properties

        //allows developer to specify the type of collidable object to be used as basis for the camera
        public CollidableFirstPersonCameraController(string id, ControllerType controllerType,
            KeyboardManager keyboardManager,
            MouseManager mouseManager,
            GamePadManager gamePadManager,
            Keys[] moveKeys, float moveSpeed, float strafeSpeed, float rotationSpeed,
            //below this is PlayerObject
            IActor parentActor, Vector3 translationOffset,
            float radius, float height, float accelerationRate, float decelerationRate, float mass, float jumpHeight)
            : base(id, controllerType,
            keyboardManager, mouseManager, gamePadManager,
            moveKeys, moveSpeed, strafeSpeed, rotationSpeed)
        {
            Radius = radius;
            Height = height;

            AccelerationRate = accelerationRate;
            DecelerationRate = decelerationRate;
            Mass = mass;
            JumpHeight = jumpHeight;

            //allows us to tweak the camera position within the player object
            TranslationOffset = translationOffset;

            //to do...instanciate player object
            playerObject = new PlayerObject(ID + " - player object",
                ActorType.CollidableCamera, StatusType.Update, (parentActor as Actor3D).Transform3D,
                null, null, //no model, no texture,
                MoveKeys, this.radius, this.height, this.accelerationRate, this.decelerationRate,
                this.jumpHeight, this.translationOffset, keyboardManager);

            playerObject.Enable(false, this.mass);

            //step 1 - add code to listen for CDCR event
            playerObject.Body.CollisionSkin.callbackFn += HandleCollision;
        }

        //step 2 - add a handler
        private bool HandleCollision(CollisionSkin collider, CollisionSkin collidee)
        {
            //step 3 - cast the collidee (the object you hit) to access its fields
            CollidableObject collidableObject = collidee.Owner.ExternalData as CollidableObject;

            //step 4 - make a decision on what you're going to do based on, say, the ActorType
            if (collidableObject.ActorType == ActorType.CollidableInventory)
            {
                //object[] parameters = { collidableObject };
                //EventDispatcher.Publish(new EventData(EventCategoryType.Object,
                //    EventActionType.OnRemoveActor, parameters));

                collidableObject.EffectParameters.DiffuseColor = Color.Red;
                collidableObject.EffectParameters.Alpha = 0.4f;

                object[] parameters = { collidableObject };
                EventDispatcher.Publish(new EventData(EventCategoryType.Opacity,
                   EventActionType.OnOpaqueToTransparent, parameters));
            }

            return true;
        }

        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            HandleForwardBackward(gameTime, parentActor);
            HandleStrafeLeftRight(gameTime, parentActor);
            HandleJumpCrouch(gameTime, parentActor);

            //move the camera to wherever the player object capsule is
            parentActor.Transform3D.Translation = playerObject.CharacterBody.Position + this.translationOffset;

            //   base.HandleKeyboardInput(gameTime, parentActor);
        }

        private void HandleForwardBackward(GameTime gameTime, Actor3D parentActor)
        {
            if (KeyboardManager.IsKeyDown(MoveKeys[0])) //[W]
            {
                Vector3 restrictedLook = parentActor.Transform3D.Look;
                restrictedLook.Y = 0;

                playerObject.CharacterBody.Velocity +=
                    MoveSpeed * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (KeyboardManager.IsKeyDown(MoveKeys[1])) //[S]
            {
                Vector3 restrictedLook = parentActor.Transform3D.Look;
                restrictedLook.Y = 0;

                playerObject.CharacterBody.Velocity -=
                    MoveSpeed * restrictedLook * gameTime.ElapsedGameTime.Milliseconds;
            }
            else //when we takes off forward or backward
            {
                playerObject.CharacterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        private void HandleStrafeLeftRight(GameTime gameTime, Actor3D parentActor)
        {
            //strafe left/right
            if (KeyboardManager.IsKeyDown(MoveKeys[2]))
            {
                Vector3 restrictedRight = parentActor.Transform3D.Right;
                restrictedRight.Y = 0;
                playerObject.CharacterBody.Velocity -= restrictedRight * StrafeSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (KeyboardManager.IsKeyDown(MoveKeys[3]))
            {
                Vector3 restrictedRight = parentActor.Transform3D.Right;
                restrictedRight.Y = 0;
                playerObject.CharacterBody.Velocity += restrictedRight * StrafeSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else //decelerate to zero when not pressed
            {
                playerObject.CharacterBody.DesiredVelocity = Vector3.Zero;
            }
        }

        private void HandleJumpCrouch(GameTime gameTime, Actor3D parentActor)
        {
            if (KeyboardManager.IsKeyDown(MoveKeys[4])) //check GameConstants.CameraMoveKeys for correct index of each move key
            {
                playerObject.CharacterBody.DoJump(jumpHeight);
            }
            //crouch
            else if (KeyboardManager.IsKeyDown(MoveKeys[5]))
            {
                playerObject.CharacterBody.IsCrouching = !playerObject.CharacterBody.IsCrouching;
            }
        }

        public override void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {
            Vector2 mouseDelta = MouseManager.GetDeltaFromCentre(new Vector2(512, 384)); //REFACTOR - NMCG
            mouseDelta *= RotationSpeed * gameTime.ElapsedGameTime.Milliseconds;

            if (mouseDelta.Length() != 0)
                parentActor.Transform3D.RotateBy(new Vector3(-1 * mouseDelta, 0));

            //  base.HandleMouseInput(gameTime, parentActor);
        }

        //to do - clone, dispose
    }
}