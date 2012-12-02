using System;
using System.Drawing;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using FPS.GLInterface;
using FPS.Game;
using FPS.Game.Entity;
using FPS.Game.HMap;
using FPS.Render;

namespace FPS {
	public class MainClass  : GameWindow {
		Vector3 _camOffset;
		World _world;
		HeightMap _map;
		WorldRenderer _ren;
		Vector2 _mouseDelta;
		Stopwatch _timer;
		int _frame;
		PlayerEntity _pe;
		bool _capMouse;

		public MainClass() : base(800, 600, OpenTK.Graphics.GraphicsMode.Default, "Defend Rome") {
		}

		protected override void OnLoad(EventArgs e) {
			_map = new HeightMap("res/map.map");
			_world = new World(_map);
			_pe = new PlayerEntity(new Vector3(0, 20, 0));
			_world.Ents.AddFirst(_pe);
			_ren = new WorldRenderer(this, _world, (float)Width / Height);
			_camOffset = new Vector3();
			GL.ClearColor(OpenTK.Graphics.Color4.SkyBlue);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Fog);
			float[] fogColor = {
				OpenTK.Graphics.Color4.SkyBlue.R,
				OpenTK.Graphics.Color4.SkyBlue.G,
				OpenTK.Graphics.Color4.SkyBlue.B,
				OpenTK.Graphics.Color4.SkyBlue.A
			};
			GL.Fog(FogParameter.FogColor, fogColor);
			GL.Fog(FogParameter.FogEnd, WorldRenderer.MAX_DEPTH);
			GL.Fog(FogParameter.FogStart, 10f);

			_mouseDelta = new Vector2(0, 0);
			_mouseDelta += new Vector2(
				System.Windows.Forms.Cursor.Position.X - Width / 2 - X,
				System.Windows.Forms.Cursor.Position.Y - Height / 2 - Y);
			System.Windows.Forms.Cursor.Position = new Point(Width / 2 + X, Height / 2 + Y);
			System.Windows.Forms.Cursor.Hide();
			_timer = new Stopwatch();
			_capMouse = true;
		}

		protected override void OnUpdateFrame(FrameEventArgs e) {
			if (Keyboard [Key.Escape]) {
				_capMouse = false;
			}

			if (Keyboard [Key.J]) {
				_capMouse = true;
			}

			if (_capMouse) {
				_mouseDelta.X = System.Windows.Forms.Cursor.Position.X - Width / 2 - X;
				_mouseDelta.Y = System.Windows.Forms.Cursor.Position.Y - Height / 2 - Y;
				System.Windows.Forms.Cursor.Position = new Point(Width / 2 + X, Height / 2 + Y);
			}

			_pe.Move(Keyboard, _mouseDelta);
			_world.Tick(1);

			_camOffset.Y = _pe.GetEyeOffset();

			_ren.Pos = Vector3.Add(_pe.Pos, _camOffset);
			_ren.Pitch = _pe.Pitch;
			_ren.Yaw = _pe.Yaw;
		}

		protected override void OnRenderFrame(FrameEventArgs e) {
			_timer.Start();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_ren.Render();
			GLUtil.PrintGLError("Main");
			SwapBuffers();
			++_frame;
			_timer.Stop();
			if (_frame % 60 == 59) {
				Console.WriteLine((float)Stopwatch.Frequency / (_timer.ElapsedTicks / 60f));
				_timer.Reset();
			}
		}

		protected override void OnResize(EventArgs e) {
			GL.Viewport(0, 0, Width, Height);
			_ren.Aspect = (float)Width / Height;
		}

		public int GetFrame() {
			return _frame;
		}

		public static void Main(String[] args) {
			new MainClass().Run(20, 60);
		}
	}
}

