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
 * 	TWO_SEVENTY = (0, -1)
 * @author Brandon
 *
 */
public enum Rotation {
	
	ZERO, NINETY, ONE_EIGHTY, TWO_SEVENTY;
	
	private static final Map<Integer, Rotation> degreesMap;
	
	private static final int ROTATE_AMT = 90;
	
	static {
		degreesMap = new HashMap<>();
		degreesMap.put(0, Rotation.ZERO);
		degreesMap.put(90, Rotation.NINETY);
		degreesMap.put(180, Rotation.ONE_EIGHTY);
		degreesMap.put(270, Rotation.TWO_SEVENTY);
	}
	
	public static double toDegrees(Rotation rot) {
		if (rot == Rotation.ZERO) {
			return 0;
		} else if (rot == Rotation.NINETY) {
			return 90;
		} else if (rot == Rotation.ONE_EIGHTY) {
			return 180;
		} else {
			return 270;
		}
	}
	
	public static double toRadians(Rotation rot) {
		return Math.toRadians(Rotation.toDegrees(rot));
	}
	
	public static Rotation rotateCounter(Rotation first) {
		int firstOrdinal = first.ordinal();
		int secondOrdinal = firstOrdinal + 1;
		int newDegrees = (secondOrdinal * ROTATE_AMT) % 360;
		return degreesMap.get(newDegrees);
	}
}
