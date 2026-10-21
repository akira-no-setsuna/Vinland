using System;
using System.Collections.Generic;
using Godot;
using Vinland.Core.Infrastructure.Channels;
using Vinland.Godot.Connector;

namespace Vinland.Godot.Main;

public partial class Manager : Node
{
	private GodotConnector _connector;

	public Manager()
	{
		_connector = new GodotConnector(new GameModules(), this);
	}

	public override void _Ready()
	{
		 _connector.Initialize();
	}

	public override void _Process(double delta)
	{
		_connector.Update((float)delta);
	}
}