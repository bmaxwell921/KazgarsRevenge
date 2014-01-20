package main.kazgarsrevenge.ui.panels;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.image.BufferedImage;
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
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.IO.ChunkComponentIO;
import main.kazgarsrevenge.util.managers.ComponentManager;

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
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageLoader.getBlockImage(name));
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
		super.editing = ChunkComponentIO.loadChunkComponent(Room.class, loadFile, errors);
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
		super.selectedItem.setLocation(editGrid.getSelectedLocation());
	}

	@Override
	public BufferedImage getSelectedImage() {
		if (!hasSelection()){
			return null;
		}
		return ImageLoader.getBlockImage(super.selectedItem.getName());
	}

}
