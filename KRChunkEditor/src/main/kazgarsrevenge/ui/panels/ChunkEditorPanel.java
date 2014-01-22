package main.kazgarsrevenge.ui.panels;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Frame;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.swing.JButton;
import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.EditableChunkComponent;
import main.kazgarsrevenge.model.impl.Chunk;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.ui.listeners.NewRoomClickListener;
import main.kazgarsrevenge.ui.listeners.SelectRoomListener;
import main.kazgarsrevenge.util.IO.ComponentIO;
import main.kazgarsrevenge.util.managers.ComponentManager;
import main.kazgarsrevenge.util.managers.ImageManager;

public class ChunkEditorPanel extends KREditorPanel {

	private static final int GRID_SIZE = 24;
	
	public ChunkEditorPanel(Frame frame, int width, int height) {
		super(frame, width, height);
	}

	@Override
	protected void createEditGrid() {
		super.editGrid = new EditGrid(this, SQUARE_SIZE, GRID_SIZE, GRID_SIZE);
		this.add(super.editGrid, BorderLayout.CENTER);
	}

	@Override
	protected void createSidePanel() {
		// Gets all of the room names from the ComponentManager, then creates ImageDescriptionPanels for them		
		super.selectables = new SidePanel("Rooms");
		super.selectables.setPreferredSize(new Dimension(200, 620));
		this.addImageDesc();
		this.add(super.selectables, BorderLayout.EAST);
	}
	
	private void addImageDesc() {
		ComponentManager cm = ComponentManager.getInstance();
		List<String> roomNames = new ArrayList<>(cm.getNames(Room.class));
		Collections.sort(roomNames);
		
		for (String name : roomNames) {
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageManager.getInstance().getImage(Room.class, name));
			super.selectables.addPanel(imgDesc);
			imgDesc.addMouseListener(new SelectRoomListener(this, name));
		}
	}
	
	@Override
	public void recreateSidePanel() {
		// Calls selectables.removeAll();
		super.recreateSidePanel();
		addImageDesc();
	}

	@Override
	protected void createButtonPanel() {
		super.createButtonPanel();
		
		super.buttonPanel.setPreferredSize(new Dimension(800, 30));
		JButton newRoom = new JButton("NEW ROOM");
		newRoom.addMouseListener(new NewRoomClickListener(super.parent, this));
		super.buttonPanel.add(newRoom);
	}

	@Override
	public void load(File loadFile, StringBuilder errors) {
		newEditable();
		super.editing = (Chunk) ComponentIO.loadChunkComponent(Chunk.class, loadFile, errors);
	}

	@Override
	public void newEditable() {
		super.editing = new Chunk(new Location(), "Chunk", Rotation.ZERO);
		super.editGrid.reset(GRID_SIZE, GRID_SIZE);
	}

	@Override
	public void select(String selectionName) {
		// Deselect if they clicked the same one
		if (super.selectedItem != null && super.selectedItem.getName().equals(selectionName)) {
			super.editGrid.resetSelectedArea(selectedItem.getLocation().getX(), selectedItem.getLocation().getY());
			super.selectedItem = null;
			return;
		}
		
		ChunkComponent newSel = ComponentManager.getInstance().getComponent(Room.class, selectionName);
		super.selectedItem = newSel;
	}
}
