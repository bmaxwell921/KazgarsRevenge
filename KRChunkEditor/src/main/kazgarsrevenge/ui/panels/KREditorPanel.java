package main.kazgarsrevenge.ui.panels;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.event.KeyEvent;
import java.awt.image.BufferedImage;
import java.io.File;

import javax.swing.InputMap;
import javax.swing.JButton;
import javax.swing.JComponent;
import javax.swing.JPanel;
import javax.swing.KeyStroke;
import javax.swing.Timer;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.EditableChunkComponent;
import main.kazgarsrevenge.ui.keybinds.KeyAction;
import main.kazgarsrevenge.ui.listeners.LoadClickListener;
import main.kazgarsrevenge.ui.listeners.NewClickListener;
import main.kazgarsrevenge.ui.listeners.SaveClickListener;
import main.kazgarsrevenge.ui.listeners.UpdateListener;
import main.kazgarsrevenge.util.IO.ChunkComponentIO;
import main.kazgarsrevenge.util.managers.UpdaterManager;

public abstract class KREditorPanel extends JPanel {
	
	protected static final int SQUARE_SIZE = 25;
	
	// The Frame this panel is in
	protected final Frame parent;
	
	/*
	 * ========================
	 * Drawing instance Fields
	 * ========================
	 */
	
	// The grid where the thing being created is shown
	protected EditGrid editGrid;
		
	// The panel holding the components of the thing being created
	protected SidePanel selectables;
	
	// The panel holding the buttons associated with this editor
	protected JPanel buttonPanel;
	
	/*
	 * ========================
	 * Logic instance Fields
	 * ========================
	 */
	
	// What we're currently editing
	protected EditableChunkComponent editing;	
	
	/*
	 *  The currently selected item
	 */
	protected ChunkComponent selectedItem;
	
	public KREditorPanel(Frame parent, int width, int height) {
		this.parent = parent;
		this.setPreferredSize(new Dimension(width, height));
		this.setLayout(new BorderLayout());
		createEditGrid();
		createSidePanel();
		createButtonPanel();
		setUpKeyBindings();
		setUpTimer();
		newEditable();
	}
	
	/**
	 * Called by the constructor to set up the editing grid, subclasses
	 * implement it as needed
	 */
	protected abstract void createEditGrid();
	
	/**
	 * Sets up the side panel with all the selectable items
	 */
	protected abstract void createSidePanel();
	
	/**
	 * Sets up the buttons needed for the editing panel as needed
	 */
	protected void createButtonPanel() {
		buttonPanel = new JPanel();
		JButton newButton = new JButton("NEW");
		newButton.addMouseListener(new NewClickListener(parent, this));
		buttonPanel.add(newButton);
		
		JButton loadButton = new JButton("LOAD");
		loadButton.addMouseListener(new LoadClickListener(parent, this));
		buttonPanel.add(loadButton);
		
		JButton saveButton = new JButton("SAVE");
		saveButton.addMouseListener(new SaveClickListener(parent, this));
		buttonPanel.add(saveButton);
		
		this.add(buttonPanel, BorderLayout.PAGE_END);
	}
	
	private void setUpKeyBindings() {
		registerMovementBinding("LEFT", KeyEvent.VK_LEFT);
		registerMovementBinding("RIGHT", KeyEvent.VK_RIGHT);
		registerMovementBinding("UP", KeyEvent.VK_UP);
		registerMovementBinding("DOWN", KeyEvent.VK_DOWN);
		
		registerMovementBinding("ENTER", KeyEvent.VK_ENTER);
		registerMovementBinding("R", KeyEvent.VK_R);
	}
	
	private void registerMovementBinding(String name, int keyCode) {
		KeyAction pressed = new KeyAction(keyCode, true);
		KeyAction released = new KeyAction(keyCode, false);
		
		String pressedCommand = name + " Pressed";
		String releasedCommand = name + " Released";
		
		InputMap inputMap = this.getInputMap(JComponent.WHEN_IN_FOCUSED_WINDOW);
		
		KeyStroke pressedKeyStroke = KeyStroke.getKeyStroke(keyCode, 0, false);
		inputMap.put(pressedKeyStroke, pressedCommand);
		this.getActionMap().put(pressedCommand, pressed);
		
		KeyStroke releasedKeyStroke = KeyStroke.getKeyStroke(keyCode, 0, true);
		inputMap.put(releasedKeyStroke, releasedCommand);
		this.getActionMap().put(releasedCommand, released);
	}
	
	private void setUpTimer() {
		UpdateListener updater = new UpdateListener(this);
		UpdaterManager.getInstance().registerListener(this.getClass(), updater);
		new Timer(100, updater).start();
	}
	
	/**
	 * Saves the the current editable to the given file
	 * @param saveFile
	 */
	public void save(File saveFile, StringBuilder errors) {
		setEditableName(saveFile);
		ChunkComponentIO.saveChunkComponent(editing, saveFile, errors);
	}
	
	private void setEditableName(File saveFile) {
		String name = saveFile.getName();
		name = name.substring(0, name.indexOf(".json"));
		editing.setName(name);
	}
	
	/**
	 * Loads the an already created editable from the given file
	 * @param loadFile
	 */
	public abstract void load(File loadFile, StringBuilder errors);
	
	/**
	 * Resets the state of this editorPanel to be of a new editable.
	 * Called by the constructor and as needed
	 */
	public abstract void newEditable();
	
	/**
	 * Places the current selection in the current location
	 */
	public void placeSelection() {
		ChunkComponent editClone = (ChunkComponent) selectedItem.clone();
		editGrid.setAtCurrentLocation(editClone);
		if (!editing.add(editClone)) {
			System.out.println("Unable to place item");
		}
	}
	
	/**
	 * Moves the selectedArea by the given amount. 
	 * Negative x is left, negative y is up
	 * @param moveAmt
	 */
	public void moveSelection(Location moveAmt) {
		editGrid.moveSelection(moveAmt);
	}
	
	/**
	 * Rotates the current selection to the given rotation
	 * @param newRotation
	 */
	public void rotateSelection() {
		Rotation cur = editing.getRotation();
		cur = Rotation.rotateCounter(cur);
		editing.setRotation(cur);
		editGrid.rotateSelectedArea();
	}
	
	/**
	 * 'Selects' the a new selectable with the given name.
	 * Subclasses should provide additional implementation, this just
	 * toggles off the selection if it's pressed twice
	 * @param selectionName
	 */
	public abstract void select(String selectionName);
	
	/**
	 * Returns whether or not the user has selected an item
	 * @return
	 */
	public boolean hasSelection() {
		return selectedItem != null;
	}
	
	/**
	 * Returns the image for the selectedItem, or null if nothing
	 * is selected
	 * @return
	 */
	public abstract BufferedImage getSelectedImage();
}
