using System;

namespace FPS.Game.HMap {
	public class Chunk {
		public static readonly int CHUNK_SIZE = 32;

		float[,] _data;
		int _x, _y;

		public int X {
			get { return _x; }
			set { _x = value; }
		}

		public int Y {
			get { return _y; }
			set { _y = value; }
		}

		public Chunk(int X, int Y, float[,] Data) {
			_x = X;
			_y = Y;
			_data = Data;
		}

		public float this [int X, int Y] {
			get {
				return _data [X, Y];
			}
			set {
				_data [X, Y] = value;
			}
		}

		public override bool Equals(object obj) {
			if (obj.GetType() != this.GetType()) {
				return false;
			}
			Chunk c = (Chunk)obj;
			return c._x == this._x && c._y == this._y;
		}

		public override int GetHashCode() {
			return (_x * 3) ^ _y;
		}
	}

	public class ChunkPos {
		int _x, _y;

	}
}

