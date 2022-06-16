using System;
using System.IO;

namespace TMode {
	public static class Program {
		public static void Main(string[] args) {
			if (args.Length < 2 || args[0].Length < 1 || args[1].Length < 1) {
				return;
			}

			// 0 = Normal, 1 = Expert, 2 = Master, 3 = Journey
			Enum.TryParse(args[1], out GameMode newMode);

			ChangeMode(args[0], args[0], newMode);
		}

		private static void ChangeMode(string source, string dest, GameMode newMode) {
			BinaryReader reader = new BinaryReader(new FileStream(source, FileMode.Open));
			int version = reader.ReadInt32();
			if (version < 209) {
				Console.WriteLine("Error: Outdated Terraria version");
				return;
			}

			ulong magic = reader.ReadUInt64();
			if ((magic & 72057594037927935uL) != 27981915666277746uL) {
				Console.WriteLine("Error: Invalid header");
				return;
			}

			// Skip other file metadata...
			reader.ReadBytes(12);
			int positionCount = reader.ReadInt16();
			int afterMetadataPos = reader.ReadInt32();
			int afterHeaderPos = reader.ReadInt32();
			// Skip positions...
			reader.ReadBytes((positionCount - 2) * 4);
			// Skip frame importance...
			reader.ReadBytes(reader.ReadInt16() / 8 + 1);
			if (reader.BaseStream.Position != afterMetadataPos) {
				Console.WriteLine(
					$"After Metadata Position Mismatch: expected {afterMetadataPos}, was {reader.BaseStream.Position}");
				return;
			}

			// Skip the first part of the header...
			string worldName = reader.ReadString();
			string worldSeed = reader.ReadString();
			
			/* 52 bytes
			long generatorVersion = reader.ReadInt64();
			byte[] guid = reader.ReadBytes(16);
			int worldId = reader.ReadInt32();
			byte[] worldBounds = reader.ReadBytes(16);
			int worldHeight = reader.ReadInt32();
			int worldWidth = reader.ReadInt32();
			*/
			// Get the offset...
			long gameModeFlagOffset = reader.BaseStream.Position + 52;
			int gameMode = reader.ReadInt32();

			reader.Dispose();
			if (newMode == (GameMode)gameMode) {
				Console.WriteLine($"The world \"{worldName}\" is already {newMode} mode.");
				return;
			}

			BinaryWriter writer = new BinaryWriter(new FileStream(dest, FileMode.Open));
			writer.BaseStream.Position = gameModeFlagOffset;
			writer.Write((int)newMode);
			writer.Dispose();
			Console.WriteLine($"The world \"{worldName}\" is now {newMode} Mode!");
		}
	}
}