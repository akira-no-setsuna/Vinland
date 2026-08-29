using System;
using System.Collections.Generic;
using Game.Core.Application;
using Game.Core.Application.Render;
using Game.Core.Data;
using Game.Core.Infrastructure;
using Game.Core.Infrastructure.Channels;
using Game.Core.Infrastructure.Channels.Commands;
using Game.Core.Infrastructure.Services;
using Game.Core.Infrastructure.Services.Threads;
using Game.Core.Logic;
using Game.Core.Main.Input;
using Game.Core.Physics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;

namespace Game.Core.Main;

public class GameManager : Microsoft.Xna.Framework.Game
{
    // Spawn entity texture
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<Guid, VisualEntity> _entities = new();
    
    private OrthographicCamera _camera;
    private ChannelHub _channelHub;

    private BodyCollection _debugBodyList;
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Input
    private InputSource _inputSource;

    // Managers
    private BaseThread _logic;
    private BaseThread _physics;
    private DataManager _data;

    private PhysicsDebugRenderer _physicsDebug;

    private Guid _playerID;

    // DI
    private IServiceProvider _services;
    private GameThreadManager _threadManager;



    private Tilemap _tilemap;
    private TilemapRenderer _tilemapRenderer;
    
    // Game Clock
    private GameClock _gameClock;

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
        
        _logic = _services.GetRequiredService<LogicManager>();
        _physics = _services.GetRequiredService<PhysicsManager>();
        _data = _services.GetRequiredService<DataManager>();
        
        _channelHub = _services.GetRequiredService<ChannelHub>();
        _threadManager = _services.GetRequiredService<GameThreadManager>();

        
        _threadManager.Start();
        
        _data.Start();
        _logic.Start();
        _physics.Start();
        

        Log.Information("=== Initializing game ===");

        _gameClock =  new GameClock();

        // Camera
        _camera = new OrthographicCamera(GraphicsDevice)
        {
            Zoom = 2f,
            Position = Vector2.Zero
        };

        // Input
        _inputSource = new KbmInputSource(_channelHub);

        // Draw
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Tilemap
        _tilemapRenderer = new TilemapRenderer(GraphicsDevice);
        
        _physicsDebug = new PhysicsDebugRenderer(_spriteBatch, GraphicsDevice);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Log.Information("=== Loading content ===");

        // Tilemap
        _tilemap = Content.Load<Tilemap>("maps/rooms/room_01");
        _tilemapRenderer.LoadTilemap(_tilemap);
        _channelHub.MainToPhysic.Writer.TryWrite(new GenerateMapColliders(_gameClock.CurrentTick, _tilemap));
    }

    protected override void Update(GameTime gameTime)
    {
        // Input
        // TODO: Problem: Loss of quick keystrokes
        // TODO: Make instant reading of input for visual
        _inputSource.Update();

        _gameClock.Tick((float)gameTime.ElapsedGameTime.TotalSeconds, (tick, deltaTime) =>
        {
            _logic.ManualUpdate(tick, deltaTime);
            _physics.ManualUpdate(tick, deltaTime);
        });
        
        DataReader();
        LogicReader();
        PhysicsReader();
        
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
    
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _threadManager?.Stop();
            _threadManager?.Join(TimeSpan.FromMilliseconds(100));

            (_services as IDisposable)?.Dispose();
        }

        base.Dispose(disposing);
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
                case PositionsUpdate command:
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
    
    private void DataReader()
    {
        while (_channelHub.DataToMain.Reader.TryRead(out var dataCommand))
            switch (dataCommand)
            {
                case TextureLoad command:
                    LoadingTexture(command);
                    break;
                default:
                    Log.Warning("Data command {cmd} not complied", dataCommand);
                    break;
            }
    }
    
    private void LoadingTexture(TextureLoad textureLoad)
    {
        if (textureLoad.TextureKey == null)
        {
            Log.Warning("TextureLoad texture null");
            return;
        }
    
        if (_textures.ContainsKey(textureLoad.TextureKey))
        {
            Log.Debug("Texture already loaded: {key}", textureLoad.TextureKey);
            return;
        }
    
        var texture = Content.Load<Texture2D>(textureLoad.TextureKey);
        _textures.Add(textureLoad.TextureKey, texture);
    }
    
    // Update visual entities positions
    private void UpdatePositions(PositionsUpdate positionBuffer)
    {
        foreach (var position in positionBuffer.Positions)
        {
            if (_entities.TryGetValue(position.EntityID, out var visualEntity))
                visualEntity.Position = position.Position;
            else Log.Warning("EntityID: {id} visual not found", position.EntityID);
        }
        

    }

    // Draw entities
    private void DrawEntitySprite()
    {
        foreach (var entity in _entities)
            _spriteBatch.Draw(entity.Value.Texture, entity.Value.Position.ToScreen(), Color.White);
    }

    private void SpawnEntityTexture(TextureSpawn spawn)
    {
        if (!_textures.TryGetValue(spawn.TextureKey, out var texture))
        {
            Log.Warning("Texture: {textureKey} not found", spawn.TextureKey);
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
            spawn.EntityID, spawn.Position, spawn.TextureKey);
    }
}