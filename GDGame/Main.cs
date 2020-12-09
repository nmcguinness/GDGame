﻿#define DEMO

using GDGame.Actors;
using GDGame.Controllers;
using GDGame.MyGame.Managers;
using GDLibrary.Actors;
using GDLibrary.Constants;
using GDLibrary.Containers;
using GDLibrary.Controllers;
using GDLibrary.Core.Controllers;
using GDLibrary.Debug;
using GDLibrary.Enums;
using GDLibrary.Events;
using GDLibrary.Factories;
using GDLibrary.Interfaces;
using GDLibrary.Managers;
using GDLibrary.Parameters;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace GDGame
{
    public class Main : Game
    {
        #region Fields

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //effects used by primitive objects (wireframe, lit, unlit) and model objects
        private BasicEffect unlitTexturedEffect, unlitWireframeEffect, modelEffect;

        private PickingManager pickingManager;

        //managers in the game
        private CameraManager<Camera3D> cameraManager;

        private ObjectManager objectManager;
        private KeyboardManager keyboardManager;
        private MouseManager mouseManager;
        private PhysicsManager physicsManager;
        private RenderManager renderManager;

        //store useful game resources (e.g. effects, models, rails and curves)
        private Dictionary<string, BasicEffect> effectDictionary;

        //use ContentDictionary to store assets (i.e. file content) that need the Content.Load() method to be called
        private ContentDictionary<Texture2D> textureDictionary;

        private ContentDictionary<SpriteFont> fontDictionary;
        private ContentDictionary<Model> modelDictionary;

        //use normal Dictionary to store objects that do NOT need the Content.Load() method to be called (i.e. the object is not based on an asset file)
        private Dictionary<string, Transform3DCurve> transform3DCurveDictionary;

        private Dictionary<string, RailParameters> railDictionary;

        //defines centre point for the mouse i.e. (w/2, h/2)
        private Vector2 screenCentre;

        private VertexPositionColorTexture[] vertices;

        #region Temp Vars Used For Demos
        private PrimitiveObject archetypalTexturedQuad;
        private PrimitiveObject primitiveObject = null;
        private ModelObject carModelObject;
        private EventDispatcher_OLD eventDispatcher;
        private EventDispatcher eventDispatcherV2;
#if DEMO
        private Curve1D curve1D;
        private UIManager uiManager;
        private MyMenuManager menuManager;
        private SoundManager soundManager;
#endif
        #endregion Temp Vars Used For Demos

        #endregion Fields

        #region Constructors

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        #endregion Constructors

        #region Demo
#if DEMO

        private void DemoEventDispatcherV2()
        {
            EventDispatcher.Subscribe(EventCategoryType.Camera, HandleCameraChangedV2);
            EventDispatcher.Subscribe(EventCategoryType.Player, HandlePlayerChangedV2);
        }

        private void HandlePlayerChangedV2(EventData eventData)
        {
            System.Diagnostics.Debug.WriteLine("HandlePlayerChangedV2:" + eventData);
        }

        private void HandleCameraChangedV2(EventData eventData)
        {
            System.Diagnostics.Debug.WriteLine("HandleCameraChangedV2:" + eventData);
        }

        private void DemoEventDispatcher()
        {
            eventDispatcher.CameraChanged += HandleCameraChanged;
            eventDispatcher.PlayerChanged += HandlePlayerChanged;
        }

        private void HandlePlayerChanged(string s)
        {
            System.Diagnostics.Debug.WriteLine("HandlePlayerChanged:" + s);
        }

        private void HandleCameraChanged(string s)
        {
            System.Diagnostics.Debug.WriteLine("HandleCameraChanged:" + s);
        }

        private void DemoCurve()
        {
            curve1D = new Curve1D(CurveLoopType.Oscillate);
            curve1D.Add(100, 2);
            curve1D.Add(250, 5);
            curve1D.Add(1500, 8);
        }

        public bool DoSomethingA(int x)
        {
            System.Diagnostics.Debug.WriteLine(x);
            return true;
        }

        public bool DoSomethingB(int x)
        {
            System.Diagnostics.Debug.WriteLine(x);
            return true;
        }

#endif
        #endregion Demo

        #region Debug
#if DEBUG

        private void InitDebug()
        {
            InitDebugInfo(true);
            InitializeDebugCollisionSkinInfo(false);
        }

        private void InitDebugInfo(bool bEnable)
        {
            if (bEnable)
            {
                //create the debug drawer to draw debug info
                DebugDrawer debugInfoDrawer = new DebugDrawer(this, _spriteBatch,
                    Content.Load<SpriteFont>("Assets/Fonts/debug"),
                    cameraManager, objectManager);

                //set the debug drawer to be drawn AFTER the object manager to the screen
                debugInfoDrawer.DrawOrder = 2;

                //add the debug drawer to the component list so that it will have its Update/Draw methods called each cycle.
                Components.Add(debugInfoDrawer);
            }
        }

        private void InitializeDebugCollisionSkinInfo(bool bEnable)
        {
            if (bEnable)
            {
                //show the collision skins
                PhysicsDebugDrawer physicsDebugDrawer = new PhysicsDebugDrawer(this, StatusType.Update | StatusType.Drawn,
                    cameraManager, objectManager);

                //set the debug drawer to be drawn AFTER the object manager to the screen
                physicsDebugDrawer.DrawOrder = 3;

                Components.Add(physicsDebugDrawer);

                //ObjectManager -> Debug -> UIManager -> MenuManager
            }
        }

#endif
        #endregion Debug

        #region Load - Assets

        private void LoadSounds()
        {
            soundManager.Add(new GDLibrary.Managers.Cue("smokealarm",
                Content.Load<SoundEffect>("Assets/Audio/Effects/smokealarm1"), SoundCategoryType.Alarm, new Vector3(1, 0, 0), false));

            //to do..add more sounds
        }

        private void LoadEffects()
        {
            //to do...
            unlitTexturedEffect = new BasicEffect(_graphics.GraphicsDevice);
            unlitTexturedEffect.VertexColorEnabled = true; //otherwise we wont see RGB
            unlitTexturedEffect.TextureEnabled = true;

            //wireframe primitives with no lighting and no texture
            unlitWireframeEffect = new BasicEffect(_graphics.GraphicsDevice);
            unlitWireframeEffect.VertexColorEnabled = true;

            //model effect
            //add a ModelObject
            modelEffect = new BasicEffect(_graphics.GraphicsDevice);
            modelEffect.TextureEnabled = true;
            modelEffect.LightingEnabled = true;
            modelEffect.PreferPerPixelLighting = true;
            modelEffect.EnableDefaultLighting();
        }

        private void LoadTextures()
        {
            //sky
            textureDictionary.Load("Assets/Textures/Skybox/back");
            textureDictionary.Load("Assets/Textures/Skybox/left");
            textureDictionary.Load("Assets/Textures/Skybox/right");
            textureDictionary.Load("Assets/Textures/Skybox/front");
            textureDictionary.Load("Assets/Textures/Skybox/sky");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");

            //demo
            textureDictionary.Load("Assets/Demo/Textures/checkerboard");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/progress_white");

            //props
            textureDictionary.Load("Assets/Textures/Props/Crates/crate1");

            //menu
            textureDictionary.Load("Assets/Textures/UI/Controls/genericbtn");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/mainmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/audiomenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/controlsmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenu");
            textureDictionary.Load("Assets/Textures/UI/Backgrounds/exitmenuwithtrans");

            //ui
            textureDictionary.Load("Assets/Textures/UI/Controls/reticuleDefault");

            //add more...
        }

        private void LoadModels()
        {
            //to do...
            modelDictionary.Load("Assets/Models/sphere");
            modelDictionary.Load("Assets/Models/box2");

            modelDictionary.Load("Assets/Models/teapot");
            modelDictionary.Load("Assets/Models/teapot_mediumpoly");
        }

        private void LoadFonts()
        {
            fontDictionary.Load("Assets/Fonts/debug");
            fontDictionary.Load("Assets/Fonts/menu");
            fontDictionary.Load("Assets/Fonts/ui");
        }

        private void LoadVertices()
        {
            /*
             * Note: These vertices were updated on 7.12.20 to fix a Skybox draw problem.
             * The vertices were properly ordered (i.e. 0-3) in order to ensure all NORMALS faced forward.
             */
            vertices = new VertexPositionColorTexture[4];

            float halfLength = 0.5f;
            //TL
            vertices[0] = new VertexPositionColorTexture(
                new Vector3(-halfLength, halfLength, 0),
                new Color(255, 255, 255, 255), new Vector2(0, 0));

            //TR
            vertices[1] = new VertexPositionColorTexture(
                new Vector3(halfLength, halfLength, 0),
                Color.White, new Vector2(1, 0));

            //BL
            vertices[2] = new VertexPositionColorTexture(
                new Vector3(-halfLength, -halfLength, 0),
                Color.White, new Vector2(0, 1));

            //BR
            vertices[3] = new VertexPositionColorTexture(
                new Vector3(halfLength, -halfLength, 0),
                Color.White, new Vector2(1, 1));
        }

        #endregion Load - Assets

        #region Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        protected override void Initialize()
        {
            float worldScale = 2000;
            //set game title
            Window.Title = "My Amazing Game";

            //graphic settings - see https://en.wikipedia.org/wiki/Display_resolution#/media/File:Vector_Video_Standards8.svg
            InitGraphics(1024, 768);

            //note that we moved this from LoadContent to allow InitDebug to be called in Initialize
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //create event dispatcher
            InitEventDispatcher();

            //managers
            InitManagers();

            //dictionaries
            InitDictionaries();

            //load from file or initialize assets, effects and vertices
            LoadEffects();
            LoadTextures();
            LoadVertices();
            LoadModels();
            LoadFonts();
            LoadSounds();

            //ui
            InitUI();
            InitMenu();

            //drawn non-collidable content
            InitNonCollidableDrawnContent(worldScale);

            //drawn collidable content
            InitCollidableDrawnContent(worldScale);

            //curves and rails used by cameras
            InitCurves();
            InitRails();

            //cameras - notice we moved the camera creation BELOW where we created the drawn content - see DriveController
            InitCameras3D();

            #region Debug & Demo
#if DEBUG
            //debug info
            InitDebug();
#endif
#if DEMO
            DemoCurve();
            DemoEventDispatcher();
            DemoEventDispatcherV2();
#endif
            #endregion Debug & Demo

            base.Initialize();
        }

        private void InitGraphics(int width, int height)
        {
            //set resolution
            _graphics.PreferredBackBufferWidth = width;
            _graphics.PreferredBackBufferHeight = height;

            //dont forget to apply resolution changes otherwise we wont see the new WxH
            _graphics.ApplyChanges();

            //set screen centre based on resolution
            screenCentre = new Vector2(width / 2, height / 2);

            //set cull mode to show front and back faces - inefficient but we will change later
            RasterizerState rs = new RasterizerState();
            rs.CullMode = CullMode.None;
            _graphics.GraphicsDevice.RasterizerState = rs;

            //we use a sampler state to set the texture address mode to solve the aliasing problem between skybox planes
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            _graphics.GraphicsDevice.SamplerStates[0] = samplerState;

            //set blending
            _graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //set screen centre for use when centering mouse
            screenCentre = new Vector2(width / 2, height / 2);
        }

        private void InitUI()
        {
            Transform2D transform2D = null;
            Texture2D texture = null;
            SpriteFont spriteFont = null;

            #region Mouse Reticule & Text
            texture = textureDictionary["reticuleDefault"];

            transform2D = new Transform2D(
                new Vector2(512, 384), //this value doesnt matter since we will recentre in UIMouseObject::Update()
                0,
                 Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(45, 46)); //read directly from the PNG file dimensions

            UIMouseObject uiMouseObject = new UIMouseObject("reticule", ActorType.UIMouse,
                StatusType.Update | StatusType.Drawn, transform2D, Color.White,
                SpriteEffects.None, fontDictionary["menu"],
                "Hello there!",
                new Vector2(0, -40),
                Color.Yellow,
                0.75f * Vector2.One,
                0,
                texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height), //how much of source image do we want to draw?
                this.mouseManager);

            uiManager.Add(uiMouseObject);
            #endregion Mouse Reticule & Text

            #region Progress Control Left
            texture = textureDictionary["progress_white"];

            transform2D = new Transform2D(new Vector2(512, 20),
                0,
                 Vector2.One,
                new Vector2(texture.Width / 2, texture.Height / 2),
                new Integer2(100, 100));

            UITextureObject uiTextureObject = new UITextureObject("progress 1", ActorType.UITextureObject,
                StatusType.Drawn | StatusType.Update, transform2D, Color.Yellow, 0, SpriteEffects.None,
                texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));

            //uiTextureObject.ControllerList.Add(new UIRotationController("rc1", ControllerType.RotationOverTime));

            //uiTextureObject.ControllerList.Add(new UIColorLerpController("clc1", ControllerType.ColorLerpOverTime,
            //    Color.White, Color.Black));

            //uiTextureObject.ControllerList.Add(new UIMouseController("moc1", ControllerType.MouseOver,
            //    this.mouseManager));

            uiTextureObject.ControllerList.Add(new UIProgressController("pc1", ControllerType.Progress, 0, 10));

            uiManager.Add(uiTextureObject);
            #endregion Progress Control Left

            #region Text Object
            spriteFont = Content.Load<SpriteFont>("Assets/Fonts/debug");

            //calculate how big the text is in (w,h)
            string text = "Hello World!!!";
            Vector2 originalDimensions = spriteFont.MeasureString(text);

            transform2D = new Transform2D(new Vector2(512, 768 - (originalDimensions.Y * 4)),
                0,
                4 * Vector2.One,
                new Vector2(originalDimensions.X / 2, originalDimensions.Y / 2), //this is text???
                new Integer2(originalDimensions)); //accurate original dimensions

            UITextObject uiTextObject = new UITextObject("hello", ActorType.UIText,
                StatusType.Update | StatusType.Drawn, transform2D, new Color(0.1f, 0, 0, 1),
                0, SpriteEffects.None, text, spriteFont);

            uiTextObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
                 mouseManager, Color.Red, Color.White));

            uiManager.Add(uiTextObject);
            #endregion Text Object
        }

        private void InitMenu()
        {
            Texture2D texture = null;
            Transform2D transform2D = null;
            DrawnActor2D uiObject = null;
            Vector2 fullScreenScaleFactor = Vector2.One;

            #region All Menu Background Images
            //background main
            texture = this.textureDictionary["exitmenuwithtrans"];
            fullScreenScaleFactor = new Vector2((float)this._graphics.PreferredBackBufferWidth / texture.Width, (float)this._graphics.PreferredBackBufferHeight / texture.Height);

            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("main_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.LightGreen, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            this.menuManager.Add("main", uiObject);

            //background audio
            texture = this.textureDictionary["audiomenu"];
            fullScreenScaleFactor = new Vector2((float)this._graphics.PreferredBackBufferWidth / texture.Width, (float)this._graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("audio_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            this.menuManager.Add("audio", uiObject);

            //background controls
            texture = this.textureDictionary["controlsmenu"];
            fullScreenScaleFactor = new Vector2((float)this._graphics.PreferredBackBufferWidth / texture.Width, (float)this._graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("controls_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            this.menuManager.Add("controls", uiObject);

            //background exit
            texture = this.textureDictionary["exitmenuwithtrans"];
            fullScreenScaleFactor = new Vector2((float)this._graphics.PreferredBackBufferWidth / texture.Width, (float)this._graphics.PreferredBackBufferHeight / texture.Height);
            transform2D = new Transform2D(fullScreenScaleFactor);
            uiObject = new UITextureObject("exit_bckgnd", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture, new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height));
            this.menuManager.Add("exit", uiObject);
            #endregion All Menu Background Images

            //main menu buttons
            texture = this.textureDictionary["genericbtn"];

            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            Integer2 imageDimensions = new Integer2(texture.Width, texture.Height);

            //play
            transform2D = new Transform2D(this.screenCentre - new Vector2(0, 50), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("play", ActorType.UITextureObject, StatusType.Drawn,
                transform2D, Color.White, 1, SpriteEffects.None, texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
                "Play",
                this.fontDictionary["menu"],
                new Vector2(1, 1),
                Color.Blue,
                new Vector2(0, 0));
            this.menuManager.Add("main", uiObject);

            //exit
            transform2D = new Transform2D(this.screenCentre + new Vector2(0, 50), 0, Vector2.One, origin, imageDimensions);
            uiObject = new UIButtonObject("exit", ActorType.UITextureObject,
                StatusType.Update | StatusType.Drawn,
             transform2D, Color.White, 1, SpriteEffects.None, texture,
             new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height),
             "Exit",
             this.fontDictionary["menu"],
             new Vector2(1, 1),
             Color.Blue,
             new Vector2(0, 0));

            uiObject.ControllerList.Add(new UIMouseOverController("moc1", ControllerType.MouseOver,
                 mouseManager, Color.Red, Color.White));

            uiObject.ControllerList.Add(new UIScaleLerpController("slc1", ControllerType.ScaleLerpOverTime,
              mouseManager, new TrigonometricParameters(0.02f, 1, 0)));

            this.menuManager.Add("main", uiObject);

            //finally dont forget to SetScene to say which menu should be drawn/updated!
            this.menuManager.SetScene("main");
        }

        private void InitEventDispatcher()
        {
            eventDispatcher = new EventDispatcher_OLD(this);
            Components.Add(eventDispatcher);

            eventDispatcherV2 = new EventDispatcher(this);
            Components.Add(eventDispatcherV2);
        }

        private void InitCurves()
        {
            //create the camera curve to be applied to the track controller
            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(0, 5, 100), -Vector3.UnitZ, Vector3.UnitY, 0); //start
            curveA.Add(new Vector3(0, 5, 80), new Vector3(1, 0, -1), Vector3.UnitY, 1000); //start position
            curveA.Add(new Vector3(0, 5, 50), -Vector3.UnitZ, Vector3.UnitY, 3000); //start position
            curveA.Add(new Vector3(0, 5, 20), new Vector3(-1, 0, -1), Vector3.UnitY, 4000); //start position
            curveA.Add(new Vector3(0, 5, 10), -Vector3.UnitZ, Vector3.UnitY, 6000); //start position

            //add to the dictionary
            transform3DCurveDictionary.Add("headshake1", curveA);
        }

        private void InitRails()
        {
            //create the track to be applied to the non-collidable track camera 1
            railDictionary.Add("rail1", new RailParameters("rail1 - parallel to z-axis", new Vector3(20, 10, 50), new Vector3(20, 10, -50)));
        }

        private void InitDictionaries()
        {
            //stores effects
            effectDictionary = new Dictionary<string, BasicEffect>();

            //stores textures, fonts & models
            modelDictionary = new ContentDictionary<Model>("models", Content);
            textureDictionary = new ContentDictionary<Texture2D>("textures", Content);
            fontDictionary = new ContentDictionary<SpriteFont>("fonts", Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            transform3DCurveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails - store rails used by cameras
            railDictionary = new Dictionary<string, RailParameters>();
        }

        private void InitManagers()
        {
            //physics and CD-CR (moved to top because MouseManager is dependent)
            physicsManager = new PhysicsManager(this, StatusType.Off, -9.81f * Vector3.UnitY);
            Components.Add(physicsManager);

            //camera
            cameraManager = new CameraManager<Camera3D>(this, StatusType.Off);
            Components.Add(cameraManager);

            //keyboard
            keyboardManager = new KeyboardManager(this);
            Components.Add(keyboardManager);

            //mouse
            mouseManager = new MouseManager(this, true, physicsManager, this.screenCentre);
            Components.Add(mouseManager);

            //object
            objectManager = new ObjectManager(this, StatusType.Off, 6, 10);
            Components.Add(objectManager);

            //render
            renderManager = new RenderManager(this, StatusType.Drawn, ScreenLayoutType.Single,
                objectManager, cameraManager);
            Components.Add(renderManager);

            //picking
            Predicate<CollidableObject> collisionPredicate
                    = (collidableObject) =>
                    {
                        if (collidableObject != null)
                        {
                            return collidableObject.ActorType == ActorType.CollidableInventory
                            || collidableObject.ActorType == ActorType.CollidablePickup;
                        }
                        else
                        {
                            return false;
                        }
                    };

            pickingManager = new PickingManager(this, StatusType.Off, keyboardManager, mouseManager,
                null, cameraManager, GameConstants.CollidableCameraCapsuleRadius * 1.5f, 1000,
                collisionPredicate);
            Components.Add(pickingManager);

            //add in-game ui
            uiManager = new UIManager(this, StatusType.Off, _spriteBatch, 10);
            uiManager.DrawOrder = 4;
            Components.Add(uiManager);

            //add menu
            menuManager = new MyMenuManager(this, StatusType.Update | StatusType.Drawn, _spriteBatch,
                this.mouseManager, this.keyboardManager);
            menuManager.DrawOrder = 5; //highest number of all drawable managers since we want it drawn on top!
            Components.Add(menuManager);

            //sound
            soundManager = new SoundManager(this, StatusType.Update);
            Components.Add(soundManager);
        }

        private void InitCameras3D()
        {
            Transform3D transform3D = null;
            Camera3D camera3D = null;
            Viewport viewPort = new Viewport(0, 0, 1024, 768);

            #region Collidable Camera - First Person

            transform3D = new Transform3D(new Vector3(15, 10, 30), Vector3.Zero, Vector3.Zero,
                -Vector3.UnitZ, Vector3.UnitY);

            camera3D = new Camera3D("Collidable 1st person",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen, new Viewport(0, 0, 1024, 768));

            //attach a controller
            camera3D.ControllerList.Add(new CollidableFirstPersonCameraController("cfpcc",
                ControllerType.FirstPersonCollidable,
                keyboardManager, mouseManager,
                null,
                GameConstants.CameraMoveKeys,
                GameConstants.CollidableCameraMoveSpeed,
                GameConstants.CollidableCameraStrafeSpeed,
                GameConstants.rotateSpeed,
                camera3D,
                new Vector3(0, 2, 0), //translation offset
                GameConstants.CollidableCameraCapsuleRadius,
                GameConstants.CollidableCameraViewHeight,
                2, 2, //accel, decel
                GameConstants.CollidableCameraMass,
                GameConstants.CollidableCameraJumpHeight));

            cameraManager.Add(camera3D);

            #endregion Collidable Camera - First Person

            #region Noncollidable Camera - First Person

            transform3D = new Transform3D(new Vector3(10, 10, 20),
                new Vector3(0, 0, -1), Vector3.UnitY);

            camera3D = new Camera3D("Noncollidable 1st person",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen, new Viewport(0, 0, 1024, 768));

            //attach a controller
            camera3D.ControllerList.Add(new FirstPersonController(
                "1st person controller A", ControllerType.FirstPerson,
                keyboardManager, mouseManager,
                GameConstants.moveSpeed, GameConstants.strafeSpeed, GameConstants.rotateSpeed));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - First Person

            #region Noncollidable Camera - Flight

            transform3D = new Transform3D(new Vector3(10, 10, 20),
                new Vector3(0, 0, -1), Vector3.UnitY);

            camera3D = new Camera3D("Noncollidable Flight",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen, new Viewport(0, 0, 1024, 768));

            //attach a controller
            camera3D.ControllerList.Add(new FlightCameraController(
                "Flight controller A", ControllerType.FlightCamera,
                keyboardManager, mouseManager, null,
                GameConstants.CameraMoveKeys,
                GameConstants.moveSpeed, GameConstants.strafeSpeed, GameConstants.rotateSpeed));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Flight

            #region Noncollidable Camera - Third Person

            //notice that it doesnt matter what translation, look, and up are since curve will set these
            transform3D = new Transform3D(Vector3.Zero,
                Vector3.Zero,
                Vector3.UnitY);

            camera3D = new Camera3D("Noncollidable 3rd person",
                ActorType.Camera3D, StatusType.Update, transform3D,
                ProjectionParameters.StandardDeepSixteenTen, viewPort);

            //attach a controller
            camera3D.ControllerList.Add(new ThirdPersonController("3rd person camera controller",
                ControllerType.ThirdPerson, carModelObject,
                165,
                50, LerpSpeed.VerySlow, mouseManager));

            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Third Person

            #region Noncollidable Camera - Security

            transform3D = new Transform3D(new Vector3(10, 10, 50),
                        new Vector3(0, 0, -1),
                        Vector3.UnitY);

            camera3D = new Camera3D("Noncollidable security",
                ActorType.Camera3D, StatusType.Update, transform3D,
            ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(new PanController(
                "pan controller", ControllerType.Pan,
                new Vector3(1, 1, 0), new TrigonometricParameters(30, GameConstants.mediumAngularSpeed, 0)));
            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Security

            #region Noncollidable Camera - Rail

            transform3D = new Transform3D(new Vector3(0, 250, 100),
                       new Vector3(-1, 0, 0), //look
                       new Vector3(0, 1, 0)); //up

            camera3D = new Camera3D("Noncollidable rail - final battle",
              ActorType.Camera3D, StatusType.Update, transform3D,
          ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(new RailController("rail controller - final battle 1",
           ControllerType.Rail,
           carModelObject,
           railDictionary["rail1"])); //use the rail dictionary to retrieve a rail by id

            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Rail

            #region Noncollidable Camera - Curve3D

            //notice that it doesnt matter what translation, look, and up are since curve will set these
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, Vector3.Zero);

            camera3D = new Camera3D("Noncollidable curve - main arena",
              ActorType.Camera3D, StatusType.Update, transform3D,
                        ProjectionParameters.StandardDeepSixteenTen, viewPort);

            camera3D.ControllerList.Add(
                new Curve3DController("main arena - fly through - 1",
                ControllerType.Curve,
                        transform3DCurveDictionary["headshake1"])); //use the curve dictionary to retrieve a transform3DCurve by id

            cameraManager.Add(camera3D);

            #endregion Noncollidable Camera - Curve3D

            cameraManager.ActiveCameraIndex = 0; //0, 1, 2, 3
        }

        #endregion Initialization - Graphics, Managers, Dictionaries, Cameras, Menu, UI

        #region Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        private void InitCollidableDrawnContent(float worldScale)
        {
            InitStaticCollidableGround(worldScale);

            InitDynamicCollidableBoxDemo();

            InitDynamicCollidableSphereDemo();

            InitStaticCollidableTriangleMeshObject();
        }

        private void InitDynamicCollidableSphereDemo()
        {
            /*
             * to do...
             * 1. 1 unit radius sphere around origin in max
             * 2. create CollidableModelObject and use new Sphere()
             * 3. Add a stack of 6 spheres along UnitY
             *
             * See InitDynamicCollidableBoxDemo()
             */

            CollidableObject archetypalCollidableSphere = null;
            CollidableObject cloneCollidableSphere = null;
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;

            effectParameters = new EffectParameters(modelEffect,
                  textureDictionary["checkerboard"],
                  Color.White, 1);

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            //make once then clone
            archetypalCollidableSphere = new CollidableObject("sphere - ", ActorType.CollidablePickup,
                                        StatusType.Update | StatusType.Drawn, transform3D, effectParameters,
                                        modelDictionary["sphere"]);

            int count = 0;
            for (int j = 0; j < 5; j++)
            {
                cloneCollidableSphere = archetypalCollidableSphere.Clone() as CollidableObject;
                cloneCollidableSphere.ID += count;
                count++;

                cloneCollidableSphere.Transform3D
                    = new Transform3D(new Vector3(-10, 20 + 5 * j, j),
                    new Vector3(0, 0, 0), 0.1f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

                cloneCollidableSphere.AddPrimitive(new Sphere(cloneCollidableSphere.Transform3D.Translation,
                    1), new MaterialProperties(0.2f, 0.8f, 0.7f));

                //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                cloneCollidableSphere.Enable(false, 1);
                objectManager.Add(cloneCollidableSphere);
            }
        }

        private void InitDynamicCollidableBoxDemo()
        {
            PickupCollidableObject archetypalCollidableBox = null;
            PickupCollidableObject cloneCollidableBox = null;
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;

            effectParameters = new EffectParameters(modelEffect,
                  textureDictionary["crate1"],
                  Color.White, 1);

            transform3D = new Transform3D(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY);

            //make once then clone
            archetypalCollidableBox = new PickupCollidableObject("box - ", ActorType.CollidableInventory,
                                        StatusType.Update | StatusType.Drawn, transform3D, effectParameters,
                                        modelDictionary["box2"],
                                        -1);

            int count = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    cloneCollidableBox = archetypalCollidableBox.Clone() as PickupCollidableObject;
                    cloneCollidableBox.ID += count;
                    count++;

                    cloneCollidableBox.Transform3D
                        = new Transform3D(new Vector3(30 + 4 * j, 5 + 10 * i, 0),
                        new Vector3(0, 0, 0), new Vector3(1, 1, 1), Vector3.UnitX, Vector3.UnitY);

                    cloneCollidableBox.AddPrimitive(new Box(cloneCollidableBox.Transform3D.Translation,
                        Matrix.Identity, /*important do not change - cm to inch*/
                        2.55f * cloneCollidableBox.Transform3D.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

                    //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                    cloneCollidableBox.Enable(false, 1);
                    objectManager.Add(cloneCollidableBox);
                }
            }
        }

        /// <summary>
        ///  Demos use of a low-polygon model to generate the triangle mesh collision skin - saving CPU cycles on CDCR checking
        /// </summary>
        private void InitStaticCollidableTriangleMeshObject()
        {
            CollidableObject collidableObject = null;
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;

            //Model model = null;
            //model = Content.Load<Model>("Assets/Models/teapot"); //try "teapot_lowpoly" and "teapot_mediumpoly"
            //Model lowPolyModel = Content.Load<Model>("Assets/Models/teapot_mediumpoly"); //try "teapot_lowpoly" and "teapot_mediumpoly"

            effectParameters = new EffectParameters(modelEffect,
                 textureDictionary["checkerboard"],
                  Color.White, 1);

            transform3D = new Transform3D(new Vector3(20, 5, 0),
                new Vector3(0, 0, 0), 0.1f * Vector3.One, -Vector3.UnitZ, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("teapot",
                ActorType.CollidableDecorator, StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters,
                        modelDictionary["teapot"],
                         modelDictionary["teapot_mediumpoly"],
                        new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        private void InitStaticCollidableGround(float worldScale)
        {
            CollidableObject collidableObject = null;
            Transform3D transform3D = null;
            EffectParameters effectParameters = null;
            effectParameters = new EffectParameters(modelEffect,
                  textureDictionary["grass1"],
                  Color.White, 1);

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.001f, worldScale), -Vector3.UnitZ, Vector3.UnitY);

            collidableObject = new CollidableObject("ground", ActorType.CollidableGround,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters,
                modelDictionary["box2"]);

            //focus on CDCR specific methods and parameters - plane, sphere, box, capsule
            collidableObject.AddPrimitive(new JigLibX.Geometry.Plane(transform3D.Up, transform3D.Translation),
                new MaterialProperties(0.8f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1); //change to false, see what happens.

            objectManager.Add(collidableObject);
        }

        //InitDrawnContent
        private void InitNonCollidableDrawnContent(float worldScale) //formerly InitPrimitives
        {
            //add archetypes that can be cloned
            InitPrimitiveArchetypes();

            //adds origin helper etc
            InitHelpers();

            //add skybox
            //size of the skybox and ground plane
            InitSkybox(worldScale);

            //add grass plane
            //InitGround();   //old non-collidable grass plane that we replaced when we added CDCR

            //models
            InitStaticModels();
        }

        private void InitStaticModels()
        {
            //transform
            Transform3D transform3D = new Transform3D(new Vector3(0, 5, 0),
                                new Vector3(0, 0, 0),       //rotation
                                new Vector3(1, 1, 4),        //scale
                                    -Vector3.UnitZ,         //look
                                    Vector3.UnitY);         //up

            //effectparameters
            EffectParameters effectParameters = new EffectParameters(modelEffect,
                textureDictionary["crate1"],
                Color.White, 1);

            //model object
            carModelObject = new ModelObject("car", ActorType.Player,
                StatusType.Drawn | StatusType.Update, transform3D,
                effectParameters,
                modelDictionary["box2"]);

            #region Controllers
            //add the controller to drive the car
            carModelObject.ControllerList.Add(new DriveController(
                "fp - car - controller", ControllerType.FirstPerson,
                keyboardManager,
                GameConstants.carMoveSpeed,
                GameConstants.carRotateSpeed));

            //add more here...
            #endregion Controllers

            #region Events
            //add an event handler to listen for EventCategoryType.Player events
            PlayerSpawnEventHandler playerSpawnEventHandler
                = new PlayerSpawnEventHandler(EventCategoryType.Player, carModelObject);

            PlayerSpawnEventHandler playerSpawnEventHandler2
                = new PlayerSpawnEventHandler(EventCategoryType.Player, carModelObject);

            //add the event handler into the list
            carModelObject.EventHandlerList.Add(playerSpawnEventHandler);

            //add more here...
            #endregion Events

            //dont forget to add to the object manager
            objectManager.Add(carModelObject);
        }

        private void InitPrimitiveArchetypes() //formerly InitTexturedQuad
        {
            Transform3D transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
               Vector3.One, Vector3.UnitZ, Vector3.UnitY);

            EffectParameters effectParameters = new EffectParameters(unlitTexturedEffect,
                textureDictionary["grass1"], Color.White, 1);

            IVertexData vertexData = new VertexData<VertexPositionColorTexture>(
                vertices, Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleStrip, 2);

            archetypalTexturedQuad = new PrimitiveObject("texture quad",
                ActorType.Decorator,
                StatusType.Update | StatusType.Drawn,
                transform3D, effectParameters, vertexData);
        }

        //VertexPositionColorTexture - 4 bytes x 3 (x,y,z) + 4 bytes x 3 (r,g,b) + 4bytes x 2 = 26 bytes
        //VertexPositionColor -  4 bytes x 3 (x,y,z) + 4 bytes x 3 (r,g,b) = 24 bytes
        private void InitHelpers()
        {
            //to do...add wireframe origin
            Microsoft.Xna.Framework.Graphics.PrimitiveType primitiveType;
            int primitiveCount;

            //step 1 - vertices
            VertexPositionColor[] vertices = VertexFactory.GetVerticesPositionColorOriginHelper(
                                    out primitiveType, out primitiveCount);

            //step 2 - make vertex data that provides Draw()
            IVertexData vertexData = new VertexData<VertexPositionColor>(vertices,
                                    primitiveType, primitiveCount);

            //step 3 - make the primitive object
            Transform3D transform3D = new Transform3D(new Vector3(0, 20, 0),
                Vector3.Zero, new Vector3(10, 10, 10),
                Vector3.UnitZ, Vector3.UnitY);

            EffectParameters effectParameters = new EffectParameters(unlitWireframeEffect,
                null, Color.White, 1);

            //at this point, we're ready!
            PrimitiveObject primitiveObject = new PrimitiveObject("origin helper",
                ActorType.Helper, StatusType.Drawn, transform3D, effectParameters, vertexData);

            objectManager.Add(primitiveObject);
        }

        private void InitSkybox(float worldScale)
        {
            //back
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            //  primitiveObject.StatusType = StatusType.Off; //Experiment of the effect of StatusType
            primitiveObject.ID = "sky back";
            primitiveObject.EffectParameters.Texture = textureDictionary["back"]; ;
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            primitiveObject.Transform3D.Translation = new Vector3(0, 0, -worldScale / 2.0f);
            objectManager.Add(primitiveObject);

            //left
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            primitiveObject.ID = "left back";
            primitiveObject.EffectParameters.Texture = textureDictionary["left"]; ;
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            primitiveObject.Transform3D.RotationInDegrees = new Vector3(0, 90, 0);
            primitiveObject.Transform3D.Translation = new Vector3(-worldScale / 2.0f, 0, 0);
            objectManager.Add(primitiveObject);

            //right
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            primitiveObject.ID = "sky right";
            primitiveObject.EffectParameters.Texture = textureDictionary["right"];
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 20);
            primitiveObject.Transform3D.RotationInDegrees = new Vector3(0, -90, 0);
            primitiveObject.Transform3D.Translation = new Vector3(worldScale / 2.0f, 0, 0);
            objectManager.Add(primitiveObject);

            //top
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            primitiveObject.ID = "sky top";
            primitiveObject.EffectParameters.Texture = textureDictionary["sky"];
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            primitiveObject.Transform3D.RotationInDegrees = new Vector3(90, -90, 0);
            primitiveObject.Transform3D.Translation = new Vector3(0, worldScale / 2.0f, 0);
            objectManager.Add(primitiveObject);

            //front
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            primitiveObject.ID = "sky front";
            primitiveObject.EffectParameters.Texture = textureDictionary["front"];
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            primitiveObject.Transform3D.RotationInDegrees = new Vector3(0, 180, 0);
            primitiveObject.Transform3D.Translation = new Vector3(0, 0, worldScale / 2.0f);
            objectManager.Add(primitiveObject);
        }

        private void InitGround(float worldScale)
        {
            //grass
            primitiveObject = archetypalTexturedQuad.Clone() as PrimitiveObject;
            primitiveObject.ID = "grass";
            primitiveObject.EffectParameters.Texture = textureDictionary["grass1"];
            primitiveObject.Transform3D.Scale = new Vector3(worldScale, worldScale, 1);
            primitiveObject.Transform3D.RotationInDegrees = new Vector3(90, 90, 0);
            objectManager.Add(primitiveObject);
        }

        #endregion Initialization - Vertices, Archetypes, Helpers, Drawn Content(e.g. Skybox)

        #region Load & Unload Game Assets

        protected override void LoadContent()
        {
        }

        protected override void UnloadContent()
        {
            //housekeeping - unload content
            textureDictionary.Dispose();
            modelDictionary.Dispose();
            fontDictionary.Dispose();
            modelDictionary.Dispose();
            soundManager.Dispose();

            base.UnloadContent();
        }

        #endregion Load & Unload Game Assets

        #region Update & Draw

        protected override void Update(GameTime gameTime)
        {
            if (keyboardManager.IsFirstKeyPress(Keys.Escape))
            {
                Exit();
            }

#if DEMO
            #region Sound Demos
            if (keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                // soundManager.Play2D("smokealarm");

                object[] parameters = { "smokealarm" };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay2D, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F2))
            {
                soundManager.Pause("smokealarm");

                object[] parameters = { "smokealarm" };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPause, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F3))
            {
                soundManager.Stop("smokealarm");

                object[] parameters = { "smokealarm" };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnStop, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F4))
            {
                soundManager.SetMasterVolume(0);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                soundManager.SetMasterVolume(0.5f);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                AudioListener listener = new AudioListener();
                listener.Position = new Vector3(0, 5, 50);
                listener.Forward = -Vector3.UnitZ;
                listener.Up = Vector3.UnitY;

                AudioEmitter emitter = new AudioEmitter();
                emitter.DopplerScale = 1;
                emitter.Position = new Vector3(0, 5, 0);
                emitter.Forward = Vector3.UnitZ;
                emitter.Up = Vector3.UnitY;

                object[] parameters = { "smokealarm", listener, emitter };
                EventDispatcher.Publish(new EventData(EventCategoryType.Sound,
                    EventActionType.OnPlay3D, parameters));
            }
            #endregion Sound Demos

            if (keyboardManager.IsFirstKeyPress(Keys.F9))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.Up))
            {
                object[] parameters = { 1 }; //will increase the progress by 1 to its max of 10 (see InitUI)
                EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.Down))
            {
                object[] parameters = { -1 }; //will decrease the progress by 1 to its min of 0 (see InitUI)
                EventDispatcher.Publish(new EventData(EventCategoryType.UI, EventActionType.OnHealthDelta, parameters));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.F3))
            {
                Vector3 pos, normal;

                CollidableObject pickedObject
                    = mouseManager.GetPickedObject(cameraManager, 10, 1000, out pos, out normal) as CollidableObject;

                if (pickedObject != null)
                {
                    System.Diagnostics.Debug.WriteLine(pickedObject);
                }
            }

            if (keyboardManager.IsFirstKeyPress(Keys.F5)) //game -> menu
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPlay, null));
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F6)) //menu -> game
            {
                EventDispatcher.Publish(new EventData(EventCategoryType.Menu, EventActionType.OnPause, null));
            }

            #region Demo
            //System.Diagnostics.Debug.WriteLine("t in ms:" + gameTime.TotalGameTime.TotalMilliseconds + " v: " + curve1D.Evaluate(gameTime.TotalGameTime.TotalMilliseconds, 2));

            if (keyboardManager.IsFirstKeyPress(Keys.P))
            {
                // object[] parameters = { "new player", "sniper", new Vector3(5, 5, 10) };

                ModelObject cloneCarModelObject = carModelObject.Clone() as ModelObject;
                cloneCarModelObject.Transform3D.TranslateBy(new Vector3(10, 0, 0));

                object[] parameters = { cloneCarModelObject };

                EventDispatcher.Publish(new EventData(EventCategoryType.Player,
                  EventActionType.OnSpawn, parameters));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.X))
            {
                object[] parameters = { carModelObject };

                EventDispatcher.Publish(new EventData(EventCategoryType.Object,
                  EventActionType.OnRemoveActor, parameters));
            }
            #endregion Demo

            #region Camera
            if (keyboardManager.IsFirstKeyPress(Keys.C))
            {
                cameraManager.CycleActiveCamera();
                EventDispatcher.Publish(new EventData(EventCategoryType.Camera,
                    EventActionType.OnCameraCycle, null));
            }

            if (keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                EventDispatcher.Subscribe(EventCategoryType.Camera, HandleCameraChangedV2);
            }
            else if (keyboardManager.IsFirstKeyPress(Keys.F2))
            {
                EventDispatcher.Unsubscribe(EventCategoryType.Camera, HandleCameraChangedV2);
            }

            #endregion Camera
#endif

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }

        #endregion Update & Draw
    }
}