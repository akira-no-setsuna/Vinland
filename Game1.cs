using System;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using nkast.Aether.Physics2D.Collision.Shapes;
using nkast.Aether.Physics2D.Common;
using nkast.Aether.Physics2D.Dynamics;
using Serilog;
using Vinland.Core;
using Vinland.Core.Input;
using Vector2 = nkast.Aether.Physics2D.Common.Vector2;

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

    // Draw
    Vector2 _playerPos;

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
        var services = new ServiceCollection();
        
        services.AddSingleton<ObjectPoolProvider>(new DefaultObjectPoolProvider());
        services.AddSingleton<ILogger>(new LoggerConfiguration()
            .WriteTo.Async(a => a.File($"logs/game-.txt\", rollingInterval: RollingInterval.Day"))
            .Enrich.WithThreadId()
            .CreateLogger());
        
        _services = services.BuildServiceProvider();
        
        // Physic
        _physicWorld = new World(Vector2.Zero);
        
        _playerBody = _physicWorld.CreateBody();
        _playerBody.BodyType = BodyType.Dynamic;
        
        float halfWidth = 1f;
        float halfHeight = 1f;
        var boxVertices = PolygonTools.CreateRectangle(halfWidth, halfHeight);
        var shape = new PolygonShape(boxVertices, 1f);
        
        _playerBody.CreateFixture(shape);
        
        base.Initialize(); 
    }

    
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
        
        _playerTexture = Content.Load<Texture2D>("Player");
    }

    private void FixedUpdate()
    {
        
        _physicWorld.Step(FixedDeltaTime);
        _currentInput = _inputBuffer.Current;
        
        // Player Movement
        _playerBody.LinearVelocity = Vector2.Zero;
        
        float movingSpeed = 1000f;
        
        if (_currentInput.MoveUp) _playerBody.ApplyLinearImpulse(new Vector2(0, -movingSpeed));
        if (_currentInput.MoveDown) _playerBody.ApplyLinearImpulse(new Vector2(0, movingSpeed));
        if (_currentInput.MoveLeft) _playerBody.ApplyLinearImpulse(new Vector2(-movingSpeed, 0));
        if (_currentInput.MoveRight) _playerBody.ApplyLinearImpulse(new Vector2(movingSpeed, 0));
        
        ChannelHub.PhysicsToMain.Writer.TryWrite(new PhysicsUpdate("Player", _playerBody.Position));
    }
    
    protected override void Update(GameTime gameTime)
    {
        // TODO: Make instant reading of input for visual
        
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
        
        while (ChannelHub.PhysicsToMain.Reader.TryRead(out var update))
        {
            if (update.EntityId == "Player")
                _playerPos = update.Position;
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        _spriteBatch.Begin();

        _spriteBatch.Draw(
            _playerTexture,
            new Rectangle((int)_playerPos.X - 16, (int)_playerPos.Y - 16, 32, 32),
            Color.White
        );
        
        _spriteBatch.End();
        
        // TODO: Add your drawing code here
        base.Draw(gameTime);
    }
}