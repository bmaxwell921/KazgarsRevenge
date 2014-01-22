package main.kazgarsrevenge.ui.listeners;

import java.awt.Frame;
import java.awt.event.MouseAdapter;
import java.awt.event.MouseEvent;

import javax.swing.JOptionPane;
import javax.swing.SwingUtilities;

import main.kazgarsrevenge.ui.panels.KREditorPanel;
import main.kazgarsrevenge.ui.panels.RoomEditorPanel;
import main.kazgarsrevenge.util.managers.ComponentManager;
import main.kazgarsrevenge.util.managers.ImageManager;
import main.kazgarsrevenge.util.managers.UpdaterManager;

public class NewRoomClickListener extends MouseAdapter {

	// The parent for the created modal
	private final Frame parent;
	
	// The chunk editor this listener is associated with. Held so this listener can disable its KeyActions
	private final KREditorPanel chunkEditor;
	
	public NewRoomClickListener(Frame parent, KREditorPanel chunkEditor) {
		this.parent = parent;
		this.chunkEditor = chunkEditor;
	}
	
	@Override
	public void mouseClicked(MouseEvent e) {
		UpdaterManager.getInstance().disableListeners(chunkEditor.getClass());
		RoomEditorPanel rep = new RoomEditorPanel(parent, 450, 300);
		JOptionPane.showOptionDialog(parent, rep, "Room Editor", JOptionPane.DEFAULT_OPTION, JOptionPane.PLAIN_MESSAGE, 
				null, new String[] {"Finish"}, "Finish");
		UpdaterManager.getInstance().enableListeners(chunkEditor.getClass());
		UpdaterManager.getInstance().disableListeners(rep.getClass());
		
		// Load up the new side Panels
		new Thread() {
			@Override
			public void run() {
				ComponentManager.getInstance().loadNewRooms();
				ImageManager.getInstance().loadNewRooms();
				chunkEditor.recreateSidePanel();		
				SwingUtilities.invokeLater(new Runnable() {
					@Override
					public void run() {
						chunkEditor.revalidate();
						chunkEditor.repaint();
					}
				});
			}
		}.start();
	}
}
