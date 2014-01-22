package main.kazgarsrevenge.ui.panels;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Frame;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.model.impl.RoomBlock;
import main.kazgarsrevenge.ui.listeners.SelectRoomListener;
import main.kazgarsrevenge.util.IO.ComponentIO;
import main.kazgarsrevenge.util.managers.ComponentManager;
import main.kazgarsrevenge.util.managers.ImageManager;

public class RoomEditorPanel extends KREditorPanel {
	
	private static final int GRID_SIZE = 10;
	
	public RoomEditorPanel(Frame frame, int width, int height) {
		super(frame, width, height);
	}

	@Override
	protected void createEditGrid() {
		super.editGrid = new EditGrid(this, SQUARE_SIZE, GRID_SIZE, GRID_SIZE);
		this.add(super.editGrid, BorderLayout.CENTER);
	}

	@Override
	protected void createSidePanel() {
		// This side panel consists of roomblocks
		super.selectables = new SidePanel("Blocks");
		super.selectables.setPreferredSize(new Dimension(200, 250));
		ComponentManager cm = ComponentManager.getInstance();
		List<String> blockNames = new ArrayList<>(cm.getNames(RoomBlock.class));
		Collections.sort(blockNames);
		
		for (String name : blockNames) {
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageManager.getInstance().getImage(RoomBlock.class, name));
			super.selectables.addPanel(imgDesc);
			imgDesc.addMouseListener(new SelectRoomListener(this, name));
		}
		this.add(super.selectables, BorderLayout.EAST);
	}

	@Override
	protected void createButtonPanel() {
		super.createButtonPanel();
		super.buttonPanel.setPreferredSize(new Dimension(450, 50));
	}

	@Override
	public void load(File loadFile, StringBuilder errors) {
		newEditable();
		super.editing = (Room) ComponentIO.loadChunkComponent(Room.class, loadFile, errors);
	}

	@Override
	public void newEditable() {
		super.editing = new Room(new Location(), "RoomName", Rotation.ZERO);
		super.editGrid.reset(GRID_SIZE, GRID_SIZE);
	}

	@Override
	public void select(String selectionName) {
		// Deselect if they pressed the same one
		if (super.selectedItem != null && super.selectedItem.getName().equals(selectionName)) {
			super.editGrid.resetSelectedArea(selectedItem.getLocation().getX(), selectedItem.getLocation().getY());
			super.selectedItem = null;
			return;
		}
		ChunkComponent newSel = ComponentManager.getInstance().getComponent(RoomBlock.class, selectionName);
		super.selectedItem = newSel;
		editGrid.changedSelected(selectedItem.getWidth(), selectedItem.getHeight());
	}
}
