using FPS.Render;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace FPS.GLInterface {

	//Vertex definition following interleaved format T2fC4fN3fV3f
	[StructLayout(LayoutKind.Sequential)]
	public struct Vertex {
		public static readonly int Size = Marshal.SizeOf(new Vertex());
		public Vector2 TexCoord;
		public Vector4 Color;
		public Vector3 Normal;
		public Vector3 Position;
	}

	public class Model {
		uint[] _buffs;
		//indicies into _buffs
		const int VERT_INDEX = 0;
		const int NUM_BUFFS = 1;
		//Texture ID
		int _texid;
		//Number of verts
		int _numverts;

		public Model(Vertex[] Verts, Bitmap Texture) {
			_buffs = new uint[NUM_BUFFS];
			_numverts = Verts.Length;
			GL.GenBuffers(NUM_BUFFS, _buffs);
			//Vertex Data
			GL.BindBuffer(BufferTarget.ArrayBuffer, _buffs [VERT_INDEX]);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Verts.Length * Vertex.Size), Verts, BufferUsageHint.StaticDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			//Texture data
			_texid = GLUtil.CreateTexture(Texture);
		}

		public void Render(WorldRenderer WR) {
			//Bind texture.
			WR.BindTexture(_texid);
			//Bind vertex data
			GL.BindBuffer(BufferTarget.ArrayBuffer, _buffs [VERT_INDEX]);
			GL.InterleavedArrays(InterleavedArrayFormat.T2fC4fN3fV3f, 0, (IntPtr)0);

			GL.DrawArrays(BeginMode.Triangles, 0, _numverts);

			//Unbind buffer
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
		}

		~Model() {
			GL.DeleteBuffers(NUM_BUFFS, _buffs);
		}
	}

	/// <summary>
	/// Model parser interface.
	/// </summary>

	public interface IModelParser {
		/// <summary>
		/// Parse the specified file.
		/// </summary>
		/// <param name='FName'>
		/// File name.
		/// </param>
		/// <returns>
		/// A Model from the FName
		/// </returns>
		/// <seealso cref="Model"/>
		Model Parse(string FName);
	}

	/// <summary>
	/// Wavefront Object model parser.
	/// <see cref="http://www.martinreddy.net/gfx/3d/OBJ.spec"/>
	/// </summary>

	class OBJModelParser : IModelParser {
		static readonly char[] SPACE = " ".ToCharArray();
		static readonly char[] FSLASH = "/".ToCharArray();
		static OBJModelParser INSTANCE = new OBJModelParser();

		public Model Parse(string FName) {
			string[] line;
			List<Vector3> pos = new List<Vector3>();
			List<Vector3> norm = new List<Vector3>();
			List<Vector2> tex = new List<Vector2>();
			List<Vertex> tr = new List<Vertex>();
			int linecount = 0;
			int objcount = 0;
			using (StreamReader s = new StreamReader(FName + ".obj")) {
				while (!s.EndOfStream) {
					++linecount;
					line = s.ReadLine().Split(SPACE);
					switch (line [0]) {
					case "v":
						pos.Add(ReadVec3(line));
						break;
					case "vn":
						norm.Add(ReadVec3(line));
						break;
					case "vt":
						tex.Add(ReadVec2(line));
						break;
					case "f":
						if (line.Length > 4) {
							ThrowBadValue("Faces must be tris.", "f", linecount, 1);
						}
						ReadFace(line, ref pos, ref norm, ref tex, ref tr);
						break;
					#region Error handling.
					case "o":
						//Make sure only one object, but other than that do nothing.
						++objcount;
						if (objcount > 1) {
							ThrowBadValue("Too many objects!", line [0], linecount, 1);
						}
						break;
					case "s":
						//Ignore shading.
						break;
					case "usemtl":
						//Ignore material
						break;
					case "mtllib":
						//Ignore material lib.
						break;
					default:
						if (!line [0].StartsWith("#")) {
							ThrowBadValue("Unsupported directive.", line [0], linecount, 1);
						}
						break;
                    #endregion
					}
				}
			}
			return new Model(tr.ToArray(), new Bitmap(FName + ".png"));
		}

		Vector3 ReadVec3(string[] Line) {
			Vector3 tmp = new Vector3();
			tmp.X = float.Parse(Line [1]);
			tmp.Y = float.Parse(Line [2]);
			tmp.Z = float.Parse(Line [3]);
			return tmp;
		}

		Vector2 ReadVec2(string[] Line) {
			Vector2 tmp = new Vector2();
			tmp.X = float.Parse(Line [1]);
			tmp.Y = float.Parse(Line [2]);
			return tmp;
		}

		void ReadFace(string[] Line, ref List<Vector3> Pos, ref List<Vector3> Norm, ref List<Vector2> Tex, ref List<Vertex> Out) {
			for (int i = 0; i < Line.Length - 1; ++i) {
				string[] v = Line [i + 1].Split(FSLASH);
				Vertex tmp = new Vertex();
				tmp.Position = Pos [int.Parse(v [0]) - 1];
				tmp.Color.X = 1;//(float)r.NextDouble();
				tmp.Color.Y = 1;//(float)r.NextDouble();
				tmp.Color.Z = 1;//(float)r.NextDouble();
				tmp.TexCoord = Tex [int.Parse(v [1]) - 1];
				tmp.TexCoord.Y = 1 - tmp.TexCoord.Y; //fix sillyness
				tmp.Normal = Norm [int.Parse(v [2]) - 1];
				Out.Add(tmp);
			}
		}

		void ThrowBadValue(string Reason, string Val, int Row, int Col) {
			string err = string.Format("Invalid character {0} at ({1}, {2}). {3}", Val, Row, Col, Reason);
			throw new ArgumentException(err);
		}

		public static OBJModelParser GetInstance() {
			return INSTANCE;
		}
	}
}

