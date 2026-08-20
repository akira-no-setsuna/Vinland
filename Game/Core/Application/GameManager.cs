using System;
using System.Collections.Generic;
using Game.Core.Application.Input;
using Game.Core.Application.Render;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services;
using Game.Core.Logic;
using Game.Core.Physics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;

namespace Game.Core.Application;

public class GameManager : Microsoft.Xna.Framework.Game
{
    // Fixed Update
    private const float FIXED_DELTA_TIME = 1f / 60f;

    // Spawn entity texture
    private readonly Dictionary<Guid, VisualEntity> _entities = new();
    private float _accumulator;
    private OrthographicCamera _camera;
    private ChannelHub _channelHub;

    private BodyCollection _debugBodyList;
    private GraphicsDeviceManager _graphics;

    // Input
    private InputSource _inputSource;

    // Managers
    private LogicManager _logic;
    private PhysicsManager _physics;

    private PhysicsDebugRenderer _physicsDebug;

    private Guid _playerID;

    // DI
    private IServiceProvider _services;
    private SpriteBatch _spriteBatch;
    private GameThreadManager _threadManager;

    private Tilemap _tilemap;
    private TilemapRenderer _tilemapRenderer;

    public GameManager()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // DI
        _services = GameBootstrapper.ConfigureServices();
        _channelHub = _services.GetRequiredService<ChannelHub>();
        _threadManager = _services.GetRequiredService<GameThreadManager>();
        _threadManager.Start();

        Log.Information("=== Initializing game ===");

        // Managers
        _logic = new LogicManager(_channelHub);
        _physics = new PhysicsManager(_channelHub);

        _logic.Initialize();
        _physics.Initialize();

        // Camera
        _camera = new OrthographicCamera(GraphicsDevice)
        {
            Zoom = 2f,
            Position = Vector2.Zero
        };

        // Input
        _inputSource = new KbmInputSource(_channelHub.InputToLogic.Writer);

        // Draw
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Tilemap
        _tilemapRenderer = new TilemapRenderer(GraphicsDevice);

        _physicsDebug = new PhysicsDebugRenderer(GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Log.Information("=== Loading content ===");


        // Managers
        _logic.LoadContent();
        _physics.LoadContent();

        LogicReader();
        PhysicsReader();

        // Tilemap
        _tilemap = Content.Load<Tilemap>("maps/rooms/room_01");
        _tilemapRenderer.LoadTilemap(_tilemap);
        _channelHub.MainToPhysic.Writer.TryWrite(new GenerateMapColliders(_tilemap));
    }


    private void FixedUpdate()
    {
        _logic.FixedUpdate();
        _physics.FixedUpdate(FIXED_DELTA_TIME);

        LogicReader();
        PhysicsReader();
    }

    protected override void Update(GameTime gameTime)
    {
        // Input
        // TODO: Problem: Loss of quick keystrokes
        // TODO: Make instant reading of input for visual
        _inputSource.Update();

        _accumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;
        while (_accumulator >= FIXED_DELTA_TIME)
        {
            FixedUpdate();
            _accumulator -= FIXED_DELTA_TIME;
        }

        // Camera
        if (_playerID != Guid.Empty && _entities.TryGetValue(_playerID, out var player))
            _camera.LookAt(player.Position.ToScreen());

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.GetViewMatrix()
        );

        _tilemapRenderer.Draw(_camera);


        DrawEntitySprite();
#if DEBUG
        _physicsDebug.Draw(_debugBodyList, _camera.GetViewMatrix());
#endif
        _spriteBatch.End();
    }

    protected override void UnloadContent()
    {
        _physicsDebug.Dispose();
        base.UnloadContent();
    }

    private void LogicReader()
    {
        while (_channelHub.LogicToMain.Reader.TryRead(out var logicCommand))
            switch (logicCommand)
            {
                case TextureSpawn spawnEntity:
                    SpawnEntityTexture(spawnEntity);
                    break;
                case SetPlayer player:
                    _playerID = player.EntityID;
                    break;
                default:
                    Log.Warning("Logic command {cmd} not complied", logicCommand);
                    break;
            }
    }

    private void PhysicsReader()
    {
        while (_channelHub.PhysicsToMain.Reader.TryRead(out var physicsCommand))
            switch (physicsCommand)
            {
                case PositionUpdate command:
                    UpdatePositions(command);
                    break;
                case BodyListRender command:
                    _debugBodyList = command.BodyList;
                    break;
                default:
                    Log.Warning("Physics command {cmd} not complied", physicsCommand);
                    break;
            }
    }

    // Update visual entities positions
    private void UpdatePositions(PositionUpdate entityPosition)
    {
        if (_entities.TryGetValue(entityPosition.EntityID, out var visualEntity))
            visualEntity.Position = entityPosition.Position;
        else Log.Warning("EntityID: {id} visual not found", entityPosition.EntityID);
    }

    // Draw entities
    private void DrawEntitySprite()
    {
        foreach (var entity in _entities)
            _spriteBatch.Draw(entity.Value.Texture, entity.Value.Position.ToScreen(), Color.White);
    }

    private void SpawnEntityTexture(TextureSpawn spawn)
    {
        var texture = Content.Load<Texture2D>(spawn.EntityData.TextureKey);
        if (texture == null)
        {
            Log.Information("SpawnEntityTexture: ID = {id}, Pos = {pos}, TextureKey = {key}",
                spawn.EntityID, spawn.Position, spawn.EntityData.TextureKey);
            return;
        }

        var entity = new VisualEntity
        {
            Id = spawn.EntityID,
            Position = spawn.Position,
            Texture = texture
        };

        _entities.Add(entity.Id, entity);

        Log.Information("SpawnEntityTexture: ID = {id}, Pos = {pos}, TextureKey = {key}",
            spawn.EntityID, spawn.Position, spawn.EntityData.TextureKey);
    }
}