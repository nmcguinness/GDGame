using GDLibrary.Enums;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary.Actors
{
    /// <summary>
    /// Represents your MOVEABLE player in the game. Sub-class this class and add your own handlers for input.
    /// </summary>
    public class PlayerObject : CharacterObject
    {
        #region Variables
        private Keys[] moveKeys;
        private Vector3 translationOffset;
        private KeyboardManager keyboardManager;
        private float jumpHeight;
        #endregion Variables

        #region Properties

        public KeyboardManager KeyboardManager
        {
            get
            {
                return keyboardManager;
            }
        }

        public float JumpHeight
        {
            get
            {
                return jumpHeight;
            }
            set
            {
                jumpHeight = (value > 0) ? value : 1;
            }
        }

        public Vector3 TranslationOffset
        {
            get
            {
                return translationOffset;
            }
            set
            {
                translationOffset = value;
            }
        }

        public Keys[] MoveKeys
        {
            get
            {
                return moveKeys;
            }
            set
            {
                moveKeys = value;
            }
        }

        #endregion Properties

        public PlayerObject(string id, ActorType actorType, StatusType statusType,
            Transform3D transform,
            EffectParameters effectParameters, Model model,
            Keys[] moveKeys, float radius, float height, float accelerationRate,
            float decelerationRate, float jumpHeight,
            Vector3 translationOffset, KeyboardManager keyboardManager)
            : base(id, actorType, statusType, transform, effectParameters, model, radius, height, accelerationRate, decelerationRate)
        {
            this.moveKeys = moveKeys;
            this.translationOffset = translationOffset;
            this.keyboardManager = keyboardManager;
            this.jumpHeight = jumpHeight;
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform3D.Scale) *
                Collision.GetPrimitiveLocal(0).Transform.Orientation *
                Body.Orientation *
                Transform3D.Orientation *
                Matrix.CreateTranslation(Body.Position + translationOffset);
        }

        public override void Update(GameTime gameTime)
        {
            HandleKeyboardInput(gameTime);
            HandleMouseInput(gameTime);
            base.Update(gameTime);
        }

        protected virtual void HandleMouseInput(GameTime gameTime)
        {
            //perhaps rotate using mouse pointer distance from centre?
        }

        protected virtual void HandleKeyboardInput(GameTime gameTime)
        {
        }

        //add clone, equals, gethashcode, remove...
    }
}