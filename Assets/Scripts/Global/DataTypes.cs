using UnityEngine;
using System.Collections;

namespace CustomDataTypes {

	[System.Serializable]
	public struct Pattern{
		public int[] coords;
		public int size;
		public float complexity;

		public int audioCategory;
		public int instrumentGroup;
		public int variation;

		public static Pattern bottom	= new Pattern(0, 0, 0, 0, 0, 0, 0, 0);
		public static Pattern top		= new Pattern(Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS, Constants.VERTICAL_POSITIONS);

		public Pattern(params int[] _coords){
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


			audioCategory	= -1;
			instrumentGroup	= -1;
			variation		= -1;
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
