package main.kazgarsrevenge.util.managers;

import java.util.HashMap;
import java.util.Map;

import main.kazgarsrevenge.ui.listeners.UpdateListener;
import main.kazgarsrevenge.ui.panels.KREditorPanel;

/**
 * Manager for KeyActions. Used to enable and disable KeyActions for a KREditor
 * @author Brandon
 *
 */
public class UpdaterManager {

	// An instance of the class
	private static UpdaterManager instance;

	/**
	 * Gotta love dat Factory Pattern!
	 * @return
	 */
	public static UpdaterManager getInstance() {
		if (instance == null) {
			instance = new UpdaterManager();
		}
		return instance;
	}
	
	// Mapping of KREditorPanel to its keyActions
	private Map<Class<? extends KREditorPanel>, UpdateListener> editorUpdateMap;
	
	private UpdaterManager() {
		editorUpdateMap = new HashMap<>();
	}
	
	public void registerListener(Class<? extends KREditorPanel> clazz, UpdateListener updater) {
		editorUpdateMap.put(clazz, updater);
	}
	
	/**
	 * Enables all KeyActions for the given KREditorPanel
	 * @param clazz
	 */
	public void disableListeners(Class<? extends KREditorPanel> clazz) {
		setEnabled(clazz, false);
	}
	
	/**
	 * Disables all KeyActions for the given KREditorPanel
	 * @param clazz
	 */
	public void enableListeners(Class<? extends KREditorPanel> clazz) {
		setEnabled(clazz, true);
	}
	
	private void setEnabled(Class<? extends KREditorPanel> clazz, boolean enabled) {
		if (!editorUpdateMap.containsKey(clazz)) {
			return;
		}
		if (enabled) {
			editorUpdateMap.get(clazz).enable();
			return;
		}
		editorUpdateMap.get(clazz).disable();
	}
}
