using System;
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
using Vinland.Core.Infrastructure;
using Vinland.Core.Input;
using Vector2Aither = nkast.Aether.Physics2D.Common.Vector2;
using Vector2Mono = Microsoft.Xna.Framework.Vector2;


namespace Vinland;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    // DI
    IServiceProvider _services;
    
    // Fixed Update
    private uint _frameId;
    private const float FixedDeltaTime = 1f / 60f;
    private float _accumulator;

    // Input
    private IInputSource _inputSource = new KbmInputSource();
    private InputCommandBuffer _inputBuffer = new InputCommandBuffer();
    
    private InputCommand _currentInput;
    
    // Physic
    private World _physicWorld;
    
    private Body _playerBody;
    
    private const int PixelsPerMeter = 64;

    // Draw
    Vector2Aither _playerRenderPos;
    
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
        
        // Physic
        _physicWorld = new World(Vector2Aither.Zero);
        
        // Tile map
        _camera = new OrthographicCamera(GraphicsDevice)
        {
            Zoom = 2f,
            Position = Vector2Mono.Zero
        };
        
        base.Initialize(); 
    }
    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _playerTexture = Content.Load<Texture2D>("textures/Player");
        
        // Tilemap
        _tilemap = Content.Load<Tilemap>("maps/rooms/room_01");
        _tilemapRenderer = new TilemapRenderer(GraphicsDevice);
        
        _tilemapRenderer.LoadTilemap(_tilemap);

        InitializePhysicFromMap();
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
        
        const string collisionLayerName = "Collision";
        
        
        // TODO: Initialize Tilemaps collision not work!
        // Logger for Collision map Layer
        if (!_tilemap.Layers.TryGetValue(collisionLayerName, out var layer))
        {
            // var logger = _services.GetRequiredService<ILogger>();
            // logger.Error($"Collision layer '{collisionLayerName}' not found in map.");
        }
        
        // Initialize Collision map Layer
        var tileLayer = layer as TilemapTileLayer;
        if (tileLayer == null) return;
        
        float tileSize = _tilemap.TileWidth;

        for (int x = 0; x < _tilemap.Width; x++)
        {
            for (int y = 0; y < _tilemap.Height; y++)
            {
                var tile = tileLayer.GetTile(x, y);
                if (tile.HasValue)
                {
                    float pixelX = x * tileSize + tileSize / 2f;
                    float pixelY = y * tileSize + tileSize / 2f;
                
                    // Converting coordinates for physic
                    float worldX = pixelX / PixelsPerMeter;
                    float worldY = -pixelY / PixelsPerMeter; 
                    
                    var body = _physicWorld.CreateBody();
                    body.BodyType = BodyType.Static;

                    var shape = new PolygonShape(
                        PolygonTools.CreateRectangle(
                            tileSize / 2f,
                            tileSize / 2f),
                        1f);
                    body.CreateFixture(shape);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        _physicWorld.Step(FixedDeltaTime);
        _currentInput = _inputBuffer.Current;
        
        // Player Movement
        PlayerMovement();
        ChannelHub.PhysicsToMain.Writer.TryWrite(new PhysicsUpdate("Player", _playerBody.Position));

        
        Log.Information("Physic: {PhysicPos}, Mono:  {MonoPos}",
            _playerBody.Position,
            _playerRenderPos);
        
    }

    private void PlayerMovement()
    {
        
        Vector2Aither velocity = Vector2Aither.Zero;
        
        float speedMetersPerSecond = 50f;

        if (_currentInput.MoveUp) velocity.Y -= 1;
        if (_currentInput.MoveDown) velocity.Y += 1;
        if (_currentInput.MoveLeft) velocity.X -= 1;
        if (_currentInput.MoveRight) velocity.X += 1;

        if (velocity.LengthSquared() > 0)
            velocity.Normalize();
        velocity *= speedMetersPerSecond;
        
        Console.WriteLine($"Speed: {velocity}");
        
        _playerBody.LinearVelocity = velocity;
    }
    
    protected override void Update(GameTime gameTime)
    {
        // TODO: Make instant reading of input for visual
        
        // Input
        KeyboardExtended.Update();
        var rawInput = _inputSource.ReadInput();
        
        ChannelHub.MainToLogic.Writer.TryWrite(rawInput);
        
        _inputBuffer.RecordForCurrentFrame(rawInput);
        
        _accumulator += (float)gameTime.ElapsedGameTime.TotalSeconds;

        while (_accumulator >= FixedDeltaTime)
        {
            _inputBuffer.AdvanceFrame();
            FixedUpdate();
            
            _frameId++;
            _accumulator -= FixedDeltaTime;
        }
        
        // Player position for Draw
        while (ChannelHub.PhysicsToMain.Reader.TryRead(out var update))
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
    }
}