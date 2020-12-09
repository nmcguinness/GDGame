﻿using GDLibrary.Actors;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDGame.MyGame.Managers
{
    public class MyMenuManager : MenuManager
    {
        private MouseManager mouseManager;
        private KeyboardManager keyboardManager;

        public MyMenuManager(Game game, StatusType statusType, SpriteBatch spriteBatch,
            MouseManager mouseManager, KeyboardManager keyboardManager)
            : base(game, statusType, spriteBatch)
        {
            this.mouseManager = mouseManager;
            this.keyboardManager = keyboardManager;
        }

        protected override void HandleEvent(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.Menu)
            {
                if (eventData.EventActionType == EventActionType.OnPause)
                    this.StatusType = StatusType.Drawn | StatusType.Update;
                else if (eventData.EventActionType == EventActionType.OnPlay)
                    this.StatusType = StatusType.Off;
            }
        }

        protected override void HandleInput(GameTime gameTime)
        {
            //bug fix - 7.12.20 - Exit button was hidden but we were still testing for mouse click
            if ((this.StatusType & StatusType.Update) != 0)
            {
                HandleMouse(gameTime);
            }

            HandleKeyboard(gameTime);
            //base.HandleInput(gameTime); //nothing happening in the base method
        }

        protected override void HandleMouse(GameTime gameTime)
        {
            foreach (DrawnActor2D actor in this.ActiveList)
            {
                if (actor is UIButtonObject)
                {
                    if (actor.Transform2D.Bounds.Contains(this.mouseManager.Bounds))
                    {
                        if (this.mouseManager.IsLeftButtonClickedOnce())
                        {
                            HandleClickedButton(gameTime, actor as UIButtonObject);
                        }
                    }
                }
            }
            base.HandleMouse(gameTime);
        }

        private void HandleClickedButton(GameTime gameTime, UIButtonObject uIButtonObject)
        {
            //benefit of switch vs if...else is that code will jump to correct switch case directly
            switch (uIButtonObject.ID)
            {
                case "play":
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
                    break;

                case "controls":
                    this.SetScene("controls");
                    break;

                case "exit":
                    this.Game.Exit();
                    break;

                default:
                    break;
            }
        }

        protected override void HandleKeyboard(GameTime gameTime)
        {
            if (this.keyboardManager.IsFirstKeyPress(Microsoft.Xna.Framework.Input.Keys.M))
            {
                if (this.StatusType == StatusType.Off)
                {
                    //show menu
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
                }
                else
                {
                    //show game
                    EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
                }
            }

            base.HandleKeyboard(gameTime);
        }
    }
}