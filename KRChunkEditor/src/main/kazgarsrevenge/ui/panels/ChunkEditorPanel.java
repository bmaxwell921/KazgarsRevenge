package main.kazgarsrevenge.ui.panels;

import java.awt.BorderLayout;
import java.awt.Dimension;
import java.awt.Frame;
import java.awt.image.BufferedImage;
import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

import javax.swing.JButton;
import javax.swing.JPanel;

import main.kazgarsrevenge.data.Location;
import main.kazgarsrevenge.data.Rotation;
import main.kazgarsrevenge.model.ChunkComponent;
import main.kazgarsrevenge.model.impl.Chunk;
import main.kazgarsrevenge.model.impl.Room;
import main.kazgarsrevenge.ui.listeners.LoadChunkClickListener;
import main.kazgarsrevenge.ui.listeners.NewChunkClickListener;
import main.kazgarsrevenge.ui.listeners.SaveChunkClickListener;
import main.kazgarsrevenge.ui.listeners.SelectRoomListener;
import main.kazgarsrevenge.util.ImageLoader;
import main.kazgarsrevenge.util.IO.ChunkComponentIO;
import main.kazgarsrevenge.util.managers.ComponentManager;

public class ChunkEditorPanel extends KREditorPanel {

	private static final int SQUARE_SIZE = 25;
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
		ComponentManager cm = ComponentManager.getInstance();
		List<String> roomNames = new ArrayList<>(cm.getNames(Room.class));
		Collections.sort(roomNames);
		
		for (String name : roomNames) {
			JPanel imgDesc = new ImageDescriptionPanel(name, ImageLoader.getRoomImage(name));
			super.selectables.addPanel(imgDesc);
			imgDesc.addMouseListener(new SelectRoomListener(this, name));
		}
		this.add(super.selectables, BorderLayout.EAST);
	}

	@Override
	protected void createButtonPanel() {
		super.buttonPanel = new JPanel();
		super.buttonPanel.setPreferredSize(new Dimension(800, 30));

		JButton newButton = new JButton("NEW");
		newButton.addMouseListener(new NewChunkClickListener(super.parent, this));
		super.buttonPanel.add(newButton);
		
		JButton loadButton = new JButton("LOAD");
		loadButton.addMouseListener(new LoadChunkClickListener(super.parent, this));
		super.buttonPanel.add(loadButton);
		
		JButton saveButton = new JButton("SAVE");
		saveButton.addMouseListener(new SaveChunkClickListener(super.parent, this));
		super.buttonPanel.add(saveButton);
		
		this.add(super.buttonPanel, BorderLayout.PAGE_END);
	}

	@Override
	public void load(File loadFile, StringBuilder errors) {
		newEditable();
		super.editing = ChunkComponentIO.loadChunkComponent(Chunk.class, loadFile, errors);
	}

	@Override
	public void newEditable() {
		super.editing = new Chunk(new Location(), "Chunk", Rotation.ZERO);
		super.editGrid.reset(GRID_SIZE, GRID_SIZE);
	}

	@Override
	public void select(String selectionName) {
		ChunkComponent newSel = ComponentManager.getInstance().getComponent(Room.class, selectionName);
		
		// Deselect if they clicked the same one
		if (super.selectedItem != null && super.selectedItem.getName().equals(selectionName)) {
			super.editGrid.resetSelectedArea(selectedItem.getLocation().getX(), selectedItem.getLocation().getY());
			super.selectedItem = null;
			return;
		}
		super.selectedItem = newSel;
		super.selectedItem.setLocation(editGrid.getSelectedLocation());
	}
	
	@Override
	public BufferedImage getSelectedImage() {
		if (!hasSelection()) {
			return null;
		}
		return ImageLoader.getRoomImage(super.selectedItem.getName());
	}
}
