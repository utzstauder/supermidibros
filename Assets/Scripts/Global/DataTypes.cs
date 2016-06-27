using UnityEngine;
using System.Collections;

namespace CustomDataTypes {

	public struct Pattern{
		public int[] coords;
		public int size;
		public float complexity;

		public Pattern(params int[] _coords){
			coords = new int[_coords.Length];
//			for (int i = 0; i < _coords.Length; i++){
//				coords[i] = (_coords[i] >= 0 && _coords[i] < Constants.VERTICAL_POSITIONS) ? _coords[i] : -1;
//			}
			coords = _coords;

			// calulate difficulty
			// 1. size
			size = 0;
			for (int i = 0; i < coords.Length; i++){
				if (coords[i] > -1) {
					size++;
				}
			}

			// 2. different neighbours
			int differentNeighbours = 0;
			for (int i = 0; i < coords.Length - 1; i++){
				if (coords[i] < 0 || coords[i + 1] < 0){
					continue;
				}
				if (coords[i] != coords[i + 1]){
					differentNeighbours++;
				}
//				for (int j = i + 1; j < coords.Length; j++){
//					if (coords[j] < 0){
//						continue;
//					}
//					//differentNeighbours += Mathf.Abs(coords[i] - coords[j]);
//					if (coords[i] != coords[j]){
//						differentNeighbours++;
//					}
//				}
			}

			// 3. min-max modifier
			// reduce difficulty for every min or max coordinate
			// these positions require close to no precision to move to
			int minMaxModifier = 0;
			for (int i = 0; i < coords.Length; i++){
				if (coords[i] == 0 || coords[i] == Constants.VERTICAL_POSITIONS - 1){
					minMaxModifier--;
				}
			}


			complexity = (float)(size + differentNeighbours + minMaxModifier) /
						(float)(size + size - 1);
			if (complexity < 0){
				complexity = 0;
			}
		}

		public override string ToString(){
			string name = "";
			string blank = "x";
			for (int i = 0; i < coords.Length; i++){
				if (coords[i] >= 0){
					name += coords[i];
				} else {
					name += blank;
				}
			}
			return name;
		}
	}

}
