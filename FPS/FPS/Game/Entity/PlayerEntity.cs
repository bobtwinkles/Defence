using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;
using FPS.Game.HMap;
using FPS.GLInterface;
using FPS.Render;

namespace FPS.Game.Entity {
	public class PlayerEntity : IEntity {
		public const float MOVE_SPEED = 0.5f;
		public static readonly float JUMP_FORCE = 1f;
		public const float MAX_JUMP_FORCE = 10f;
		public const float MOUSE_SPEED = 0.001f;
		public const float KNOCKBACK = 2;
		const float HALFPI = (float)(Math.PI * 0.5);
		const int SWING_FRAMES = 10;

		int _swingFrame;
		int _walkFrame;
		Model _sword;

		public PlayerEntity(Vector3 Pos) : base(Pos, new AABB(1, 2, 1), 10) {
			_walkFrame = 0;
			_swingFrame = 0;
			_sword = OBJModelParser.GetInstance().Parse("res/mdl/sword");
		}

		public void Move(KeyboardDevice KD, Vector2 MouseDelta) {
			Vector3 moveForce = new Vector3(0, 0, 0);
			bool moved = false;
			if (KD [Key.W]) {
				moveForce.Z -= (float)Math.Cos(-Yaw) * MOVE_SPEED;
				moveForce.X -= (float)Math.Sin(-Yaw) * MOVE_SPEED;
				moved = true;
			}
			if (KD [Key.S]) {
				moveForce.Z += (float)Math.Cos(-Yaw) * MOVE_SPEED;
				moveForce.X += (float)Math.Sin(-Yaw) * MOVE_SPEED;
				moved = true;
			}
			if (KD [Key.A]) {
				moveForce.Z -= (float)Math.Cos(-Yaw + HALFPI) * MOVE_SPEED;
				moveForce.X -= (float)Math.Sin(-Yaw + HALFPI) * MOVE_SPEED;
				moved = true;
			}
			if (KD [Key.D]) {
				moveForce.Z += (float)Math.Cos(-Yaw + HALFPI) * MOVE_SPEED;
				moveForce.X += (float)Math.Sin(-Yaw + HALFPI) * MOVE_SPEED;
				moved = true;
			}
			if (KD [Key.Space] && this.OnGround) {
				//moveForce.Y += JUMP_FORCE;
			}
			if (moved)
				++_walkFrame;
			Yaw += MouseDelta.X * MOUSE_SPEED;
			Pitch += MouseDelta.Y * MOUSE_SPEED;
			if (Pitch < WorldRenderer.MIN_PITCH) {
				Pitch = WorldRenderer.MIN_PITCH;
			}
			if (Pitch > WorldRenderer.MAX_PITCH) {
				Pitch = WorldRenderer.MAX_PITCH;
			}
			if (_swingFrame < SWING_FRAMES)
				++_swingFrame;
			ApplyForce(moveForce);
			if (Health <= 0)
				Dead = true;
		}

		public void SwingSword(World W) {
			if (_swingFrame >= SWING_FRAMES) {
				_swingFrame = 0;
				Vector3 off = new Vector3(0.5f, 1.5f, 0.5f);
				Vector3 moff = new Vector3(0.5f, 1, 0.5f);
				Vector3 rpos = _pos + off;
				Vector4 rdir = new Vector4(-Vector4.UnitZ);
				Matrix4 trans = Matrix4.Mult(
			                 Matrix4.CreateFromAxisAngle(Vector3.UnitX, Pitch),
			                 Matrix4.CreateFromAxisAngle(Vector3.UnitY, -Yaw)
				);
				rdir = Vector4.Transform(rdir, trans);
				rdir.Normalize();
				double min = 10;
				Enemy dmg = null;
				foreach (IEntity ent in W.Ents) {
					Enemy e = ent as Enemy;
					if (e != null) {
						Enemy.BS.Pos = e.Pos + moff;
						double d = Enemy.BS.RayIntersection(rpos, rdir.Xyz);
						if (!Double.IsNaN(d) && d > 0 && d < min) {
							min = d;
							dmg = e;
						}
					}
				}
				if (dmg != null) {
					dmg.Hurt(10);
					dmg.ApplyForce(new Vector3(
						(float)-Math.Sin(-Yaw) * KNOCKBACK,
						0,
						(float)-Math.Cos(-Yaw) * KNOCKBACK
					)
					);
				}
			}
		}

		public float GetEyeOffset() {
			return 2 + 0.2f * (float)Math.Sin(Math.PI * _walkFrame * 0.2);
		}

		public override void Render(WorldRenderer In) {
			In.PushMatrix();
			const double angleoffset = 0.6;
			const float offsetscale = 0.1f;
			float sinyaw = (float)Math.Sin(-Yaw - angleoffset);
			float cosyaw = (float)Math.Cos(-Yaw - angleoffset);
			In.Translate(_pos.X - sinyaw * offsetscale, In.Pos.Y - 0.12f, _pos.Z - cosyaw * offsetscale);
			In.Rotate(Vector3.UnitY, -Yaw);
			const float arange = (float)(Math.PI * 0.01);
			const double afreq = Math.PI * 0.01;
			float swingX = (float)(Math.PI * 0.5 * Math.Sin(Math.PI * _swingFrame / SWING_FRAMES));
			float swingZ = (float)(Math.PI * 0.5 * Math.Sin(Math.PI * _swingFrame / SWING_FRAMES));
			In.Rotate(Vector3.UnitX, arange * (float)Math.Sin(afreq * In.GetFrame() + 2 * arange) - swingX);
			In.Rotate(Vector3.UnitZ, arange * (float)Math.Cos(afreq * In.GetFrame()) + swingZ);
			_sword.Render(In);
			In.PopMatrix();
		}

		public override void OnCollide(IEntity With) {
			//Do nothing.
		}
	}
}

