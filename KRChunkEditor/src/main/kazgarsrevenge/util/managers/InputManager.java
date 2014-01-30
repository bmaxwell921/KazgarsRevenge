package main.kazgarsrevenge.util.managers;

import java.awt.event.KeyEvent;
import java.util.HashMap;
import java.util.Map;

/**
 * Class used to handle input properly. It's a singleton
 * @author Brandon
 *
 */
public class InputManager {

	// Singleton instance
	private static InputManager instance;
	
	// Map of pressed keys
	private Map<Integer, Boolean> keyPressedMap;
	
	private InputManager() {
		keyPressedMap = new HashMap<>();
		init();
	}
	
	private void init() {
		keyPressedMap.put(KeyEvent.VK_LEFT, false);
		keyPressedMap.put(KeyEvent.VK_RIGHT, false);
		keyPressedMap.put(KeyEvent.VK_DOWN, false);
		keyPressedMap.put(KeyEvent.VK_UP, false);
		keyPressedMap.put(KeyEvent.VK_ENTER, false);
		keyPressedMap.put(KeyEvent.VK_R, false);
		keyPressedMap.put(KeyEvent.VK_BACK_SPACE, false);
		keyPressedMap.put(KeyEvent.VK_M, false);
	}
	
	/**
	 * Sets the value for the given key as given
	 * @param keyCode
	 * @param value
	 */
	public void updateValue(int keyCode, boolean value) {
		keyPressedMap.put(keyCode, value);
	}
	
	public boolean isPressed(int keyCode) {
		return keyPressedMap.get(keyCode);
	}
	
	public static InputManager getInstance() {
		if (instance == null) {
			instance = new InputManager();
		}
		return instance;
	}

}
