using GDLibrary.Enums;
using GDLibrary.Parameters;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary.Actors
{
    /// <summary>
    /// Allows us to draw a model and add a collision skin and physics response. Use this class to create collidable objects
    /// and add one or more of the 3 collision primitive types (i.e. Box, Sphere, Capsule). Unlike the TriangleMeshObject
    /// this class will allow an object to MOVE within the game (e.g. fall, by lifted and dropped, be pushed etc).
    /// </summary>
    public class CollidableObject : ModelObject
    {
        #region Variables
        private Body body;
        private CollisionSkin collision;
        private float mass;
        #endregion Variables

        #region Properties

        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }

        public CollisionSkin Collision
        {
            get
            {
                return collision;
            }
            set
            {
                collision = value;
            }
        }

        public Body Body
        {
            get
            {
                return body;
            }
            set
            {
                body = value;
            }
        }

        #endregion Properties

        public CollidableObject(string id, ActorType actorType, StatusType statusType, Transform3D transform, EffectParameters effectParameters, Model model)
            : base(id, actorType, statusType, transform, effectParameters, model)
        {
            body = new Body();
            body.ExternalData = this;
            collision = new CollisionSkin(body);
            body.CollisionSkin = collision;

            //we will only add this event handling in a class that sub-classes CollidableObject e.g. PickupCollidableObject or PlayerCollidableObject
            //this.body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        //we will only add this method in a class that sub-classes CollidableObject e.g. PickupCollidableObject or PlayerCollidableObject
        //private bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        //{
        //    return true;
        //}

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform3D.Scale) *
                    collision.GetPrimitiveLocal(0).Transform.Orientation *
                        body.Orientation *
                            Transform3D.Orientation *
                                Matrix.CreateTranslation(body.Position);
        }

        private float junk;
        private Vector3 com;
        private Matrix it, itCoM;

        protected Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProperties = new PrimitiveProperties(PrimitiveProperties.MassDistributionEnum.Solid, PrimitiveProperties.MassTypeEnum.Density, mass);
            collision.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            body.BodyInertia = itCoM;
            body.Mass = junk;

            return com;
        }

        public void AddPrimitive(Primitive primitive, MaterialProperties materialProperties)
        {
            collision.AddPrimitive(primitive, materialProperties);
        }

        public virtual void Enable(bool bImmovable, float mass)
        {
            this.mass = mass;

            //set whether the object can move
            body.Immovable = bImmovable;
            //calculate the centre of mass
            Vector3 com = SetMass(mass);
            //adjust skin so that it corresponds to the 3D mesh as drawn on screen
            body.MoveTo(Transform3D.Translation, Matrix.Identity);
            //set the centre of mass
            collision.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            //enable so that any applied forces (e.g. gravity) will affect the object
            body.EnableBody();
        }

        public new object Clone()
        {
            return new CollidableObject("clone - " + ID, //deep
                ActorType,   //deep
                StatusType,
                Transform3D.Clone() as Transform3D,  //deep
                EffectParameters.Clone() as EffectParameters, //hybrid - shallow (texture and effect) and deep (all other fields)
                Model); //shallow i.e. a reference
        }
    }
}