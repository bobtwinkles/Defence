using System;
using FPS.Game;
using FPS.Game.Entity;
using FPS.GLInterface;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace FPS.Render {
	public class WorldRenderer {
		public const float MAX_DEPTH = 128f;
		public const float FOV = (float)(0.5 * Math.PI); //90 degrees

		World _for;
		HeightmapRenderer _hmap;
		ShaderProgram _simple;
		ShaderProgram _underwater;
		ShaderProgram _water;
		int _projectionLoc;
		Matrix4 _projectionMatrix;
		int _modelviewLoc;
		Matrix4 _modelview;
		float _aspect;
		float _pitch;
		float _yaw;
		Vector3 _pos;

		public float Pitch {
			get { return _pitch; }
			set { 
				_pitch = value;
				if (_pitch < -(0.5 * Math.PI))
					_pitch = -(float)Math.PI * 0.5f;
				if (_pitch > 0)
					_pitch = 0;
			}
		}

		public float Yaw {
			get { return _yaw; }
			set {
				_yaw = value;
				while (_yaw > 2 * Math.PI) {
					_yaw -= (float)(2 * Math.PI);
				}
				while (_yaw < 0) {
					_yaw += (float)(2 * Math.PI);
				}
			}
		}

		public Vector3 Pos {
			get { return _pos; }
			set { _pos = value; }
		}

		public float Aspect {
			get { return _aspect; }
			set { 
				_aspect = value;
				_projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(FOV), _aspect, 0.01f, MAX_DEPTH);
			}
		}

		public WorldRenderer(World For, float Aspect) {
			_for = For;
			_hmap = new HeightmapRenderer(this, _for.Terrain);
			VertexShader vbase = new VertexShader("res/base.vert");
			FragmentShader fbase = new FragmentShader("res/base.frag");
			FragmentShader fwater = new FragmentShader("res/water.frag");
			FragmentShader fuwater = new FragmentShader("res/underwater.frag");
			_simple = new ShaderProgram(vbase, fbase);
			_underwater = new ShaderProgram(vbase, fuwater);
			_water = new ShaderProgram(vbase, fwater);
			_projectionLoc = _simple.GetUniformLocation("projection");
			_modelviewLoc = _simple.GetUniformLocation("modelview");
			_aspect = Aspect;
			_projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(FOV), _aspect, 0.01f, MAX_DEPTH);
			_modelview = Matrix4.CreateTranslation(-10f, -5f, -10f);
			_pos = new Vector3(10, 2, 10);
		}

		public void Render() {
			if (true) {
				_simple.Use();
				_projectionLoc = _simple.GetUniformLocation("projection");
				_modelviewLoc = _simple.GetUniformLocation("modelview");
			} else {
				_underwater.Use();
				_projectionLoc = _underwater.GetUniformLocation("projection");
				_modelviewLoc = _underwater.GetUniformLocation("modelview");
			}
			_modelview = Matrix4.Identity;
			_modelview = Matrix4.Mult(_modelview, Matrix4.CreateTranslation(-_pos.X, -_pos.Y, -_pos.Z));
			_modelview = Matrix4.Mult(_modelview, Matrix4.CreateFromAxisAngle(Vector3.UnitY, -_yaw));
			_modelview = Matrix4.Mult(_modelview, Matrix4.CreateFromAxisAngle(Vector3.UnitX, -_pitch));

			LoadMatricies();
			_hmap.Render(_pos.X, _pos.Z);
			//Draw axies
			GL.Begin(BeginMode.Lines);
			float basex = 0;
			float basey = 50;
			float basez = 0;

			GL.Color3(1, 0, 0);
			GL.Normal3(1, 0, 0);
			GL.Vertex3(basex, basey, basez);
			GL.Vertex3(basex + 10, basey, basez);

			GL.Color3(0, 1, 0);
			GL.Normal3(0, 1, 0);
			GL.Vertex3(basex, basey, basez);
			GL.Vertex3(basex, basey + 10, basez);

			/*
			GL.Color3(0, 0, 1);
			GL.Normal3(0, 0, 1);
			GL.Vertex3(basex, basey, basez);
			GL.Vertex3(basex, basey, basez + 10);
*/
			GL.End();

			foreach (IEntity ent in _for.Ents) {
				ent.Render();
			}

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			_water.Use();
			_projectionLoc = _water.GetUniformLocation("projection");
			_modelviewLoc = _water.GetUniformLocation("modelview");
			LoadMatricies();
			_hmap.RenderWater(_pos.X, _pos.Z);
			GL.Disable(EnableCap.Blend);
		}

		void LoadMatricies() {
			GL.UniformMatrix4(_projectionLoc, false, ref _projectionMatrix);
			GL.UniformMatrix4(_modelviewLoc, false, ref _modelview);
		}
	}
}

