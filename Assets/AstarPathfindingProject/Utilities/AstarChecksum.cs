using System;
namespace Pathfinding.Util {
	/** Calculates checksums of byte arrays */
	public class Checksum {
		/** Calculate checksum for the byte array starting from a previous values.
		 * Useful if data is split up between several byte arrays */
		public static uint GetChecksum (byte[] arr, uint hash) {
			// Sort of implements the Fowler–Noll–Vo hash function
			const int prime = 16777619;

			hash ^= 2166136261U;

			for (int i = 0; i < arr.Length; i++)
				hash = (hash ^ arr[i]) * prime;

			return hash;
		}
	}
}
