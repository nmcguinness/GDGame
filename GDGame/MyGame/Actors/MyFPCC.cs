using GDLibrary;
using GDLibrary.Actors;
using GDLibrary.Controllers;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using JigLibX.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDGame.MyGame.Actors
{
    public class MyFPCC : CollidableFirstPersonCameraController
    {
        public MyFPCC(string id, ControllerType controllerType, KeyboardManager keyboardManager,
            MouseManager mouseManager, GamePadManager gamePadManager, Keys[] moveKeys, float moveSpeed,
            float strafeSpeed, float rotationSpeed, IActor parentActor, Vector3 translationOffset,
            float radius, float height, float accelerationRate, float decelerationRate, float mass,
            float jumpHeight)
            : base(id, controllerType, keyboardManager, mouseManager, gamePadManager, moveKeys,
                  moveSpeed, strafeSpeed, rotationSpeed, parentActor, translationOffset,
                  radius, height, accelerationRate, decelerationRate, mass, jumpHeight)
        {
            //step 1 - add code to listen for CDCR event
            this.PlayerObject.Body.CollisionSkin.callbackFn += HandleCollision;
        }

        //step 2 - add a handler
        private bool HandleCollision(CollisionSkin collider, CollisionSkin collidee)
        {
            //step 3 - cast the collidee (the object you hit) to access its fields
            CollidableObject collidableObject = collidee.Owner.ExternalData as CollidableObject;

            //step 4 - make a decision on what you're going to do based on, say, the ActorType
            if (collidableObject.ActorType == ActorType.CollidableInventory)
            {
                //       object[] parameters = { collidableObject };
                //      EventDispatcher.Publish(new EventData(EventCategoryType.Object,
                //         EventActionType.OnRemoveActor, parameters));

                //  collidableObject.EffectParameters.Alpha = 0.5f;
            }

            return true;
        }
    }
}