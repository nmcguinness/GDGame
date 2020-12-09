using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using JigLibX.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDGame.MyGame.Actors
{
    public class HeroPlayerObject : PlayerObject
    {
        public HeroPlayerObject(string id, ActorType actorType, StatusType statusType,
            Transform3D transform, EffectParameters effectParameters, Model model,
            Keys[] moveKeys, float radius, float height, float accelerationRate,
            float decelerationRate, float jumpHeight, Vector3 translationOffset,
            KeyboardManager keyboardManager)
            : base(id, actorType, statusType, transform, effectParameters, model, moveKeys,
                  radius, height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //step 1 - add code to listen for CDCR event
            this.Body.CollisionSkin.callbackFn += HandleCollision;
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

        protected override void HandleKeyboardInput(GameTime gameTime)
        {
            if (this.KeyboardManager.IsFirstKeyPress(Keys.J))
                this.CharacterBody.DoJump(50);

            base.HandleKeyboardInput(gameTime);
        }

        protected override void HandleMouseInput(GameTime gameTime)
        {
            base.HandleMouseInput(gameTime);
        }
    }
}