using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Parameters;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.Actors
{
    public class PickupCollidableObject : CollidableObject
    {
        private int value;

        public int Value { get => value; set => this.value = value; }

        public PickupCollidableObject(string id, ActorType actorType, StatusType statusType,
            Transform3D transform, EffectParameters effectParameters, Model model, int value)
            : base(id, actorType, statusType, transform, effectParameters, model)
        {
            Value = value;

            //step 1 - add code to listen for CDCR event
            this.Body.CollisionSkin.callbackFn += HandleCollision;
        }

        //step 2 - add a handler
        private bool HandleCollision(CollisionSkin collider, CollisionSkin collidee)
        {
            //step 3 - cast the collidee (the object you hit) to access its fields
            CollidableObject collidableObject = collidee.Owner.ExternalData as CollidableObject;

            //step 4 - make a decision on what you're going to do based on, say, the ActorType
            if (collidableObject.ActorType == ActorType.CollidablePickup)
            {
            }

            return true;
        }

        public new object Clone()
        {
            return new PickupCollidableObject("clone - " + ID, //deep
                ActorType,   //deep
                StatusType,
                Transform3D.Clone() as Transform3D,  //deep
                EffectParameters.Clone() as EffectParameters, //hybrid - shallow (texture and effect) and deep (all other fields)
                Model,
                this.value); //shallow i.e. a reference
        }
    }
}