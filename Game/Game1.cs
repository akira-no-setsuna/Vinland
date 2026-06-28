using System;
using Game.Core.Entities;
using Game.Core.Infrastructure;
using Game.Core.Physics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Input;
using MonoGame.Extended.Tilemaps;
using MonoGame.Extended.Tilemaps.Rendering;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;
using Vinland.Core.Infrastructure;
using Vinland.Core.Input;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;


namespace Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // DI
    private IServiceProvider _services;
    private ChannelHub _channelHub;
    
    // Fixed Update

    private const float FIXED_DELTA_TIME = 1f / 60f;
    private float _accumulator;

    // Input
    private readonly IInputSource _inputSource = new KbmInputSource();
    
    private InputCommand _input;
    
    // Physic
    private World _physicWorld;
    private Body _playerBody;
    private PlayerController _playerController;
    private MapColliderGenerator  _mapColliderGenerator;
    // Draw
    Vector2Mono _playerRenderPos;
    
    // Tile map
    Tilemap _tilemap;
    TilemapRenderer _tilemapRenderer;
    
    OrthographicCamera _camera;

    private Texture2D _playerTexture;
    private PhysicsDebugRenderer _physicsDebug;
    private GameThreadManager _threadManager;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    protected override void Initialize()
    {
        // DI
        _services = DependencyInjection.ConfigureServices();
        _channelHub = _services.GetRequiredService<ChannelHub>();
        _threadManager = _services.GetRequiredService<GameThreadManager>();
        
        Log.Information("=== Initializing game ===");
        
        // Physic
        _physicWorld = new World(Vector2Aether.Zero);
        
        // Tile map
        _camera = new OrthographicCamera(GraphicsDevice)
        {
            Zoom = 2f,
            Position = Vector2Mono.Zero
        };
        
        // Thread
        _threadManager.Start();
        
        base.Initialize(); 
    }
    
    protected override void LoadContent()
    {
        Log.Information("=== Loading content ===");
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _playerTexture = Content.Load<Texture2D>("textures/Player");
        
        // Tilemap
        _tilemap = Content.Load<Tilemap>("maps/rooms/room_01");
        _tilemapRenderer = new TilemapRenderer(GraphicsDevice);
        
        _tilemapRenderer.LoadTilemap(_tilemap);

        InitializePhysicFromMap();
        _mapColliderGenerator = new MapColliderGenerator();
        _mapColliderGenerator.InitializeFromMap(_physicWorld,  _tilemap);
        _playerController = new PlayerController(_playerBody);
        
        _physicsDebug = new PhysicsDebugRenderer(GraphicsDevice);
    }

    private void InitializePhysicFromMap()
    {
        // Player Physic
        _playerBody = _physicWorld.CreateBody();
        _playerBody.BodyType = BodyType.Dynamic;

        var playerShape = new PolygonShape(
            PolygonTools.CreateRectangle(
                1f,
                1f),
            1f);
        _playerBody.CreateFixture(playerShape);
    }

    private void FixedUpdate()
    {
        _physicWorld.Step(FIXED_DELTA_TIME);
        
        // TODO: Problem: Loss of quick keystrokes
        // Input Read
        while (_channelHub.MainToLogic.Reader.TryRead(out var input))
        {
            _input = input;
        }
        
        // Player Movement
        _playerController.FixedUpdate(_input);
        _channelHub.PhysicsToMain.Writer.TryWrite(new PhysicsUpdate("Player", _playerBody.Position));
    }
    
    
    protected override void Update(GameTime gameTime)
    {
        // TODO: Make instant reading of input for visual
        
        // Input
        KeyboardExtended.Update();
        var input = _inputSource.ReadInput();
        _channelHub.MainToLogic.Writer.TryWrite(input);
        
        _accumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;

        while (_accumulator >= FIXED_DELTA_TIME)
        {
            FixedUpdate();
            _accumulator -= FIXED_DELTA_TIME;
        }
        
        // Player position for Draw
        while (_channelHub.PhysicsToMain.Reader.TryRead(out var update))
        {
            if (update.EntityId == "Player")
                _playerRenderPos = update.Position;
        }
        _camera.LookAt(_playerRenderPos);
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
        _spriteBatch.Draw(_playerTexture, _playerRenderPos, Color.White);
        _spriteBatch.End();
        
        _physicsDebug.Draw(_physicWorld, _camera.GetViewMatrix());
    }

    protected override void UnloadContent()
    {
        Log.Information("=== Unloading content ===");
        
        _threadManager.Dispose();
        
        Log.CloseAndFlush();
        base.UnloadContent();
    }
}