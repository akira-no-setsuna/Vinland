using System;
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
using Vinland.Core;
using Vinland.Core.Entities;
using Vinland.Core.Infrastructure;
using Vinland.Core.Input;
using Vinland.Core.Physic;
using Vector2Aether = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;


namespace Vinland;

public class Game1 : Game
{
    private PhysicsDebugRenderer _debugRenderer;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // DI
    private IServiceProvider _services;
    private ChannelHub _channelHub;
    
    // Fixed Update
    private uint _frameId;
    private const float FixedDeltaTime = 1f / 60f;
    private float _accumulator;

    // Input
    private readonly IInputSource _inputSource = new KbmInputSource();
    
    private InputCommand _input;
    
    // Physic
    private World _physicWorld;
    private Body _playerBody;
    private PlayerController _playerController;
    

    // Draw
    Vector2Aether _playerRenderPos;
    
    // Tile map
    Tilemap _tilemap;
    TilemapRenderer _tilemapRenderer;
    
    OrthographicCamera _camera;

    private Texture2D _playerTexture;
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
        
        // Physic
        _physicWorld = new World(Vector2Aether.Zero);
        
        // Tile map
        _camera = new OrthographicCamera(GraphicsDevice)
        {
            Zoom = 2f,
            Position = Vector2Mono.Zero
        };
        
        Log.Information("=========== New Initialization ===========");
        base.Initialize(); 
    }
    
    protected override void LoadContent()
    {
        _debugRenderer = new PhysicsDebugRenderer(GraphicsDevice);
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _playerTexture = Content.Load<Texture2D>("textures/Player");
        
        // Tilemap
        _tilemap = Content.Load<Tilemap>("maps/rooms/room_01");
        _tilemapRenderer = new TilemapRenderer(GraphicsDevice);
        
        _tilemapRenderer.LoadTilemap(_tilemap);

        InitializePhysicFromMap();

        _playerController = new PlayerController(_playerBody);
    }

    private void InitializePhysicFromMap()
    {
        // Player Physic
        if (_playerBody != null) _physicWorld.Remove(_playerBody);
        
        _playerBody = _physicWorld.CreateBody();
        _playerBody.BodyType = BodyType.Dynamic;

        var playerShape = new PolygonShape(
            PolygonTools.CreateRectangle(
                1f,
                1f),
            1f);
        _playerBody.CreateFixture(playerShape);
        
        var builder = new MapColliderGenerator();
        builder.InitializeFromMap(_physicWorld, _tilemap);
        
    }

    private void FixedUpdate()
    {
        _physicWorld.Step(FixedDeltaTime);
        
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

        while (_accumulator >= FixedDeltaTime)
        {
            FixedUpdate();
            _frameId++;
            _accumulator -= FixedDeltaTime;
        }
        
        // Player position for Draw
        while (_channelHub.PhysicsToMain.Reader.TryRead(out var update))
        {
            if (update.EntityId == "Player")
                _playerRenderPos = update.Position;
        }
        
        // Camera
        
        // _camera.Position = Microsoft.Xna.Framework.Vector2.SmoothStep(_camera.Position, _playerRenderPos.ToMono(), 0.1f);
        // _camera.Position = _playerRenderPos.ToMono();
        
        _camera.LookAt(_playerRenderPos.ToMono());
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
        _spriteBatch.Draw(_playerTexture, _playerRenderPos.ToMono(), Color.White);
        _spriteBatch.End();
        
        _debugRenderer.Draw(_physicWorld, _camera.GetViewMatrix());
    }

    protected override void UnloadContent()
    {
        Log.CloseAndFlush();
        base.UnloadContent();
    }
}