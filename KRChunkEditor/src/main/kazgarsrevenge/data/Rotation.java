package main.kazgarsrevenge.data;

import java.util.HashMap;
import java.util.Map;

/**
 * Representation of rotations by 90 degrees.
 * Values should be used like you do in math....
 * 
 * On the unit circle
 * 	ZERO = (1, 0)
 * 	NINETY = (0, 1)
 * 	ONE_EIGHTY = (-1, 0)
 * 	TWO_FORTY = (0, -1)
 * @author Brandon
 *
 */
public enum Rotation {

	ZERO, NINETY, ONE_EIGHTY, TWO_FORTY;
	
	private static final Map<Integer, Rotation> ordinalMap;
	
	static {
		ordinalMap = new HashMap<>();
		for (Rotation rot : Rotation.values()) {
			ordinalMap.put(rot.ordinal(), rot);
		}
	}
	
	public static double toDegrees(Rotation rot) {
		if (rot == Rotation.ZERO) {
			return 0;
		} else if (rot == Rotation.NINETY) {
			return 90;
		} else if (rot == Rotation.ONE_EIGHTY) {
			return 180;
		} else {
			return 240;
		}
	}
	
	public static double toRadians(Rotation rot) {
		return Math.toRadians(Rotation.toDegrees(rot));
	}
	
	public static Rotation rotateCounter(Rotation first) {
		int firstOrd = first.ordinal();
		int nextOrd = ((firstOrd == 0) ? Rotation.values().length : firstOrd) - 1;
		return ordinalMap.get(nextOrd);		
	}
}
