using UnityEngine;
using System.Collections;

public class MathHelper {

	public static int Faculty(int n){
		int result = 0;
		for (int i = 0; i < n - 1; i++){
			for (int j = i + 1; j < n; j++){
				result++;
			}
		}
		return n;
	}
}
