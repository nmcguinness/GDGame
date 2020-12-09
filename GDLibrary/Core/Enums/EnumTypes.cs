﻿namespace GDLibrary.Enums
{
    public enum SoundCategoryType : sbyte
    {
        WinLose,
        Explosion,
        BackgroundMusic,
        Alarm
    }

    /// <summary>
    /// Used to indicate if single or multiple cameras are drawn to the screen at the same time
    /// </summary>
    /// <see cref="GDLibrary.Managers.RenderManager.ApplyDraw(Microsoft.Xna.Framework.GameTime)"/>
    public enum ScreenLayoutType : sbyte
    {
        Single,
        Multi
    }

    /// <summary>
    /// Actor types within the game (both drawn and undrawn)
    /// </summary>
    /// <see cref="GDLibrary.Actors.Actor.Actor(string, ActorType, StatusType)"/>
    public enum ActorType : sbyte
    {
        NonPlayer,
        Player,    //hero (rendered using Max/Maya file)
        Decorator, //architecture, obstacle (rendered using Max/Maya file)
        Primitive, //make this type using IVertexData

        Camera2D,
        Camera3D,

        Helper,

        CollidableGround,
        CollidablePickup,
        CollidableDecorator, //architecture
        CollidableCamera,
        CollidableInventory,
        UITextureObject,
        UIText,
        UIMouse,
    }

    /// <summary>
    /// Possible status types for an actor within the game (e.g. Update | Drawn, Update, Drawn, Off)
    /// </summary>
    /// <see cref="GDLibrary.Actors.Actor.Actor(string, ActorType, StatusType)"/>
    public enum StatusType
    {
        //used for enabling objects for updating and drawing e.g. a model or a camera, or a controller
        Drawn = 1,

        Update = 2,
        Off = 0, //neither drawn, nor updated e.g. the objectmanager when the menu is shown at startup - see Main::InitializeManagers()

        /*
        * Q. Why do we use powers of 2? Will it allow us to do anything different?
        * A. StatusType.Updated | StatusType.Drawn - See ObjectManager::Update() or Draw()
        */
    }

    /// <summary>
    /// Controller types to be applied to an actor (both drawn and undrawn) within the game
    /// </summary>
    /// <see cref="GDLibrary.Controllers.Controller.Controller(string, ControllerType)"/>
    public enum ControllerType
    {
        //camera specific
        FlightCamera,

        ThirdPerson,

        //applied to any Actor3D
        FirstPerson,

        Pan,
        Rail,
        Curve,

        AlphaCycle,
        SinTranslation,
        FirstPersonCollidable,
        RotationOverTime,
        ColorLerpOverTime,
        MouseOver,
        Progress,
        ScaleLerpOverTime
    }

    /// <summary>
    /// Alignment plane types for a surface within the game (e.g. a primitive object, such as a circle, is aligned with the XY plane)
    /// </summary>
    /// <see cref="GDLibrary.Factories.VertexFactory"/>
    public enum AlignmentPlaneType : sbyte
    {
        XY,
        XZ,
        YZ
    }

    /// <summary>
    /// Event categories within the game that a subscriber can subscribe to in the EventDispatcher
    /// </summary>
    /// <see cref="GDLibrary.Events.EventData"/>
    /// <seealso cref="GDLibrary.Events.EventDispatcher.Subscribe(EventCategoryType, Events.EventDispatcher.EventHandlerDelegate)"/>
    public enum EventCategoryType
    {
        Camera,
        Player,
        NonPlayer,
        Pickup,
        Sound,
        Menu,
        UI,
        Object,
        Opacity,
        UIPicking
        //add more here...
    }

    /// <summary>
    /// Event actions that can occur within a category (e.g. EventCategoryType.Sound with EventActionType.OnPlay)
    /// </summary>
    /// <see cref="GDLibrary.Events.EventData"/>
    /// <seealso cref="GDLibrary.Events.EventDispatcher.Subscribe(EventCategoryType, Events.EventDispatcher.EventHandlerDelegate)"/>
    public enum EventActionType
    {
        //sent by audio, video
        OnPlay,

        OnPause,
        OnResume,
        OnStop,
        OnStopAll,

        //processed by many managers (incl. menu, sound, object, ui, physic) and video controller
        OnStart,

        OnRestart,
        OnVolumeDelta,
        OnVolumeSet,
        OnMute,
        OnUnMute,
        OnExit,

        //send by mouse or gamepad manager
        OnClick,

        OnHover,

        //sent by camera manager
        OnCameraSetActive,

        OnCameraCycle,

        //sent by player when gains or loses health
        OnHealthDelta,

        //sent to set player health to a specific start/end value
        OnHealthSet,

        //sent by game state manager
        OnLose,

        OnWin,
        OnPickup,

        //sent whenever we change the opacity of a drawn object - remember ObjectManager has two draw lists (opaque and transparent)
        OnOpaqueToTransparent,

        OnTransparentToOpaque,

        //sent when we want to add/remove an Actor from the game - see ObjectManager::Remove()
        OnAddActor,

        OnRemoveActor,
        OnSpawn,
        OnPlay2D,
        OnPlay3D,
        OnObjectPicked,
        OnNoObjectPicked,

        //add more here...
    }
}